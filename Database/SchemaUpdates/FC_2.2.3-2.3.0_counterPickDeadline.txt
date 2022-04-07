ALTER TABLE `tbl_league_year`
	ADD COLUMN `CounterPickDeadlineMonth` TINYINT NOT NULL DEFAULT 12 AFTER `TradingSystem`,
	ADD COLUMN `CounterPickDeadlineDay` TINYINT NOT NULL DEFAULT 31 AFTER `CounterPickDeadlineMonth`;

ALTER TABLE `tbl_league_year`
	CHANGE COLUMN `CounterPickDeadlineMonth` `CounterPickDeadlineMonth` TINYINT(3) NOT NULL AFTER `TradingSystem`,
	CHANGE COLUMN `CounterPickDeadlineDay` `CounterPickDeadlineDay` TINYINT(3) NOT NULL AFTER `CounterPickDeadlineMonth`;

ALTER TABLE `tbl_league_year`
	CHANGE COLUMN `TradingSystem` `TradingSystem` VARCHAR(50) NOT NULL AFTER `ScoringSystem`;
