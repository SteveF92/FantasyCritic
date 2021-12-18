ALTER TABLE `tbl_user_persistedgrant`
	CHANGE COLUMN `SessionId` `SessionId` VARCHAR(255) NULL AFTER `Description`;