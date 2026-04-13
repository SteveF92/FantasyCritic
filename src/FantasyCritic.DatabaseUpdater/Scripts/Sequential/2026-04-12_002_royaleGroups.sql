CREATE TABLE IF NOT EXISTS `tbl_royale_group` (
  `GroupID` char(36) NOT NULL,
  `GroupName` varchar(150) NOT NULL,
  `ManagerUserID` char(36) DEFAULT NULL,
  `GroupType` tinyint NOT NULL COMMENT '0=Manual, 1=LeagueTied, 2=RulesBased',
  `LeagueID` char(36) DEFAULT NULL,
  `RuleSetType` varchar(50) DEFAULT NULL,
  `CreatedTimestamp` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`GroupID`),
  UNIQUE KEY `UNQ_League` (`LeagueID`),
  UNIQUE KEY `UNQ_RuleSet` (`RuleSetType`),
  KEY `FK_tbl_royale_group_tbl_user` (`ManagerUserID`),
  CONSTRAINT `FK_tbl_royale_group_tbl_user` FOREIGN KEY (`ManagerUserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_royale_group_tbl_league` FOREIGN KEY (`LeagueID`) REFERENCES `tbl_league` (`LeagueID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `tbl_royale_group_member` (
  `GroupID` char(36) NOT NULL,
  `UserID` char(36) NOT NULL,
  PRIMARY KEY (`GroupID`,`UserID`),
  KEY `FK_tbl_royale_group_member_tbl_user` (`UserID`),
  CONSTRAINT `FK_tbl_royale_group_member_tbl_royale_group` FOREIGN KEY (`GroupID`) REFERENCES `tbl_royale_group` (`GroupID`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_tbl_royale_group_member_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `tbl_royale_group_invitelink` (
  `InviteID` char(36) NOT NULL,
  `GroupID` char(36) NOT NULL,
  `InviteCode` char(36) NOT NULL,
  `Active` bit(1) NOT NULL DEFAULT b'1',
  PRIMARY KEY (`InviteID`),
  UNIQUE KEY `UNQ_InviteCode` (`InviteCode`),
  KEY `FK_tbl_royale_group_invitelink_tbl_royale_group` (`GroupID`),
  CONSTRAINT `FK_tbl_royale_group_invitelink_tbl_royale_group` FOREIGN KEY (`GroupID`) REFERENCES `tbl_royale_group` (`GroupID`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `tbl_royale_group` (`GroupID`, `GroupName`, `ManagerUserID`, `GroupType`, `LeagueID`, `RuleSetType`, `CreatedTimestamp`)
VALUES (UUID(), 'Previous Royale Winners', NULL, 2, NULL, 'PreviousWinners', UTC_TIMESTAMP());
