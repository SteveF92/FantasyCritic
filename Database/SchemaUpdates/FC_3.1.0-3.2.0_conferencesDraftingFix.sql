ALTER TABLE `tbl_league_year`
	ADD COLUMN `ConferenceLocked` BIT NULL DEFAULT NULL AFTER `WinningUserID`;

UPDATE tbl_league_year
JOIN tbl_league ON tbl_league_year.LeagueID = tbl_league.LeagueID
JOIN tbl_conference_year ON tbl_league.ConferenceID = tbl_conference_year.ConferenceID AND tbl_conference_year.Year = tbl_league_year.Year
SET tbl_league_year.ConferenceLocked = tbl_conference_year.OpenForDrafting;

ALTER TABLE `tbl_conference_year`
	DROP COLUMN `OpenForDrafting`;
