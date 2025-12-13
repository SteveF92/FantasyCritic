CREATE TABLE `tbl_royale_action` (
	`ID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`PublisherID` CHAR(36) NOT NULL,
	`Timestamp` DATETIME(6) NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL,
	`ActionType` VARCHAR(255) NOT NULL,
	`Description` TEXT NOT NULL,
	PRIMARY KEY (`ID`) USING BTREE,
	INDEX `FK__tbl_royale_publisher` (`PublisherID`) USING BTREE,
	INDEX `FK_tbl_royale_action_tbl_mastergame` (`MasterGameID`) USING BTREE,
	CONSTRAINT `FK_tbl_royale_action_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK__tbl_royale_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_royale_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
