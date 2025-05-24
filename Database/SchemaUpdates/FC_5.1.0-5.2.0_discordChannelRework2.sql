ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `ShowIneligibleGameNews` BIT NULL DEFAULT NULL AFTER `ShowEligibleGameNews`;

UPDATE tbl_discord_leaguechannel SET ShowIneligibleGameNews = 0;

ALTER TABLE `tbl_discord_leaguechannel`
	CHANGE COLUMN `ShowIneligibleGameNews` `ShowIneligibleGameNews` BIT(1) NOT NULL AFTER `ShowEligibleGameNews`;

INSERT IGNORE INTO tbl_discord_gamenewschannel (
GuildID,
ChannelID,
ShowWillReleaseInYearNews,
ShowMightReleaseInYearNews,
ShowWillNotReleaseInYearNews,
ShowScoreGameNews,
ShowReleasedGameNews,
ShowNewGameNews,
ShowEditedGameNews)
SELECT GuildID, ChannelID, 1, 1, 0, 1, 1, 0, 1
FROM tbl_discord_leaguechannel
WHERE ShowPickedGameNews = 1 OR ShowEligibleGameNews = 1;