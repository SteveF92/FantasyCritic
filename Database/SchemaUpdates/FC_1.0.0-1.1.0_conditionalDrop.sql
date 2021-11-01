ALTER TABLE `tbl_league_pickupbid`
	ADD COLUMN `ConditionalDropMasterGameID` CHAR(36) NOT NULL AFTER `MasterGameID`,
	ADD CONSTRAINT `FK_tbl_league_pickupbid_tbl_mastergame` FOREIGN KEY (`ConditionalDropMasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION;

DROP VIEW IF EXISTS `vw_league_pickupbid`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league_pickupbid` AS select `tbl_league_pickupbid`.`BidID` AS `BidID`,`tbl_league_publisher`.`PublisherID` AS `PublisherID`,`tbl_league_publisher`.`LeagueID` AS `LeagueID`,`tbl_league_year`.`Year` AS `Year`,`tbl_league_publisher`.`PublisherName` AS `PublisherName`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_mastergame`.`MasterGameID` AS `MasterGameID`,`tbl_mastergame`.`GameName` AS `GameName`,`tbl_league_pickupbid`.`ConditionalDropMasterGameID` AS `ConditionalDropMasterGameID`,`tbl_league_pickupbid`.`Priority` AS `Priority`,`tbl_league_pickupbid`.`BidAmount` AS `BidAmount`,`tbl_league_pickupbid`.`Successful` AS `Successful`,`tbl_league_pickupbid`.`Timestamp` AS `Timestamp`,`tbl_league`.`IsDeleted` AS `IsDeleted` from ((((`tbl_league_pickupbid` join `tbl_league_publisher` on((`tbl_league_pickupbid`.`PublisherID` = `tbl_league_publisher`.`PublisherID`))) join `tbl_mastergame` on((`tbl_league_pickupbid`.`MasterGameID` = `tbl_mastergame`.`MasterGameID`))) join `tbl_league_year` on(((`tbl_league_publisher`.`LeagueID` = `tbl_league_year`.`LeagueID`) and (`tbl_league_year`.`Year` = `tbl_league_publisher`.`Year`)))) join `tbl_league` on((`tbl_league_publisher`.`LeagueID` = `tbl_league`.`LeagueID`)));

ALTER TABLE `tbl_meta_systemwidesettings`
	CHANGE COLUMN `BidProcessingMode` `ActionProcessingMode` BIT(1) NOT NULL FIRST;
