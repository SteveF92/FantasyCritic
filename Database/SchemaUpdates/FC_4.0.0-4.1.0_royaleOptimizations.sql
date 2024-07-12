ALTER TABLE `tbl_royale_supportedquarter`
	ADD COLUMN `WinningUser` CHAR(36) NULL DEFAULT NULL AFTER `Finished`;

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

-- Dumping structure for procedure fantasycritic.sp_gethomepagedata
DROP PROCEDURE IF EXISTS `sp_gethomepagedata`;
DELIMITER //
CREATE PROCEDURE `sp_gethomepagedata`(
	IN `P_UserID` CHAR(36)
)
BEGIN 
	DECLARE userEmailAddress VARCHAR(255);
	DECLARE maxProcessDate DATE;
	DECLARE maxProcessYear INT;
	DECLARE highestYear INT;
	DECLARE highestQuarter INT;
	
	
	SELECT EmailAddress INTO userEmailAddress
	FROM tbl_user
	WHERE UserID = P_UserID;
	
	-- My Leagues
	 CALL sp_getleaguesforuser(P_UserID);
	
	-- League Invites
	
	SELECT tbl_league_invite.*,
	       tbl_league.LeagueName,
	       inviteUser.DisplayName AS InviteUserName,
	       inviteUser.EmailAddress AS InviteUserEmailAddress,
	       leagueManager.DisplayName AS ManagerUserName,
	       leagueYear.ActiveYear
	FROM tbl_league_invite
	JOIN tbl_league ON tbl_league.LeagueID = tbl_league_invite.LeagueID
	JOIN tbl_user leagueManager ON tbl_league.LeagueManager = leagueManager.UserID
	LEFT JOIN tbl_user inviteUser ON tbl_league_invite.UserID = inviteUser.UserID
	LEFT JOIN
	  (SELECT LeagueID,
	          MAX(YEAR) AS ActiveYear
	   FROM tbl_league_year
	   GROUP BY LeagueID) leagueYear ON tbl_league.LeagueID = leagueYear.LeagueID
	WHERE tbl_league_invite.EmailAddress = userEmailAddress
	  OR tbl_league_invite.UserID = P_UserID;
	
	-- My Conferences
	
	SELECT c.ConferenceID,
	       c.ConferenceName,
	       c.CustomRulesConference,
	       u.UserID AS ConferenceManagerID,
	       u.DisplayName AS ConferenceManagerDisplayName
	FROM tbl_conference c
	JOIN tbl_conference_hasuser chu ON c.ConferenceID = chu.ConferenceID
	JOIN tbl_user u ON c.ConferenceManager = u.UserID
	WHERE chu.UserID = P_UserID
	  AND c.IsDeleted = 0;
	
	
	SELECT cy.ConferenceID,
	       cy.Year
	FROM tbl_conference_year cy
	JOIN tbl_conference_hasuser chu ON cy.ConferenceID = chu.ConferenceID
	WHERE chu.UserID = P_UserID;
	
	-- Top Bids and Drops
	
	SELECT MAX(ProcessDate) INTO maxProcessDate
	FROM tbl_caching_topbidsanddrops;
	
	
	SET maxProcessYear = YEAR(maxProcessDate);
	
	
	SELECT *
	FROM tbl_caching_topbidsanddrops
	WHERE ProcessDate = maxProcessDate;
	
	
	SELECT tbl_caching_mastergameyear.*,
	       tbl_user.DisplayName AS AddedByUserDisplayName
	FROM tbl_caching_mastergameyear
	JOIN tbl_user ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
	WHERE tbl_caching_mastergameyear.`Year` IN
	    (SELECT YEAR
	     FROM tbl_meta_supportedyear
	     WHERE OpenForPlay = 1
	       AND Finished = 0);
	
	
	SELECT *
	FROM tbl_mastergame_tag;
	
	
	SELECT *
	FROM tbl_mastergame_subgame
	JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_subgame.MasterGameID
	WHERE YEAR IN
	    (SELECT YEAR
	     FROM tbl_meta_supportedyear
	     WHERE OpenForPlay = 1
	       AND Finished = 0);
	
	
	SELECT *
	FROM tbl_mastergame_hastag
	JOIN tbl_caching_mastergameyear ON tbl_caching_mastergameyear.MasterGameID = tbl_mastergame_hastag.MasterGameID
	WHERE YEAR IN
	    (SELECT YEAR
	     FROM tbl_meta_supportedyear
	     WHERE OpenForPlay = 1
	       AND Finished = 0);
	
	-- My Game News
	
	SELECT tbl_league_publishergame.MasterGameId,
	       tbl_league_publishergame.CounterPick,
	       tbl_league.LeagueID,
	       tbl_league.LeagueName,
	       tbl_league_publisher.Year,
	       tbl_league_publisher.PublisherID,
	       tbl_league_publisher.PublisherName
	FROM tbl_league_publishergame
	JOIN tbl_league_publisher ON tbl_league_publisher.PublisherID = tbl_league_publishergame.PublisherID
	JOIN tbl_league ON tbl_league.LeagueID = tbl_league_publisher.LeagueID
	WHERE tbl_league_publishergame.MasterGameID IS NOT NULL
	  AND tbl_league_publisher.UserID = P_UserID
	  AND tbl_league_publisher.Year IN
	    (SELECT YEAR
	     FROM tbl_meta_supportedyear
	     WHERE OpenForPlay = 1
	       AND Finished = 0);
	
	-- Public League Years
	
	SELECT vw_league.LeagueID,
	       vw_league.LeagueName,
	       vw_league.NumberOfFollowers,
	       tbl_league_year.PlayStatus
	FROM vw_league
	JOIN tbl_league_year ON vw_league.LeagueID = tbl_league_year.LeagueID
	WHERE vw_league.PublicLeague = 1
	  AND tbl_league_year.`Year` = maxProcessYear
	ORDER BY NumberOfFollowers DESC
	LIMIT 10;
	
	-- Active Royale Quarter
	
	SELECT YEAR,
	       QUARTER INTO highestYear,
	                    highestQuarter
	FROM tbl_royale_supportedquarter
	WHERE tbl_royale_supportedquarter.OpenForPlay = 1
	ORDER BY YEAR DESC, QUARTER DESC
	LIMIT 1;
	
	
	SELECT highestYear AS YEAR,
	       highestQuarter AS QUARTER;
	
	
	-- Active User Royale Publisher ID
	
	SELECT PublisherID
	FROM tbl_royale_publisher
	WHERE UserID = P_UserID
	  AND YEAR = highestYear
	  AND QUARTER = highestQuarter;

END//
DELIMITER ;

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
END//
DELIMITER ;

-- Dumping structure for procedure fantasycritic.sp_getroyaleyearquarterdata
DROP PROCEDURE IF EXISTS `sp_getroyaleyearquarterdata`;
DELIMITER //
CREATE PROCEDURE `sp_getroyaleyearquarterdata`(
	IN `P_Year` INT,
	IN `P_Quarter` INT
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
	WHERE YEAR = P_Year
	  AND QUARTER = P_Quarter;
	  
	SELECT *
	FROM tbl_royale_publishergame
	JOIN tbl_royale_publisher ON tbl_royale_publishergame.PublisherID = tbl_royale_publisher.PublisherID
	WHERE tbl_royale_publisher.Year = P_Year
	  AND tbl_royale_publisher.Quarter = P_Quarter;
	
	
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
	WHERE tbl_caching_mastergameyear.`Year` = P_Year;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
