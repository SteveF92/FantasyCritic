CREATE TABLE `tbl_league_manageraction` (
	`ID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`Timestamp` DATETIME(6) NOT NULL,
	`ActionType` VARCHAR(255) NOT NULL,
	`Description` TEXT NOT NULL,
	PRIMARY KEY (`ID`) USING BTREE,
	INDEX `FK_tbl_league_manageraction_tbl_league_year` (`LeagueID`, `Year`) USING BTREE,
	CONSTRAINT `FK_tbl_league_manageraction_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
