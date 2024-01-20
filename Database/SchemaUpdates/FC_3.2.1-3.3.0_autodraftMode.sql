CREATE TABLE `tbl_meta_autodraftmode` (
	`Mode` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`Mode`)
);

INSERT INTO `tbl_meta_autodraftmode` (`Mode`) VALUES ('Off');
INSERT INTO `tbl_meta_autodraftmode` (`Mode`) VALUES ('StandardGamesOnly');
INSERT INTO `tbl_meta_autodraftmode` (`Mode`) VALUES ('All');

ALTER TABLE `tbl_league_publisher`
	ADD COLUMN `AutoDraftMode` VARCHAR(50) NOT NULL DEFAULT 'Off' AFTER `AutoDraft`;

ALTER TABLE `tbl_league_publisher`
	ADD CONSTRAINT `FK_tbl_league_publisher_tbl_meta_autodraftmode` FOREIGN KEY (`AutoDraftMode`) REFERENCES `tbl_meta_autodraftmode` (`Mode`) ON UPDATE NO ACTION ON DELETE NO ACTION;

UPDATE tbl_league_publisher SET AutoDraftMode = 'All' WHERE AutoDraft = 1;

ALTER TABLE `tbl_league_publisher`
	DROP COLUMN `AutoDraft`;
