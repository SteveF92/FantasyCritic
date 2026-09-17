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

-- Dumping structure for table tbl_job_status
CREATE TABLE IF NOT EXISTS `tbl_job_status` (
  `Status` varchar(50) NOT NULL,
  PRIMARY KEY (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table tbl_job_status: ~7 rows (approximately)
INSERT INTO `tbl_job_status` (`Status`) VALUES
	('Cancelled'),
	('CancelledInProgress'),
	('Cancelling'),
	('Complete'),
	('Error'),
	('Queued'),
	('Running');

-- Dumping structure for table tbl_job_runtype
CREATE TABLE IF NOT EXISTS `tbl_job_runtype` (
  `RunType` varchar(50) NOT NULL,
  PRIMARY KEY (`RunType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table tbl_job_runtype: ~4 rows (approximately)
INSERT INTO `tbl_job_runtype` (`RunType`) VALUES
	('Cron'),
	('Disabled'),
	('Manual'),
	('ManualOrCron');

-- Dumping structure for table tbl_job_type
CREATE TABLE IF NOT EXISTS `tbl_job_type` (
  `Name` varchar(100) NOT NULL,
  `DisplayName` varchar(100) NOT NULL,
  `Category` varchar(50) NOT NULL,
  `Severity` varchar(50) NOT NULL,
  `RunType` varchar(50) NOT NULL,
  PRIMARY KEY (`Name`),
  KEY `FK_tbl_job_type_tbl_job_runtype` (`RunType`),
  CONSTRAINT `FK_tbl_job_type_tbl_job_runtype` FOREIGN KEY (`RunType`) REFERENCES `tbl_job_runtype` (`RunType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table tbl_job_type: ~23 rows (approximately)
INSERT INTO `tbl_job_type` (`Name`, `DisplayName`, `Category`, `Severity`, `RunType`) VALUES
	('ExpireTrades', 'Expire Trades', 'Other', 'Danger', 'ManualOrCron'),
	('FullDataRefresh', 'Full Refresh', 'Data Actions', 'Info', 'ManualOrCron'),
	('GrantSuperDrops', 'Grant Super Drops', 'Other', 'Danger', 'ManualOrCron'),
	('MakeSlotsConsistent', 'Make Slots Consistent', 'Other', 'Danger', 'ManualOrCron'),
	('PrepareForActionProcessing', 'Prepare For Action Processing', 'Bids', 'Warning', 'ManualOrCron'),
	('ProcessActions', 'Process Actions', 'Bids', 'Danger', 'Manual'),
	('ProcessSpecialAuctions', 'Process Special Auctions', 'Bids', 'Danger', 'ManualOrCron'),
	('PushGameReleaseMessages', 'Push Game Release Messages', 'Other', 'Info', 'Cron'),
	('SendAllPublicBiddingMessages', 'Send All Public Bidding Messages', 'Other', 'Danger', 'Cron'),
	('SendPublicBiddingDiscordMessages', 'Send Public Bidding Discord Messages', 'Other', 'Danger', 'Manual'),
	('SendPublicBiddingEmails', 'Send Public Bidding Emails', 'Other', 'Danger', 'Manual'),
	('RecalculateLastSeasonWinners', 'Recalculate Last Season Winners', 'Other', 'Danger', 'Manual'),
	('RecalculateRoyaleWinners', 'Recalculate Royale Winners', 'Other', 'Danger', 'Manual'),
	('RecomputeRulesBasedRoyaleGroups', 'Recompute Rules Based Royale Groups', 'Other', 'Info', 'ManualOrCron'),
	('RefreshCaches', 'Refresh Caches', 'Data Actions', 'Info', 'Manual'),
	('RefreshCriticScores', 'Refresh Critic Scores', 'Data Actions', 'Info', 'Manual'),
	('RefreshGGInfo', 'Refresh GG Info', 'Data Actions', 'Info', 'Manual'),
	('RefreshPatreonInfo', 'Refresh Patreon', 'User Support Actions', 'Info', 'ManualOrCron'),
	('SendReleasingThisWeekUpdate', 'Send Releasing This Week Update', 'Other', 'Danger', 'ManualOrCron'),
	('SetTimeFlags', 'Set Time Flags', 'Other', 'Info', 'Cron'),
	('SnapshotDatabase', 'Snapshot Database', 'Database', 'Warning', 'Manual'),
	('UpdateDailyPublisherStatistics', 'Update Daily Publisher Statistics', 'Other', 'Info', 'ManualOrCron'),
	('UpdateFantasyPoints', 'Update Fantasy Points', 'Data Actions', 'Info', 'Manual'),
	('UpdateTopBidsAndDrops', 'Update Top Bids And Drops', 'Bids', 'Danger', 'ManualOrCron');

-- Dumping structure for table tbl_job
CREATE TABLE IF NOT EXISTS `tbl_job` (
  `JobID` char(36) NOT NULL,
  `JobType` varchar(100) NOT NULL,
  `CreatedByUserID` char(36) DEFAULT NULL,
  `Status` varchar(50) NOT NULL,
  `DetailedStatus` text,
  `ErrorMessage` text,
  `ScheduledFor` timestamp(6) NULL DEFAULT NULL,
  `CreatedAt` timestamp(6) NOT NULL,
  `StartedAt` timestamp(6) NULL DEFAULT NULL,
  `FinishedAt` timestamp(6) NULL DEFAULT NULL,
  PRIMARY KEY (`JobID`),
  UNIQUE KEY `UQ_tbl_job_scheduledslot` (`JobType`,`ScheduledFor`),
  KEY `FK_tbl_job_tbl_job_status` (`Status`),
  KEY `FK_tbl_job_tbl_user` (`CreatedByUserID`),
  KEY `IX_tbl_job_pollqueue` (`Status`,`CreatedAt`),
  CONSTRAINT `FK_tbl_job_tbl_job_status` FOREIGN KEY (`Status`) REFERENCES `tbl_job_status` (`Status`),
  CONSTRAINT `FK_tbl_job_tbl_job_type` FOREIGN KEY (`JobType`) REFERENCES `tbl_job_type` (`Name`),
  CONSTRAINT `FK_tbl_job_tbl_user` FOREIGN KEY (`CreatedByUserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table tbl_job: ~0 rows (approximately)

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
