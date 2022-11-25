ALTER TABLE `tbl_league_publisherqueue`
	CHANGE COLUMN `Rank` `Ranking` INT(10) UNSIGNED NOT NULL DEFAULT '0' AFTER `MasterGameID`;
