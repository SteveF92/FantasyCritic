ALTER TABLE `tbl_league_year`
	ADD COLUMN `CounterPicksToDraft` TINYINT(3) NOT NULL AFTER `CounterPicks`;
UPDATE tbl_league_year SET CounterPicksToDraft = CounterPicks;

ALTER TABLE `tbl_league_pickupbid`
	ADD COLUMN `Counterpick` BIT NOT NULL AFTER `ConditionalDropMasterGameID`;
UPDATE tbl_league_pickupbid SET Counterpick = 0;

DROP VIEW IF EXISTS `vw_league_pickupbid`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_pickupbid` AS select `tbl_league_pickupbid`.`BidID` AS `BidID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_pickupbid`.`ConditionalDropMasterGameID` AS `ConditionalDropMasterGameID`,`tbl_league_pickupbid`.`Counterpick` AS `CounterPick`,`tbl_league_pickupbid`.`Priority` AS `Priority`,`tbl_league_pickupbid`.`BidAmount` AS `BidAmount`,`tbl_league_pickupbid`.`Successful` AS `Successful`,`tbl_league_pickupbid`.`Timestamp` AS `Timestamp`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_pickupbid` join `tbl_league_publisher` on((`tbl_league_pickupbid`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_pickupbid`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));