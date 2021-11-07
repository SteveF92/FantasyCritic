ALTER TABLE `tbl_user`
	ADD COLUMN `TwoFactorEnabled` BIT NOT NULL AFTER `SecurityStamp`,
	ADD COLUMN `AuthenticatorKey` VARCHAR(255) NULL AFTER `TwoFactorEnabled`;

CREATE TABLE `tbl_user_recoverycode` (
	`UserID` CHAR(36) NOT NULL,
	`RecoveryCode` VARCHAR(255) NOT NULL,
	`CreatedTimestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (`UserID`, `RecoveryCode`) USING BTREE,
	CONSTRAINT `tbl_user_recoverycode_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `fantasycritic`.`tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;
