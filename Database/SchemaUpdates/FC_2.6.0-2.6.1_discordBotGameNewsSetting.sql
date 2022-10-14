ALTER TABLE `tbl_league_managermessage`
	ADD COLUMN `IsGameNewsEnabled` BIT NOT NULL DEFAULT 1 AFTER `LeagueID`;
