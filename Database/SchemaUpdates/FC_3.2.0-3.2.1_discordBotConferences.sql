CREATE TABLE `fantasycritic`.`tbl_discord_conferencechannel` (
	`GuildID` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
	`ChannelID` BIGINT(20) UNSIGNED NOT NULL DEFAULT '0',
	`ConferenceID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	PRIMARY KEY (`GuildID`, `ChannelID`) USING BTREE,
	INDEX `FK_ConferenceID` (`ConferenceID`) USING BTREE,
	FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
