-- --------------------------------------------------------
-- Host:                         fantasy-critic-beta-rds.cldutembgs4w.us-east-1.rds.amazonaws.com
-- Server version:               8.0.35 - Source distribution
-- Server OS:                    Linux
-- HeidiSQL Version:             12.8.0.6908
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Dumping structure for procedure fantasycritic.sp_getroyalepublisher
DROP PROCEDURE IF EXISTS `sp_getroyalepublisher`;
DELIMITER //
CREATE PROCEDURE `sp_getroyalepublisher`(
	IN `P_PublisherID` CHAR(36)
)
BEGIN
	SELECT tbl_royale_supportedquarter.*,
	     tbl_user.DisplayName AS WinningUserDisplayName
	FROM tbl_royale_supportedquarter
	LEFT JOIN tbl_user ON tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
	ORDER BY YEAR,
	         QUARTER;

	SELECT tbl_royale_publisher.*,
	       tbl_user.DisplayName AS PublisherDisplayName
	FROM tbl_royale_publisher
	JOIN tbl_user ON tbl_user.UserID = tbl_royale_publisher.UserID
	WHERE PublisherID = P_PublisherID;
	
	
	SELECT *
	FROM tbl_royale_publishergame
	WHERE tbl_royale_publishergame.PublisherID = P_PublisherID; 
	
	-- Master Game Data
	
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
	join tbl_royale_publisher on tbl_royale_publisher.Year = tbl_caching_mastergameyear.Year
	WHERE tbl_royale_publisher.PublisherID = P_PublisherID;
	
	SELECT * FROM tbl_royale_action WHERE tbl_royale_action.PublisherID = P_PublisherID;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
