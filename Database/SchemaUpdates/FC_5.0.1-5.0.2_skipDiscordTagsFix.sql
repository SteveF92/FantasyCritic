ALTER TABLE `tbl_discord_gamenewschannelskiptag`
	DROP PRIMARY KEY,
	ADD PRIMARY KEY (`GuildID`, `ChannelID`, `TagName`) USING BTREE;
