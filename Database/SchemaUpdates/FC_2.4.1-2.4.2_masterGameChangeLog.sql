CREATE TABLE `tbl_mastergame_changelog` (
	`MasterGameChangeID` CHAR(36) NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL,
	`ChangedByUserID` CHAR(36) NOT NULL,
	`Timestamp` DATETIME NOT NULL,
	`Change` TEXT NOT NULL,
	PRIMARY KEY (`MasterGameChangeID`) USING BTREE,
	INDEX `FK__tbl_mastergame` (`MasterGameID`) USING BTREE,
	INDEX `FK__tbl_user` (`ChangedByUserID`) USING BTREE,
	CONSTRAINT `FK__tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK__tbl_user` FOREIGN KEY (`ChangedByUserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
