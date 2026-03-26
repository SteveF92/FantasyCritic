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

-- Dumping structure for view fantasycritic.vw_meta_sitecounts
DROP VIEW IF EXISTS `vw_meta_sitecounts`;
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_meta_sitecounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_meta_sitecounts` AS select `usertable`.`usercount` AS `usercount`,`leaguetable`.`leaguecount` AS `leaguecount`,`mastergametable`.`mastergamecount` AS `mastergamecount`,`publishergametable`.`publishergamecount` AS `publishergamecount` from ((((select count(0) AS `usercount` from `tbl_user` where (`tbl_user`.`IsDeleted` = 0)) `usertable` join (select count(0) AS `leaguecount` from `tbl_league`) `leaguetable`) join (select count(0) AS `mastergamecount` from `tbl_mastergame`) `mastergametable`) join (select count(0) AS `publishergamecount` from `tbl_league_publishergame`) `publishergametable`);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
