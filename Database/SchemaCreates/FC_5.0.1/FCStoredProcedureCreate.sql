-- --------------------------------------------------------
-- Host:                         fantasy-critic-rds.cldutembgs4w.us-east-1.rds.amazonaws.com
-- Server version:               8.0.31 - Source distribution
-- Server OS:                    Linux
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

-- Dumping structure for procedure fantasycritic.sp_getcombinedleagueyearuserstatus
DELIMITER //
CREATE PROCEDURE `sp_getcombinedleagueyearuserstatus`(
	IN `P_LeagueID` CHAR(36),
	IN `P_Year` INT
)
BEGIN
	SELECT tbl_user.*
	FROM tbl_user
	JOIN tbl_league_hasuser ON (tbl_user.UserID = tbl_league_hasuser.UserID)
	WHERE tbl_league_hasuser.LeagueID = P_LeagueID;
	
	
	SELECT YEAR,
	       PlayStatus
	FROM tbl_league_year
	WHERE LeagueID = P_LeagueID;
	
	
	SELECT UserID,
	       YEAR
	FROM tbl_league_publisher
	WHERE LeagueID = P_LeagueID;
	
	
	SELECT tbl_user.*
	FROM tbl_user
	JOIN tbl_league_activeplayer ON (tbl_user.UserID = tbl_league_activeplayer.UserID)
	WHERE tbl_league_activeplayer.LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT tbl_league_invite.*,
	       tbl_user.DisplayName AS UserName,
	       tbl_user.EmailAddress AS UserEmailAddress
	FROM tbl_league_invite
	LEFT JOIN tbl_user ON tbl_league_invite.UserID = tbl_user.UserID
	WHERE tbl_league_invite.LeagueID = P_LeagueID;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getconferenceyeardata
DELIMITER //
CREATE PROCEDURE `sp_getconferenceyeardata`(
	IN `P_ConferenceID` CHAR(36),
	IN `P_Year` INT
)
BEGIN
	SELECT tbl_conference.*,
	       tbl_user.DisplayName AS ConferenceManagerDisplayName,
	       tbl_user.EmailAddress AS ConferenceManagerEmailAddress
	FROM tbl_conference
	JOIN tbl_user ON tbl_conference.ConferenceManager = tbl_user.UserID
	WHERE ConferenceID = P_ConferenceID
	  AND tbl_conference.IsDeleted = 0;
	
	
	SELECT YEAR
	FROM tbl_conference_year
	WHERE ConferenceID = P_ConferenceID;
	
	
	SELECT LeagueID,
	       LeagueManager
	FROM tbl_league
	WHERE ConferenceID = P_ConferenceID;
	
	
	SELECT *
	FROM tbl_conference_year
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_meta_supportedyear
	WHERE YEAR = P_Year;
	
	SELECT NULL AS LeagueID,
			tbl_conference_hasuser.UserID,
			tbl_user.DisplayName,
			tbl_user.EmailAddress
	FROM tbl_conference_hasuser
	JOIN tbl_user ON tbl_conference_hasuser.UserID = tbl_user.UserID
	WHERE tbl_conference_hasuser.ConferenceID = P_ConferenceID
	UNION
	SELECT tbl_league_hasuser.LeagueID,
	       tbl_league_hasuser.UserID,
	       tbl_user.DisplayName,
	       tbl_user.EmailAddress
	FROM tbl_league_hasuser
	JOIN tbl_league ON tbl_league_hasuser.LeagueID = tbl_league.LeagueID
	JOIN tbl_user ON tbl_league_hasuser.UserID = tbl_user.UserID
	WHERE tbl_league.ConferenceID = P_ConferenceID;
	
	
	SELECT tbl_league_activeplayer.LeagueID,
	       tbl_league_activeplayer.Year,
	       tbl_league_activeplayer.UserID
	FROM tbl_league_activeplayer
	JOIN tbl_league ON tbl_league_activeplayer.LeagueID = tbl_league.LeagueID
	WHERE tbl_league.ConferenceID = P_ConferenceID;
	
	SELECT 
		tbl_conference_activeplayer.ConferenceID, 
		tbl_conference_activeplayer.Year, 
		tbl_conference_activeplayer.UserID 
	FROM tbl_conference_activeplayer
	WHERE tbl_conference_activeplayer.ConferenceID = P_ConferenceID;
	
	SELECT *
	FROM tbl_conference_managermessage
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year
	  AND Deleted = 0;
	
	
	SELECT tbl_conference_managermessagedismissal.*
	FROM tbl_conference_managermessage
	JOIN tbl_conference_managermessagedismissal ON tbl_conference_managermessage.MessageID = tbl_conference_managermessagedismissal.MessageID
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	  
	select * from tbl_caching_averagepositionpoints;
	SELECT *	from tbl_caching_averagebidamountpoints;
	select * from tbl_caching_systemwidevalues;
	  
	CALL sp_getleagueyearsforconferenceyear(P_ConferenceID, P_Year);
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
	
	SELECT vw_league.LeagueID,
	       vw_league.LeagueName,
	       vw_league.NumberOfFollowers,
	       tbl_league_year.PlayStatus
	FROM vw_league
	JOIN tbl_league_year ON vw_league.LeagueID = tbl_league_year.LeagueID
	WHERE vw_league.PublicLeague = 1
	  AND tbl_league_year.`Year` = maxProcessYear
	ORDER BY NumberOfFollowers DESC
	LIMIT 10;
	
	-- Active Royale Quarter
	
	SELECT YEAR,
	       QUARTER INTO highestYear,
	                    highestQuarter
	FROM tbl_royale_supportedquarter
	WHERE tbl_royale_supportedquarter.OpenForPlay = 1
	ORDER BY YEAR DESC, QUARTER DESC
	LIMIT 1;
	
	
	SELECT highestYear AS YEAR,
	       highestQuarter AS QUARTER;
	
	
	-- Active User Royale Publisher ID
	
	SELECT PublisherID
	FROM tbl_royale_publisher
	WHERE UserID = P_UserID
	  AND YEAR = highestYear
	  AND QUARTER = highestQuarter;

