BEGIN 
	DECLARE userEmailAddress VARCHAR(255);
	DECLARE maxProcessDate DATE;
	DECLARE maxProcessYear INT;
	DECLARE highestNormalYear INT;
	DECLARE highestRoyaleYear INT;
	DECLARE highestRoyaleQuarter INT;
	
	
	SELECT EmailAddress INTO userEmailAddress
	FROM tbl_user
	WHERE UserID = P_UserID;
	
	-- My Leagues
	 CALL sp_getleaguesforuser(P_UserID);
	
	-- League Invites
	
	SELECT tbl_league_invite.*,
	       tbl_league.LeagueName,
	       inviteUser.DisplayName AS InviteUserName,
	       inviteUser.EmailAddress AS InviteUserEmailAddress,
	       leagueManager.DisplayName AS ManagerUserName,
	       leagueYear.ActiveYear
	FROM tbl_league_invite
	JOIN tbl_league ON tbl_league.LeagueID = tbl_league_invite.LeagueID
	JOIN tbl_user leagueManager ON tbl_league.LeagueManager = leagueManager.UserID
	LEFT JOIN tbl_user inviteUser ON tbl_league_invite.UserID = inviteUser.UserID
	LEFT JOIN
	  (SELECT LeagueID,
	          MAX(YEAR) AS ActiveYear
	   FROM tbl_league_year
	   GROUP BY LeagueID) leagueYear ON tbl_league.LeagueID = leagueYear.LeagueID
	WHERE tbl_league_invite.EmailAddress = userEmailAddress
	  OR tbl_league_invite.UserID = P_UserID;
	
	-- My Conferences
	
	SELECT c.ConferenceID,
	       c.ConferenceName,
	       c.CustomRulesConference,
	       u.UserID AS ConferenceManagerID,
	       u.DisplayName AS ConferenceManagerDisplayName
	FROM tbl_conference c
	JOIN tbl_conference_hasuser chu ON c.ConferenceID = chu.ConferenceID
	JOIN tbl_user u ON c.ConferenceManager = u.UserID
	WHERE chu.UserID = P_UserID
	  AND c.IsDeleted = 0;
	
	
	SELECT cy.ConferenceID,
	       cy.Year
	FROM tbl_conference_year cy
	JOIN tbl_conference_hasuser chu ON cy.ConferenceID = chu.ConferenceID
	WHERE chu.UserID = P_UserID;
	
	-- Top Bids and Drops
	
	SELECT MAX(ProcessDate) INTO maxProcessDate
	FROM tbl_caching_topbidsanddrops;
	
	
	SET maxProcessYear = YEAR(maxProcessDate);
	
	
	SELECT *
	FROM tbl_caching_topbidsanddrops
	WHERE ProcessDate = maxProcessDate;
	
	
	CREATE TEMPORARY TABLE RelevantYears AS
	  SELECT Year
	  FROM   tbl_meta_supportedyear
	  WHERE  OpenForPlay = 1
	         AND Finished = 0
	  UNION
	  SELECT DISTINCT Year
	  FROM   tbl_caching_topbidsanddrops
	  WHERE  ProcessDate = maxProcessDate; 
	
	
	SELECT tbl_caching_mastergameyear.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM   tbl_caching_mastergameyear
	       JOIN tbl_user
	         ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
	WHERE  tbl_caching_mastergameyear.`Year` IN (SELECT YEAR FROM RelevantYears);
	
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT DISTINCT tbl_mastergame_subgame.*
	FROM tbl_mastergame_subgame
	JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_subgame.MasterGameID
	WHERE YEAR IN (SELECT YEAR FROM RelevantYears);
	
	
	SELECT distinct tbl_mastergame_hastag.MasterGameID, tbl_mastergame_hastag.TagName
	FROM tbl_mastergame_hastag
	JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_hastag.MasterGameID
	WHERE YEAR IN (SELECT YEAR FROM RelevantYears);
	
	-- My Game News
	
	SELECT tbl_league_publishergame.MasterGameId,
	       tbl_league_publishergame.CounterPick,
	       tbl_league.LeagueID,
	       tbl_league.LeagueName,
	       tbl_league_publisher.Year,
	       tbl_league_publisher.PublisherID,
	       tbl_league_publisher.PublisherName
	FROM tbl_league_publishergame
	JOIN tbl_league_publisher ON tbl_league_publisher.PublisherID = tbl_league_publishergame.PublisherID
	JOIN tbl_league ON tbl_league.LeagueID = tbl_league_publisher.LeagueID
	WHERE tbl_league_publishergame.MasterGameID IS NOT NULL
	  AND tbl_league.TestLeague = 0
	  AND tbl_league_publisher.UserID = P_UserID
	  AND tbl_league_publisher.Year IN (SELECT YEAR FROM RelevantYears);
	
	-- Public League Years
	
	SELECT YEAR INTO highestNormalYear
	FROM tbl_meta_supportedyear
	WHERE tbl_meta_supportedyear.Finished = 0
	ORDER BY YEAR DESC
	LIMIT 1;

	SELECT vw_league.LeagueID,
	       vw_league.LeagueName,
	       vw_league.NumberOfFollowers,
	       tbl_league_year.PlayStatus
	FROM vw_league
	JOIN tbl_league_year ON vw_league.LeagueID = tbl_league_year.LeagueID
	WHERE vw_league.PublicLeague = 1
	  AND tbl_league_year.`Year` = highestNormalYear
	ORDER BY NumberOfFollowers DESC
	LIMIT 10;
	
	-- Active Royale Quarter
	
	SELECT YEAR,
	       QUARTER INTO highestRoyaleYear,
	                    highestRoyaleQuarter
	FROM tbl_royale_supportedquarter
	WHERE tbl_royale_supportedquarter.OpenForPlay = 1
	ORDER BY YEAR DESC, QUARTER DESC
	LIMIT 1;
	
	
	SELECT highestRoyaleYear AS YEAR,
	       highestRoyaleQuarter AS QUARTER;
	
	
	-- Active User Royale Publisher ID
	
	SELECT PublisherID
	FROM tbl_royale_publisher
	WHERE UserID = P_UserID
	  AND YEAR = highestRoyaleYear
	  AND QUARTER = highestRoyaleQuarter;

END