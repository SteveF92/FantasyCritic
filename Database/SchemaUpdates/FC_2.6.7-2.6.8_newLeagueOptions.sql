ALTER TABLE `tbl_league_year`
	ADD COLUMN `MightReleaseDroppableMonth` TINYINT(3) NULL AFTER `CounterPickDeadlineDay`,
	ADD COLUMN `MightReleaseDroppableDay` TINYINT(3) NULL AFTER `MightReleaseDroppableMonth`;

CREATE TABLE `tbl_settings_releasesystem` (
	`ReleaseSystem` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`ReleaseSystem`) USING BTREE
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

INSERT INTO `tbl_settings_releasesystem` (`ReleaseSystem`) VALUES ('MustBeReleased');
INSERT INTO `tbl_settings_releasesystem` (`ReleaseSystem`) VALUES ('OnlyNeedsScore');

ALTER TABLE `tbl_league_year`
	ADD COLUMN `ReleaseSystem` VARCHAR(50) NOT NULL DEFAULT 'MustBeReleased' AFTER `TradingSystem`,
	ADD CONSTRAINT `FK_tbl_league_year_tbl_settings_releasesystem` FOREIGN KEY (`ReleaseSystem`) REFERENCES `tbl_settings_releasesystem` (`ReleaseSystem`) ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	CHANGE COLUMN `ReleaseSystem` `ReleaseSystem` VARCHAR(50) NOT NULL AFTER `TradingSystem`;