END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleague
DELIMITER //
CREATE PROCEDURE `sp_getleague`(
	IN `P_LeagueID` CHAR(36)
)
BEGIN
	SELECT vw_league.*,
	       tbl_user.DisplayName AS ManagerDisplayName,
	       tbl_user.EmailAddress AS ManagerEmailAddress,
	       CASE
	           WHEN tbl_caching_leagueyear.OneShotMode = 1 THEN 'true'
	           ELSE 'false'
	       END AS MostRecentOneShotMode
	FROM vw_league
	JOIN tbl_user ON tbl_user.UserID = vw_league.LeagueManager
	LEFT JOIN
	  (SELECT tbl_caching_leagueyear.LeagueID,
	          tbl_caching_leagueyear.OneShotMode,
	          ROW_NUMBER() OVER (PARTITION BY tbl_caching_leagueyear.LeagueID
	                             ORDER BY tbl_caching_leagueyear.Year DESC) AS rn
	   FROM tbl_caching_leagueyear) AS tbl_caching_leagueyear ON vw_league.LeagueID = tbl_caching_leagueyear.LeagueID
	AND tbl_caching_leagueyear.rn = 1
	WHERE vw_league.LeagueID = P_LeagueID
	  AND vw_league.IsDeleted = 0;
	
	SELECT YEAR
	FROM tbl_league_year
	WHERE LeagueID = P_LeagueID;
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
	           WHEN tbl_caching_leagueyear.OneShotMode = 1 THEN 1
	           ELSE 0
	       END AS MostRecentYearOneShot,
	       CASE
	           WHEN tbl_league_hasuser.UserID IS NOT NULL THEN 1
	           ELSE 0
	       END AS UserIsInLeague,
	       CASE
	           WHEN tbl_user_followingleague.UserID IS NOT NULL THEN 1
	           ELSE 0
	       END AS UserIsFollowingLeague,
	       CASE
	           WHEN EXISTS
	                  (SELECT 1
	                   FROM tbl_league_activeplayer ap
	                   JOIN tbl_league_year ly ON ap.LeagueID = ly.LeagueID
	                   WHERE ap.UserID = P_UserID
	                     AND ap.LeagueID = vw_league.LeagueID
	                     AND ap.Year =
	                       (SELECT MAX(ly2.Year)
	                        FROM tbl_league_year ly2
	                        WHERE ly2.LeagueID = vw_league.LeagueID)) THEN 1
	           ELSE 0
	       END AS UserIsActiveInMostRecentYearForLeague,
	       CASE
	           WHEN EXISTS
	                  (SELECT 1
	                   FROM tbl_league_year ly
	                   JOIN tbl_meta_supportedyear sy ON ly.Year = sy.Year
	                   WHERE ly.LeagueID = vw_league.LeagueID
	                     AND sy.OpenForPlay = 1) THEN 1
	           ELSE 0
	       END AS LeagueIsActiveInActiveYear
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
	WHERE tbl_league_hasuser.UserID = P_UserID
	UNION
	SELECT tbl_league_year.LeagueID,
	       tbl_league_year.Year
	FROM tbl_league_year
	JOIN tbl_user_followingleague ON tbl_league_year.LeagueID = tbl_user_followingleague.LeagueID
	WHERE tbl_user_followingleague.UserID = P_UserID;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleagueyear
