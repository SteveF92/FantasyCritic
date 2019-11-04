INSERT INTO tbl_league_activeplayer(LeagueID,YEAR,UserID)
SELECT tbl_league_year.LeagueID, tbl_league_year.Year, tbl_league_hasuser.UserID FROM tbl_league_hasuser
JOIN tbl_league_year
ON tbl_league_hasuser.LeagueID = tbl_league_year.LeagueID