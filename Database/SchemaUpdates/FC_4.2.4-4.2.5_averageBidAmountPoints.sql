CREATE TABLE `tbl_caching_averagebidamountpoints` (
	`BidAmount` INT(10) UNSIGNED NOT NULL,
	`DataPoints` INT(10) UNSIGNED NOT NULL,
	`AveragePoints` DECIMAL(12,9) NOT NULL,
	PRIMARY KEY (`BidAmount`) USING BTREE
)
ENGINE=InnoDB
;

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

-- Dumping structure for procedure fantasycritic.sp_getleagueyearsupplementaldata
DROP PROCEDURE IF EXISTS `sp_getleagueyearsupplementaldata`;
DELIMITER //
CREATE PROCEDURE `sp_getleagueyearsupplementaldata`(
	IN `P_LeagueID` CHAR(36),
	IN `P_Year` INT,
	IN `P_UserID` CHAR(36)
)
BEGIN
	DECLARE v_PublisherID CHAR(36);
	
	
	SELECT *
	FROM tbl_caching_averagepositionpoints;
	
	SELECT *
	FROM tbl_caching_averagebidamountpoints;
	
	SELECT *
	FROM tbl_caching_systemwidevalues;
	
	
	SELECT *
	FROM tbl_league_managermessage
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year
	  AND Deleted = 0;
	
	
	SELECT tbl_league_managermessagedismissal.*
	FROM tbl_league_managermessage
	JOIN tbl_league_managermessagedismissal ON tbl_league_managermessage.MessageID = tbl_league_managermessagedismissal.MessageID
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT WinningUserID
	FROM tbl_league_year
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year - 1;
	
	
	SELECT *
	FROM tbl_league_trade
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT tbl_league_tradecomponent.*
	FROM tbl_league_tradecomponent
	JOIN tbl_league_trade ON tbl_league_tradecomponent.TradeID = tbl_league_trade.TradeID
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT tbl_league_tradevote.*
	FROM tbl_league_tradevote
	JOIN tbl_league_trade ON tbl_league_tradevote.TradeID = tbl_league_trade.TradeID
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_league_specialauction
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM vw_league_pickupbid
	WHERE LeagueID = P_LeagueID
	  AND YEAR = P_Year
	  AND SUCCESSFUL IS NULL;
	
	
	SELECT COUNT(*) AS UserIsFollowingLeague
	FROM tbl_user
	JOIN tbl_user_followingleague ON (tbl_user.UserID = tbl_user_followingleague.UserID)
	WHERE tbl_user_followingleague.LeagueID = P_LeagueID
	  AND tbl_user.UserID = P_UserID;
	
	-- Create a temporary table
	
	DROP
	TEMPORARY TABLE IF EXISTS TempTable;
	
	
	CREATE
	TEMPORARY TABLE TempTable AS
	SELECT PublisherID,
	       PublisherName,
	       l.LeagueID,
	       LeagueName,
	       `Year`
	FROM tbl_league_publisher p
	JOIN tbl_league l ON p.LeagueID = l.LeagueID
	WHERE UserID = P_UserID
	  	AND `Year` = P_Year;
	
	-- Retrieve the PublisherID from the temporary table
	
	SELECT PublisherID INTO v_PublisherID
	FROM TempTable
	WHERE LeagueID = P_LeagueID
	LIMIT 1;
	
	
	SELECT *
	FROM TempTable;
	
	-- Second query: Use the stored PublisherID
	
	SELECT *
	FROM tbl_league_droprequest
	WHERE PublisherID = v_PublisherID
	  AND SUCCESSFUL IS NULL;
	
	-- Third query: Use the stored PublisherID
	
	SELECT *
	FROM tbl_league_publisherqueue
	WHERE PublisherID = v_PublisherID;
	
	
	DROP
	TEMPORARY TABLE IF EXISTS TempTable;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