DELIMITER //
CREATE PROCEDURE `sp_getleagueyear`(
	IN `P_LeagueID` CHAR(36),
	IN `P_Year` SMALLINT
)
    DETERMINISTIC
BEGIN
	-- Supported Year
	
	SELECT *
	FROM tbl_meta_supportedyear
	WHERE YEAR = P_Year;
	
	-- League
	
	SELECT *
	FROM vw_league
	WHERE LeagueID = P_LeagueID
	  AND IsDeleted = 0;
	
	
	SELECT YEAR
	FROM tbl_league_year
	WHERE LeagueID = P_LeagueID;
	
	-- League Year
	
	SELECT *
	FROM tbl_league_year
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_yearusestag
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_specialgameslot
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_eligibilityoverride
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT tbl_league_tagoverride.*
	FROM tbl_league_tagoverride
	JOIN tbl_mastergame_tag ON tbl_league_tagoverride.TagName = tbl_mastergame_tag.Name
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	-- Publishers
	
	SELECT tbl_user.*
	FROM tbl_user
	JOIN tbl_league_hasuser ON (tbl_user.UserID = tbl_league_hasuser.UserID)
	WHERE tbl_league_hasuser.LeagueID = P_LeagueID
	UNION
	SELECT tbl_user.*
	FROM tbl_user
	JOIN tbl_league ON (tbl_user.UserID = tbl_league.LeagueManager)
	WHERE tbl_league.LeagueID = P_LeagueID;
	
	
	SELECT *
	FROM tbl_league_publisher
	WHERE tbl_league_publisher.LeagueID = P_LeagueID
	  AND tbl_league_publisher.Year = P_Year;
	
	
	SELECT tbl_league_publishergame.*
	FROM tbl_league_publishergame
	JOIN tbl_league_publisher ON (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID)
	WHERE tbl_league_publisher.LeagueID = P_LeagueID
	  AND tbl_league_publisher.Year = P_Year;
	
	
	SELECT tbl_league_formerpublishergame.*
	FROM tbl_league_formerpublishergame
	JOIN tbl_league_publisher ON (tbl_league_formerpublishergame.PublisherID = tbl_league_publisher.PublisherID)
	WHERE tbl_league_publisher.LeagueID = P_LeagueID
	  AND tbl_league_publisher.Year = P_Year;
	
	-- Master Game Data
	
	SELECT tbl_mastergame.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_mastergame
	JOIN tbl_user ON tbl_user.UserID = tbl_mastergame.AddedByUserID;
	
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT *
	FROM tbl_mastergame_subgame;
	
	
	SELECT *
	FROM tbl_mastergame_hastag;
	
	
	SELECT tbl_caching_mastergameyear.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_caching_mastergameyear
	JOIN tbl_user ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
	WHERE tbl_caching_mastergameyear.`Year` = P_Year;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleagueyearsforconferenceyear
