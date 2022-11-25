ALTER TABLE `tbl_discord_leaguechannel`
	CHANGE COLUMN `IsGameNewsEnabled` `GameNewsSetting` VARCHAR(50) NOT NULL DEFAULT 'Relevant' AFTER `ChannelID`;
ALTER TABLE `tbl_discord_leaguechannel`
	CHANGE COLUMN `GameNewsSetting` `GameNewsSetting` VARCHAR(50) NOT NULL AFTER `ChannelID`;
