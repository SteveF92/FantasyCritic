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

-- Dumping structure for procedure fantasycritic.sp_getcombinedleagueyearuserstatus
DROP PROCEDURE IF EXISTS `sp_getcombinedleagueyearuserstatus`;
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

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
