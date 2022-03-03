CREATE TABLE `tbl_settings_tiebreaksystem` (
	`TiebreakSystem` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`TiebreakSystem`) USING BTREE
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

INSERT INTO `tbl_settings_tiebreaksystem` (`TiebreakSystem`) VALUES ('LowestProjectedPoints');
INSERT INTO `tbl_settings_tiebreaksystem` (`TiebreakSystem`) VALUES ('EarliestBid');

ALTER TABLE `tbl_league_year`
	ADD COLUMN `TiebreakSystem` VARCHAR(50) NOT NULL DEFAULT 'LowestProjectedPoints' AFTER `PickupSystem`,
	ADD CONSTRAINT `FK_tbl_league_year_tbl_settings_tiebreaksystem` FOREIGN KEY (`TiebreakSystem`) REFERENCES `tbl_settings_tiebreaksystem` (`TiebreakSystem`) ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	CHANGE COLUMN `TiebreakSystem` `TiebreakSystem` VARCHAR(50) NOT NULL AFTER `PickupSystem`;
