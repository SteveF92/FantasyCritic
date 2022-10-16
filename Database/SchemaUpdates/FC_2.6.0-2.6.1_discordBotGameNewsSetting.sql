ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `IsGameNewsEnabled` BIT NOT NULL AFTER `ChannelID`;
