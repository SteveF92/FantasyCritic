ALTER TABLE `tbl_league_year`
	ADD COLUMN `CounterPicksToDraft` TINYINT(3) NOT NULL AFTER `CounterPicks`;
UPDATE tbl_league_year SET CounterPicksToDraft = CounterPicks;