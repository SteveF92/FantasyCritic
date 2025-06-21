-- --------------------------------------------------------
-- Host:                         fantasy-critic-rds.cldutembgs4w.us-east-1.rds.amazonaws.com
-- Server version:               8.0.31 - Source distribution
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

-- Dumping structure for table fantasycritic.tbl_caching_averagebidamountpoints
CREATE TABLE IF NOT EXISTS `tbl_caching_averagebidamountpoints` (
  `BidAmount` int unsigned NOT NULL,
  `DataPoints` int unsigned NOT NULL,
  `AveragePoints` decimal(12,9) NOT NULL,
  PRIMARY KEY (`BidAmount`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_caching_averagepositionpoints
CREATE TABLE IF NOT EXISTS `tbl_caching_averagepositionpoints` (
  `PickPosition` int unsigned NOT NULL,
  `DataPoints` int unsigned NOT NULL,
  `AveragePoints` decimal(12,9) NOT NULL,
  PRIMARY KEY (`PickPosition`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_caching_conferenceyearstandings
CREATE TABLE IF NOT EXISTS `tbl_caching_conferenceyearstandings` (
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueName` varchar(150) COLLATE utf8mb4_general_ci NOT NULL,
  `PublisherName` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`ConferenceID`,`Year`,`LeagueID`,`PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_caching_leagueyear
CREATE TABLE IF NOT EXISTS `tbl_caching_leagueyear` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `OneShotMode` bit(1) NOT NULL,
  PRIMARY KEY (`LeagueID`,`Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_caching_mastergameyear
CREATE TABLE IF NOT EXISTS `tbl_caching_mastergameyear` (
  `Year` year NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `GameName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `EstimatedReleaseDate` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `MinimumReleaseDate` date NOT NULL,
  `MaximumReleaseDate` date DEFAULT NULL,
  `EarlyAccessReleaseDate` date DEFAULT NULL,
  `InternationalReleaseDate` date DEFAULT NULL,
  `AnnouncementDate` date DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `OpenCriticID` int DEFAULT NULL,
  `GGToken` char(6) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `GGSlug` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CriticScore` decimal(7,4) DEFAULT NULL,
  `HasAnyReviews` bit(1) NOT NULL,
  `OpenCriticSlug` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Notes` mediumtext COLLATE utf8mb4_general_ci NOT NULL,
  `BoxartFileName` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `GGCoverArtFileName` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `UseSimpleEligibility` bit(1) NOT NULL,
  `DelayContention` bit(1) NOT NULL,
  `ShowNote` bit(1) NOT NULL,
  `FirstCriticScoreTimestamp` timestamp NULL DEFAULT NULL,
  `AddedTimestamp` timestamp NOT NULL,
  `AddedByUserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PercentStandardGame` double NOT NULL,
  `PercentCounterPick` double NOT NULL,
  `EligiblePercentStandardGame` double NOT NULL,
  `AdjustedPercentCounterPick` double DEFAULT NULL,
  `NumberOfBids` int NOT NULL,
  `TotalBidAmount` int NOT NULL,
  `BidPercentile` double NOT NULL DEFAULT '0',
  `AverageDraftPosition` double DEFAULT NULL,
  `AverageWinningBid` double DEFAULT NULL,
  `HypeFactor` double NOT NULL,
  `DateAdjustedHypeFactor` double NOT NULL,
  `PeakHypeFactor` double NOT NULL,
  `LinearRegressionHypeFactor` double NOT NULL,
  PRIMARY KEY (`Year`,`MasterGameID`),
  KEY `FK_tbl_caching_mastergameyear_tbl_user` (`AddedByUserID`),
  CONSTRAINT `FK_tbl_caching_mastergameyear_tbl_user` FOREIGN KEY (`AddedByUserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_caching_systemwidevalues
CREATE TABLE IF NOT EXISTS `tbl_caching_systemwidevalues` (
  `AverageStandardGamePoints` decimal(12,9) NOT NULL,
  `AveragePickupOnlyStandardGamePoints` decimal(12,9) NOT NULL,
  `AverageCounterPickPoints` decimal(12,9) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_caching_topbidsanddrops
CREATE TABLE IF NOT EXISTS `tbl_caching_topbidsanddrops` (
  `ProcessDate` date NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` int NOT NULL,
  `TotalStandardBidCount` int NOT NULL,
  `SuccessfulStandardBids` int NOT NULL,
  `FailedStandardBids` int NOT NULL,
  `TotalStandardBidLeagues` int NOT NULL,
  `TotalStandardBidAmount` int NOT NULL,
  `TotalCounterPickBidCount` int NOT NULL,
  `SuccessfulCounterPickBids` int NOT NULL,
  `FailedCounterPickBids` int NOT NULL,
  `TotalCounterPickBidLeagues` int NOT NULL,
  `TotalCounterPickBidAmount` int NOT NULL,
  `TotalDropCount` int NOT NULL,
  `SuccessfulDrops` int NOT NULL,
  `FailedDrops` int NOT NULL,
  PRIMARY KEY (`ProcessDate`,`MasterGameID`,`Year`) USING BTREE,
  KEY `FK_tbl_caching_topbidsanddrops_tbl_mastergame` (`MasterGameID`) USING BTREE,
  CONSTRAINT `FK_tbl_caching_topbidsanddrops_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_conference
CREATE TABLE IF NOT EXISTS `tbl_conference` (
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ConferenceName` varchar(150) COLLATE utf8mb4_general_ci NOT NULL,
  `ConferenceManager` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PrimaryLeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `CustomRulesConference` bit(1) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`ConferenceID`) USING BTREE,
  KEY `FK_tbl_conference_tbl_user` (`ConferenceManager`) USING BTREE,
  KEY `FK_tbl_conference_tbl_league` (`PrimaryLeagueID`) USING BTREE,
  CONSTRAINT `FK_tbl_conference_tbl_league` FOREIGN KEY (`PrimaryLeagueID`) REFERENCES `tbl_league` (`LeagueID`),
  CONSTRAINT `FK_tbl_conference_tbl_user` FOREIGN KEY (`ConferenceManager`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_conference_activeplayer
CREATE TABLE IF NOT EXISTS `tbl_conference_activeplayer` (
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`ConferenceID`,`Year`,`UserID`) USING BTREE,
  KEY `tbl_` (`ConferenceID`,`UserID`) USING BTREE,
  CONSTRAINT `FK__tbl_conference_year` FOREIGN KEY (`ConferenceID`, `Year`) REFERENCES `tbl_conference_year` (`ConferenceID`, `Year`),
  CONSTRAINT `tbl_` FOREIGN KEY (`ConferenceID`, `UserID`) REFERENCES `tbl_conference_hasuser` (`ConferenceID`, `UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_conference_hasuser
CREATE TABLE IF NOT EXISTS `tbl_conference_hasuser` (
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ConferenceID`,`UserID`) USING BTREE,
  KEY `FK_tbl_conference_hasuser_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_conference_hasuser_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`),
  CONSTRAINT `FK_tbl_conference_hasuser_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_conference_invitelink
CREATE TABLE IF NOT EXISTS `tbl_conference_invitelink` (
  `InviteID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `InviteCode` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Active` bit(1) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InviteID`) USING BTREE,
  UNIQUE KEY `Unique_Conference_Code` (`ConferenceID`,`InviteCode`) USING BTREE,
  CONSTRAINT `FK_tbl_conference_invitelink_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_conference_managermessage
CREATE TABLE IF NOT EXISTS `tbl_conference_managermessage` (
  `MessageID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `MessageText` text COLLATE utf8mb4_general_ci NOT NULL,
  `IsPublic` bit(1) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Deleted` bit(1) NOT NULL,
  PRIMARY KEY (`MessageID`) USING BTREE,
  KEY `FK_tbl_conference_managermessage_tbl_conference_year` (`ConferenceID`,`Year`) USING BTREE,
  CONSTRAINT `FK_tbl_conference_managermessage_tbl_conference_year` FOREIGN KEY (`ConferenceID`, `Year`) REFERENCES `tbl_conference_year` (`ConferenceID`, `Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_conference_managermessagedismissal
CREATE TABLE IF NOT EXISTS `tbl_conference_managermessagedismissal` (
  `MessageID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`MessageID`,`UserID`) USING BTREE,
  KEY `FK_tbl_conference_managermessagedismissal_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_conference_managermessagedismissal` FOREIGN KEY (`MessageID`) REFERENCES `tbl_conference_managermessage` (`MessageID`),
  CONSTRAINT `FK_tbl_conference_managermessagedismissal_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_conference_year
CREATE TABLE IF NOT EXISTS `tbl_conference_year` (
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ConferenceID`,`Year`) USING BTREE,
  KEY `FK_tbl_conference_year_tbl_meta_supportedyear` (`Year`) USING BTREE,
  CONSTRAINT `FK_tbl_conference_year_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`),
  CONSTRAINT `FK_tbl_conference_year_tbl_meta_supportedyear` FOREIGN KEY (`Year`) REFERENCES `tbl_meta_supportedyear` (`Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_discord_conferencechannel
CREATE TABLE IF NOT EXISTS `tbl_discord_conferencechannel` (
  `GuildID` bigint unsigned NOT NULL DEFAULT '0',
  `ChannelID` bigint unsigned NOT NULL DEFAULT '0',
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`GuildID`,`ChannelID`) USING BTREE,
  KEY `FK_ConferenceID` (`ConferenceID`) USING BTREE,
  CONSTRAINT `tbl_discord_conferencechannel_ibfk_1` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_discord_gamenewschannel
CREATE TABLE IF NOT EXISTS `tbl_discord_gamenewschannel` (
  `GuildID` bigint unsigned NOT NULL DEFAULT '0',
  `ChannelID` bigint unsigned NOT NULL DEFAULT '0',
  `GameNewsSetting` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`GuildID`,`ChannelID`) USING BTREE,
  KEY `FK_tbl_discord_leaguechannel_tbl_discord_gamenewsoptions` (`GameNewsSetting`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_discord_gamenewschannelskiptag
CREATE TABLE IF NOT EXISTS `tbl_discord_gamenewschannelskiptag` (
  `GuildID` bigint unsigned NOT NULL DEFAULT '0',
  `ChannelID` bigint unsigned NOT NULL DEFAULT '0',
  `TagName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`GuildID`,`ChannelID`) USING BTREE,
  KEY `FK_tbl_discord_gamenewschannelskiptag_tbl_mastergame_tag` (`TagName`) USING BTREE,
  CONSTRAINT `FK_tbl_discord_gamenewschannel` FOREIGN KEY (`GuildID`, `ChannelID`) REFERENCES `tbl_discord_gamenewschannel` (`GuildID`, `ChannelID`),
  CONSTRAINT `FK_tbl_discord_gamenewschannelskiptag_tbl_mastergame_tag` FOREIGN KEY (`TagName`) REFERENCES `tbl_mastergame_tag` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_discord_gamenewsoptions
CREATE TABLE IF NOT EXISTS `tbl_discord_gamenewsoptions` (
  `Name` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_discord_leaguechannel
CREATE TABLE IF NOT EXISTS `tbl_discord_leaguechannel` (
  `GuildID` bigint unsigned NOT NULL DEFAULT '0',
  `ChannelID` bigint unsigned NOT NULL DEFAULT '0',
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `SendLeagueMasterGameUpdates` bit(1) NOT NULL DEFAULT b'1',
  `SendNotableMisses` bit(1) NOT NULL DEFAULT b'1',
  `BidAlertRoleID` bigint DEFAULT NULL,
  PRIMARY KEY (`GuildID`,`ChannelID`) USING BTREE,
  KEY `FK_LeagueID` (`LeagueID`),
  CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_league` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league
CREATE TABLE IF NOT EXISTS `tbl_league` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueName` varchar(150) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueManager` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ConferenceID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `PublicLeague` bit(1) NOT NULL DEFAULT b'0',
  `TestLeague` bit(1) NOT NULL DEFAULT b'0',
  `CustomRulesLeague` bit(1) NOT NULL DEFAULT b'0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`LeagueID`),
  KEY `FK_tblleague_tbluser` (`LeagueManager`),
  KEY `FK_tbl_league_tbl_conference` (`ConferenceID`),
  CONSTRAINT `FK_tbl_league_tbl_conference` FOREIGN KEY (`ConferenceID`) REFERENCES `tbl_conference` (`ConferenceID`),
  CONSTRAINT `FK_tblleague_tbluser` FOREIGN KEY (`LeagueManager`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_action
CREATE TABLE IF NOT EXISTS `tbl_league_action` (
  `ID` int unsigned NOT NULL AUTO_INCREMENT,
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `ActionType` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `Description` text COLLATE utf8mb4_general_ci NOT NULL,
  `ManagerAction` bit(1) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_tblactionhistory_tblpublisher` (`PublisherID`),
  CONSTRAINT `FK_tblactionhistory_tblpublisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=985995 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_activeplayer
CREATE TABLE IF NOT EXISTS `tbl_league_activeplayer` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`LeagueID`,`Year`,`UserID`),
  KEY `FK_tbl_league_activeplayer_tbl_league_hasuser` (`LeagueID`,`UserID`),
  CONSTRAINT `FK_tbl_league_activeplayer_tbl_league_hasuser` FOREIGN KEY (`LeagueID`, `UserID`) REFERENCES `tbl_league_hasuser` (`LeagueID`, `UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_league_activeplayer_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_droprequest
CREATE TABLE IF NOT EXISTS `tbl_league_droprequest` (
  `DropRequestID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` datetime NOT NULL,
  `Successful` bit(1) DEFAULT NULL,
  `ProcessSetID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`DropRequestID`),
  KEY `FK_tblacquisitionbid_tblpublisher` (`PublisherID`),
  KEY `FK_tblacquisitionbid_tblmastergame` (`MasterGameID`),
  KEY `FK_tbl_league_droprequest_tbl_meta_actionprocessingset` (`ProcessSetID`),
  CONSTRAINT `FK_tbl_league_droprequest_tbl_meta_actionprocessingset` FOREIGN KEY (`ProcessSetID`) REFERENCES `tbl_meta_actionprocessingset` (`ProcessSetID`),
  CONSTRAINT `tbl_league_droprequest_ibfk_1` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `tbl_league_droprequest_ibfk_2` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_eligibilityoverride
CREATE TABLE IF NOT EXISTS `tbl_league_eligibilityoverride` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Eligible` bit(1) NOT NULL,
  PRIMARY KEY (`LeagueID`,`Year`,`MasterGameID`),
  KEY `FK_tbl_league_eligibilityoverride_tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_league_eligibilityoverride_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`),
  CONSTRAINT `FK_tbl_league_eligibilityoverride_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_formerpublishergame
CREATE TABLE IF NOT EXISTS `tbl_league_formerpublishergame` (
  `PublisherGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `GameName` varchar(150) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` datetime NOT NULL,
  `CounterPick` bit(1) NOT NULL,
  `ManualCriticScore` decimal(7,4) DEFAULT NULL,
  `ManualWillNotRelease` bit(1) NOT NULL,
  `FantasyPoints` decimal(12,4) DEFAULT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `DraftPosition` tinyint DEFAULT NULL,
  `OverallDraftPosition` smallint DEFAULT NULL,
  `BidAmount` smallint DEFAULT NULL,
  `AcquiredInTradeID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `RemovedTimestamp` datetime NOT NULL,
  `RemovedNote` text COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`PublisherGameID`) USING BTREE,
  KEY `FK_tbl_league_formerpublishergame_tbl_league_publisher` (`PublisherID`) USING BTREE,
  KEY `FK_tbl_league_formerpublishergame_tbl_mastergame` (`MasterGameID`) USING BTREE,
  KEY `FK_tbl_league_formerpublishergame_tbl_league_trade` (`AcquiredInTradeID`),
  CONSTRAINT `FK_tbl_league_formerpublishergame_tbl_league_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`),
  CONSTRAINT `FK_tbl_league_formerpublishergame_tbl_league_trade` FOREIGN KEY (`AcquiredInTradeID`) REFERENCES `tbl_league_trade` (`TradeID`),
  CONSTRAINT `FK_tbl_league_formerpublishergame_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_hasuser
CREATE TABLE IF NOT EXISTS `tbl_league_hasuser` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Archived` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`LeagueID`,`UserID`),
  KEY `FK_tblleaguehasuser_tbluser` (`UserID`),
  CONSTRAINT `FK_tblleaguehasuser_tblleague` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tblleaguehasuser_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_invite
CREATE TABLE IF NOT EXISTS `tbl_league_invite` (
  `InviteID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `EmailAddress` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InviteID`),
  UNIQUE KEY `unique_league_email` (`LeagueID`,`EmailAddress`),
  UNIQUE KEY `unique_league_user` (`LeagueID`,`UserID`),
  CONSTRAINT `FK_tblleagueinvite_tblleague` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_invitelink
CREATE TABLE IF NOT EXISTS `tbl_league_invitelink` (
  `InviteID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `InviteCode` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Active` bit(1) NOT NULL DEFAULT b'0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InviteID`),
  UNIQUE KEY `Unique_League_Code` (`LeagueID`,`InviteCode`),
  CONSTRAINT `FK_tbl_league_invitelink_tbl_league` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_manageraction
CREATE TABLE IF NOT EXISTS `tbl_league_manageraction` (
  `ID` int unsigned NOT NULL AUTO_INCREMENT,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `ActionType` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `Description` text COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`ID`) USING BTREE,
  KEY `FK_tbl_league_manageraction_tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  CONSTRAINT `FK_tbl_league_manageraction_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`)
) ENGINE=InnoDB AUTO_INCREMENT=14895 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_managermessage
CREATE TABLE IF NOT EXISTS `tbl_league_managermessage` (
  `MessageID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `MessageText` text COLLATE utf8mb4_general_ci NOT NULL,
  `IsPublic` bit(1) NOT NULL DEFAULT b'0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Deleted` bit(1) NOT NULL,
  PRIMARY KEY (`MessageID`) USING BTREE,
  KEY `FK__tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  CONSTRAINT `FK_tbl_league_managermessage_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_managermessagedismissal
CREATE TABLE IF NOT EXISTS `tbl_league_managermessagedismissal` (
  `MessageID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`MessageID`,`UserID`) USING BTREE,
  KEY `FK_tbl_league_managermessagedismissal_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_managermessagedismissal_tbl_league_managermessage` FOREIGN KEY (`MessageID`) REFERENCES `tbl_league_managermessage` (`MessageID`),
  CONSTRAINT `FK_tbl_league_managermessagedismissal_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_pickupbid
CREATE TABLE IF NOT EXISTS `tbl_league_pickupbid` (
  `BidID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ConditionalDropMasterGameID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CounterPick` bit(1) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `Priority` int NOT NULL,
  `BidAmount` int unsigned NOT NULL,
  `AllowIneligibleSlot` bit(1) NOT NULL DEFAULT b'0',
  `Successful` bit(1) DEFAULT NULL,
  `ProcessSetID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Outcome` text COLLATE utf8mb4_general_ci,
  `ProjectedPointsAtTimeOfBid` decimal(12,4) DEFAULT NULL,
  PRIMARY KEY (`BidID`),
  KEY `FK_tblacquisitionbid_tblpublisher` (`PublisherID`),
  KEY `FK_tblacquisitionbid_tblmastergame` (`MasterGameID`),
  KEY `FK_tbl_league_pickupbid_tbl_mastergame` (`ConditionalDropMasterGameID`),
  KEY `FK_tbl_league_pickupbid_tbl_meta_actionprocessingset` (`ProcessSetID`),
  CONSTRAINT `FK_tbl_league_pickupbid_tbl_mastergame` FOREIGN KEY (`ConditionalDropMasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_league_pickupbid_tbl_meta_actionprocessingset` FOREIGN KEY (`ProcessSetID`) REFERENCES `tbl_meta_actionprocessingset` (`ProcessSetID`),
  CONSTRAINT `tbl_league_pickupbid_ibfk_1` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `tbl_league_pickupbid_ibfk_2` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_publisher
CREATE TABLE IF NOT EXISTS `tbl_league_publisher` (
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PublisherName` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `PublisherIcon` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `PublisherSlogan` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `DraftPosition` tinyint DEFAULT NULL,
  `Budget` int unsigned NOT NULL,
  `FreeGamesDropped` int DEFAULT NULL,
  `WillNotReleaseGamesDropped` int DEFAULT NULL,
  `WillReleaseGamesDropped` int DEFAULT NULL,
  `SuperDropsAvailable` int NOT NULL DEFAULT '0',
  `AutoDraftMode` varchar(50) COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'Off',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PublisherID`),
  UNIQUE KEY `Unique_League_Year_User` (`LeagueID`,`Year`,`UserID`),
  UNIQUE KEY `Unique_League_Year_DraftPosition` (`Year`,`LeagueID`,`DraftPosition`),
  KEY `FK_tblpublisher_tblleaguehasuser` (`LeagueID`,`UserID`),
  KEY `FK_tbl_league_publisher_tbl_meta_autodraftmode` (`AutoDraftMode`),
  CONSTRAINT `FK_tbl_league_publisher_tbl_meta_autodraftmode` FOREIGN KEY (`AutoDraftMode`) REFERENCES `tbl_meta_autodraftmode` (`Mode`),
  CONSTRAINT `FK_tblpublisher_tblleaguehasuser` FOREIGN KEY (`LeagueID`, `UserID`) REFERENCES `tbl_league_hasuser` (`LeagueID`, `UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tblpublisher_tblleagueyear` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_publishergame
CREATE TABLE IF NOT EXISTS `tbl_league_publishergame` (
  `PublisherGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `GameName` varchar(150) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` datetime NOT NULL,
  `CounterPick` bit(1) NOT NULL,
  `ManualCriticScore` decimal(7,4) DEFAULT NULL,
  `ManualWillNotRelease` bit(1) NOT NULL DEFAULT b'0',
  `FantasyPoints` decimal(12,4) DEFAULT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `DraftPosition` tinyint DEFAULT NULL,
  `OverallDraftPosition` smallint DEFAULT NULL,
  `BidAmount` smallint DEFAULT NULL,
  `AcquiredInTradeID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `SlotNumber` int NOT NULL,
  PRIMARY KEY (`PublisherGameID`),
  UNIQUE KEY `UNQ_Slot` (`PublisherID`,`CounterPick`,`SlotNumber`),
  KEY `FK_tblpublishergame_tblmastergame` (`MasterGameID`),
  KEY `FK_tbl_league_publishergame_tbl_league_trade` (`AcquiredInTradeID`),
  CONSTRAINT `FK_tbl_league_publishergame_tbl_league_trade` FOREIGN KEY (`AcquiredInTradeID`) REFERENCES `tbl_league_trade` (`TradeID`) ON DELETE SET NULL,
  CONSTRAINT `FK_tblpublishergame_tblmastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tblpublishergame_tblpublisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_publisherqueue
CREATE TABLE IF NOT EXISTS `tbl_league_publisherqueue` (
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Ranking` int unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`PublisherID`,`MasterGameID`),
  KEY `FK_tbl_league_publisherqueue_tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_league_publisherqueue_tbl_league_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_league_publisherqueue_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_publisherstatistics
CREATE TABLE IF NOT EXISTS `tbl_league_publisherstatistics` (
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Date` date NOT NULL,
  `FantasyPoints` decimal(12,9) NOT NULL,
  `ProjectedPoints` decimal(12,9) NOT NULL,
  `RemainingBudget` smallint unsigned NOT NULL,
  `NumberOfStandardGames` tinyint unsigned NOT NULL,
  `NumberOfStandardGamesReleased` tinyint unsigned NOT NULL,
  `NumberOfStandardGamesExpectedToRelease` tinyint unsigned NOT NULL,
  `NumberOfStandardGamesNotExpectedToRelease` tinyint unsigned NOT NULL,
  `NumberOfCounterPicks` tinyint unsigned NOT NULL,
  `NumberOfCounterPicksReleased` tinyint unsigned NOT NULL,
  `NumberOfCounterPicksExpectedToRelease` tinyint unsigned NOT NULL,
  `NumberOfCounterPicksNotExpectedToRelease` tinyint unsigned NOT NULL,
  PRIMARY KEY (`PublisherID`,`Date`) USING BTREE,
  CONSTRAINT `FK_tbl_league_publisherstatistics_tbl_league_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_specialauction
CREATE TABLE IF NOT EXISTS `tbl_league_specialauction` (
  `SpecialAuctionID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `CreationTime` datetime NOT NULL,
  `ScheduledEndTime` datetime NOT NULL,
  `Processed` bit(1) NOT NULL,
  PRIMARY KEY (`SpecialAuctionID`) USING BTREE,
  KEY `FK_tbl_league_specialauction_tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  KEY `FK_tbl_league_specialauction_tbl_mastergame` (`MasterGameID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_specialauction_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`),
  CONSTRAINT `FK_tbl_league_specialauction_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_specialgameslot
CREATE TABLE IF NOT EXISTS `tbl_league_specialgameslot` (
  `SpecialSlotID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `SpecialSlotPosition` int NOT NULL,
  `Tag` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`SpecialSlotID`) USING BTREE,
  KEY `FK_tbl_league_gameslotusestag_tbl_mastergame_tag` (`Tag`) USING BTREE,
  KEY `FK_tbl_league_specialgameslot_tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  CONSTRAINT `FK_tbl_league_gameslotusestag_tbl_mastergame_tag` FOREIGN KEY (`Tag`) REFERENCES `tbl_mastergame_tag` (`Name`),
  CONSTRAINT `FK_tbl_league_specialgameslot_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_tagoverride
CREATE TABLE IF NOT EXISTS `tbl_league_tagoverride` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `TagName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`LeagueID`,`Year`,`MasterGameID`,`TagName`) USING BTREE,
  KEY `FK_tbl_league_tagoverride_tbl_mastergame` (`MasterGameID`) USING BTREE,
  KEY `FK_tbl_league_tagoverride_tbl_mastergame_tag` (`TagName`) USING BTREE,
  CONSTRAINT `FK_tbl_league_tagoverride_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`),
  CONSTRAINT `FK_tbl_league_tagoverride_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_league_tagoverride_tbl_mastergame_tag` FOREIGN KEY (`TagName`) REFERENCES `tbl_mastergame_tag` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_trade
CREATE TABLE IF NOT EXISTS `tbl_league_trade` (
  `TradeID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `ProposerPublisherID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CounterPartyPublisherID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `ProposerBudgetSendAmount` int unsigned NOT NULL,
  `CounterPartyBudgetSendAmount` int unsigned NOT NULL,
  `Message` text COLLATE utf8mb4_general_ci NOT NULL,
  `ProposedTimestamp` datetime NOT NULL,
  `AcceptedTimestamp` datetime DEFAULT NULL,
  `CompletedTimestamp` datetime DEFAULT NULL,
  `Status` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`TradeID`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_league_publisher` (`ProposerPublisherID`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_meta_tradestatus` (`Status`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_league_publisher_2` (`CounterPartyPublisherID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher` FOREIGN KEY (`ProposerPublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`),
  CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher_2` FOREIGN KEY (`CounterPartyPublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`),
  CONSTRAINT `FK_tbl_league_trade_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`),
  CONSTRAINT `FK_tbl_league_trade_tbl_meta_tradestatus` FOREIGN KEY (`Status`) REFERENCES `tbl_meta_tradestatus` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_tradecomponent
CREATE TABLE IF NOT EXISTS `tbl_league_tradecomponent` (
  `TradeID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `CurrentParty` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `CounterPick` bit(1) NOT NULL,
  PRIMARY KEY (`TradeID`,`CurrentParty`,`MasterGameID`,`CounterPick`) USING BTREE,
  KEY `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` (`CurrentParty`) USING BTREE,
  KEY `FK_tbl_league_tradecomponent_tbl_mastergame` (`MasterGameID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_tradecomponent_tbl_league_trade` FOREIGN KEY (`TradeID`) REFERENCES `tbl_league_trade` (`TradeID`),
  CONSTRAINT `FK_tbl_league_tradecomponent_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` FOREIGN KEY (`CurrentParty`) REFERENCES `tbl_meta_tradingparty` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_tradevote
CREATE TABLE IF NOT EXISTS `tbl_league_tradevote` (
  `TradeID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Approved` bit(1) NOT NULL,
  `Comment` text COLLATE utf8mb4_general_ci,
  `Timestamp` datetime NOT NULL,
  PRIMARY KEY (`TradeID`,`UserID`) USING BTREE,
  KEY `FK_tbl_league_tradevote_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_tradevote_tbl_league_trade` FOREIGN KEY (`TradeID`) REFERENCES `tbl_league_trade` (`TradeID`),
  CONSTRAINT `FK_tbl_league_tradevote_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_year
CREATE TABLE IF NOT EXISTS `tbl_league_year` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `StandardGames` tinyint NOT NULL,
  `GamesToDraft` tinyint NOT NULL,
  `CounterPicks` tinyint NOT NULL,
  `CounterPicksToDraft` tinyint NOT NULL,
  `FreeDroppableGames` tinyint NOT NULL DEFAULT '0',
  `WillNotReleaseDroppableGames` tinyint NOT NULL DEFAULT '0',
  `WillReleaseDroppableGames` tinyint NOT NULL DEFAULT '0',
  `DropOnlyDraftGames` bit(1) NOT NULL DEFAULT b'0',
  `GrantSuperDrops` bit(1) NOT NULL DEFAULT b'0',
  `CounterPicksBlockDrops` bit(1) NOT NULL DEFAULT b'0',
  `AllowMoveIntoIneligible` bit(1) NOT NULL DEFAULT b'0',
  `MinimumBidAmount` tinyint NOT NULL DEFAULT '0',
  `DraftSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `PickupSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `TiebreakSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `ScoringSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `TradingSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `ReleaseSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `PlayStatus` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `DraftOrderSet` bit(1) NOT NULL,
  `CounterPickDeadlineMonth` tinyint NOT NULL,
  `CounterPickDeadlineDay` tinyint NOT NULL,
  `MightReleaseDroppableMonth` tinyint DEFAULT NULL,
  `MightReleaseDroppableDay` tinyint DEFAULT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `DraftStartedTimestamp` timestamp NULL DEFAULT NULL,
  `WinningUserID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `ConferenceLocked` bit(1) DEFAULT NULL,
  PRIMARY KEY (`LeagueID`,`Year`) USING BTREE,
  KEY `FK_tblleagueyear_tblleague` (`LeagueID`),
  KEY `FK_tblleagueyear_tbldraftsystem` (`DraftSystem`),
  KEY `FK_tblleagueyear_tblwaiversystem` (`PickupSystem`),
  KEY `FK_tblleagueyear_tblscoringsystem` (`ScoringSystem`),
  KEY `FK_tblleagueyear_tblplaystatus` (`PlayStatus`),
  KEY `FK_tbl_league_year_tbl_settings_releasesystem` (`ReleaseSystem`),
  KEY `FK_tbl_league_year_tbl_settings_tiebreaksystem` (`TiebreakSystem`),
  KEY `FK_tbl_league_year_tbl_settings_tradingsystem` (`TradingSystem`),
  KEY `FK_tbl_league_year_tbl_user` (`WinningUserID`),
  KEY `tbl_league_year_ibfk_6` (`Year`),
  CONSTRAINT `FK_tbl_league_year_tbl_settings_releasesystem` FOREIGN KEY (`ReleaseSystem`) REFERENCES `tbl_settings_releasesystem` (`ReleaseSystem`),
  CONSTRAINT `FK_tbl_league_year_tbl_settings_tiebreaksystem` FOREIGN KEY (`TiebreakSystem`) REFERENCES `tbl_settings_tiebreaksystem` (`TiebreakSystem`),
  CONSTRAINT `FK_tbl_league_year_tbl_settings_tradingsystem` FOREIGN KEY (`TradingSystem`) REFERENCES `tbl_settings_tradingsystem` (`TradingSystem`),
  CONSTRAINT `FK_tbl_league_year_tbl_user` FOREIGN KEY (`WinningUserID`) REFERENCES `tbl_user` (`UserID`),
  CONSTRAINT `tbl_league_year_ibfk_1` FOREIGN KEY (`DraftSystem`) REFERENCES `tbl_settings_draftsystem` (`DraftSystem`),
  CONSTRAINT `tbl_league_year_ibfk_3` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`),
  CONSTRAINT `tbl_league_year_ibfk_4` FOREIGN KEY (`PlayStatus`) REFERENCES `tbl_settings_playstatus` (`Name`),
  CONSTRAINT `tbl_league_year_ibfk_5` FOREIGN KEY (`ScoringSystem`) REFERENCES `tbl_settings_scoringsystem` (`ScoringSystem`),
  CONSTRAINT `tbl_league_year_ibfk_6` FOREIGN KEY (`Year`) REFERENCES `tbl_meta_supportedyear` (`Year`),
  CONSTRAINT `tbl_league_year_ibfk_7` FOREIGN KEY (`PickupSystem`) REFERENCES `tbl_settings_pickupsystem` (`PickupSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_yearusestag
CREATE TABLE IF NOT EXISTS `tbl_league_yearusestag` (
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `Year` year NOT NULL,
  `Tag` varchar(255) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `Status` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`LeagueID`,`Year`,`Tag`) USING BTREE,
  KEY `FK_tbl_league_yearusestag_tbl_mastergame_tag` (`Tag`) USING BTREE,
  KEY `FK_tbl_league_yearusestag_tbl_settings_tagoption` (`Status`) USING BTREE,
  CONSTRAINT `FK_tbl_league_yearusestag_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`),
  CONSTRAINT `FK_tbl_league_yearusestag_tbl_mastergame_tag` FOREIGN KEY (`Tag`) REFERENCES `tbl_mastergame_tag` (`Name`),
  CONSTRAINT `FK_tbl_league_yearusestag_tbl_settings_tagoption` FOREIGN KEY (`Status`) REFERENCES `tbl_settings_tagstatus` (`Status`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame
CREATE TABLE IF NOT EXISTS `tbl_mastergame` (
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `GameName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `EstimatedReleaseDate` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `MinimumReleaseDate` date NOT NULL,
  `MaximumReleaseDate` date DEFAULT NULL,
  `EarlyAccessReleaseDate` date DEFAULT NULL,
  `InternationalReleaseDate` date DEFAULT NULL,
  `AnnouncementDate` date DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `OpenCriticID` int DEFAULT NULL,
  `GGToken` char(6) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `GGSlug` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CriticScore` decimal(7,4) DEFAULT NULL,
  `HasAnyReviews` bit(1) NOT NULL,
  `OpenCriticSlug` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Notes` mediumtext COLLATE utf8mb4_general_ci,
  `BoxartFileName` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `GGCoverArtFileName` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `FirstCriticScoreTimestamp` timestamp NULL DEFAULT NULL,
  `DoNotRefreshDate` bit(1) NOT NULL DEFAULT b'0',
  `DoNotRefreshAnything` bit(1) NOT NULL DEFAULT b'0',
  `UseSimpleEligibility` bit(1) NOT NULL DEFAULT b'0',
  `DelayContention` bit(1) NOT NULL DEFAULT b'0',
  `ShowNote` bit(1) NOT NULL,
  `AddedTimestamp` timestamp NOT NULL,
  `AddedByUserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`MasterGameID`),
  UNIQUE KEY `UniqueName` (`GameName`),
  KEY `FK_tbl_mastergame_tbl_user` (`AddedByUserID`),
  CONSTRAINT `FK_tbl_mastergame_tbl_user` FOREIGN KEY (`AddedByUserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_changelog
CREATE TABLE IF NOT EXISTS `tbl_mastergame_changelog` (
  `MasterGameChangeID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ChangedByUserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` datetime NOT NULL,
  `Description` text COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`MasterGameChangeID`) USING BTREE,
  KEY `FK__tbl_mastergame` (`MasterGameID`) USING BTREE,
  KEY `FK__tbl_user` (`ChangedByUserID`) USING BTREE,
  CONSTRAINT `FK__tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK__tbl_user` FOREIGN KEY (`ChangedByUserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_changerequest
CREATE TABLE IF NOT EXISTS `tbl_mastergame_changerequest` (
  `RequestID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `RequestTimestamp` datetime NOT NULL,
  `RequestNote` text COLLATE utf8mb4_general_ci NOT NULL,
  `OpenCriticID` int DEFAULT NULL,
  `GGToken` char(6) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Answered` bit(1) NOT NULL,
  `ResponseTimestamp` datetime DEFAULT NULL,
  `ResponseNote` text COLLATE utf8mb4_general_ci,
  `ResponseUserID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Hidden` bit(1) NOT NULL,
  PRIMARY KEY (`RequestID`),
  KEY `FK_tblmastergamerequest_tbluser` (`UserID`),
  KEY `FK_tblmastergamerequest_tblmastergame` (`MasterGameID`),
  CONSTRAINT `tbl_mastergame_changerequest_ibfk_2` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `tbl_mastergame_changerequest_ibfk_3` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_hastag
CREATE TABLE IF NOT EXISTS `tbl_mastergame_hastag` (
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `TagName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `TimeAdded` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MasterGameID`,`TagName`) USING BTREE,
  KEY `FK_tbl_mastergame_hastag_tbl_mastergame_tag` (`TagName`) USING BTREE,
  CONSTRAINT `FK_tbl_mastergame_hastag_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_mastergame_hastag_tbl_mastergame_tag` FOREIGN KEY (`TagName`) REFERENCES `tbl_mastergame_tag` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_request
CREATE TABLE IF NOT EXISTS `tbl_mastergame_request` (
  `RequestID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `RequestTimestamp` datetime NOT NULL,
  `RequestNote` text COLLATE utf8mb4_general_ci NOT NULL,
  `GameName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `SteamID` int DEFAULT NULL,
  `OpenCriticID` int DEFAULT NULL,
  `GGToken` char(6) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `EstimatedReleaseDate` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Answered` bit(1) NOT NULL,
  `ResponseTimestamp` datetime DEFAULT NULL,
  `ResponseNote` text COLLATE utf8mb4_general_ci,
  `ResponseUserID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Hidden` bit(1) NOT NULL,
  PRIMARY KEY (`RequestID`),
  KEY `FK_tblmastergamerequest_tbluser` (`UserID`),
  KEY `FK_tblmastergamerequest_tblmastergame` (`MasterGameID`),
  CONSTRAINT `FK_tblmastergamerequest_tblmastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tblmastergamerequest_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_subgame
CREATE TABLE IF NOT EXISTS `tbl_mastergame_subgame` (
  `MasterSubGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `GameName` varchar(150) COLLATE utf8mb4_general_ci NOT NULL,
  `EstimatedReleaseDate` varchar(150) COLLATE utf8mb4_general_ci NOT NULL,
  `MinimumReleaseDate` date NOT NULL,
  `MaximumReleaseDate` date DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `OpenCriticID` int DEFAULT NULL,
  `CriticScore` decimal(7,4) DEFAULT NULL,
  PRIMARY KEY (`MasterSubGameID`),
  KEY `FK_tblmastersubgame_tblmastergame` (`MasterGameID`),
  CONSTRAINT `FK_tblmastersubgame_tblmastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_tag
CREATE TABLE IF NOT EXISTS `tbl_mastergame_tag` (
  `Name` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `ReadableName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `ShortName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `TagType` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `HasCustomCode` bit(1) NOT NULL DEFAULT b'0',
  `SystemTagOnly` bit(1) NOT NULL DEFAULT b'0',
  `Description` text COLLATE utf8mb4_general_ci NOT NULL,
  `Examples` json NOT NULL,
  `BadgeColor` char(6) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Name`) USING BTREE,
  KEY `FK_tbl_mastergame_tag_tbl_mastergame_tagtype` (`TagType`) USING BTREE,
  CONSTRAINT `FK_tbl_mastergame_tag_tbl_mastergame_tagtype` FOREIGN KEY (`TagType`) REFERENCES `tbl_mastergame_tagtype` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_tagtype
CREATE TABLE IF NOT EXISTS `tbl_mastergame_tagtype` (
  `Name` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Name`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_actionprocessingset
CREATE TABLE IF NOT EXISTS `tbl_meta_actionprocessingset` (
  `ProcessSetID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ProcessTime` datetime NOT NULL,
  `ProcessName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`ProcessSetID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_autodraftmode
CREATE TABLE IF NOT EXISTS `tbl_meta_autodraftmode` (
  `Mode` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Mode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_quarters
CREATE TABLE IF NOT EXISTS `tbl_meta_quarters` (
  `Quarter` tinyint NOT NULL,
  PRIMARY KEY (`Quarter`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_supportedyear
CREATE TABLE IF NOT EXISTS `tbl_meta_supportedyear` (
  `Year` year NOT NULL,
  `OpenForCreation` bit(1) NOT NULL,
  `OpenForPlay` bit(1) NOT NULL,
  `OpenForBetaUsers` bit(1) NOT NULL,
  `StartDate` date NOT NULL,
  `Finished` bit(1) NOT NULL,
  PRIMARY KEY (`Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_systemwidesettings
CREATE TABLE IF NOT EXISTS `tbl_meta_systemwidesettings` (
  `ActionProcessingMode` bit(1) NOT NULL,
  `RefreshOpenCritic` bit(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_tradestatus
CREATE TABLE IF NOT EXISTS `tbl_meta_tradestatus` (
  `Status` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Status`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_tradingparty
CREATE TABLE IF NOT EXISTS `tbl_meta_tradingparty` (
  `Name` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Name`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_royale_publisher
CREATE TABLE IF NOT EXISTS `tbl_royale_publisher` (
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Year` year NOT NULL,
  `Quarter` tinyint NOT NULL DEFAULT '0',
  `PublisherName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '0',
  `PublisherIcon` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `PublisherSlogan` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Budget` decimal(11,2) unsigned NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`PublisherID`),
  UNIQUE KEY `UNQ_Publisher` (`UserID`,`Year`,`Quarter`),
  KEY `FK_tbl_royale_publisher_tbl_user` (`UserID`),
  KEY `FK_tbl_royale_publisher_tbl_royale_supportedquarter` (`Year`,`Quarter`),
  CONSTRAINT `FK_tbl_royale_publisher_tbl_royale_supportedquarter` FOREIGN KEY (`Year`, `Quarter`) REFERENCES `tbl_royale_supportedquarter` (`Year`, `Quarter`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_royale_publisher_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_royale_publishergame
CREATE TABLE IF NOT EXISTS `tbl_royale_publishergame` (
  `PublisherID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `MasterGameID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` datetime NOT NULL,
  `AmountSpent` decimal(11,2) NOT NULL DEFAULT '0.00',
  `AdvertisingMoney` decimal(11,2) NOT NULL DEFAULT '0.00',
  `FantasyPoints` decimal(12,4) DEFAULT '0.0000',
  PRIMARY KEY (`PublisherID`,`MasterGameID`),
  KEY `FK_tbl_royale_publishergame_tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_royale_publishergame_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_royale_publishergame_tbl_royale_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_royale_publisher` (`PublisherID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_royale_supportedquarter
CREATE TABLE IF NOT EXISTS `tbl_royale_supportedquarter` (
  `Year` year NOT NULL,
  `Quarter` tinyint NOT NULL,
  `OpenForPlay` bit(1) NOT NULL,
  `Finished` bit(1) NOT NULL,
  `WinningUser` char(36) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`Year`,`Quarter`),
  KEY `FK_tbl_royale_supportedquarter_tbl_meta_quarters` (`Quarter`),
  KEY `FK_tbl_royale_supportedquarter_tbl_user` (`WinningUser`),
  CONSTRAINT `FK_tbl_royale_supportedquarter_tbl_meta_quarters` FOREIGN KEY (`Quarter`) REFERENCES `tbl_meta_quarters` (`Quarter`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_royale_supportedquarter_tbl_meta_supportedyear` FOREIGN KEY (`Year`) REFERENCES `tbl_meta_supportedyear` (`Year`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_royale_supportedquarter_tbl_user` FOREIGN KEY (`WinningUser`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_draftsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_draftsystem` (
  `DraftSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`DraftSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_pickupsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_pickupsystem` (
  `PickupSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`PickupSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_playstatus
CREATE TABLE IF NOT EXISTS `tbl_settings_playstatus` (
  `Name` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_releasesystem
CREATE TABLE IF NOT EXISTS `tbl_settings_releasesystem` (
  `ReleaseSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`ReleaseSystem`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_scoringsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_scoringsystem` (
  `ScoringSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`ScoringSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_tagstatus
CREATE TABLE IF NOT EXISTS `tbl_settings_tagstatus` (
  `Status` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Status`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_tiebreaksystem
CREATE TABLE IF NOT EXISTS `tbl_settings_tiebreaksystem` (
  `TiebreakSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`TiebreakSystem`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_tradingsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_tradingsystem` (
  `TradingSystem` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`TradingSystem`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_system_patreonkeys
CREATE TABLE IF NOT EXISTS `tbl_system_patreonkeys` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `AccessToken` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `RefreshToken` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `CreatedTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_system_xmlkey
CREATE TABLE IF NOT EXISTS `tbl_system_xmlkey` (
  `Id` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `FriendlyName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `Xml` text COLLATE utf8mb4_general_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user
CREATE TABLE IF NOT EXISTS `tbl_user` (
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `DisplayName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `PatreonDonorNameOverride` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `DisplayNumber` smallint DEFAULT NULL,
  `EmailAddress` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `NormalizedEmailAddress` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `PasswordHash` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `SecurityStamp` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `TwoFactorEnabled` bit(1) NOT NULL,
  `AuthenticatorKey` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `LastChangedCredentials` datetime NOT NULL,
  `ShowDecimalPoints` bit(1) NOT NULL DEFAULT b'0',
  `EmailConfirmed` bit(1) NOT NULL,
  `AccountCreated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` bit(1) NOT NULL,
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `Index 3` (`NormalizedEmailAddress`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_donorname
CREATE TABLE IF NOT EXISTS `tbl_user_donorname` (
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `DonorName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`UserID`) USING BTREE,
  CONSTRAINT `tbl_user_donorname_ibfk_2` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_emailsettings
CREATE TABLE IF NOT EXISTS `tbl_user_emailsettings` (
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `EmailType` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`UserID`,`EmailType`) USING BTREE,
  KEY `FK_tbl_user_emailsettings_tbl_user_emailtype` (`EmailType`) USING BTREE,
  CONSTRAINT `FK_tbl_user_emailsettings_tbl_user_emailtype` FOREIGN KEY (`EmailType`) REFERENCES `tbl_user_emailtype` (`EmailType`),
  CONSTRAINT `tbl_user_emailsettings_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_emailtype
CREATE TABLE IF NOT EXISTS `tbl_user_emailtype` (
  `EmailType` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `ReadableName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`EmailType`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_externallogin
CREATE TABLE IF NOT EXISTS `tbl_user_externallogin` (
  `LoginProvider` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `ProviderKey` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `ProviderDisplayName` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `TimeAdded` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`) USING BTREE,
  UNIQUE KEY `UNQ_Login` (`LoginProvider`,`ProviderKey`,`UserID`) USING BTREE,
  KEY `FK_tbl_user_externallogin_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_user_externallogin_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_followingleague
CREATE TABLE IF NOT EXISTS `tbl_user_followingleague` (
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `LeagueID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserID`,`LeagueID`),
  KEY `FK_tbluserfollowingleague_tblleague` (`LeagueID`),
  CONSTRAINT `FK_tbluserfollowingleague_tblleague` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbluserfollowingleague_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_hasrole
CREATE TABLE IF NOT EXISTS `tbl_user_hasrole` (
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `RoleID` tinyint NOT NULL,
  `ProgrammaticallyAssigned` bit(1) NOT NULL,
  PRIMARY KEY (`UserID`,`RoleID`),
  KEY `FK_tbluserhasrole_tblrole` (`RoleID`),
  CONSTRAINT `FK_tbluserhasrole_tblrole` FOREIGN KEY (`RoleID`) REFERENCES `tbl_user_role` (`RoleID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbluserhasrole_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_persistedgrant
CREATE TABLE IF NOT EXISTS `tbl_user_persistedgrant` (
  `Key` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `Type` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `SubjectId` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `ClientId` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `CreationTime` datetime NOT NULL,
  `ConsumedTime` datetime DEFAULT NULL,
  `Expiration` datetime DEFAULT NULL,
  `Data` text COLLATE utf8mb4_general_ci NOT NULL,
  `Description` text COLLATE utf8mb4_general_ci,
  `SessionId` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  PRIMARY KEY (`Key`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_recoverycode
CREATE TABLE IF NOT EXISTS `tbl_user_recoverycode` (
  `UserID` char(36) COLLATE utf8mb4_general_ci NOT NULL,
  `RecoveryCode` varchar(255) COLLATE utf8mb4_general_ci NOT NULL,
  `CreatedTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserID`,`RecoveryCode`) USING BTREE,
  CONSTRAINT `tbl_user_recoverycode_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_role
CREATE TABLE IF NOT EXISTS `tbl_user_role` (
  `RoleID` tinyint NOT NULL,
  `Name` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  `NormalizedName` varchar(50) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`RoleID`),
  UNIQUE KEY `Index 2` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for view fantasycritic.vw_discord_leaguechannel
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `vw_discord_leaguechannel` (
	`LeagueID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`GuildID` BIGINT(20) UNSIGNED NOT NULL,
	`ChannelID` BIGINT(20) UNSIGNED NOT NULL,
	`SendLeagueMasterGameUpdates` BIT(1) NOT NULL,
	`SendNotableMisses` BIT(1) NOT NULL,
	`BidAlertRoleID` BIGINT(19) NULL,
	`MinimumLeagueYear` YEAR NULL
) ENGINE=MyISAM;

-- Dumping structure for view fantasycritic.vw_league
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `vw_league` (
	`LeagueID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`LeagueName` VARCHAR(150) NOT NULL COLLATE 'utf8mb4_general_ci',
	`LeagueManager` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`ConferenceID` CHAR(36) NULL COLLATE 'utf8mb4_general_ci',
	`ConferenceName` VARCHAR(150) NULL COLLATE 'utf8mb4_general_ci',
	`PublicLeague` BIT(1) NOT NULL,
	`TestLeague` BIT(1) NOT NULL,
	`CustomRulesLeague` BIT(1) NOT NULL,
	`Timestamp` TIMESTAMP NOT NULL,
	`NumberOfFollowers` BIGINT(19) NOT NULL,
	`IsDeleted` BIT(1) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view fantasycritic.vw_league_droprequest
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `vw_league_droprequest` (
	`DropRequestID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`PublisherID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`LeagueID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`Year` YEAR NOT NULL,
	`PublisherName` VARCHAR(100) NULL COLLATE 'utf8mb4_general_ci',
	`LeagueName` VARCHAR(150) NOT NULL COLLATE 'utf8mb4_general_ci',
	`MasterGameID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`GameName` VARCHAR(255) NOT NULL COLLATE 'utf8mb4_general_ci',
	`Successful` BIT(1) NULL,
	`Timestamp` DATETIME NOT NULL,
	`ProcessSetID` CHAR(36) NULL COLLATE 'utf8mb4_general_ci',
	`IsDeleted` BIT(1) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view fantasycritic.vw_league_pickupbid
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `vw_league_pickupbid` (
	`BidID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`PublisherID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`LeagueID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`Year` YEAR NOT NULL,
	`PublisherName` VARCHAR(100) NULL COLLATE 'utf8mb4_general_ci',
	`LeagueName` VARCHAR(150) NOT NULL COLLATE 'utf8mb4_general_ci',
	`MasterGameID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`GameName` VARCHAR(255) NOT NULL COLLATE 'utf8mb4_general_ci',
	`ConditionalDropMasterGameID` CHAR(36) NULL COLLATE 'utf8mb4_general_ci',
	`CounterPick` BIT(1) NOT NULL,
	`Priority` INT(10) NOT NULL,
	`BidAmount` INT(10) UNSIGNED NOT NULL,
	`AllowIneligibleSlot` BIT(1) NOT NULL,
	`Successful` BIT(1) NULL,
	`Timestamp` DATETIME NOT NULL,
	`ProcessSetID` CHAR(36) NULL COLLATE 'utf8mb4_general_ci',
	`Outcome` TEXT NULL COLLATE 'utf8mb4_general_ci',
	`ProjectedPointsAtTimeOfBid` DECIMAL(12,4) NULL,
	`IsDeleted` BIT(1) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view fantasycritic.vw_meta_sitecounts
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `vw_meta_sitecounts` (
	`usercount` BIGINT(19) NOT NULL,
	`leaguecount` BIGINT(19) NOT NULL,
	`mastergamecount` BIGINT(19) NOT NULL,
	`publishergamecount` BIGINT(19) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view fantasycritic.vw_discord_leaguechannel
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_discord_leaguechannel`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_discord_leaguechannel` AS select `tbl_discord_leaguechannel`.`LeagueID` AS `LeagueID`,`tbl_discord_leaguechannel`.`GuildID` AS `GuildID`,`tbl_discord_leaguechannel`.`ChannelID` AS `ChannelID`,`tbl_discord_leaguechannel`.`SendLeagueMasterGameUpdates` AS `SendLeagueMasterGameUpdates`,`tbl_discord_leaguechannel`.`SendNotableMisses` AS `SendNotableMisses`,`tbl_discord_leaguechannel`.`BidAlertRoleID` AS `BidAlertRoleID`,min(`tbl_league_year`.`Year`) AS `MinimumLeagueYear` from (`tbl_discord_leaguechannel` join `tbl_league_year` on((`tbl_discord_leaguechannel`.`LeagueID` = `tbl_league_year`.`LeagueID`))) group by `tbl_discord_leaguechannel`.`LeagueID`,`tbl_discord_leaguechannel`.`GuildID`,`tbl_discord_leaguechannel`.`ChannelID`;

-- Dumping structure for view fantasycritic.vw_league
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_league`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league` AS select `tbl_league`.`LeagueID` AS `LeagueID`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_league`.`LeagueManager` AS `LeagueManager`,`tbl_league`.`ConferenceID` AS `ConferenceID`,`tbl_conference`.`ConferenceName` AS `ConferenceName`,`tbl_league`.`PublicLeague` AS `PublicLeague`,`tbl_league`.`TestLeague` AS `TestLeague`,`tbl_league`.`CustomRulesLeague` AS `CustomRulesLeague`,`tbl_league`.`Timestamp` AS `Timestamp`,count(`tbl_user_followingleague`.`UserID`) AS `NumberOfFollowers`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((`tbl_league` left join `tbl_user_followingleague` on((`tbl_league`.`LeagueID` = `tbl_user_followingleague`.`LeagueID`))) left join `tbl_conference` on((`tbl_league`.`ConferenceID` = `tbl_conference`.`ConferenceID`))) group by `tbl_league`.`LeagueID`;

-- Dumping structure for view fantasycritic.vw_league_droprequest
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_league_droprequest`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_droprequest` AS select `tbl_league_droprequest`.`DropRequestID` AS `DropRequestID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_droprequest`.`Successful` AS `Successful`,`tbl_league_droprequest`.`Timestamp` AS `Timestamp`,`tbl_league_droprequest`.`ProcessSetID` AS `ProcessSetID`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_droprequest` join `tbl_league_publisher` on((`tbl_league_droprequest`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_droprequest`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));

-- Dumping structure for view fantasycritic.vw_league_pickupbid
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_league_pickupbid`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_pickupbid` AS select `tbl_league_pickupbid`.`BidID` AS `BidID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_pickupbid`.`ConditionalDropMasterGameID` AS `ConditionalDropMasterGameID`,`tbl_league_pickupbid`.`CounterPick` AS `CounterPick`,`tbl_league_pickupbid`.`Priority` AS `Priority`,`tbl_league_pickupbid`.`BidAmount` AS `BidAmount`,`tbl_league_pickupbid`.`AllowIneligibleSlot` AS `AllowIneligibleSlot`,`tbl_league_pickupbid`.`Successful` AS `Successful`,`tbl_league_pickupbid`.`Timestamp` AS `Timestamp`,`tbl_league_pickupbid`.`ProcessSetID` AS `ProcessSetID`,`tbl_league_pickupbid`.`Outcome` AS `Outcome`,`tbl_league_pickupbid`.`ProjectedPointsAtTimeOfBid` AS `ProjectedPointsAtTimeOfBid`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_pickupbid` join `tbl_league_publisher` on((`tbl_league_pickupbid`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_pickupbid`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));

-- Dumping structure for view fantasycritic.vw_meta_sitecounts
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `vw_meta_sitecounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_meta_sitecounts` AS select `usertable`.`usercount` AS `usercount`,`leaguetable`.`leaguecount` AS `leaguecount`,`mastergametable`.`mastergamecount` AS `mastergamecount`,`publishergametable`.`publishergamecount` AS `publishergamecount` from ((((select count(0) AS `usercount` from `tbl_user` where (`tbl_user`.`IsDeleted` = 0)) `usertable` join (select count(0) AS `leaguecount` from `tbl_league`) `leaguetable`) join (select count(0) AS `mastergamecount` from `tbl_mastergame`) `mastergametable`) join (select count(0) AS `publishergamecount` from `tbl_league_publishergame`) `publishergametable`);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
