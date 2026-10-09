-- Publisher slots as data (docs/publisher-slot-plan.md, Step 1).
--
-- Slots are defined once per league year in tbl_league_slot; every publisher has the same slots. A publisher game
-- points at the slot it occupies instead of carrying a slot number, so a game can no longer sit outside the roster.
-- tbl_league_specialgameslot and the StandardGames / CounterPicks columns on tbl_league_year are replaced by these
-- rows and dropped at the end.
--
-- The script is ordered so that a data problem stops it before anything is dropped, and so that a rerun after a
-- data fix picks up where it left off: tables are created IF NOT EXISTS, slot rows are only generated for league
-- years that have none, tag rows ignore duplicates, and the new column is only added when missing.
--
-- If the "games outside the roster" guard fails, list the offending games with:
--   SELECT * FROM tbl_league_publishergame WHERE LeagueSlotID IS NULL;
-- Their SlotNumber is past the roster (>= StandardGames, or >= CounterPicks for counter picks). Fix their
-- SlotNumber or remove them, then rerun the migrator.

-- 1. The slot tables.

CREATE TABLE IF NOT EXISTS `tbl_league_slot` (
	`LeagueSlotID` CHAR(36) NOT NULL,
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`CounterPick` BIT(1) NOT NULL,
	`SlotNumber` INT NOT NULL,
	`SlotName` VARCHAR(50) NULL DEFAULT NULL,
	PRIMARY KEY (`LeagueSlotID`) USING BTREE,
	UNIQUE INDEX `UNQ_LeagueSlot` (`LeagueID`, `Year`, `CounterPick`, `SlotNumber`) USING BTREE,
	-- Target of the composite foreign key from tbl_league_publishergame (LeagueSlotID, CounterPick).
	UNIQUE INDEX `UNQ_LeagueSlot_CounterPick` (`LeagueSlotID`, `CounterPick`) USING BTREE,
	CONSTRAINT `FK_tbl_league_slot_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;

CREATE TABLE IF NOT EXISTS `tbl_league_slotrequirestag` (
	`LeagueSlotID` CHAR(36) NOT NULL,
	`Tag` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`LeagueSlotID`, `Tag`) USING BTREE,
	INDEX `FK_tbl_league_slotrequirestag_tbl_mastergame_tag` (`Tag`) USING BTREE,
	CONSTRAINT `FK_tbl_league_slotrequirestag_tbl_league_slot` FOREIGN KEY (`LeagueSlotID`) REFERENCES `tbl_league_slot` (`LeagueSlotID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_slotrequirestag_tbl_mastergame_tag` FOREIGN KEY (`Tag`) REFERENCES `tbl_mastergame_tag` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;

-- 2. One slot row per standard game and per counter pick, for every league year.
--    StandardGames and CounterPicks are TINYINT, so 0..255 covers every possible slot number.

INSERT INTO `tbl_league_slot` (`LeagueSlotID`, `LeagueID`, `Year`, `CounterPick`, `SlotNumber`, `SlotName`)
WITH RECURSIVE numbers AS (
	SELECT 0 AS n
	UNION ALL
	SELECT n + 1 FROM numbers WHERE n < 255
)
SELECT UUID(), ly.`LeagueID`, ly.`Year`, b'0', numbers.n, NULL
FROM `tbl_league_year` ly
JOIN numbers ON numbers.n < ly.`StandardGames`
WHERE NOT EXISTS (
	SELECT 1 FROM `tbl_league_slot` s
	WHERE s.`LeagueID` = ly.`LeagueID` AND s.`Year` = ly.`Year` AND s.`CounterPick` = b'0'
);

INSERT INTO `tbl_league_slot` (`LeagueSlotID`, `LeagueID`, `Year`, `CounterPick`, `SlotNumber`, `SlotName`)
WITH RECURSIVE numbers AS (
	SELECT 0 AS n
	UNION ALL
	SELECT n + 1 FROM numbers WHERE n < 255
)
SELECT UUID(), ly.`LeagueID`, ly.`Year`, b'1', numbers.n, NULL
FROM `tbl_league_year` ly
JOIN numbers ON numbers.n < ly.`CounterPicks`
WHERE NOT EXISTS (
	SELECT 1 FROM `tbl_league_slot` s
	WHERE s.`LeagueID` = ly.`LeagueID` AND s.`Year` = ly.`Year` AND s.`CounterPick` = b'1'
);

-- 3. Special slot tags. Special slots have always been the last slots of the roster:
--    slot number = StandardGames - (number of special slots) + SpecialSlotPosition.

INSERT INTO `tbl_league_slotrequirestag` (`LeagueSlotID`, `Tag`)
SELECT s.`LeagueSlotID`, sgs.`Tag`
FROM `tbl_league_specialgameslot` sgs
JOIN `tbl_league_year` ly ON ly.`LeagueID` = sgs.`LeagueID` AND ly.`Year` = sgs.`Year`
JOIN (
	SELECT `LeagueID`, `Year`, COUNT(DISTINCT `SpecialSlotPosition`) AS SpecialSlotCount
	FROM `tbl_league_specialgameslot`
	GROUP BY `LeagueID`, `Year`
) counts ON counts.`LeagueID` = sgs.`LeagueID` AND counts.`Year` = sgs.`Year`
JOIN `tbl_league_slot` s
	ON s.`LeagueID` = sgs.`LeagueID`
	AND s.`Year` = sgs.`Year`
	AND s.`CounterPick` = b'0'
	AND s.`SlotNumber` = ly.`StandardGames` - counts.SpecialSlotCount + sgs.`SpecialSlotPosition`
WHERE sgs.`Tag` IS NOT NULL
ON DUPLICATE KEY UPDATE `Tag` = `tbl_league_slotrequirestag`.`Tag`;

-- 4. Each publisher game points at its slot, with two guards. This runs inside a throwaway procedure because
--    MySQL has no ADD COLUMN IF NOT EXISTS and MySqlConnector rejects user variables in scripts; a procedure gives
--    us IF and SIGNAL. MESSAGE_TEXT is limited to 128 characters.

DROP PROCEDURE IF EXISTS `sp_tmp_publisherslotrefactor`;
DELIMITER //
CREATE PROCEDURE `sp_tmp_publisherslotrefactor`()
BEGIN
	DECLARE tagsWithoutSlot INT;
	DECLARE gamesOutsideRoster INT;
	DECLARE message VARCHAR(128);

	-- Guard: every special slot tag found its slot. A special slot whose position doesn't fit inside the roster
	-- would have been skipped silently by the join in step 3.
	SELECT (SELECT COUNT(DISTINCT `LeagueID`, `Year`, `SpecialSlotPosition`, `Tag`) FROM `tbl_league_specialgameslot` WHERE `Tag` IS NOT NULL)
	     - (SELECT COUNT(*) FROM `tbl_league_slotrequirestag`)
	INTO tagsWithoutSlot;
	IF tagsWithoutSlot <> 0 THEN
		SET message = CONCAT('publisherSlotRefactor: ', tagsWithoutSlot, ' special slot tag(s) have no slot inside the roster. Nothing dropped.');
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = message;
	END IF;

	IF NOT EXISTS (
		SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tbl_league_publishergame' AND COLUMN_NAME = 'LeagueSlotID'
	) THEN
		ALTER TABLE `tbl_league_publishergame` ADD COLUMN `LeagueSlotID` CHAR(36) NULL DEFAULT NULL AFTER `SlotNumber`;
	END IF;

	UPDATE `tbl_league_publishergame` pg
	JOIN `tbl_league_publisher` p ON p.`PublisherID` = pg.`PublisherID`
	JOIN `tbl_league_slot` s
		ON s.`LeagueID` = p.`LeagueID`
		AND s.`Year` = p.`Year`
		AND s.`CounterPick` = pg.`CounterPick`
		AND s.`SlotNumber` = pg.`SlotNumber`
	SET pg.`LeagueSlotID` = s.`LeagueSlotID`;

	-- Guard: every game is in a slot. This is where a game past the roster stops the migration, with nothing
	-- dropped yet. See the header for how to find them.
	SELECT COUNT(*) INTO gamesOutsideRoster FROM `tbl_league_publishergame` WHERE `LeagueSlotID` IS NULL;
	IF gamesOutsideRoster <> 0 THEN
		SET message = CONCAT('publisherSlotRefactor: ', gamesOutsideRoster, ' publisher game(s) sit outside the roster. Nothing dropped.');
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = message;
	END IF;
END //
DELIMITER ;

CALL `sp_tmp_publisherslotrefactor`();
DROP PROCEDURE `sp_tmp_publisherslotrefactor`;

-- 5. Lock it in: a game always has a slot, a publisher has one game per slot, and a counter pick sits in a
--    counter-pick slot. The new unique index also takes over from UNQ_Slot as the index behind the PublisherID
--    foreign key, which is why it is added before UNQ_Slot is dropped.

ALTER TABLE `tbl_league_publishergame`
	MODIFY COLUMN `LeagueSlotID` CHAR(36) NOT NULL,
	ADD UNIQUE INDEX `UNQ_PublisherSlot` (`PublisherID`, `LeagueSlotID`) USING BTREE,
	ADD INDEX `FK_tbl_league_publishergame_tbl_league_slot` (`LeagueSlotID`, `CounterPick`) USING BTREE,
	ADD CONSTRAINT `FK_tbl_league_publishergame_tbl_league_slot` FOREIGN KEY (`LeagueSlotID`, `CounterPick`) REFERENCES `tbl_league_slot` (`LeagueSlotID`, `CounterPick`) ON UPDATE NO ACTION ON DELETE NO ACTION;

-- 6. Remove the old model.

ALTER TABLE `tbl_league_publishergame`
	DROP INDEX `UNQ_Slot`,
	DROP COLUMN `SlotNumber`;

ALTER TABLE `tbl_league_year`
	DROP COLUMN `StandardGames`,
	DROP COLUMN `CounterPicks`;

DROP TABLE IF EXISTS `tbl_league_specialgameslot`;
