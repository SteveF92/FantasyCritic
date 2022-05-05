ALTER TABLE `tbl_league_publisher`
	ADD COLUMN `SuperDropsAvailable` INT(10) NOT NULL DEFAULT 0 AFTER `WillReleaseGamesDropped`;
ALTER TABLE `tbl_league_publisher`
	ADD COLUMN `SuperDropsAvailable` INT(10) NOT NULL AFTER `WillReleaseGamesDropped`;
