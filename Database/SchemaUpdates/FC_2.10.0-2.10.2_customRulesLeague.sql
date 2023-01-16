ALTER TABLE `tbl_league`
	ADD COLUMN `CustomRulesLeague` BIT(1) NOT NULL DEFAULT 0 AFTER `TestLeague`;

DROP VIEW IF EXISTS `vw_league`;
DROP TABLE IF EXISTS `vw_league`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `vw_league` AS select `tbl_league`.`LeagueID` AS `LeagueID`,`tbl_league`.`LeagueName` AS `LeagueName`,`tbl_league`.`LeagueManager` AS `LeagueManager`,`tbl_league`.`PublicLeague` AS `PublicLeague`,`tbl_league`.`TestLeague` AS `TestLeague`,`tbl_league`.`CustomRulesLeague` AS `CustomRulesLeague`,`tbl_league`.`Timestamp` AS `Timestamp`,count(`tbl_user_followingleague`.`UserID`) AS `NumberOfFollowers`,`tbl_league`.`IsDeleted` AS `IsDeleted` from (`tbl_league` left join `tbl_user_followingleague` on((`tbl_league`.`LeagueID` = `tbl_user_followingleague`.`LeagueID`))) group by `tbl_league`.`LeagueID`;