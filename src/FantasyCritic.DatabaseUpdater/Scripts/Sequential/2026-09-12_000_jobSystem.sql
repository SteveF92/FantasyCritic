-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.4.8 - MySQL Community Server - GPL
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

-- Dumping structure for table fantasycritic-fromsnapshot.tbl_job
CREATE TABLE IF NOT EXISTS `tbl_job` (
  `JobID` char(36) NOT NULL,
  `JobType` varchar(100) NOT NULL,
  `CreatedByUserID` char(36) DEFAULT NULL,
  `Status` varchar(50) NOT NULL,
  `DetailedStatus` text,
  `ErrorMessage` text,
  `CreatedAt` timestamp NOT NULL,
  `StartedAt` timestamp NULL DEFAULT NULL,
  `FinishedAt` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`JobID`),
  KEY `FK_tbl_job_tbl_job_type` (`JobType`),
  KEY `FK_tbl_job_tbl_job_status` (`Status`),
  KEY `FK_tbl_job_tbl_user` (`CreatedByUserID`),
  CONSTRAINT `FK_tbl_job_tbl_job_status` FOREIGN KEY (`Status`) REFERENCES `tbl_job_status` (`Status`),
  CONSTRAINT `FK_tbl_job_tbl_job_type` FOREIGN KEY (`JobType`) REFERENCES `tbl_job_type` (`Name`),
  CONSTRAINT `FK_tbl_job_tbl_user` FOREIGN KEY (`CreatedByUserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table fantasycritic-fromsnapshot.tbl_job: ~0 rows (approximately)

-- Dumping structure for table fantasycritic-fromsnapshot.tbl_job_status
CREATE TABLE IF NOT EXISTS `tbl_job_status` (
  `Status` varchar(50) NOT NULL,
  PRIMARY KEY (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table fantasycritic-fromsnapshot.tbl_job_status: ~6 rows (approximately)
INSERT INTO `tbl_job_status` (`Status`) VALUES
	('Cancelled'),
	('CancelledInProgress'),
	('Cancelling'),
	('Complete'),
	('Error'),
	('Queued'),
	('Running');

-- Dumping structure for table fantasycritic-fromsnapshot.tbl_job_type
CREATE TABLE IF NOT EXISTS `tbl_job_type` (
  `Name` varchar(100) NOT NULL,
  `CronEnabled` bit(1) NOT NULL,
  PRIMARY KEY (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table fantasycritic-fromsnapshot.tbl_job_type: ~19 rows (approximately)
INSERT INTO `tbl_job_type` (`Name`, `CronEnabled`) VALUES
	('ExpireTrades', b'1'),
	('FullDataRefresh', b'1'),
	('GrantSuperDrops', b'1'),
	('MakeSlotsConsistent', b'1'),
	('PrepareForActionProcessing', b'1'),
	('ProcessActions', b'0'),
	('ProcessSpecialAuctions', b'1'),
	('PushPublicBiddingMessages', b'1'),
	('RecalculateLastSeasonWinners', b'0'),
	('RecalculateRoyaleWinners', b'0'),
	('RecomputeRulesBasedRoyaleGroups', b'1'),
	('RefreshCaches', b'0'),
	('RefreshCriticScores', b'0'),
	('RefreshGGInfo', b'0'),
	('SendPublicBiddingEmails', b'1'),
	('SnapshotDatabase', b'0'),
	('UpdateDailyPublisherStatistics', b'1'),
	('UpdateFantasyPoints', b'0'),
	('UpdateTopBidsAndDrops', b'1');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
