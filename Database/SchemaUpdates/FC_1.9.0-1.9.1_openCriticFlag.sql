ALTER TABLE `tbl_meta_systemwidesettings`
	ADD COLUMN `RefreshOpenCritic` BIT NOT NULL AFTER `ActionProcessingMode`;

update tbl_meta_systemwidesettings SET RefreshOpenCritic = 1;