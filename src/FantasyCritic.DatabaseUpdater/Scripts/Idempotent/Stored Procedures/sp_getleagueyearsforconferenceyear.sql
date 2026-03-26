-- --------------------------------------------------------
-- Host:                         fantasy-critic-rds.cldutembgs4w.us-east-1.rds.amazonaws.com
-- Server version:               8.4.7 - Source distribution
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