DELIMITER //
CREATE PROCEDURE `sp_getleagueyearsforconferenceyear`(
	IN `P_ConferenceID` CHAR(36),
	IN `P_Year` INT
)
BEGIN
	-- League
	
	SELECT *
	FROM vw_league
	WHERE ConferenceID = P_ConferenceID
	  AND IsDeleted = 0;
	
	
	SELECT tbl_league.LeagueID, YEAR
	FROM tbl_league_year
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_year.LeagueID
	WHERE ConferenceID = P_ConferenceID;
	
	-- League Year
	
	SELECT *
	FROM tbl_league_year
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_year.LeagueID
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_yearusestag
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_yearusestag.LeagueID
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_specialgameslot
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_specialgameslot.LeagueID
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_eligibilityoverride
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_eligibilityoverride.LeagueID
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	
	
	SELECT tbl_league_tagoverride.*
	FROM tbl_league_tagoverride
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_tagoverride.LeagueID
	JOIN tbl_mastergame_tag ON tbl_league_tagoverride.TagName = tbl_mastergame_tag.Name
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	
	-- Publishers
	
	SELECT tbl_user.*
	FROM tbl_user
	JOIN tbl_league_hasuser ON (tbl_user.UserID = tbl_league_hasuser.UserID)
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_hasuser.LeagueID
	WHERE tbl_league.ConferenceID = P_ConferenceID
	UNION
	SELECT tbl_user.*
	FROM tbl_user
	JOIN tbl_league ON (tbl_user.UserID = tbl_league.LeagueManager)
	WHERE tbl_league.ConferenceID = P_ConferenceID;
	
	
	SELECT *
	FROM tbl_league_publisher
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_publisher.LeagueID
	WHERE tbl_league.ConferenceID = P_ConferenceID
	  AND tbl_league_publisher.Year = P_Year;
	
	
	SELECT tbl_league_publishergame.*
	FROM tbl_league_publishergame
	JOIN tbl_league_publisher ON (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID)
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_publisher.LeagueID
	WHERE tbl_league.ConferenceID = P_ConferenceID
	  AND tbl_league_publisher.Year = P_Year;
	
	
	SELECT tbl_league_formerpublishergame.*
	FROM tbl_league_formerpublishergame
	JOIN tbl_league_publisher ON (tbl_league_formerpublishergame.PublisherID = tbl_league_publisher.PublisherID)
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_publisher.LeagueID
	WHERE tbl_league.ConferenceID = P_ConferenceID
	  AND tbl_league_publisher.Year = P_Year;
	
	-- Master Game Data
	
	SELECT tbl_mastergame.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_mastergame
	JOIN tbl_user ON tbl_user.UserID = tbl_mastergame.AddedByUserID;
	
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT *
	FROM tbl_mastergame_subgame;
	
	
	SELECT *
	FROM tbl_mastergame_hastag;
	
	
	SELECT tbl_caching_mastergameyear.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_caching_mastergameyear
	JOIN tbl_user ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
	WHERE tbl_caching_mastergameyear.`Year` = P_Year;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleagueyearsupplementaldata
