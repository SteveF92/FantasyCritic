CREATE TABLE `tbl_league_publisherslot` (
	`PublisherSlotID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`PublisherID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`SpecialSlotID` CHAR(36) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`CounterPick` BIT(1) NOT NULL,
	`SlotNumber` INT NOT NULL,
	`PublisherGameID` CHAR(36) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
	PRIMARY KEY (`PublisherSlotID`) USING BTREE,
	UNIQUE INDEX `UNQ_PublilsherSlot` (`PublisherID`, `CounterPick`, `SlotNumber`) USING BTREE,
	INDEX `FK_tbl_league_publisherslot_tbl_league_specialgameslot` (`SpecialSlotID`) USING BTREE,
	INDEX `FK_tbl_league_publisherslot_tbl_league_publishergame` (`PublisherGameID`) USING BTREE,
	CONSTRAINT `FK_tbl_league_publisherslot_tbl_league_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_publisherslot_tbl_league_publishergame` FOREIGN KEY (`PublisherGameID`) REFERENCES `tbl_league_publishergame` (`PublisherGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_publisherslot_tbl_league_specialgameslot` FOREIGN KEY (`SpecialSlotID`) REFERENCES `tbl_league_specialgameslot` (`SpecialSlotID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;
