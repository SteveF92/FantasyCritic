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

-- Dumping structure for view fantasycritic.vw_league
DROP VIEW IF EXISTS `vw_league`;
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_league`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league` AS select `tbl_league`.`LeagueID` AS `LeagueID`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_league`.`LeagueManager` AS `LeagueManager`,`tbl_league`.`ConferenceID` AS `ConferenceID`,`tbl_conference`.`ConferenceName` AS `ConferenceName`,`tbl_league`.`PublicLeague` AS `PublicLeague`,`tbl_league`.`TestLeague` AS `TestLeague`,`tbl_league`.`CustomRulesLeague` AS `CustomRulesLeague`,`tbl_league`.`Timestamp` AS `Timestamp`,count(`tbl_user_followingleague`.`UserID`) AS `NumberOfFollowers`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((`tbl_league` left join `tbl_user_followingleague` on((`tbl_league`.`LeagueID` = `tbl_user_followingleague`.`LeagueID`))) left join `tbl_conference` on((`tbl_league`.`ConferenceID` = `tbl_conference`.`ConferenceID`))) group by `tbl_league`.`LeagueID`;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
