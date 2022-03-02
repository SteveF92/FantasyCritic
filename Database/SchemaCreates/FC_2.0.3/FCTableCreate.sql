-- --------------------------------------------------------
-- Host:                         fantasy-critic-beta-rds.cldutembgs4w.us-east-1.rds.amazonaws.com
-- Server version:               5.7.33-log - Source distribution
-- Server OS:                    Linux
-- HeidiSQL Version:             11.3.0.6295
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for fantasycritic
CREATE DATABASE IF NOT EXISTS `fantasycritic` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;
USE `fantasycritic`;

-- Dumping structure for table fantasycritic.tbl_caching_mastergameyear
CREATE TABLE IF NOT EXISTS `tbl_caching_mastergameyear` (
  `Year` year(4) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `GameName` varchar(255) NOT NULL,
  `EstimatedReleaseDate` varchar(255) NOT NULL,
  `MinimumReleaseDate` date NOT NULL,
  `MaximumReleaseDate` date DEFAULT NULL,
  `EarlyAccessReleaseDate` date DEFAULT NULL,
  `InternationalReleaseDate` date DEFAULT NULL,
  `AnnouncementDate` date DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `OpenCriticID` int(11) DEFAULT NULL,
  `GGToken` char(6) DEFAULT NULL,
  `CriticScore` decimal(7,4) DEFAULT NULL,
  `Notes` mediumtext NOT NULL,
  `BoxartFileName` varchar(255) DEFAULT NULL,
  `GGCoverArtFileName` varchar(255) DEFAULT NULL,
  `EligibilityChanged` bit(1) NOT NULL,
  `DelayContention` bit(1) NOT NULL,
  `FirstCriticScoreTimestamp` timestamp NULL DEFAULT NULL,
  `AddedTimestamp` timestamp NOT NULL,
  `PercentStandardGame` double NOT NULL,
  `PercentCounterPick` double NOT NULL,
  `EligiblePercentStandardGame` double NOT NULL,
  `AdjustedPercentCounterPick` double DEFAULT NULL,
  `NumberOfBids` int(11) NOT NULL,
  `TotalBidAmount` int(11) NOT NULL,
  `BidPercentile` double NOT NULL DEFAULT '0',
  `AverageDraftPosition` double DEFAULT NULL,
  `AverageWinningBid` double DEFAULT NULL,
  `HypeFactor` double NOT NULL,
  `DateAdjustedHypeFactor` double NOT NULL,
  `PeakHypeFactor` double NOT NULL,
  `LinearRegressionHypeFactor` double NOT NULL,
  PRIMARY KEY (`Year`,`MasterGameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_caching_systemwidevalues
CREATE TABLE IF NOT EXISTS `tbl_caching_systemwidevalues` (
  `AverageStandardGamePoints` decimal(12,9) NOT NULL,
  `AverageCounterPickPoints` decimal(12,9) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league
CREATE TABLE IF NOT EXISTS `tbl_league` (
  `LeagueID` char(36) NOT NULL,
  `LeagueName` varchar(150) NOT NULL,
  `LeagueManager` char(36) NOT NULL,
  `PublicLeague` bit(1) NOT NULL DEFAULT b'0',
  `TestLeague` bit(1) NOT NULL DEFAULT b'0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`LeagueID`),
  KEY `FK_tblleague_tbluser` (`LeagueManager`),
  CONSTRAINT `FK_tblleague_tbluser` FOREIGN KEY (`LeagueManager`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_action
CREATE TABLE IF NOT EXISTS `tbl_league_action` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `PublisherID` char(36) NOT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `ActionType` varchar(255) NOT NULL,
  `Description` text NOT NULL,
  `ManagerAction` bit(1) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_tblactionhistory_tblpublisher` (`PublisherID`),
  CONSTRAINT `FK_tblactionhistory_tblpublisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_activeplayer
CREATE TABLE IF NOT EXISTS `tbl_league_activeplayer` (
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `UserID` char(36) NOT NULL DEFAULT '',
  PRIMARY KEY (`LeagueID`,`Year`,`UserID`),
  KEY `FK_tbl_league_activeplayer_tbl_league_hasuser` (`LeagueID`,`UserID`),
  CONSTRAINT `FK_tbl_league_activeplayer_tbl_league_hasuser` FOREIGN KEY (`LeagueID`, `UserID`) REFERENCES `tbl_league_hasuser` (`LeagueID`, `UserID`),
  CONSTRAINT `FK_tbl_league_activeplayer_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_droprequest
CREATE TABLE IF NOT EXISTS `tbl_league_droprequest` (
  `DropRequestID` char(36) NOT NULL,
  `PublisherID` char(36) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `Successful` bit(1) DEFAULT NULL,
  `ProcessSetID` char(36) DEFAULT NULL,
  PRIMARY KEY (`DropRequestID`),
  KEY `FK_tblacquisitionbid_tblpublisher` (`PublisherID`),
  KEY `FK_tblacquisitionbid_tblmastergame` (`MasterGameID`),
  KEY `FK_tbl_league_droprequest_tbl_meta_actionprocessingset` (`ProcessSetID`),
  CONSTRAINT `FK_tbl_league_droprequest_tbl_meta_actionprocessingset` FOREIGN KEY (`ProcessSetID`) REFERENCES `tbl_meta_actionprocessingset` (`ProcessSetID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `tbl_league_droprequest_ibfk_1` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `tbl_league_droprequest_ibfk_2` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_eligibilityoverride
CREATE TABLE IF NOT EXISTS `tbl_league_eligibilityoverride` (
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `Eligible` bit(1) NOT NULL,
  PRIMARY KEY (`LeagueID`,`Year`,`MasterGameID`),
  KEY `FK_tbl_league_eligibilityoverride_tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_league_eligibilityoverride_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`),
  CONSTRAINT `FK_tbl_league_eligibilityoverride_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_formerpublishergame
CREATE TABLE IF NOT EXISTS `tbl_league_formerpublishergame` (
  `PublisherGameID` char(36) NOT NULL,
  `PublisherID` char(36) NOT NULL,
  `GameName` varchar(150) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `CounterPick` bit(1) NOT NULL,
  `ManualCriticScore` decimal(7,4) DEFAULT NULL,
  `ManualWillNotRelease` bit(1) NOT NULL,
  `FantasyPoints` decimal(12,4) DEFAULT NULL,
  `MasterGameID` char(36) DEFAULT NULL,
  `DraftPosition` tinyint(3) DEFAULT NULL,
  `OverallDraftPosition` smallint(5) DEFAULT NULL,
  `BidAmount` smallint(5) DEFAULT NULL,
  `AcquiredInTradeID` char(36) DEFAULT NULL,
  `RemovedTimestamp` datetime NOT NULL,
  `RemovedNote` text NOT NULL,
  PRIMARY KEY (`PublisherGameID`) USING BTREE,
  KEY `FK_tbl_league_formerpublishergame_tbl_league_publisher` (`PublisherID`) USING BTREE,
  KEY `FK_tbl_league_formerpublishergame_tbl_mastergame` (`MasterGameID`) USING BTREE,
  KEY `FK_tbl_league_formerpublishergame_tbl_league_trade` (`AcquiredInTradeID`),
  CONSTRAINT `FK_tbl_league_formerpublishergame_tbl_league_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_formerpublishergame_tbl_league_trade` FOREIGN KEY (`AcquiredInTradeID`) REFERENCES `tbl_league_trade` (`TradeID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_formerpublishergame_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_hasuser
CREATE TABLE IF NOT EXISTS `tbl_league_hasuser` (
  `LeagueID` char(36) NOT NULL,
  `UserID` char(36) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Archived` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`LeagueID`,`UserID`),
  KEY `FK_tblleaguehasuser_tbluser` (`UserID`),
  CONSTRAINT `FK_tblleaguehasuser_tblleague` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`),
  CONSTRAINT `FK_tblleaguehasuser_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_invite
CREATE TABLE IF NOT EXISTS `tbl_league_invite` (
  `InviteID` char(36) NOT NULL,
  `LeagueID` char(36) NOT NULL,
  `EmailAddress` varchar(255) NOT NULL,
  `UserID` char(36) DEFAULT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InviteID`),
  UNIQUE KEY `unique_league_email` (`LeagueID`,`EmailAddress`),
  UNIQUE KEY `unique_league_user` (`LeagueID`,`UserID`),
  CONSTRAINT `FK_tblleagueinvite_tblleague` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_invitelink
CREATE TABLE IF NOT EXISTS `tbl_league_invitelink` (
  `InviteID` char(36) NOT NULL,
  `LeagueID` char(36) NOT NULL,
  `InviteCode` char(36) NOT NULL,
  `Active` bit(1) NOT NULL DEFAULT b'0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`InviteID`),
  UNIQUE KEY `Unique_League_Code` (`LeagueID`,`InviteCode`),
  CONSTRAINT `FK_tbl_league_invitelink_tbL_league` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_managermessage
CREATE TABLE IF NOT EXISTS `tbl_league_managermessage` (
  `MessageID` char(36) NOT NULL,
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `MessageText` text NOT NULL,
  `IsPublic` bit(1) NOT NULL DEFAULT b'0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Deleted` bit(1) NOT NULL,
  PRIMARY KEY (`MessageID`) USING BTREE,
  KEY `FK__tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  CONSTRAINT `FK__tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_managermessagedismissal
CREATE TABLE IF NOT EXISTS `tbl_league_managermessagedismissal` (
  `MessageID` char(36) NOT NULL,
  `UserID` char(36) NOT NULL,
  PRIMARY KEY (`MessageID`,`UserID`) USING BTREE,
  KEY `FK_tbl_league_managermessagedismissal_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_managermessagedismissal_tbl_league_managermessage` FOREIGN KEY (`MessageID`) REFERENCES `tbl_league_managermessage` (`MessageID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_managermessagedismissal_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_pickupbid
CREATE TABLE IF NOT EXISTS `tbl_league_pickupbid` (
  `BidID` char(36) NOT NULL,
  `PublisherID` char(36) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `ConditionalDropMasterGameID` char(36) DEFAULT NULL,
  `Counterpick` bit(1) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `Priority` int(11) NOT NULL,
  `BidAmount` int(11) unsigned NOT NULL,
  `Successful` bit(1) DEFAULT NULL,
  `ProcessSetID` char(36) DEFAULT NULL,
  `Outcome` text,
  `ProjectedPointsAtTimeOfBid` decimal(12,4) DEFAULT NULL,
  PRIMARY KEY (`BidID`),
  KEY `FK_tblacquisitionbid_tblpublisher` (`PublisherID`),
  KEY `FK_tblacquisitionbid_tblmastergame` (`MasterGameID`),
  KEY `FK_tbl_league_pickupbid_tbl_mastergame` (`ConditionalDropMasterGameID`),
  CONSTRAINT `FK_tbl_league_pickupbid_tbl_mastergame` FOREIGN KEY (`ConditionalDropMasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `tbl_league_pickupbid_ibfk_1` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `tbl_league_pickupbid_ibfk_2` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_publisher
CREATE TABLE IF NOT EXISTS `tbl_league_publisher` (
  `PublisherID` char(36) NOT NULL,
  `PublisherName` varchar(100) DEFAULT NULL,
  `PublisherIcon` varchar(255) DEFAULT NULL,
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `UserID` char(36) NOT NULL,
  `DraftPosition` tinyint(4) DEFAULT NULL,
  `Budget` int(10) unsigned NOT NULL,
  `FreeGamesDropped` int(11) DEFAULT NULL,
  `WillNotReleaseGamesDropped` int(11) DEFAULT NULL,
  `WillReleaseGamesDropped` int(11) DEFAULT NULL,
  `AutoDraft` bit(1) NOT NULL DEFAULT b'0',
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`PublisherID`),
  UNIQUE KEY `Unique_League_Year_User` (`LeagueID`,`Year`,`UserID`),
  UNIQUE KEY `Unique_League_Year_DraftPosition` (`Year`,`LeagueID`,`DraftPosition`),
  KEY `FK_tblpublisher_tblleaguehasuser` (`LeagueID`,`UserID`),
  CONSTRAINT `FK_tblpublisher_tblleaguehasuser` FOREIGN KEY (`LeagueID`, `UserID`) REFERENCES `tbl_league_hasuser` (`LeagueID`, `UserID`),
  CONSTRAINT `FK_tblpublisher_tblleagueyear` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_publishergame
CREATE TABLE IF NOT EXISTS `tbl_league_publishergame` (
  `PublisherGameID` char(36) NOT NULL,
  `PublisherID` char(36) NOT NULL,
  `GameName` varchar(150) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `CounterPick` bit(1) NOT NULL,
  `ManualCriticScore` decimal(7,4) DEFAULT NULL,
  `ManualWillNotRelease` bit(1) NOT NULL DEFAULT b'0',
  `FantasyPoints` decimal(12,4) DEFAULT NULL,
  `MasterGameID` char(36) DEFAULT NULL,
  `DraftPosition` tinyint(4) DEFAULT NULL,
  `OverallDraftPosition` smallint(6) DEFAULT NULL,
  `BidAmount` smallint(5) DEFAULT NULL,
  `AcquiredInTradeID` char(36) DEFAULT NULL,
  `SlotNumber` int(11) NOT NULL,
  PRIMARY KEY (`PublisherGameID`),
  UNIQUE KEY `UNQ_Slot` (`PublisherID`,`CounterPick`,`SlotNumber`),
  KEY `FK_tblpublishergame_tblmastergame` (`MasterGameID`),
  KEY `FK_tbl_league_publishergame_tbl_league_trade` (`AcquiredInTradeID`),
  CONSTRAINT `FK_tbl_league_publishergame_tbl_league_trade` FOREIGN KEY (`AcquiredInTradeID`) REFERENCES `tbl_league_trade` (`TradeID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tblpublishergame_tblmastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tblpublishergame_tblpublisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_publisherqueue
CREATE TABLE IF NOT EXISTS `tbl_league_publisherqueue` (
  `PublisherID` char(36) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `Rank` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`PublisherID`,`MasterGameID`),
  KEY `FK_tbl_league_publisherqueue_tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_league_publisherqueue_tbl_league_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`),
  CONSTRAINT `FK_tbl_league_publisherqueue_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_specialgameslot
CREATE TABLE IF NOT EXISTS `tbl_league_specialgameslot` (
  `SpecialSlotID` char(36) NOT NULL,
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `SpecialSlotPosition` int(10) NOT NULL,
  `Tag` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`SpecialSlotID`) USING BTREE,
  KEY `FK_tbl_league_gameslotusestag_tbl_mastergame_tag` (`Tag`) USING BTREE,
  KEY `FK_tbl_league_specialgameslot_tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  CONSTRAINT `FK_tbl_league_gameslotusestag_tbl_mastergame_tag` FOREIGN KEY (`Tag`) REFERENCES `tbl_mastergame_tag` (`Name`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_specialgameslot_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_tagoverride
CREATE TABLE IF NOT EXISTS `tbl_league_tagoverride` (
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `TagName` varchar(255) NOT NULL,
  PRIMARY KEY (`LeagueID`,`Year`,`MasterGameID`,`TagName`) USING BTREE,
  KEY `FK_tbl_league_tagoverride_tbl_mastergame` (`MasterGameID`) USING BTREE,
  KEY `FK_tbl_league_tagoverride_tbl_mastergame_tag` (`TagName`) USING BTREE,
  CONSTRAINT `FK_tbl_league_tagoverride_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_tagoverride_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_tagoverride_tbl_mastergame_tag` FOREIGN KEY (`TagName`) REFERENCES `tbl_mastergame_tag` (`Name`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_trade
CREATE TABLE IF NOT EXISTS `tbl_league_trade` (
  `TradeID` char(36) NOT NULL,
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `ProposerPublisherID` char(36) NOT NULL,
  `CounterPartyPublisherID` char(36) NOT NULL,
  `ProposerBudgetSendAmount` int(10) unsigned NOT NULL,
  `CounterPartyBudgetSendAmount` int(10) unsigned NOT NULL,
  `Message` text NOT NULL,
  `ProposedTimestamp` datetime NOT NULL,
  `AcceptedTimestamp` datetime DEFAULT NULL,
  `CompletedTimestamp` datetime DEFAULT NULL,
  `Status` varchar(255) NOT NULL,
  PRIMARY KEY (`TradeID`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_league_year` (`LeagueID`,`Year`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_league_publisher` (`ProposerPublisherID`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_meta_tradestatus` (`Status`) USING BTREE,
  KEY `FK_tbl_league_trade_tbl_league_publisher_2` (`CounterPartyPublisherID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher` FOREIGN KEY (`ProposerPublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher_2` FOREIGN KEY (`CounterPartyPublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_trade_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_trade_tbl_meta_tradestatus` FOREIGN KEY (`Status`) REFERENCES `tbl_meta_tradestatus` (`Status`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_tradecomponent
CREATE TABLE IF NOT EXISTS `tbl_league_tradecomponent` (
  `TradeID` char(36) NOT NULL,
  `CurrentParty` varchar(255) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `CounterPick` bit(1) NOT NULL,
  PRIMARY KEY (`TradeID`,`CurrentParty`,`MasterGameID`,`CounterPick`) USING BTREE,
  KEY `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` (`CurrentParty`) USING BTREE,
  KEY `FK_tbl_league_tradecomponent_tbl_mastergame` (`MasterGameID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_tradecomponent_tbl_league_trade` FOREIGN KEY (`TradeID`) REFERENCES `tbl_league_trade` (`TradeID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_tradecomponent_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` FOREIGN KEY (`CurrentParty`) REFERENCES `tbl_meta_tradingparty` (`Name`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_tradevote
CREATE TABLE IF NOT EXISTS `tbl_league_tradevote` (
  `TradeID` char(36) NOT NULL,
  `UserID` char(36) NOT NULL,
  `Approved` bit(1) NOT NULL,
  `Comment` text,
  `Timestamp` datetime NOT NULL,
  PRIMARY KEY (`TradeID`,`UserID`) USING BTREE,
  KEY `FK_tbl_league_tradevote_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_league_tradevote_tbl_league_trade` FOREIGN KEY (`TradeID`) REFERENCES `tbl_league_trade` (`TradeID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_tradevote_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_year
CREATE TABLE IF NOT EXISTS `tbl_league_year` (
  `LeagueID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `StandardGames` tinyint(4) NOT NULL,
  `GamesToDraft` tinyint(4) NOT NULL,
  `CounterPicks` tinyint(4) NOT NULL,
  `CounterPicksToDraft` tinyint(3) NOT NULL,
  `FreeDroppableGames` tinyint(4) NOT NULL DEFAULT '0',
  `WillNotReleaseDroppableGames` tinyint(4) NOT NULL DEFAULT '0',
  `WillReleaseDroppableGames` tinyint(4) NOT NULL DEFAULT '0',
  `DropOnlyDraftGames` bit(1) NOT NULL DEFAULT b'0',
  `CounterPicksBlockDrops` bit(1) NOT NULL DEFAULT b'0',
  `MinimumBidAmount` tinyint(4) NOT NULL DEFAULT '0',
  `DraftSystem` varchar(50) NOT NULL,
  `PickupSystem` varchar(50) NOT NULL,
  `ScoringSystem` varchar(50) NOT NULL,
  `PlayStatus` varchar(50) NOT NULL,
  `TradingSystem` varchar(50) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `DraftStartedTimestamp` timestamp NULL DEFAULT NULL,
  `WinningUserID` char(36) DEFAULT NULL,
  PRIMARY KEY (`Year`,`LeagueID`),
  KEY `FK_tblleagueyear_tblleague` (`LeagueID`),
  KEY `FK_tblleagueyear_tbldraftsystem` (`DraftSystem`),
  KEY `FK_tblleagueyear_tblwaiversystem` (`PickupSystem`),
  KEY `FK_tblleagueyear_tblscoringsystem` (`ScoringSystem`),
  KEY `FK_tblleagueyear_tblplaystatus` (`PlayStatus`),
  KEY `FK_tbl_league_year_tbl_settings_tradingsystem` (`TradingSystem`),
  KEY `FK_tbl_league_year_tbl_user` (`WinningUserID`),
  CONSTRAINT `FK_tbl_league_year_tbl_settings_tradingsystem` FOREIGN KEY (`TradingSystem`) REFERENCES `tbl_settings_tradingsystem` (`TradingSystem`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_year_tbl_user` FOREIGN KEY (`WinningUserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `tbl_league_year_ibfk_1` FOREIGN KEY (`DraftSystem`) REFERENCES `tbl_settings_draftsystem` (`DraftSystem`),
  CONSTRAINT `tbl_league_year_ibfk_3` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`),
  CONSTRAINT `tbl_league_year_ibfk_4` FOREIGN KEY (`PlayStatus`) REFERENCES `tbl_settings_playstatus` (`Name`),
  CONSTRAINT `tbl_league_year_ibfk_5` FOREIGN KEY (`ScoringSystem`) REFERENCES `tbl_settings_scoringsystem` (`ScoringSystem`),
  CONSTRAINT `tbl_league_year_ibfk_6` FOREIGN KEY (`Year`) REFERENCES `tbl_meta_supportedyear` (`Year`),
  CONSTRAINT `tbl_league_year_ibfk_7` FOREIGN KEY (`PickupSystem`) REFERENCES `tbl_settings_pickupsystem` (`PickupSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_league_yearusestag
CREATE TABLE IF NOT EXISTS `tbl_league_yearusestag` (
  `LeagueID` char(36) NOT NULL DEFAULT '',
  `Year` year(4) NOT NULL,
  `Tag` varchar(255) NOT NULL DEFAULT '',
  `Status` varchar(50) NOT NULL,
  PRIMARY KEY (`LeagueID`,`Year`,`Tag`) USING BTREE,
  KEY `FK_tbl_league_yearusestag_tbl_mastergame_tag` (`Tag`) USING BTREE,
  KEY `FK_tbl_league_yearusestag_tbl_settings_tagoption` (`Status`) USING BTREE,
  CONSTRAINT `FK_tbl_league_yearusestag_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_yearusestag_tbl_mastergame_tag` FOREIGN KEY (`Tag`) REFERENCES `tbl_mastergame_tag` (`Name`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_league_yearusestag_tbl_settings_tagoption` FOREIGN KEY (`Status`) REFERENCES `tbl_settings_tagstatus` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame
CREATE TABLE IF NOT EXISTS `tbl_mastergame` (
  `MasterGameID` char(36) NOT NULL,
  `GameName` varchar(255) NOT NULL,
  `EstimatedReleaseDate` varchar(255) NOT NULL,
  `MinimumReleaseDate` date NOT NULL,
  `MaximumReleaseDate` date DEFAULT NULL,
  `EarlyAccessReleaseDate` date DEFAULT NULL,
  `InternationalReleaseDate` date DEFAULT NULL,
  `AnnouncementDate` date DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `OpenCriticID` int(11) DEFAULT NULL,
  `GGToken` char(6) DEFAULT NULL,
  `CriticScore` decimal(7,4) DEFAULT NULL,
  `Notes` mediumtext NOT NULL,
  `BoxartFileName` varchar(255) DEFAULT NULL,
  `GGCoverArtFileName` varchar(255) DEFAULT NULL,
  `FirstCriticScoreTimestamp` timestamp NULL DEFAULT NULL,
  `DoNotRefreshDate` bit(1) NOT NULL DEFAULT b'0',
  `DoNotRefreshAnything` bit(1) NOT NULL DEFAULT b'0',
  `EligibilityChanged` bit(1) NOT NULL DEFAULT b'0',
  `DelayContention` bit(1) NOT NULL DEFAULT b'0',
  `AddedTimestamp` timestamp NOT NULL,
  PRIMARY KEY (`MasterGameID`),
  UNIQUE KEY `UniqueName` (`GameName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_changerequest
CREATE TABLE IF NOT EXISTS `tbl_mastergame_changerequest` (
  `RequestID` char(36) NOT NULL,
  `UserID` char(36) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `RequestTimestamp` datetime NOT NULL,
  `RequestNote` text NOT NULL,
  `OpenCriticID` int(11) DEFAULT NULL,
  `GGToken` char(6) DEFAULT NULL,
  `Answered` bit(1) NOT NULL,
  `ResponseTimestamp` datetime DEFAULT NULL,
  `ResponseNote` text,
  `Hidden` bit(1) NOT NULL,
  PRIMARY KEY (`RequestID`),
  KEY `FK_tblmastergamerequest_tbluser` (`UserID`),
  KEY `FK_tblmastergamerequest_tblmastergame` (`MasterGameID`),
  CONSTRAINT `tbl_mastergame_changerequest_ibfk_2` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `tbl_mastergame_changerequest_ibfk_3` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_hastag
CREATE TABLE IF NOT EXISTS `tbl_mastergame_hastag` (
  `MasterGameID` char(36) NOT NULL,
  `TagName` varchar(255) NOT NULL,
  `TimeAdded` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MasterGameID`,`TagName`) USING BTREE,
  KEY `FK_tbl_mastergame_hastag_tbl_mastergame_tag` (`TagName`) USING BTREE,
  CONSTRAINT `FK_tbl_mastergame_hastag_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `FK_tbl_mastergame_hastag_tbl_mastergame_tag` FOREIGN KEY (`TagName`) REFERENCES `tbl_mastergame_tag` (`Name`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_request
CREATE TABLE IF NOT EXISTS `tbl_mastergame_request` (
  `RequestID` char(36) NOT NULL,
  `UserID` char(36) NOT NULL,
  `RequestTimestamp` datetime NOT NULL,
  `RequestNote` text NOT NULL,
  `GameName` varchar(255) NOT NULL,
  `SteamID` int(11) DEFAULT NULL,
  `OpenCriticID` int(11) DEFAULT NULL,
  `GGToken` char(6) DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `EstimatedReleaseDate` varchar(255) DEFAULT NULL,
  `Answered` bit(1) NOT NULL,
  `ResponseTimestamp` datetime DEFAULT NULL,
  `ResponseNote` text,
  `MasterGameID` char(36) DEFAULT NULL,
  `Hidden` bit(1) NOT NULL,
  PRIMARY KEY (`RequestID`),
  KEY `FK_tblmastergamerequest_tbluser` (`UserID`),
  KEY `FK_tblmastergamerequest_tblmastergame` (`MasterGameID`),
  CONSTRAINT `FK_tblmastergamerequest_tblmastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tblmastergamerequest_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_subgame
CREATE TABLE IF NOT EXISTS `tbl_mastergame_subgame` (
  `MasterSubGameID` char(36) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `GameName` varchar(150) NOT NULL,
  `EstimatedReleaseDate` varchar(150) NOT NULL,
  `MinimumReleaseDate` date NOT NULL,
  `MaximumReleaseDate` date DEFAULT NULL,
  `ReleaseDate` date DEFAULT NULL,
  `OpenCriticID` int(11) DEFAULT NULL,
  `CriticScore` decimal(7,4) DEFAULT NULL,
  PRIMARY KEY (`MasterSubGameID`),
  KEY `FK_tblmastersubgame_tblmastergame` (`MasterGameID`),
  CONSTRAINT `FK_tblmastersubgame_tblmastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_tag
CREATE TABLE IF NOT EXISTS `tbl_mastergame_tag` (
  `Name` varchar(255) NOT NULL,
  `ReadableName` varchar(255) NOT NULL,
  `ShortName` varchar(255) NOT NULL,
  `TagType` varchar(255) NOT NULL,
  `HasCustomCode` bit(1) NOT NULL DEFAULT b'0',
  `SystemTagOnly` bit(1) NOT NULL DEFAULT b'0',
  `Description` text NOT NULL,
  `Examples` json NOT NULL,
  `BadgeColor` char(6) NOT NULL,
  PRIMARY KEY (`Name`) USING BTREE,
  KEY `FK_tbl_mastergame_tag_tbl_mastergame_tagtype` (`TagType`) USING BTREE,
  CONSTRAINT `FK_tbl_mastergame_tag_tbl_mastergame_tagtype` FOREIGN KEY (`TagType`) REFERENCES `tbl_mastergame_tagtype` (`Name`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_mastergame_tagtype
CREATE TABLE IF NOT EXISTS `tbl_mastergame_tagtype` (
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`Name`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_actionprocessingset
CREATE TABLE IF NOT EXISTS `tbl_meta_actionprocessingset` (
  `ProcessSetID` char(36) NOT NULL,
  `ProcessTime` datetime NOT NULL,
  `ProcessName` varchar(255) NOT NULL,
  PRIMARY KEY (`ProcessSetID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_quarters
CREATE TABLE IF NOT EXISTS `tbl_meta_quarters` (
  `Quarter` tinyint(4) NOT NULL,
  PRIMARY KEY (`Quarter`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_supportedyear
CREATE TABLE IF NOT EXISTS `tbl_meta_supportedyear` (
  `Year` year(4) NOT NULL,
  `OpenForCreation` bit(1) NOT NULL,
  `OpenForPlay` bit(1) NOT NULL,
  `OpenForBetaUsers` bit(1) NOT NULL,
  `StartDate` date NOT NULL,
  `Finished` bit(1) NOT NULL,
  PRIMARY KEY (`Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_systemwidesettings
CREATE TABLE IF NOT EXISTS `tbl_meta_systemwidesettings` (
  `ActionProcessingMode` bit(1) NOT NULL,
  `RefreshOpenCritic` bit(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_tradestatus
CREATE TABLE IF NOT EXISTS `tbl_meta_tradestatus` (
  `Status` varchar(255) NOT NULL,
  PRIMARY KEY (`Status`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_meta_tradingparty
CREATE TABLE IF NOT EXISTS `tbl_meta_tradingparty` (
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`Name`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_royale_publisher
CREATE TABLE IF NOT EXISTS `tbl_royale_publisher` (
  `PublisherID` char(36) NOT NULL,
  `UserID` char(36) NOT NULL,
  `Year` year(4) NOT NULL,
  `Quarter` tinyint(4) NOT NULL DEFAULT '0',
  `PublisherName` varchar(255) NOT NULL DEFAULT '0',
  `PublisherIcon` varchar(255) DEFAULT NULL,
  `Budget` decimal(11,1) unsigned NOT NULL DEFAULT '0.0',
  PRIMARY KEY (`PublisherID`),
  KEY `FK_tbl_royale_publisher_tbl_user` (`UserID`),
  KEY `FK_tbl_royale_publisher_tbl_royale_supportedquarter` (`Year`,`Quarter`),
  CONSTRAINT `FK_tbl_royale_publisher_tbl_royale_supportedquarter` FOREIGN KEY (`Year`, `Quarter`) REFERENCES `tbl_royale_supportedquarter` (`Year`, `Quarter`),
  CONSTRAINT `FK_tbl_royale_publisher_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_royale_publishergame
CREATE TABLE IF NOT EXISTS `tbl_royale_publishergame` (
  `PublisherID` char(36) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `AmountSpent` decimal(11,2) NOT NULL DEFAULT '0.00',
  `AdvertisingMoney` decimal(11,2) NOT NULL DEFAULT '0.00',
  `FantasyPoints` decimal(12,4) DEFAULT '0.0000',
  PRIMARY KEY (`PublisherID`,`MasterGameID`),
  KEY `FK_tbl_royale_publishergame_tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_royale_publishergame_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`),
  CONSTRAINT `FK_tbl_royale_publishergame_tbl_royale_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_royale_publisher` (`PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_royale_supportedquarter
CREATE TABLE IF NOT EXISTS `tbl_royale_supportedquarter` (
  `Year` year(4) NOT NULL,
  `Quarter` tinyint(4) NOT NULL,
  `OpenForPlay` bit(1) DEFAULT NULL,
  `Finished` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Year`,`Quarter`),
  KEY `FK_tbl_royale_supportedquarter_tbl_meta_quarters` (`Quarter`),
  CONSTRAINT `FK_tbl_royale_supportedquarter_tbl_meta_quarters` FOREIGN KEY (`Quarter`) REFERENCES `tbl_meta_quarters` (`Quarter`),
  CONSTRAINT `FK_tbl_royale_supportedquarter_tbl_meta_supportedyear` FOREIGN KEY (`Year`) REFERENCES `tbl_meta_supportedyear` (`Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_draftsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_draftsystem` (
  `DraftSystem` varchar(50) NOT NULL,
  PRIMARY KEY (`DraftSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_pickupsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_pickupsystem` (
  `PickupSystem` varchar(50) NOT NULL,
  PRIMARY KEY (`PickupSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_playstatus
CREATE TABLE IF NOT EXISTS `tbl_settings_playstatus` (
  `Name` varchar(50) NOT NULL,
  PRIMARY KEY (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_scoringsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_scoringsystem` (
  `ScoringSystem` varchar(50) NOT NULL,
  PRIMARY KEY (`ScoringSystem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_tagstatus
CREATE TABLE IF NOT EXISTS `tbl_settings_tagstatus` (
  `Status` varchar(50) NOT NULL,
  PRIMARY KEY (`Status`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_settings_tradingsystem
CREATE TABLE IF NOT EXISTS `tbl_settings_tradingsystem` (
  `TradingSystem` varchar(50) NOT NULL,
  PRIMARY KEY (`TradingSystem`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user
CREATE TABLE IF NOT EXISTS `tbl_user` (
  `UserID` char(36) NOT NULL,
  `DisplayName` varchar(255) NOT NULL,
  `PatreonDonorNameOverride` varchar(255) DEFAULT NULL,
  `DisplayNumber` smallint(6) DEFAULT NULL,
  `EmailAddress` varchar(255) NOT NULL,
  `NormalizedEmailAddress` varchar(255) NOT NULL,
  `PasswordHash` varchar(255) DEFAULT NULL,
  `SecurityStamp` varchar(255) NOT NULL,
  `TwoFactorEnabled` bit(1) NOT NULL,
  `AuthenticatorKey` varchar(255) DEFAULT NULL,
  `LastChangedCredentials` datetime NOT NULL,
  `EmailConfirmed` bit(1) NOT NULL,
  `AccountCreated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` bit(1) NOT NULL,
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `Index 3` (`NormalizedEmailAddress`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_donorname
CREATE TABLE IF NOT EXISTS `tbl_user_donorname` (
  `UserID` char(36) NOT NULL,
  `DonorName` varchar(255) NOT NULL,
  PRIMARY KEY (`UserID`) USING BTREE,
  CONSTRAINT `tbl_user_donorname_ibfk_2` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_emailsettings
CREATE TABLE IF NOT EXISTS `tbl_user_emailsettings` (
  `UserID` char(36) NOT NULL,
  `EmailType` varchar(255) NOT NULL,
  PRIMARY KEY (`UserID`,`EmailType`) USING BTREE,
  KEY `FK_tbl_user_emailsettings_tbl_user_emailtype` (`EmailType`) USING BTREE,
  CONSTRAINT `FK_tbl_user_emailsettings_tbl_user_emailtype` FOREIGN KEY (`EmailType`) REFERENCES `tbl_user_emailtype` (`EmailType`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `tbl_user_emailsettings_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_emailtype
CREATE TABLE IF NOT EXISTS `tbl_user_emailtype` (
  `EmailType` varchar(255) NOT NULL,
  `ReadableName` varchar(255) NOT NULL,
  PRIMARY KEY (`EmailType`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_externallogin
CREATE TABLE IF NOT EXISTS `tbl_user_externallogin` (
  `LoginProvider` varchar(255) NOT NULL,
  `ProviderKey` varchar(255) NOT NULL,
  `UserID` char(36) NOT NULL,
  `ProviderDisplayName` varchar(255) NOT NULL,
  `TimeAdded` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`) USING BTREE,
  UNIQUE KEY `UNQ_Login` (`LoginProvider`,`ProviderKey`,`UserID`) USING BTREE,
  KEY `FK_tbl_user_externallogin_tbl_user` (`UserID`) USING BTREE,
  CONSTRAINT `FK_tbl_user_externallogin_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_followingleague
CREATE TABLE IF NOT EXISTS `tbl_user_followingleague` (
  `UserID` char(36) NOT NULL,
  `LeagueID` char(36) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserID`,`LeagueID`),
  KEY `FK_tbluserfollowingleague_tblleague` (`LeagueID`),
  CONSTRAINT `FK_tbluserfollowingleague_tblleague` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`),
  CONSTRAINT `FK_tbluserfollowingleague_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_hasrole
CREATE TABLE IF NOT EXISTS `tbl_user_hasrole` (
  `UserID` char(36) NOT NULL,
  `RoleID` tinyint(4) NOT NULL,
  `ProgrammaticallyAssigned` bit(1) NOT NULL,
  PRIMARY KEY (`UserID`,`RoleID`),
  KEY `FK_tbluserhasrole_tblrole` (`RoleID`),
  CONSTRAINT `FK_tbluserhasrole_tblrole` FOREIGN KEY (`RoleID`) REFERENCES `tbl_user_role` (`RoleID`),
  CONSTRAINT `FK_tbluserhasrole_tbluser` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_persistedgrant
CREATE TABLE IF NOT EXISTS `tbl_user_persistedgrant` (
  `Key` varchar(255) NOT NULL,
  `Type` varchar(255) NOT NULL,
  `SubjectId` varchar(255) NOT NULL,
  `ClientId` varchar(255) NOT NULL,
  `CreationTime` datetime NOT NULL,
  `ConsumedTime` datetime DEFAULT NULL,
  `Expiration` datetime DEFAULT NULL,
  `Data` text NOT NULL,
  `Description` text,
  `SessionId` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Key`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_recoverycode
CREATE TABLE IF NOT EXISTS `tbl_user_recoverycode` (
  `UserID` char(36) NOT NULL,
  `RecoveryCode` varchar(255) NOT NULL,
  `CreatedTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserID`,`RecoveryCode`) USING BTREE,
  CONSTRAINT `tbl_user_recoverycode_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Data exporting was unselected.

-- Dumping structure for table fantasycritic.tbl_user_role
CREATE TABLE IF NOT EXISTS `tbl_user_role` (
  `RoleID` tinyint(4) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `NormalizedName` varchar(50) NOT NULL,
  PRIMARY KEY (`RoleID`),
  UNIQUE KEY `Index 2` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.


CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league` AS select `tbl_league`.`LeagueID` AS `LeagueID`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_league`.`LeagueManager` AS `LeagueManager`,`tbl_league`.`PublicLeague` AS `PublicLeague`,`tbl_league`.`TestLeague` AS `TestLeague`,`tbl_league`.`Timestamp` AS `Timestamp`,count(`tbl_user_followingleague`.`UserID`) AS `NumberOfFollowers`,`tbl_league`.`IsDeleted` AS `IsDeleted` from (`tbl_league` left join `tbl_user_followingleague` on((`tbl_league`.`LeagueID` = `tbl_user_followingleague`.`LeagueID`))) group by `tbl_league`.`LeagueID`;

CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_droprequest` AS select `tbl_league_droprequest`.`DropRequestID` AS `DropRequestID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_droprequest`.`Successful` AS `Successful`,`tbl_league_droprequest`.`Timestamp` AS `Timestamp`,`tbl_league_droprequest`.`ProcessSetID` AS `ProcessSetID`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_droprequest` join `tbl_league_publisher` on((`tbl_league_droprequest`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_droprequest`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));

CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_pickupbid` AS select `tbl_league_pickupbid`.`BidID` AS `BidID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_league_pickupbid`.`MasterGameID` AS `MasterGameID`,`pickupMasterGame`.`GameName` AS `GameName`,`tbl_league_pickupbid`.`ConditionalDropMasterGameID` AS `ConditionalDropMasterGameID`,`conditionalDropMasterGame`.`GameName` AS `ConditionalDropGameName`,`tbl_league_pickupbid`.`Counterpick` AS `CounterPick`,`tbl_league_pickupbid`.`Priority` AS `Priority`,`tbl_league_pickupbid`.`BidAmount` AS `BidAmount`,`tbl_league_pickupbid`.`Successful` AS `Successful`,`tbl_league_pickupbid`.`Timestamp` AS `Timestamp`,`tbl_league_pickupbid`.`ProcessSetID` AS `ProcessSetID`,`tbl_league_pickupbid`.`Outcome` AS `Outcome`,`tbl_league_pickupbid`.`ProjectedPointsAtTimeOfBid` AS `ProjectedPointsAtTimeOfBid`,`tbl_league`.`IsDeleted` AS `IsDeleted` from (((((`tbl_league_pickupbid` join `tbl_league_publisher` on((`tbl_league_pickupbid`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` `pickupMasterGame` on((`tbl_league_pickupbid`.`MasterGameID` = `pickupMasterGame`.`MasterGameID`))) left join `tbl_mastergame` `conditionalDropMasterGame` on((`tbl_league_pickupbid`.`ConditionalDropMasterGameID` = `conditionalDropMasterGame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));

CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_mastergame_statisticsinput` AS select `tbl_caching_mastergameyear`.`Year` AS `YEAR`,`tbl_caching_mastergameyear`.`MasterGameID` AS `MasterGameID`,`tbl_caching_mastergameyear`.`GameName` AS `GameName`,`tbl_caching_mastergameyear`.`DateAdjustedHypeFactor` AS `DateAdjustedHypeFactor`,`tbl_caching_mastergameyear`.`TotalBidAmount` AS `TotalBidAmount`,`tbl_caching_mastergameyear`.`BidPercentile` AS `BidPercentile`,`tbl_caching_mastergameyear`.`EligiblePercentCounterPick` AS `EligiblePercentCounterPick`,ifnull(`tbl_caching_mastergameyear`.`CriticScore`,70) AS `CriticScore` from `tbl_caching_mastergameyear` where ((`tbl_caching_mastergameyear`.`Year` = 2019) and (isnull(`tbl_caching_mastergameyear`.`ReleaseDate`) or (`tbl_caching_mastergameyear`.`ReleaseDate` >= '2019-01-01')) and ((`tbl_caching_mastergameyear`.`CriticScore` is not null) or (year(`tbl_caching_mastergameyear`.`MinimumReleaseDate`) > 2019)) and (`tbl_caching_mastergameyear`.`DateAdjustedHypeFactor` > 0)) order by `tbl_caching_mastergameyear`.`DateAdjustedHypeFactor` desc;

CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_meta_sitecounts` AS select `userTable`.`UserCount` AS `UserCount`,`leagueTable`.`LeagueCount` AS `LeagueCount`,`masterGameTable`.`MasterGameCount` AS `MasterGameCount`,`publisherGameTable`.`PublisherGameCount` AS `PublisherGameCount` from (((((select count(0) AS `UserCount` from `fantasycritic`.`tbl_user` where (`fantasycritic`.`tbl_user`.`IsDeleted` = 0))) `userTable` join (select count(0) AS `LeagueCount` from `fantasycritic`.`tbl_league`) `leagueTable`) join (select count(0) AS `MasterGameCount` from `fantasycritic`.`tbl_mastergame`) `masterGameTable`) join (select count(0) AS `PublisherGameCount` from `fantasycritic`.`tbl_league_publishergame`) `publisherGameTable`);