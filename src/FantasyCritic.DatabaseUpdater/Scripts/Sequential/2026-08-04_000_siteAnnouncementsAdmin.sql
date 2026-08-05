ALTER TABLE `tbl_meta_siteannouncements`
	DROP COLUMN `HtmlID`,
	ADD COLUMN `IsDeleted` BIT(1) NOT NULL DEFAULT b'0';
