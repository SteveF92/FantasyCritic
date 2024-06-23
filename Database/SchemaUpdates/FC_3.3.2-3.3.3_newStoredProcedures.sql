-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.0.28 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Version:             12.3.0.6589
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Dumping structure for procedure fantasycritic.sp_getbasicdata
DELIMITER //
CREATE PROCEDURE `sp_getbasicdata`()
BEGIN
	select * from tbl_meta_systemwidesettings;
	select * from tbl_mastergame_tag;
	select * from tbl_meta_supportedyear;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_gethomepagedata
DELIMITER //
CREATE PROCEDURE `sp_gethomepagedata`(
	IN `P_UserID` CHAR(36)
)
BEGIN
	DECLARE userEmailAddress VARCHAR(255);
	DECLARE maxProcessDate DATE;
	DECLARE maxProcessYear INT;
	DECLARE highestYear INT;
	DECLARE highestQuarter INT;
	

	SELECT EmailAddress INTO userEmailAddress FROM tbl_user WHERE UserID = P_UserID;
	
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
	LEFT JOIN (
	    SELECT LeagueID, MAX(Year) AS ActiveYear
	    FROM tbl_league_year
	    GROUP BY LeagueID
	) leagueYear ON tbl_league.LeagueID = leagueYear.LeagueID
	where tbl_league_invite.EmailAddress = userEmailAddress OR tbl_league_invite.UserID = P_UserID;
	
	-- My Conferences
	SELECT 
	c.ConferenceID, 
	c.ConferenceName, 
	c.CustomRulesConference, 
	u.UserID as ConferenceManagerID, 
	u.DisplayName as ConferenceManagerDisplayName
	FROM 
	    tbl_conference c
	JOIN 
	    tbl_conference_hasuser chu ON c.ConferenceID = chu.ConferenceID
	JOIN 
	    tbl_user u ON c.ConferenceManager = u.UserID
	WHERE 
	    chu.UserID = P_UserID AND c.IsDeleted = 0;
	        
	SELECT 
	cy.ConferenceID, 
	cy.Year
	FROM 
	tbl_conference_year cy
	JOIN 
	tbl_conference_hasuser chu ON cy.ConferenceID = chu.ConferenceID
	WHERE 
	chu.UserID = P_UserID;
	
	-- Top Bids and Drops
	SELECT MAX(ProcessDate) INTO maxProcessDate FROM tbl_caching_topbidsanddrops;
	SET maxProcessYear = YEAR(maxProcessDate);
	
	SELECT *
	FROM tbl_caching_topbidsanddrops
	WHERE ProcessDate = maxProcessDate;
	
	select tbl_caching_mastergameyear.*, tbl_user.DisplayName as AddedByUserDisplayName from tbl_caching_mastergameyear join tbl_user on tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID WHERE tbl_caching_mastergameyear.`Year` IN (SELECT YEAR FROM tbl_meta_supportedyear WHERE OpenForPlay = 1 AND Finished = 0);
	select * from tbl_mastergame_tag;
	select * from tbl_mastergame_subgame JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_subgame.MasterGameID WHERE YEAR IN (SELECT YEAR FROM tbl_meta_supportedyear WHERE OpenForPlay = 1 AND Finished = 0);
	select * from tbl_mastergame_hastag JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_hastag.MasterGameID WHERE YEAR IN (SELECT YEAR FROM tbl_meta_supportedyear WHERE OpenForPlay = 1 AND Finished = 0);

	-- My Game News
	SELECT tbl_league_publishergame.MasterGameId, tbl_league_publishergame.CounterPick, tbl_league.LeagueID, tbl_league.LeagueName, tbl_league_publisher.Year, tbl_league_publisher.PublisherID, tbl_league_publisher.PublisherName
	FROM tbl_league_publishergame
	JOIN tbl_league_publisher ON tbl_league_publisher.PublisherID = tbl_league_publishergame.PublisherID
	JOIN tbl_league ON tbl_league.LeagueID = tbl_league_publisher.LeagueID
	WHERE tbl_league_publisher.UserID = P_UserID AND tbl_league_publisher.Year IN (SELECT YEAR FROM tbl_meta_supportedyear WHERE OpenForPlay = 1 AND Finished = 0);

	-- Public League Years
	SELECT vw_league.LeagueID, vw_league.LeagueName, vw_league.NumberOfFollowers, tbl_league_year.PlayStatus
	FROM vw_league
	JOIN tbl_league_year ON vw_league.LeagueID = tbl_league_year.LeagueID
	WHERE vw_league.PublicLeague = 1
	AND tbl_league_year.`Year` = maxProcessYear
	ORDER BY NumberOfFollowers DESC LIMIT 10;
	
	-- Active Royale Quarter
	SELECT Year, Quarter
	INTO highestYear, highestQuarter
	FROM tbl_royale_supportedquarter
	WHERE tbl_royale_supportedquarter.OpenForPlay = 1
	ORDER BY Year DESC, Quarter DESC
	LIMIT 1;
	
	SELECT highestYear AS Year, highestQuarter AS Quarter;
	
	SELECT * FROM tbl_meta_supportedyear WHERE YEAR = highestYear;
	
	-- Active User Royale Publisher ID
	SELECT PublisherID
	FROM tbl_royale_publisher
	WHERE UserID = P_UserID
	AND Year = highestYear
	AND Quarter = highestQuarter;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleague
