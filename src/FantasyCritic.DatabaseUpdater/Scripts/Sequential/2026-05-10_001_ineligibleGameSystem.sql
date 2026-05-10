CREATE TABLE IF NOT EXISTS `tbl_settings_ineligiblegamesystem` (
	`IneligibleGameSystem` VARCHAR(50) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	PRIMARY KEY (`IneligibleGameSystem`) USING BTREE
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;

INSERT INTO `tbl_settings_ineligiblegamesystem` (`IneligibleGameSystem`) VALUES
	('CaseByCase'),
	('DroppableAsWillNotRelease'),
	('DroppableAsWillRelease'),
	('NotDroppable');

ALTER TABLE `tbl_league_year`
	ADD COLUMN `IneligibleGameSystem` varchar(50) NOT NULL DEFAULT 'CaseByCase' AFTER `ReleaseSystem`,
	ADD KEY `FK_tbl_league_year_tbl_settings_ineligiblegamesystem` (`IneligibleGameSystem`),
	ADD CONSTRAINT `FK_tbl_league_year_tbl_settings_ineligiblegamesystem` FOREIGN KEY (`IneligibleGameSystem`) REFERENCES `tbl_settings_ineligiblegamesystem` (`IneligibleGameSystem`);

-- Backfill uses DEFAULT above; drop it so new rows must set IneligibleGameSystem explicitly.
ALTER TABLE `tbl_league_year`
	MODIFY COLUMN `IneligibleGameSystem` varchar(50) NOT NULL;
