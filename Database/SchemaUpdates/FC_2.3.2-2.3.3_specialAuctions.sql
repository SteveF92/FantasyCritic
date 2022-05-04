CREATE TABLE `tbl_league_specialauction` (
	`SpecialAuctionID` CHAR(36) NOT NULL,
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL,
	`CreationTime` DATETIME NOT NULL,
	`ScheduledEndTime` DATETIME NOT NULL,
	`Processed` BIT(1) NOT NULL,
	PRIMARY KEY (`SpecialAuctionID`) USING BTREE,
	UNIQUE INDEX `UNQ_LeagueID_Year_MasterGame` (`LeagueID`, `Year`, `MasterGameID`) USING BTREE,
	INDEX `FK_tbl_league_specialauction_tbl_mastergame` (`MasterGameID`) USING BTREE,
	CONSTRAINT `FK_tbl_league_specialauction_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_specialauction_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
