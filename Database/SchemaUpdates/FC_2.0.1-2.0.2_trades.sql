CREATE TABLE `tbl_meta_tradestatus` (
	`Status` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`Status`) USING BTREE
)
ENGINE=InnoDB
;

INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('Proposed');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('Accepted');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('RejectedByCounterParty');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('RejectedByManager');
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES ('Executed');

CREATE TABLE `tbl_meta_tradestatus` (
	`Name` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`Name`) USING BTREE
)
ENGINE=InnoDB
;

INSERT INTO `tbl_meta_tradestatus` (`Name`) VALUES ('Proposer');
INSERT INTO `tbl_meta_tradestatus` (`Name`) VALUES ('CounterParty');

CREATE TABLE `tbl_league_trade` (
	`TradeID` CHAR(36) NOT NULL,
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`ProposingPublisherID` CHAR(36) NOT NULL,
	`CounterPartyPublisherID` CHAR(36) NOT NULL,
	`ProposingBudgetSendAmount` INT(10) UNSIGNED NOT NULL,
	`CounterPartyBudgetSendAmount` INT(10) UNSIGNED NOT NULL,
	`Status` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`TradeID`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_league_year` (`LeagueID`, `Year`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_league_publisher` (`ProposingPublisherID`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_meta_tradestatus` (`Status`) USING BTREE,
	INDEX `FK_tbl_league_trade_tbl_league_publisher_2` (`CounterPartyPublisherID`) USING BTREE,
	CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher` FOREIGN KEY (`ProposingPublisherID`) REFERENCES `fantasycritic`.`tbl_league_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_trade_tbl_league_publisher_2` FOREIGN KEY (`CounterPartyPublisherID`) REFERENCES `fantasycritic`.`tbl_league_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_trade_tbl_league_year` FOREIGN KEY (`LeagueID`, `Year`) REFERENCES `fantasycritic`.`tbl_league_year` (`LeagueID`, `Year`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_trade_tbl_meta_tradestatus` FOREIGN KEY (`Status`) REFERENCES `fantasycritic`.`tbl_meta_tradestatus` (`Status`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;

CREATE TABLE `tbl_league_tradecomponent` (
	`TradeID` CHAR(36) NOT NULL,
	`CurrentParty` VARCHAR(255) NOT NULL,
	`PublisherGameID` CHAR(36) NOT NULL,
	PRIMARY KEY (`TradeID`, `CurrentParty`, `PublisherGameID`) USING BTREE,
	INDEX `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` (`CurrentParty`) USING BTREE,
	INDEX `FK_tbl_league_tradecomponent_tbl_league_publishergame` (`PublisherGameID`) USING BTREE,
	CONSTRAINT `FK_tbl_league_tradecomponent_tbl_league_publishergame` FOREIGN KEY (`PublisherGameID`) REFERENCES `fantasycritic`.`tbl_league_publishergame` (`PublisherGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_tradecomponent_tbl_league_trade` FOREIGN KEY (`TradeID`) REFERENCES `fantasycritic`.`tbl_league_trade` (`TradeID`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT `FK_tbl_league_tradecomponent_tbl_meta_tradingparty` FOREIGN KEY (`CurrentParty`) REFERENCES `fantasycritic`.`tbl_meta_tradingparty` (`Status`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;
