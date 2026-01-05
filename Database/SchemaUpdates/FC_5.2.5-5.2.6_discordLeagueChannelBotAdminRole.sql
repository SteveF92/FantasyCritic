ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `BotAdminRoleID` BIGINT NULL DEFAULT NULL AFTER `LeagueID`;
