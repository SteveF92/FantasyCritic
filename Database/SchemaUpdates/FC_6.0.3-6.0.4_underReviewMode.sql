ALTER TABLE `tbl_league_year`
	ADD COLUMN `UnderReview` BIT(1) NOT NULL AFTER `ConferenceLocked`;
