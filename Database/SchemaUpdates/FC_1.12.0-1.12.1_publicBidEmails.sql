CREATE TABLE `tbl_user_emailtype` (
	`EmailType` VARCHAR(255) NOT NULL,
	`ReadableName` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`EmailType`) USING BTREE
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

INSERT INTO `tbl_user_emailtype` (`EmailType`, `ReadableName`) VALUES ('PublicBids', 'Public Bids');

CREATE TABLE `tbl_user_emailsettings` (
	`UserID` CHAR(36) NOT NULL,
	`EmailType` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`UserID`, `EmailType`) USING BTREE,
	INDEX `FK_tbl_user_emailsettings_tbl_user_emailtype` (`EmailType`) USING BTREE,
	CONSTRAINT `FK_tbl_user_emailsettings_tbl_user_emailtype` FOREIGN KEY (`EmailType`) REFERENCES `fantasycritic`.`tbl_user_emailtype` (`EmailType`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `tbl_user_emailsettings_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `fantasycritic`.`tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;
