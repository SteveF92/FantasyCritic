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

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
