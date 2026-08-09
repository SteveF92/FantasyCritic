ALTER TABLE `tbl_league_publishergame`
	CHANGE COLUMN `DraftPosition` `PickNumber` TINYINT(3) NULL DEFAULT NULL AFTER `MasterGameID`,
	CHANGE COLUMN `OverallDraftPosition` `OverallPickNumber` SMALLINT(5) NULL DEFAULT NULL AFTER `PickNumber`;

ALTER TABLE `tbl_league_formerpublishergame`
	CHANGE COLUMN `DraftPosition` `PickNumber` TINYINT(3) NULL DEFAULT NULL AFTER `MasterGameID`,
	CHANGE COLUMN `OverallDraftPosition` `OverallPickNumber` SMALLINT(5) NULL DEFAULT NULL AFTER `PickNumber`;
