ALTER TABLE `tbl_league_publisher`
	ADD COLUMN `OnlyAutoDraftFromWatchlist` BIT NOT NULL DEFAULT 0 AFTER `AutoDraftMode`;
