ALTER TABLE `tbl_league_year`
	ADD COLUMN `WinningUserID` CHAR(36) NULL DEFAULT NULL AFTER `DraftStartedTimestamp`,
	ADD CONSTRAINT `FK_tbl_league_year_tbl_user` FOREIGN KEY (`WinningUserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION;
