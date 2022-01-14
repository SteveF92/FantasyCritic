ALTER TABLE `tbl_league_managermessage`
	ADD COLUMN `Deleted` BIT NOT NULL DEFAULT 0 AFTER `Timestamp`;

ALTER TABLE `tbl_league_managermessage`
	CHANGE COLUMN `Deleted` `Deleted` BIT(1) NOT NULL AFTER `Timestamp`;