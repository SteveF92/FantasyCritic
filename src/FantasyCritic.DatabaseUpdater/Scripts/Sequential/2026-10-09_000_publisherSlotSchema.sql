-- Publisher slots as data, part 1 of 3: the new schema (docs/publisher-slot-plan.md, Step 1).
--
-- Slots are defined once per league year in tbl_league_slot; every publisher has the same slots. A publisher game
-- points at the slot it occupies through LeagueSlotID, which starts out nullable here. The code migration
-- 2026-10-09_001_publisherSlotData.cs fills these tables and the column, and 2026-10-09_002_publisherSlotCleanup.sql
-- makes the column NOT NULL and drops the old model.

CREATE TABLE `tbl_league_slot` (
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

CREATE TABLE `tbl_league_slotrequirestag` (
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

-- A publisher has one game per slot, and a counter pick sits in a counter-pick slot. Both constraints accept NULL,
-- so they can go in now and validate the backfill as it happens.
ALTER TABLE `tbl_league_publishergame`
	ADD COLUMN `LeagueSlotID` CHAR(36) NULL DEFAULT NULL AFTER `SlotNumber`,
	ADD UNIQUE INDEX `UNQ_PublisherSlot` (`PublisherID`, `LeagueSlotID`) USING BTREE,
	ADD INDEX `FK_tbl_league_publishergame_tbl_league_slot` (`LeagueSlotID`, `CounterPick`) USING BTREE,
	ADD CONSTRAINT `FK_tbl_league_publishergame_tbl_league_slot` FOREIGN KEY (`LeagueSlotID`, `CounterPick`) REFERENCES `tbl_league_slot` (`LeagueSlotID`, `CounterPick`) ON UPDATE NO ACTION ON DELETE NO ACTION;
