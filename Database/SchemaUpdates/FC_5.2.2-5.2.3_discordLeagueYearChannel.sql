ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `Year` YEAR NULL AFTER `NotableMissSetting`,
	ADD CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_caching_leagueyear` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_caching_leagueyear` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION;