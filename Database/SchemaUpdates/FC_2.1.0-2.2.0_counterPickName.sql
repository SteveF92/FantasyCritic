ALTER TABLE `tbl_league_pickupbid`
	CHANGE COLUMN `Counterpick` `CounterPick` BIT(1) NOT NULL AFTER `ConditionalDropMasterGameID`;