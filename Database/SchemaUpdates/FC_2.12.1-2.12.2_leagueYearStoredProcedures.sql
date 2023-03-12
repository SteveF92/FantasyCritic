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

-- Dumping structure for procedure fantasycritic.sp_getcombinedleagueyearuserstatus
DROP PROCEDURE IF EXISTS `sp_getcombinedleagueyearuserstatus`;
DELIMITER //
CREATE PROCEDURE `sp_getcombinedleagueyearuserstatus`(
    IN `P_LeagueID` CHAR(36),
    IN `P_Year` INT
)
BEGIN
    SELECT tbl_user.* from tbl_user join tbl_league_hasuser on (tbl_user.UserID = tbl_league_hasuser.UserID) where tbl_league_hasuser.LeagueID = P_LeagueID;
    
    SELECT YEAR, PlayStatus FROM tbl_league_year where LeagueID = P_LeagueID;
    
    SELECT UserID, Year from tbl_league_publisher where LeagueID = P_LeagueID;
    
    SELECT tbl_user.* from tbl_user join tbl_league_activeplayer on (tbl_user.UserID = tbl_league_activeplayer.UserID)
        where tbl_league_activeplayer.LeagueID = P_LeagueID AND Year = P_Year;
        
    SELECT * from tbl_league_invite where tbl_league_invite.LeagueID = P_LeagueID;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getleagueyear
DROP PROCEDURE IF EXISTS `sp_getleagueyear`;
DELIMITER //
CREATE PROCEDURE `sp_getleagueyear`(
    IN `P_LeagueID` CHAR(36),
    IN `P_Year` SMALLINT
)
BEGIN
    -- League
    select * from vw_league where LeagueID = P_LeagueID and IsDeleted = 0;
    
    select Year from tbl_league_year where LeagueID = P_LeagueID;
    
    -- League Year
    select * from tbl_league_year where LeagueID = P_LeagueID and Year = P_Year;
    
    select * from tbl_league_yearusestag where LeagueID = P_LeagueID AND YEAR = P_Year;
    
    select * from tbl_league_specialgameslot where LeagueID = P_LeagueID AND Year = P_Year;
    
    select * from tbl_league_eligibilityoverride where LeagueID = P_LeagueID and Year = P_Year;
    
    select tbl_league_tagoverride.* from tbl_league_tagoverride
    JOIN tbl_mastergame_tag ON tbl_league_tagoverride.TagName = tbl_mastergame_tag.Name
    WHERE LeagueID = P_LeagueID AND Year = P_Year;
    
    -- Publishers
    select tbl_user.* from tbl_user join tbl_league_hasuser on (tbl_user.UserID = tbl_league_hasuser.UserID) where tbl_league_hasuser.LeagueID = P_LeagueID;
    
    select * from tbl_league_publisher where tbl_league_publisher.LeagueID = P_LeagueID and tbl_league_publisher.Year = P_Year;
    
    select tbl_league_publishergame.* from tbl_league_publishergame
    join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID)
    join tbl_league_year on (tbl_league_year.LeagueID = tbl_league_publisher.LeagueID AND tbl_league_year.Year = tbl_league_publisher.Year)
    where tbl_league_year.LeagueID = P_LeagueID AND tbl_league_year.Year = P_Year;
    
   select tbl_league_formerpublishergame.* from tbl_league_formerpublishergame
    join tbl_league_publisher on (tbl_league_formerpublishergame.PublisherID = tbl_league_publisher.PublisherID)
    join tbl_league_year on (tbl_league_year.LeagueID = tbl_league_publisher.LeagueID AND tbl_league_year.Year = tbl_league_publisher.Year)
    where tbl_league_year.LeagueID = P_LeagueID AND tbl_league_year.Year = P_Year;
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getusersinleague
DROP PROCEDURE IF EXISTS `sp_getusersinleague`;
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
