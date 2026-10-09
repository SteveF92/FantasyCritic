CREATE TABLE `tbl_league_slot` (
	`LeagueSlotID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`LeagueID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`Year` YEAR NOT NULL,
	`CounterPick` BIT(1) NOT NULL,
	`SlotNumber` INT NOT NULL,
	PRIMARY KEY (`LeagueSlotID`) USING BTREE,
	UNIQUE INDEX `UNQ_LeagueSlot` (`LeagueID`, `Year`, `CounterPick`, `SlotNumber`) USING BTREE,
	CONSTRAINT `FK_tbl_league_slot_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;

CREATE TABLE `tbl_league_slotrequirestag` (
	`LeagueSlotID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`Tag` VARCHAR(255) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	PRIMARY KEY (`LeagueSlotID`, `Tag`) USING BTREE,
	INDEX `FK_tbl_league_slotrequirestag_tbl_mastergame_tag` (`Tag`) USING BTREE,
	CONSTRAINT `FK_tbl_league_slotrequirestag_tbl_league_slot` FOREIGN KEY (`LeagueSlotID`) REFERENCES `tbl_league_slot` (`LeagueSlotID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_slotrequirestag_tbl_mastergame_tag` FOREIGN KEY (`Tag`) REFERENCES `tbl_mastergame_tag` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;
