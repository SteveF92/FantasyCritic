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
	ADD COLUMN `SendLeagueMasterGameUpdates` BIT NOT NULL DEFAULT 0 AFTER `ChannelID`,
	DROP COLUMN `GameNewsSetting`,
	DROP INDEX `FK_tbl_discord_leaguechannel_tbl_discord_gamenewsoptions`,
	DROP FOREIGN KEY `FK_tbl_discord_leaguechannel_tbl_discord_gamenewsoptions`;

