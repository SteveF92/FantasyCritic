CREATE TABLE `tbl_conference` (
	`ConferenceID` CHAR(36) NOT NULL,
	`ConferenceName` VARCHAR(150) NOT NULL,
	`ConferenceManager` CHAR(36) NOT NULL,
	`CustomRulesConference` BIT(1) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL,
	`IsDeleted` BIT(1) NOT NULL DEFAULT 'b\'0\'',
	PRIMARY KEY (`ConferenceID`) USING BTREE,
	INDEX `FK_tbl_league_conference_tbl_user` (`ConferenceManager`) USING BTREE,
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