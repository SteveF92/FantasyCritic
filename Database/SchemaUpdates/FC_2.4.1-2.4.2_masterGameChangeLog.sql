CREATE TABLE `tbl_mastergame_changelog` (
	`MasterGameChangeID` CHAR(36) NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL,
	`ChangedByUserID` CHAR(36) NOT NULL,
	`Timestamp` DATETIME NOT NULL,
	`Description` TEXT NOT NULL,
	PRIMARY KEY (`MasterGameChangeID`) USING BTREE,
	INDEX `FK__tbl_mastergame` (`MasterGameID`) USING BTREE,
	INDEX `FK__tbl_user` (`ChangedByUserID`) USING BTREE,
	CONSTRAINT `FK__tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK__tbl_user` FOREIGN KEY (`ChangedByUserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;

ALTER TABLE `tbl_mastergame_changerequest`
	ADD COLUMN `ResponseUserID` CHAR(36) NULL DEFAULT NULL AFTER `ResponseNote`;

UPDATE tbl_mastergame_changerequest SET ResponseUserID = {USERID} WHERE ResponseNote IS NOT NULL;

ALTER TABLE `tbl_mastergame_request`
	ADD COLUMN `ResponseUserID` CHAR(36) NULL DEFAULT NULL AFTER `ResponseNote`;

UPDATE tbl_mastergame_request SET ResponseUserID = {USERID} WHERE ResponseNote IS NOT NULL;

ALTER TABLE `tbl_mastergame`
	ADD COLUMN `AddedByUserID` CHAR(36) NULL DEFAULT NULL AFTER `AddedTimestamp`;

UPDATE tbl_mastergame SET AddedByUserID = {USERID};

ALTER TABLE `tbl_mastergame`
	CHANGE COLUMN `AddedByUserID` `AddedByUserID` CHAR(36) NOT NULL AFTER `AddedTimestamp`;
