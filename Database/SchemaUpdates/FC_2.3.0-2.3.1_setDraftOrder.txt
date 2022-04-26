ALTER TABLE `tbl_league_year`
	ADD COLUMN `DraftOrderSet` BIT NOT NULL DEFAULT 1 AFTER `PlayStatus`;
ALTER TABLE `tbl_league_year`
	CHANGE COLUMN `DraftOrderSet` `DraftOrderSet` BIT(1) NOT NULL AFTER `PlayStatus`;
