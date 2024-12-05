CREATE TABLE `tbl_conference_activeplayer` (
	`ConferenceID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`UserID` CHAR(36) NOT NULL,
	PRIMARY KEY (`ConferenceID`, `Year`, `UserID`) USING BTREE,
	INDEX `tbl_` (`ConferenceID`, `UserID`) USING BTREE,
	CONSTRAINT `FK__tbl_conference_year` FOREIGN KEY (`ConferenceID`, `Year`) REFERENCES `tbl_conference_year` (`ConferenceID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `tbl_` FOREIGN KEY (`ConferenceID`, `UserID`) REFERENCES `tbl_conference_hasuser` (`ConferenceID`, `UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
