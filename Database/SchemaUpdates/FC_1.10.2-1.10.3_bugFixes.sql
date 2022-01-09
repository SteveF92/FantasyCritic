ALTER TABLE `tbl_user`
	CHANGE COLUMN `SecurityStamp` `SecurityStamp` VARCHAR(255) NULL COLLATE 'utf8mb4_general_ci' AFTER `PasswordHash`;

ALTER TABLE `tbl_mastergame`
	DROP INDEX `UniqueBoxArt`;
