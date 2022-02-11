CREATE TABLE `tbl_meta_actionprocessingset` (
	`ProcessSetID` CHAR(36) NOT NULL,
	`ProcessTime` DATETIME NOT NULL,
	`ProcessName` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`ProcessSetID`) USING BTREE
)
ENGINE=InnoDB
;

ALTER TABLE `tbl_league_droprequest`
	ADD COLUMN `ProcessSetID` CHAR(36) NULL DEFAULT NULL AFTER `Successful`,
	ADD CONSTRAINT `FK_tbl_league_droprequest_tbl_meta_actionprocessingset` FOREIGN KEY (`ProcessSetID`) REFERENCES `tbl_meta_actionprocessingset` (`ProcessSetID`) ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_pickupbid`
	ADD COLUMN `ProcessSetID` CHAR(36) NULL DEFAULT NULL AFTER `Successful`,
	ADD COLUMN `Outcome` VARCHAR(255) NULL DEFAULT NULL AFTER `ProcessSetID`,
	ADD COLUMN `ProjectedPointsAtTimeOfBid` DECIMAL(12,4) NULL DEFAULT NULL AFTER `Outcome`;

ALTER TABLE `tbl_league_publishergame`
	ADD COLUMN `BidAmount` SMALLINT(5) NULL DEFAULT NULL AFTER `OverallDraftPosition`;

DROP VIEW IF EXISTS `vw_league_droprequest`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_droprequest` AS select `tbl_league_droprequest`.`DropRequestID` AS `DropRequestID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_droprequest`.`Successful` AS `Successful`,`tbl_league_droprequest`.`Timestamp` AS `Timestamp`,`tbl_league_droprequest`.`ProcessSetID` AS `ProcessSetID`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_droprequest` join `tbl_league_publisher` on((`tbl_league_droprequest`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_droprequest`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));

DROP VIEW IF EXISTS `vw_league_pickupbid`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_pickupbid` AS select `tbl_league_pickupbid`.`BidID` AS `BidID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_pickupbid`.`ConditionalDropMasterGameID` AS `ConditionalDropMasterGameID`,`tbl_league_pickupbid`.`Counterpick` AS `CounterPick`,`tbl_league_pickupbid`.`Priority` AS `Priority`,`tbl_league_pickupbid`.`BidAmount` AS `BidAmount`,`tbl_league_pickupbid`.`Successful` AS `Successful`,`tbl_league_pickupbid`.`Timestamp` AS `Timestamp`,`tbl_league_pickupbid`.`ProcessSetID` AS `ProcessSetID`,`tbl_league_pickupbid`.`Outcome` AS `Outcome`,`tbl_league_pickupbid`.`ProjectedPointsAtTimeOfBid` AS `ProjectedPointsAtTimeOfBid`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_pickupbid` join `tbl_league_publisher` on((`tbl_league_pickupbid`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_pickupbid`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));
