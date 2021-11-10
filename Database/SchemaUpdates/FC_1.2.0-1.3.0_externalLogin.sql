CREATE TABLE `tbl_user_externallogin` (
	`LoginProvider` VARCHAR(255) NOT NULL,
	`ProviderKey` VARCHAR(255) NOT NULL,
	`UserID` CHAR(36) NOT NULL,
	`ProviderDisplayName` VARCHAR(255) NOT NULL,
	`TimeAdded` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`LoginProvider`, `ProviderKey`) USING BTREE,
	UNIQUE INDEX `UNQ_Login` (`LoginProvider`, `ProviderKey`, `UserID`) USING BTREE,
	INDEX `FK_tbl_user_externallogin_tbl_user` (`UserID`) USING BTREE,
	CONSTRAINT `FK_tbl_user_externallogin_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `fantasycritic`.`tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
