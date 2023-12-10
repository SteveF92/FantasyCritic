CREATE TABLE `tbl_conference` (
	`ConferenceID` CHAR(36) NOT NULL,
	`ConferenceName` VARCHAR(150) NOT NULL,
	`ConferenceManager` CHAR(36) NOT NULL,
	`PrimaryLeagueID` CHAR(36) NOT NULL,
	`CustomRulesConference` BIT(1) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`IsDeleted` BIT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`ConferenceID`) USING BTREE,
	INDEX `FK_tbl_conference_tbl_user` (`ConferenceManager`) USING BTREE,
	INDEX `FK_tbl_conference_tbl_league` (`PrimaryLeagueID`) USING BTREE,
	CONSTRAINT `FK_tbl_conference_tbl_league` FOREIGN KEY (`PrimaryLeagueID`) REFERENCES `tbl_league` (`LeagueID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_conference_tbl_user` FOREIGN KEY (`ConferenceManager`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;

CREATE TABLE `tbl_conference_hasuser` (
	`ConferenceID` CHAR(36) NOT NULL,
	`UserID` CHAR(36) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`ConferenceID`, `UserID`) USING BTREE,
	INDEX `FK_tbl_conference_hasuser_tbl_user` (`UserID`) USING BTREE,
	CONSTRAINT `FK_tbl_conference_hasuser_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_conference_hasuser_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;

CREATE TABLE `tbl_conference_invitelink` (
	`InviteID` CHAR(36) NOT NULL,
	`ConferenceID` CHAR(36) NOT NULL,
	`InviteCode` CHAR(36) NOT NULL,
	`Active` BIT(1) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`InviteID`) USING BTREE,
	UNIQUE INDEX `Unique_Conference_Code` (`ConferenceID`, `InviteCode`) USING BTREE,
	CONSTRAINT `FK_tbl_conference_invitelink_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

ALTER TABLE `tbl_league`
	ADD COLUMN `ConferenceID` CHAR(36) NULL AFTER `LeagueManager`,
	ADD CONSTRAINT `FK_tbl_league_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`) ON UPDATE NO ACTION ON DELETE NO ACTION;

	CREATE TABLE `tbl_conference_year` (
	`ConferenceID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`OpenForDrafting` BIT(1) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`ConferenceID`, `Year`) USING BTREE,
	INDEX `FK_tbl_conference_year_tbl_meta_supportedyear` (`Year`) USING BTREE,
	CONSTRAINT `FK_tbl_conference_year_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_conference_year_tbl_meta_supportedyear` FOREIGN KEY (`Year`) REFERENCES `tbl_meta_supportedyear` (`Year`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;

CREATE TABLE `tbl_conference_managermessage` (
	`MessageID` CHAR(36) NOT NULL,
	`ConferenceID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`MessageText` TEXT NOT NULL,
	`IsPublic` BIT(1) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	`Deleted` BIT(1) NOT NULL,
	PRIMARY KEY (`MessageID`) USING BTREE,
	INDEX `FK_tbl_conference_managermessage_tbl_conference_year` (`ConferenceID`, `Year`) USING BTREE,
	CONSTRAINT `FK_tbl_conference_managermessage_tbl_conference_year` FOREIGN KEY (`ConferenceID`, `Year`) REFERENCES `tbl_conference_year` (`ConferenceID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

CREATE TABLE `tbl_conference_managermessagedismissal` (
	`MessageID` CHAR(36) NOT NULL,
	`UserID` CHAR(36) NOT NULL,
	PRIMARY KEY (`MessageID`, `UserID`) USING BTREE,
	INDEX `FK_tbl_conference_managermessagedismissal_tbl_user` (`UserID`) USING BTREE,
	CONSTRAINT `FK_tbl_conference_managermessagedismissal` FOREIGN KEY (`MessageID`) REFERENCES `tbl_conference_managermessage` (`MessageID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_conference_managermessagedismissal_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

CREATE TABLE `tbl_caching_conferenceyearstandings` (
	`ConferenceID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`LeagueID` CHAR(36) NOT NULL,
	`PublisherID` CHAR(36) NOT NULL,
	`LeagueName` VARCHAR(150) NOT NULL,
	`PublisherName` VARCHAR(100) NOT NULL,
	PRIMARY KEY (`ConferenceID`, `Year`, `LeagueID`, `PublisherID`)
)
ENGINE=InnoDB
;

DROP VIEW IF EXISTS `vw_league`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league` AS select `tbl_league`.`LeagueID` AS `LeagueID`,
`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_league`.`LeagueManager` AS `LeagueManager`,`tbl_league`.`ConferenceID` AS `ConferenceID`,
`tbl_conference`.`ConferenceName` AS `ConferenceName`,`tbl_league`.`PublicLeague` AS `PublicLeague`,`tbl_league`.`TestLeague` AS `TestLeague`,`tbl_league`.`CustomRulesLeague` AS `CustomRulesLeague`,
`tbl_league`.`Timestamp` AS `Timestamp`,count(`tbl_user_followingleague`.`UserID`) AS `NumberOfFollowers`,`tbl_league`.`IsDeleted` AS `IsDeleted` 
from ((`tbl_league` left join `tbl_user_followingleague` on((`tbl_league`.`LeagueID` = `tbl_user_followingleague`.`LeagueID`))) left join `tbl_conference` on((`tbl_league`.`ConferenceID` = `tbl_conference`.`ConferenceID`))) 
group by `tbl_league`.`LeagueID`;