ALTER TABLE `tbl_discord_leaguechannel`
	DROP FOREIGN KEY `FK_tbl_discord_leaguechannel_tbl_caching_leagueyear`,
	ADD CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION;
