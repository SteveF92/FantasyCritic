CREATE TABLE `tbl_discord_gamenewschannel` (
	`GuildID` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
	`ChannelID` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
	`GameNewsSetting` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`LeagueID`, `GuildID`, `ChannelID`) USING BTREE,
	INDEX `FK_tbl_discord_leaguechannel_tbl_discord_gamenewsoptions` (`GameNewsSetting`) USING BTREE
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

INSERT INTO `tbl_discord_gamenewsoptions` (`Name`) VALUES ('MightReleaseInYear');
INSERT INTO `tbl_discord_gamenewsoptions` (`Name`) VALUES ('WillReleaseInYear');

ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `SendLeagueMasterGameUpdates` BIT NOT NULL DEFAULT 1 AFTER `ChannelID`,
	DROP COLUMN `GameNewsSetting`,
	DROP INDEX `FK_tbl_discord_leaguechannel_tbl_discord_gamenewsoptions`,
	DROP FOREIGN KEY `FK_tbl_discord_leaguechannel_tbl_discord_gamenewsoptions`;

CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_discord_leaguechannel` AS select `tbl_discord_leaguechannel`.`LeagueID` AS `LeagueID`,`tbl_discord_leaguechannel`.`GuildID` AS `GuildID`,`tbl_discord_leaguechannel`.`ChannelID` AS `ChannelID`,`tbl_discord_leaguechannel`.`SendLeagueMasterGameUpdates` AS `SendLeagueMasterGameUpdates`,`tbl_discord_leaguechannel`.`BidAlertRoleID` AS `BidAlertRoleID`,min(`tbl_league_year`.`Year`) AS `MinimumLeagueYear` from (`tbl_discord_leaguechannel` join `tbl_league_year` on((`tbl_discord_leaguechannel`.`LeagueID` = `tbl_league_year`.`LeagueID`))) group by `tbl_discord_leaguechannel`.`LeagueID`,`tbl_discord_leaguechannel`.`GuildID`,`tbl_discord_leaguechannel`.`ChannelID`;
