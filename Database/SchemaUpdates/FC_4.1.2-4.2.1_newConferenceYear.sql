CREATE TABLE `tbl_conference_activeplayer` (
	`ConferenceID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`UserID` CHAR(36) NOT NULL,
	PRIMARY KEY (`ConferenceID`, `Year`, `UserID`) USING BTREE,
	INDEX `tbl_` (`ConferenceID`, `UserID`) USING BTREE,
	CONSTRAINT `FK__tbl_conference_year` FOREIGN KEY (`ConferenceID`, `Year`) REFERENCES `tbl_conference_year` (`ConferenceID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `tbl_` FOREIGN KEY (`ConferenceID`, `UserID`) REFERENCES `tbl_conference_hasuser` (`ConferenceID`, `UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;

INSERT INTO tbl_conference_activeplayer (ConferenceID, UserID, Year)
SELECT ConferenceID, UserID, 2024 as Year
FROM tbl_conference_hasuser;

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

-- Dumping structure for procedure fantasycritic.sp_getconferenceyeardata
DROP PROCEDURE IF EXISTS `sp_getconferenceyeardata`;
DELIMITER //
CREATE PROCEDURE `sp_getconferenceyeardata`(
	IN `P_ConferenceID` CHAR(36),
	IN `P_Year` INT
)
BEGIN
	SELECT tbl_conference.*,
	       tbl_user.DisplayName AS ConferenceManagerDisplayName,
	       tbl_user.EmailAddress AS ConferenceManagerEmailAddress
	FROM tbl_conference
	JOIN tbl_user ON tbl_conference.ConferenceManager = tbl_user.UserID
	WHERE ConferenceID = P_ConferenceID
	  AND tbl_conference.IsDeleted = 0;
	
	
	SELECT YEAR
	FROM tbl_conference_year
	WHERE ConferenceID = P_ConferenceID;
	
	
	SELECT LeagueID,
	       LeagueManager
	FROM tbl_league
	WHERE ConferenceID = P_ConferenceID;
	
	
	SELECT *
	FROM tbl_conference_year
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	
	
	SELECT *
	FROM tbl_meta_supportedyear
	WHERE YEAR = P_Year;
	
	SELECT NULL AS LeagueID,
			tbl_conference_hasuser.UserID,
			tbl_user.DisplayName,
			tbl_user.EmailAddress
	FROM tbl_conference_hasuser
	JOIN tbl_user ON tbl_conference_hasuser.UserID = tbl_user.UserID
	WHERE tbl_conference_hasuser.ConferenceID = P_ConferenceID
	UNION
	SELECT tbl_league_hasuser.LeagueID,
	       tbl_league_hasuser.UserID,
	       tbl_user.DisplayName,
	       tbl_user.EmailAddress
	FROM tbl_league_hasuser
	JOIN tbl_league ON tbl_league_hasuser.LeagueID = tbl_league.LeagueID
	JOIN tbl_user ON tbl_league_hasuser.UserID = tbl_user.UserID
	WHERE tbl_league.ConferenceID = P_ConferenceID;
	
	
	SELECT tbl_league_activeplayer.LeagueID,
	       tbl_league_activeplayer.Year,
	       tbl_league_activeplayer.UserID
	FROM tbl_league_activeplayer
	JOIN tbl_league ON tbl_league_activeplayer.LeagueID = tbl_league.LeagueID
	WHERE tbl_league.ConferenceID = P_ConferenceID;
	
	SELECT 
		tbl_conference_activeplayer.ConferenceID, 
		tbl_conference_activeplayer.Year, 
		tbl_conference_activeplayer.UserID 
	FROM tbl_conference_activeplayer
	WHERE tbl_conference_activeplayer.ConferenceID = P_ConferenceID;
	
	SELECT *
	FROM tbl_conference_managermessage
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year
	  AND Deleted = 0;
	
	
	SELECT tbl_conference_managermessagedismissal.*
	FROM tbl_conference_managermessage
	JOIN tbl_conference_managermessagedismissal ON tbl_conference_managermessage.MessageID = tbl_conference_managermessagedismissal.MessageID
	WHERE ConferenceID = P_ConferenceID
	  AND YEAR = P_Year;
	  
	select * from tbl_caching_averagepositionpoints;
	select * from tbl_caching_systemwidevalues;
	  
	CALL sp_getleagueyearsforconferenceyear(P_ConferenceID, P_Year);
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