DELIMITER //
CREATE PROCEDURE `sp_getleagueyearsupplementaldata`(
	IN `P_LeagueID` CHAR(36),
	IN `P_Year` INT,
	IN `P_UserID` CHAR(36)
)
BEGIN
	DECLARE v_PublisherID CHAR(36);
	
	
	SELECT *
	FROM tbl_caching_averagepositionpoints;
	
	SELECT *
	FROM tbl_caching_averagebidamountpoints;
	
	SELECT *
	FROM tbl_caching_systemwidevalues;
	
	
	SELECT *
	FROM tbl_league_managermessage
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year
	  AND Deleted = 0;
	
	
	SELECT tbl_league_managermessagedismissal.*
	FROM tbl_league_managermessage
	JOIN tbl_league_managermessagedismissal ON tbl_league_managermessage.MessageID = tbl_league_managermessagedismissal.MessageID
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT WinningUserID
	FROM tbl_league_year
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year - 1;
	
	
	SELECT *
	FROM tbl_league_trade
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT tbl_league_tradecomponent.*
	FROM tbl_league_tradecomponent
	JOIN tbl_league_trade ON tbl_league_tradecomponent.TradeID = tbl_league_trade.TradeID
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT tbl_league_tradevote.*
	FROM tbl_league_tradevote
	JOIN tbl_league_trade ON tbl_league_tradevote.TradeID = tbl_league_trade.TradeID
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_specialauction
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM vw_league_pickupbid
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year
	  AND SUCCESSFUL IS NULL;
	
	
	SELECT COUNT(*) AS UserIsFollowingLeague
	FROM tbl_user
	JOIN tbl_user_followingleague ON (tbl_user.UserID = tbl_user_followingleague.UserID)
	WHERE tbl_user_followingleague.LeagueID = P_LeagueID
	  AND tbl_user.UserID = P_UserID;
	
	-- Create a temporary table
	
	DROP
	TEMPORARY TABLE IF EXISTS TempTable;
	
	
	CREATE
	TEMPORARY TABLE TempTable AS
	SELECT PublisherID,
	       PublisherName,
	       l.LeagueID,
	       LeagueName,
	       `Year`
	FROM tbl_league_publisher p
	JOIN tbl_league l ON p.LeagueID = l.LeagueID
	WHERE UserID = P_UserID
	  	AND `Year` = P_Year;
	
	-- Retrieve the PublisherID from the temporary table
	
	SELECT PublisherID INTO v_PublisherID
	FROM TempTable
	WHERE LeagueID = P_LeagueID
	LIMIT 1;
	
	
	SELECT *
	FROM TempTable;
	
	-- Second query: Use the stored PublisherID
	
	SELECT *
	FROM tbl_league_droprequest
	WHERE PublisherID = v_PublisherID
	  AND SUCCESSFUL IS NULL;
	
	-- Third query: Use the stored PublisherID
	
	SELECT *
	FROM tbl_league_publisherqueue
	WHERE PublisherID = v_PublisherID;
	
	
	DROP
	TEMPORARY TABLE IF EXISTS TempTable;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleagueyearwithsupplementaldata
DELIMITER //
CREATE PROCEDURE `sp_getleagueyearwithsupplementaldata`(
	IN `P_LeagueID` CHAR(36),
	IN `P_Year` INT,
	IN `P_UserID` CHAR(36)
)
BEGIN
	CALL sp_getleagueyear(P_LeagueID, P_Year);
	CALL sp_getleagueyearsupplementaldata(P_LeagueID, P_Year, P_UserID);
	CALL sp_getcombinedleagueyearuserstatus(P_LeagueID, P_Year);
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleagueyearwithuserstatus
DELIMITER //
CREATE PROCEDURE `sp_getleagueyearwithuserstatus`(
	IN `P_LeagueID` CHAR(36),
	IN `P_Year` CHAR(36)
)
BEGIN
	CALL sp_getleagueyear(P_LeagueID, P_Year);
	CALL sp_getcombinedleagueyearuserstatus(P_LeagueID, P_Year);
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getmastergames
DELIMITER //
CREATE PROCEDURE `sp_getmastergames`()
BEGIN
	SELECT tbl_mastergame.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_mastergame
	JOIN tbl_user ON tbl_user.UserID = tbl_mastergame.AddedByUserID;
	
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT *
	FROM tbl_mastergame_subgame;
	
	
	SELECT *
	FROM tbl_mastergame_hastag;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getmastergameyears
