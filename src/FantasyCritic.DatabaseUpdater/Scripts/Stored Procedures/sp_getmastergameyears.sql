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

-- Dumping structure for procedure fantasycritic.sp_getmastergameyears
DROP PROCEDURE IF EXISTS `sp_getmastergameyears`;
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

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
