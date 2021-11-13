CREATE TABLE `tbl_league_tagoverride` (
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL,
	`TagName` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`LeagueID`, `Year`, `MasterGameID`, `TagName`) USING BTREE,
	INDEX `FK_tbl_league_tagoverride_tbl_mastergame` (`MasterGameID`) USING BTREE,
	INDEX `FK_tbl_league_tagoverride_tbl_mastergame_tag` (`TagName`) USING BTREE,
	CONSTRAINT `FK_tbl_league_tagoverride_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `fantasycritic`.`tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_tagoverride_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `fantasycritic`.`tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_tagoverride_tbl_mastergame_tag` FOREIGN KEY (`TagName`) REFERENCES `fantasycritic`.`tbl_mastergame_tag` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
