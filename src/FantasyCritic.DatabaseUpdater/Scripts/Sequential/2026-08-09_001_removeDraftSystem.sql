ALTER TABLE `tbl_league_year`
	DROP FOREIGN KEY `tbl_league_year_ibfk_1`,
	DROP INDEX `FK_tblleagueyear_tbldraftsystem`,
	DROP COLUMN `DraftSystem`;

DROP TABLE IF EXISTS `tbl_settings_draftsystem`;
