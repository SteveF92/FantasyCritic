CREATE TABLE `tbl_settings_ineligiblegamesystem` (
	`IneligibleGameSystem` VARCHAR(50) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	PRIMARY KEY (`IneligibleGameSystem`) USING BTREE
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;

INSERT INTO `tbl_settings_ineligiblegamesystem` (`IneligibleGameSystem`) VALUES 
('CaseByCase', 'DroppableAsWillNotRelease', 'DroppableAsWillRelease', 'NotDroppable');
