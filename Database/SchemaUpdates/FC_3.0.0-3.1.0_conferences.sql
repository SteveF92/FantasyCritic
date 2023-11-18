CREATE TABLE `tbl_league_conference` (
	`ConferenceID` CHAR(36) NOT NULL,
	`ConferenceName` VARCHAR(150) NOT NULL,
	`ConferenceManager` CHAR(36) NOT NULL,
	`CustomRulesConference` BIT(1) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL,
	`IsDeleted` BIT(1) NOT NULL DEFAULT 'b\'0\'',
	PRIMARY KEY (`ConferenceID`) USING BTREE,
	INDEX `FK_tbl_league_conference_tbl_user` (`ConferenceManager`) USING BTREE,
	CONSTRAINT `FK_tbl_league_conference_tbl_user` FOREIGN KEY (`ConferenceManager`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
