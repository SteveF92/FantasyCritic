CREATE TABLE IF NOT EXISTS `tbl_league_draft` (
  `DraftID` char(36) NOT NULL,
  `LeagueID` char(36) NOT NULL,
  `Year` year NOT NULL,
  `DraftNumber` tinyint NOT NULL,
  `Name` varchar(255) NOT NULL DEFAULT 'Initial Draft',
  `ScheduledDate` date NULL,
  `GamesToDraft` tinyint NOT NULL,
  `CounterPicksToDraft` tinyint NOT NULL,
  `DraftOrderSet` bit(1) NOT NULL,
  `PlayStatus` varchar(50) NOT NULL,
  `DraftStartedTimestamp` timestamp NULL,
  PRIMARY KEY (`DraftID`),
  UNIQUE KEY `UNQ_League_Year_DraftNumber` (`LeagueID`,`Year`,`DraftNumber`),
  KEY `FK_tbl_league_draft_tbl_league_year` (`LeagueID`,`Year`),
  KEY `FK_tbl_league_draft_tbl_settings_playstatus` (`PlayStatus`),
  CONSTRAINT `FK_tbl_league_draft_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `tbl_league_year` (`LeagueID`, `Year`),
  CONSTRAINT `FK_tbl_league_draft_tbl_settings_playstatus` FOREIGN KEY (`PlayStatus`) REFERENCES `tbl_settings_playstatus` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

ALTER TABLE `tbl_league_year`
  ADD COLUMN `EnableBids` bit(1) NOT NULL DEFAULT b'1' AFTER `UnderReview`;

UPDATE `tbl_league_year`
SET `EnableBids` = 0
WHERE `StandardGames` = `GamesToDraft`
  AND `CounterPicks` = `CounterPicksToDraft`
  AND `UnrestrictedReleaseStatusDroppableGames` = 0
  AND `WillNotReleaseDroppableGames` = 0
  AND `WillReleaseDroppableGames` = 0
  AND `GrantSuperDrops` = 0
  AND `TradingSystem` = 'NoTrades';

ALTER TABLE `tbl_league_year`
  MODIFY COLUMN `EnableBids` bit(1) NOT NULL;

INSERT INTO `tbl_league_draft` (`DraftID`, `LeagueID`, `Year`, `DraftNumber`, `Name`, `ScheduledDate`, `GamesToDraft`, `CounterPicksToDraft`, `DraftOrderSet`, `PlayStatus`, `DraftStartedTimestamp`)
SELECT UUID(), `LeagueID`, `Year`, 1, 'Initial Draft', DATE(`DraftStartedTimestamp`), `GamesToDraft`, `CounterPicksToDraft`, `DraftOrderSet`, `PlayStatus`, `DraftStartedTimestamp`
FROM `tbl_league_year`;

ALTER TABLE `tbl_league_draft`
  MODIFY COLUMN `Name` varchar(255) NOT NULL;

ALTER TABLE `tbl_league_draft`
  ADD UNIQUE INDEX `UNQ_DraftID_LeagueID_Year` (`DraftID`, `LeagueID`, `Year`);

ALTER TABLE `tbl_league_publisher`
  ADD UNIQUE INDEX `UNQ_PublisherID_LeagueID_Year` (`PublisherID`, `LeagueID`, `Year`);

CREATE TABLE IF NOT EXISTS `tbl_league_draftpublisher` (
  `LeagueID` char(36) NOT NULL,
  `Year` year NOT NULL,
  `DraftID` char(36) NOT NULL,
  `PublisherID` char(36) NOT NULL,
  `DraftPosition` tinyint unsigned NOT NULL,
  PRIMARY KEY (`DraftID`, `PublisherID`),
  UNIQUE KEY `UNQ_Draft_DraftPosition` (`DraftID`, `DraftPosition`),
  KEY `IX_DraftPublisher_Publisher` (`PublisherID`),
  KEY `FK_draftpublisher_draft` (`DraftID`, `LeagueID`, `Year`),
  KEY `FK_draftpublisher_publisher` (`PublisherID`, `LeagueID`, `Year`),
  CONSTRAINT `FK_draftpublisher_draft` FOREIGN KEY (`DraftID`, `LeagueID`, `Year`) REFERENCES `tbl_league_draft` (`DraftID`, `LeagueID`, `Year`),
  CONSTRAINT `FK_draftpublisher_publisher` FOREIGN KEY (`PublisherID`, `LeagueID`, `Year`) REFERENCES `tbl_league_publisher` (`PublisherID`, `LeagueID`, `Year`),
  CONSTRAINT `tbl_league_draftpublisher_chk_1` CHECK (`DraftPosition` > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `tbl_league_draftpublisher` (`LeagueID`, `Year`, `DraftID`, `PublisherID`, `DraftPosition`)
SELECT p.`LeagueID`, p.`Year`, d.`DraftID`, p.`PublisherID`, p.`DraftPosition`
FROM `tbl_league_publisher` p
JOIN `tbl_league_draft` d ON d.`LeagueID` = p.`LeagueID` AND d.`Year` = p.`Year` AND d.`DraftNumber` = 1
WHERE p.`DraftPosition` IS NOT NULL;

CREATE TABLE IF NOT EXISTS `tbl_league_draftpickskip` (
  `DraftID` char(36) NOT NULL,
  `PublisherID` char(36) NOT NULL,
  `CounterPick` bit(1) NOT NULL,
  `PickNumber` int unsigned NOT NULL,
  PRIMARY KEY (`DraftID`, `CounterPick`, `PickNumber`, `PublisherID`),
  KEY `FK_draftpickskip_draftpublisher` (`DraftID`, `PublisherID`),
  CONSTRAINT `FK_draftpickskip_draftpublisher` FOREIGN KEY (`DraftID`, `PublisherID`) REFERENCES `tbl_league_draftpublisher` (`DraftID`, `PublisherID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

ALTER TABLE `tbl_league_publishergame`
  ADD COLUMN `DraftID` char(36) NULL AFTER `SlotNumber`,
  ADD KEY `FK_tbl_league_publishergame_tbl_league_draft` (`DraftID`),
  ADD CONSTRAINT `FK_tbl_league_publishergame_tbl_league_draft` FOREIGN KEY (`DraftID`) REFERENCES `tbl_league_draft` (`DraftID`);

ALTER TABLE `tbl_league_formerpublishergame`
  ADD COLUMN `DraftID` char(36) NULL AFTER `RemovedNote`,
  ADD KEY `FK_tbl_league_formerpublishergame_tbl_league_draft` (`DraftID`),
  ADD CONSTRAINT `FK_tbl_league_formerpublishergame_tbl_league_draft` FOREIGN KEY (`DraftID`) REFERENCES `tbl_league_draft` (`DraftID`);

UPDATE `tbl_league_publishergame` pg
JOIN `tbl_league_publisher` p ON pg.`PublisherID` = p.`PublisherID`
JOIN `tbl_league_draft` d ON d.`LeagueID` = p.`LeagueID` AND d.`Year` = p.`Year` AND d.`DraftNumber` = 1
SET pg.`DraftID` = d.`DraftID`
WHERE pg.`DraftPosition` IS NOT NULL;

UPDATE `tbl_league_formerpublishergame` pg
JOIN `tbl_league_publisher` p ON pg.`PublisherID` = p.`PublisherID`
JOIN `tbl_league_draft` d ON d.`LeagueID` = p.`LeagueID` AND d.`Year` = p.`Year` AND d.`DraftNumber` = 1
SET pg.`DraftID` = d.`DraftID`
WHERE pg.`DraftPosition` IS NOT NULL;

ALTER TABLE `tbl_league_year`
  DROP FOREIGN KEY `tbl_league_year_ibfk_4`;

ALTER TABLE `tbl_league_year`
  DROP COLUMN `GamesToDraft`,
  DROP COLUMN `CounterPicksToDraft`,
  DROP COLUMN `PlayStatus`,
  DROP COLUMN `DraftOrderSet`,
  DROP COLUMN `DraftStartedTimestamp`;

ALTER TABLE `tbl_league_publisher`
  DROP INDEX `Unique_League_Year_DraftPosition`,
  DROP COLUMN `DraftPosition`;
