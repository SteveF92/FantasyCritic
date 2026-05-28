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

-- Dumping structure for procedure fantasycritic.sp_getleague
-- TODO(Phase2-MultiDraft): Any implicit use of DraftNumber = 1 needs to be updated to something more robust once multi-draft is implemented.
DROP PROCEDURE IF EXISTS `sp_getleague`;
DELIMITER //
CREATE PROCEDURE `sp_getleague`(
  IN `P_LeagueID` CHAR(36)
)
BEGIN
  SELECT vw_league.*,
         tbl_user.DisplayName AS ManagerDisplayName,
         tbl_user.EmailAddress AS ManagerEmailAddress
  FROM vw_league
  JOIN tbl_user ON tbl_user.UserID = vw_league.LeagueManager
  WHERE vw_league.LeagueID = P_LeagueID
    AND vw_league.IsDeleted = 0;
  
  SELECT 
  tbl_league_year.`Year`,
  tbl_meta_supportedyear.Finished AS "SupportedYearIsFinished",
  ld.PlayStatus
  FROM tbl_league_year
  JOIN tbl_meta_supportedyear ON tbl_meta_supportedyear.`Year` = tbl_league_year.`Year`
  JOIN tbl_league_draft ld ON ld.LeagueID = tbl_league_year.LeagueID AND ld.Year = tbl_league_year.Year AND ld.DraftNumber = 1
  WHERE tbl_league_year.LeagueID = P_LeagueID;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
