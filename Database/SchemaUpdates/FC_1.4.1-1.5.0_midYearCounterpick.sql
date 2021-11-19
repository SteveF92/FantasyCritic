ALTER TABLE `tbl_league_year`
	ADD COLUMN `CounterPicksToDraft` TINYINT(3) NOT NULL AFTER `CounterPicks`;
UPDATE tbl_league_year SET CounterPicksToDraft = CounterPicks;

ALTER TABLE `tbl_league_pickupbid`
	ADD COLUMN `Counterpick` BIT NOT NULL AFTER `ConditionalDropMasterGameID`;
UPDATE tbl_league_pickupbid SET Counterpick = 0;