DELIMITER //
CREATE PROCEDURE `sp_getmastergameyears`(
	IN `P_Year` INT
)
BEGIN
	SELECT tbl_caching_mastergameyear.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_caching_mastergameyear
	JOIN tbl_user ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
	WHERE tbl_caching_mastergameyear.`Year` = P_Year;
	
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT *
	FROM tbl_mastergame_subgame
	JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_subgame.MasterGameID
	WHERE YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_mastergame_hastag
	JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_hastag.MasterGameID
	WHERE YEAR = P_Year;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getroyalepublisher
DELIMITER //
CREATE PROCEDURE `sp_getroyalepublisher`(
	IN `P_PublisherID` CHAR(36)
)
BEGIN
	SELECT tbl_royale_supportedquarter.*,
	     tbl_user.DisplayName AS WinningUserDisplayName
	FROM tbl_royale_supportedquarter
	LEFT JOIN tbl_user ON tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
	ORDER BY YEAR,
	         QUARTER;

	SELECT tbl_royale_publisher.*,
	       tbl_user.DisplayName AS PublisherDisplayName
	FROM tbl_royale_publisher
	JOIN tbl_user ON tbl_user.UserID = tbl_royale_publisher.UserID
	WHERE PublisherID = P_PublisherID;
	
	
	SELECT *
	FROM tbl_royale_publishergame
	WHERE tbl_royale_publishergame.PublisherID = P_PublisherID; 
	
	-- Master Game Data
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT *
	FROM tbl_mastergame_subgame;
	
	
	SELECT *
	FROM tbl_mastergame_hastag;
	
	
	SELECT tbl_caching_mastergameyear.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_caching_mastergameyear
	JOIN tbl_user ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
	join tbl_royale_publisher on tbl_royale_publisher.Year = tbl_caching_mastergameyear.Year
	WHERE tbl_royale_publisher.PublisherID = P_PublisherID;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getroyaleyearquarterdata
DELIMITER //
CREATE PROCEDURE `sp_getroyaleyearquarterdata`(
	IN `P_Year` INT,
	IN `P_Quarter` INT
)
BEGIN
	SELECT tbl_royale_supportedquarter.*,
	       tbl_user.DisplayName AS WinningUserDisplayName
	FROM tbl_royale_supportedquarter
	LEFT JOIN tbl_user ON tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
	ORDER BY YEAR,
	         QUARTER;
	
	SELECT tbl_royale_publisher.*,
	       tbl_user.DisplayName AS PublisherDisplayName
	FROM tbl_royale_publisher
	JOIN tbl_user ON tbl_user.UserID = tbl_royale_publisher.UserID
	WHERE YEAR = P_Year
	  AND QUARTER = P_Quarter;
	  
	SELECT *
	FROM tbl_royale_publishergame
	JOIN tbl_royale_publisher ON tbl_royale_publishergame.PublisherID = tbl_royale_publisher.PublisherID
	WHERE tbl_royale_publisher.Year = P_Year
	  AND tbl_royale_publisher.Quarter = P_Quarter;
	
	
	-- Master Game Data
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT *
	FROM tbl_mastergame_subgame;
	
	
	SELECT *
	FROM tbl_mastergame_hastag;
	
	
	SELECT tbl_caching_mastergameyear.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_caching_mastergameyear
	JOIN tbl_user ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
	WHERE tbl_caching_mastergameyear.`Year` = P_Year;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getusersinleague
DELIMITER //
CREATE PROCEDURE `sp_getusersinleague`(
	IN `P_LeagueID` CHAR(36)
)
BEGIN
	SELECT tbl_user.* from tbl_user join tbl_league_hasuser on (tbl_user.UserID = tbl_league_hasuser.UserID) where tbl_league_hasuser.LeagueID = P_LeagueID;
	
	SELECT YEAR, PlayStatus FROM tbl_league_year where LeagueID = P_LeagueID;
	
	SELECT UserID, Year from tbl_league_publisher where LeagueID = P_LeagueID;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