DELIMITER //
CREATE PROCEDURE `sp_getleague`(
	IN `LeagueID` CHAR(36)
)
BEGIN
	 SELECT
	    vw_league.*,
	    tbl_user.DisplayName AS ManagerDisplayName,
	    tbl_user.EmailAddress AS ManagerEmailAddress,
	    CASE
	        WHEN tbl_caching_leagueyear.OneShotMode = 1 THEN 'true'
	        ELSE 'false'
	    END AS MostRecentOneShotMode
	FROM
	    vw_league
	JOIN
	    tbl_user ON tbl_user.UserID = vw_league.LeagueManager
	LEFT JOIN
	    (SELECT
	         tbl_caching_leagueyear.LeagueID,
	         tbl_caching_leagueyear.OneShotMode,
	         ROW_NUMBER() OVER (PARTITION BY tbl_caching_leagueyear.LeagueID ORDER BY tbl_caching_leagueyear.Year DESC) AS rn
	     FROM
	         tbl_caching_leagueyear) AS tbl_caching_leagueyear
	ON
	    vw_league.LeagueID = tbl_caching_leagueyear.LeagueID
	AND tbl_caching_leagueyear.rn = 1
	WHERE
	    vw_league.LeagueID = @LeagueID
	AND
	    vw_league.IsDeleted = 0;
	
	 select Year from tbl_league_year where LeagueID = @LeagueID;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleaguesforuser
DELIMITER //
CREATE PROCEDURE `sp_getleaguesforuser`(
	IN `P_UserID` CHAR(36)
)
BEGIN 

-- Main query with added fields UserIsInLeague and UserIsFollowingLeague
SELECT vw_league.*,
       tbl_league_hasuser.Archived,
       tbl_user.DisplayName AS ManagerDisplayName,
       tbl_user.EmailAddress AS ManagerEmailAddress,
       CASE
           WHEN tbl_caching_leagueyear.OneShotMode = 1 THEN 'true'
           ELSE 'false'
       END AS MostRecentOneShotMode,
       CASE
           WHEN tbl_league_hasuser.UserID IS NOT NULL THEN 1
           ELSE 0
       END AS UserIsInLeague,
       CASE
           WHEN tbl_user_followingleague.UserID IS NOT NULL THEN 1
           ELSE 0
       END AS UserIsFollowingLeague
FROM vw_league
LEFT JOIN tbl_league_hasuser ON vw_league.LeagueID = tbl_league_hasuser.LeagueID
AND tbl_league_hasuser.UserID = P_UserID
LEFT JOIN tbl_user_followingleague ON vw_league.LeagueID = tbl_user_followingleague.LeagueID
AND tbl_user_followingleague.UserID = P_UserID
LEFT JOIN
  (SELECT tbl_caching_leagueyear.LeagueID,
          tbl_caching_leagueyear.OneShotMode,
          ROW_NUMBER() OVER (PARTITION BY tbl_caching_leagueyear.LeagueID
                             ORDER BY tbl_caching_leagueyear.Year DESC) AS rn
   FROM tbl_caching_leagueyear) AS tbl_caching_leagueyear ON vw_league.LeagueID = tbl_caching_leagueyear.LeagueID
AND tbl_caching_leagueyear.rn = 1
JOIN tbl_user ON tbl_user.UserID = vw_league.LeagueManager
WHERE (tbl_league_hasuser.UserID IS NOT NULL
       OR tbl_user_followingleague.UserID IS NOT NULL)
  AND vw_league.IsDeleted = 0;

-- Second result set
SELECT tbl_league_year.LeagueID,
       tbl_league_year.Year
FROM tbl_league_year
JOIN tbl_league_hasuser ON tbl_league_year.LeagueID = tbl_league_hasuser.LeagueID
WHERE tbl_league_hasuser.UserID = P_UserID;

END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getmastergames
DELIMITER //
CREATE PROCEDURE `sp_getmastergames`()
BEGIN
	select tbl_mastergame.*, tbl_user.DisplayName as AddedByUserDisplayName from tbl_mastergame join tbl_user on tbl_user.UserID = tbl_mastergame.AddedByUserID;
	select * from tbl_mastergame_tag;
	select * from tbl_mastergame_subgame;
	select * from tbl_mastergame_hastag;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getmastergameyears
DELIMITER //
CREATE PROCEDURE `sp_getmastergameyears`(
	IN `P_Year` INT
)
BEGIN
	select tbl_caching_mastergameyear.*, tbl_user.DisplayName as AddedByUserDisplayName from tbl_caching_mastergameyear join tbl_user on tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID WHERE tbl_caching_mastergameyear.`Year` = P_Year;
	select * from tbl_mastergame_tag;
	select * from tbl_mastergame_subgame JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_subgame.MasterGameID WHERE YEAR = P_Year;
	select * from tbl_mastergame_hastag JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_hastag.MasterGameID WHERE YEAR = P_Year;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
