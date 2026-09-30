CREATE TABLE `tbl_discord_pendingmastergameupdatetype` (
  `UpdateType` varchar(50) NOT NULL,
  PRIMARY KEY (`UpdateType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `tbl_discord_pendingmastergameupdatetype` (`UpdateType`) VALUES
	('Edit'),
	('NewGame'),
	('ScoreUpdate');

-- Game news waiting to go to Discord. Web and the worker write rows, and the worker sends and deletes them at the end of RefreshCaches.
-- Each row carries JSON snapshots of the game as it was when queued, so there is no FK on MasterGameID:
-- merging games and the beta sync both delete master games.
CREATE TABLE `tbl_discord_pendingmastergameupdate` (
  `PendingUpdateID` char(36) NOT NULL,
  `UpdateType` varchar(50) NOT NULL,
  `MasterGameID` char(36) NOT NULL,
  `MasterGameSnapshot` json NOT NULL,
  `EditedMasterGameSnapshot` json DEFAULT NULL,
  `Year` int DEFAULT NULL,
  `OldCriticScore` decimal(7,4) DEFAULT NULL,
  `NewCriticScore` decimal(7,4) DEFAULT NULL,
  `Changes` json DEFAULT NULL,
  `QueuedTimestamp` timestamp(6) NOT NULL,
  PRIMARY KEY (`PendingUpdateID`),
  KEY `FK_tbl_discord_pendingmastergameupdate_type` (`UpdateType`),
  KEY `IX_tbl_discord_pendingmastergameupdate_queuedtimestamp` (`QueuedTimestamp`),
  CONSTRAINT `FK_tbl_discord_pendingmastergameupdate_type` FOREIGN KEY (`UpdateType`) REFERENCES `tbl_discord_pendingmastergameupdatetype` (`UpdateType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
