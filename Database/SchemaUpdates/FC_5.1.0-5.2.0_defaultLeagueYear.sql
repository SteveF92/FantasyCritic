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

-- Dumping structure for procedure fantasycritic.sp_getconferenceyeardata
DROP PROCEDURE IF EXISTS `sp_getconferenceyeardata`;
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
	
	
	SELECT 
    cy.Year,
    sy.Finished AS SupportedYearIsFinished,
    CASE 
        WHEN SUM(CASE WHEN ly.PlayStatus <> 'NotStartedDraft' THEN 1 ELSE 0 END) > 0
            THEN 1
            ELSE 0
    END AS AtLeastOneDraftStarted
	FROM tbl_conference_year cy
	JOIN tbl_meta_supportedyear sy 
	    ON sy.Year = cy.Year
	JOIN tbl_league l
	    ON l.ConferenceID = cy.ConferenceID
	LEFT JOIN tbl_league_year ly
	    ON ly.LeagueID = l.LeagueID
	    AND ly.Year = cy.Year
	WHERE cy.ConferenceID = P_ConferenceID
	GROUP BY cy.Year, sy.Finished;
	
	
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

-- Dumping structure for procedure fantasycritic.sp_getleague
DROP PROCEDURE IF EXISTS `sp_getleague`;
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
	
	SELECT 
	tbl_league_year.`Year`,
	tbl_meta_supportedyear.Finished AS "SupportedYearIsFinished",
	tbl_league_year.PlayStatus
	FROM tbl_league_year
	JOIN tbl_meta_supportedyear ON tbl_meta_supportedyear.`Year` = tbl_league_year.`Year`
	WHERE LeagueID = P_LeagueID;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleaguesforuser
DROP PROCEDURE IF EXISTS `sp_getleaguesforuser`;
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
	       tbl_league_year.Year,
	       tbl_meta_supportedyear.Finished AS 'SupportedYearIsFinished',
	       tbl_league_year.PlayStatus
	FROM tbl_league_year
	JOIN tbl_league_hasuser ON tbl_league_year.LeagueID = tbl_league_hasuser.LeagueID
	JOIN tbl_meta_supportedyear ON tbl_league_year.Year = tbl_meta_supportedyear.`Year`
	WHERE tbl_league_hasuser.UserID = P_UserID
	UNION
	SELECT tbl_league_year.LeagueID,
	       tbl_league_year.Year,
	       tbl_meta_supportedyear.Finished AS 'SupportedYearIsFinished',
	       tbl_league_year.PlayStatus
	FROM tbl_league_year
	JOIN tbl_user_followingleague ON tbl_league_year.LeagueID = tbl_user_followingleague.LeagueID
	JOIN tbl_meta_supportedyear ON tbl_league_year.Year = tbl_meta_supportedyear.`Year`
	WHERE tbl_user_followingleague.UserID = P_UserID;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleagueyear
DROP PROCEDURE IF EXISTS `sp_getleagueyear`;
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
	
	SELECT 
	tbl_league_year.`Year`,
	tbl_meta_supportedyear.Finished AS "SupportedYearIsFinished",
	tbl_league_year.PlayStatus
	FROM tbl_league_year
	JOIN tbl_meta_supportedyear ON tbl_meta_supportedyear.`Year` = tbl_league_year.`Year`
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
DROP PROCEDURE IF EXISTS `sp_getleagueyearsforconferenceyear`;
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
	
	
	SELECT tbl_league.LeagueID, 
	tbl_league_year.`YEAR`,
	tbl_meta_supportedyear.Finished AS "SupportedYearIsFinished",
	tbl_league_year.PlayStatus
	FROM tbl_league_year
	JOIN tbl_league on tbl_league.LeagueID = tbl_league_year.LeagueID
	JOIN tbl_meta_supportedyear ON tbl_meta_supportedyear.`Year` = tbl_league_year.`Year`
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

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
