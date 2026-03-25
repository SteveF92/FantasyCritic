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

-- Dumping structure for view fantasycritic.vw_league_droprequest
DROP VIEW IF EXISTS `vw_league_droprequest`;
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_league_droprequest`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_droprequest` AS select `tbl_league_droprequest`.`DropRequestID` AS `DropRequestID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_droprequest`.`Successful` AS `Successful`,`tbl_league_droprequest`.`Timestamp` AS `Timestamp`,`tbl_league_droprequest`.`ProcessSetID` AS `ProcessSetID`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_droprequest` join `tbl_league_publisher` on((`tbl_league_droprequest`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_droprequest`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
