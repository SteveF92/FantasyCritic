CREATE TABLE `tbl_meta_tradestatus` (
	`Status` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`Status`) USING BTREE
)
ENGINE=InnoDB
;

INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('Proposed');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('Accepted');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('Rescinded');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('RejectedByCounterParty');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('RejectedByManager');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('Executed');

CREATE TABLE `tbl_meta_tradestatus` (
	`Party` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`Party`) USING BTREE
)
ENGINE=InnoDB
;

INSERT INTO `tbl_meta_tradingparty` (`Name`) VALUES ('Proposer');
INSERT INTO `tbl_meta_tradingparty` (`Name`) VALUES ('CounterParty');

CREATE TABLE `tbl_league_trade` (
	`TradeID` CHAR(36) NOT NULL,
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`ProposerPublisherID` CHAR(36) NOT NULL,
	`CounterPartyPublisherID` CHAR(36) NOT NULL,
	`ProposerBudgetSendAmount` INT(10) UNSIGNED NOT NULL,
	`CounterPartyBudgetSendAmount` INT(10) UNSIGNED NOT NULL,
	`Message` TEXT NOT NULL,
	`ProposedTimestamp` DATETIME NOT NULL,
	`AcceptedTimestamp` DATETIME NULL DEFAULT NULL,
	`CompletedTimestamp` DATETIME NULL DEFAULT NULL,
	`Status` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`TradeID`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_league_year` (`LeagueID`, `Year`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_league_publisher` (`ProposerPublisherID`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_meta_tradestatus` (`Status`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_league_publisher_2` (`CounterPartyPublisherID`) USING BTREE,
	CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher` FOREIGN KEY (`ProposerPublisherID`) REFERENCES `fantasycritic`.`tbl_league_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher_2` FOREIGN KEY (`CounterPartyPublisherID`) REFERENCES `fantasycritic`.`tbl_league_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_trade_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `fantasycritic`.`tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_trade_tbl_meta_tradestatus` FOREIGN KEY (`Status`) REFERENCES `fantasycritic`.`tbl_meta_tradestatus` (`Status`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;

CREATE TABLE `tbl_league_tradecomponent` (
	`TradeID` CHAR(36) NOT NULL,
	`CurrentParty` VARCHAR(255) NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL,
	`CounterPick` BIT(1) NOT NULL,
	PRIMARY KEY (`TradeID`, `CurrentParty`, `MasterGameID`, `CounterPick`) USING BTREE,
	INDEX `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` (`CurrentParty`) USING BTREE,
	INDEX `FK_tbl_league_tradecomponent_tbl_mastergame` (`MasterGameID`) USING BTREE,
	CONSTRAINT `FK_tbl_league_tradecomponent_tbl_league_trade` FOREIGN KEY (`TradeID`) REFERENCES `fantasycritic`.`tbl_league_trade` (`TradeID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_tradecomponent_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `fantasycritic`.`tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` FOREIGN KEY (`CurrentParty`) REFERENCES `fantasycritic`.`tbl_meta_tradingparty` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

CREATE TABLE `tbl_league_tradevote` (
	`TradeID` CHAR(36) NOT NULL,
	`UserID` CHAR(36) NOT NULL,
	`Approved` BIT(1) NOT NULL,
	`Timestamp` DATETIME NOT NULL,
	`Comment` TEXT NULL DEFAULT NULL,
	PRIMARY KEY (`TradeID`, `UserID`) USING BTREE,
	INDEX `FK_tbl_league_tradevote_tbl_user` (`UserID`) USING BTREE,
	CONSTRAINT `FK_tbl_league_tradevote_tbl_league_trade` FOREIGN KEY (`TradeID`) REFERENCES `fantasycritic`.`tbl_league_trade` (`TradeID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_tradevote_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `fantasycritic`.`tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
