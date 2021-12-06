ALTER TABLE `tbl_league_publishergame`
	ADD COLUMN `SlotNumber` INT NOT NULL AFTER `CurrentlyIneligible`;

CREATE TABLE `tbl_league_specialgameslot` (
	`SpecialSlotID` CHAR(36) NOT NULL,
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`SpecialSlotPosition` INT(10) NOT NULL,
	`Tag` VARCHAR(255) NULL DEFAULT NULL,
	PRIMARY KEY (`SpecialSlotID`) USING BTREE,
	INDEX `FK_tbl_league_gameslotusestag_tbl_mastergame_tag` (`Tag`) USING BTREE,
	INDEX `FK_tbl_league_specialgameslot_tbl_league_year` (`LeagueID`, `Year`) USING BTREE,
	CONSTRAINT `FK_tbl_league_gameslotusestag_tbl_mastergame_tag` FOREIGN KEY (`Tag`) REFERENCES `fantasycritic`.`tbl_mastergame_tag` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_specialgameslot_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `fantasycritic`.`tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

-- Run the Utility now, then run the statement below

-- ALTER TABLE `tbl_league_publishergame`
--	ADD UNIQUE INDEX `UNQ_Slot` (`PublisherID`, `CounterPick`, `SlotNumber`);

