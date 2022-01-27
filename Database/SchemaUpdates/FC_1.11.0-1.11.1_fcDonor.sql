CREATE TABLE `tbl_user_donorname` (
	`UserID` CHAR(36) NOT NULL,
	`DonorName` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`UserID`) USING BTREE,
	CONSTRAINT `tbl_user_donorname_ibfk_2` FOREIGN KEY (`UserID`) REFERENCES `fantasycritic`.`tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
