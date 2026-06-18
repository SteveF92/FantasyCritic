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

-- Dumping structure for procedure fantasycritic.sp_getbasicdata
DROP PROCEDURE IF EXISTS `sp_getbasicdata`;
DELIMITER //
CREATE PROCEDURE `sp_getbasicdata`()
BEGIN
  select * from tbl_meta_systemwidesettings;
  select * from tbl_mastergame_tag;
  select * from tbl_meta_supportedyear;

  -- Active Royale Quarter
  SELECT tbl_royale_supportedquarter.*,
         tbl_user.DisplayName AS WinningUserDisplayName
  FROM tbl_royale_supportedquarter
  LEFT JOIN tbl_user ON tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
  WHERE tbl_royale_supportedquarter.OpenForPlay = 1
  ORDER BY tbl_royale_supportedquarter.YEAR DESC, tbl_royale_supportedquarter.QUARTER DESC
  LIMIT 1;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
