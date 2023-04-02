ALTER TABLE `tbl_league_year`
	ADD COLUMN `AllowMoveIntoIneligible` BIT(1) NOT NULL DEFAULT 0 AFTER `CounterPicksBlockDrops`;
