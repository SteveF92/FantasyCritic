ALTER TABLE `tbl_user`
	ADD COLUMN `ShowDecimalPoints` BIT NOT NULL DEFAULT 0 AFTER `LastChangedCredentials`;
