CREATE TABLE `tbl_meta_actionprocessingset` (
	`ProcessSetID` CHAR(36) NOT NULL,
	`ProcessTime` DATETIME NOT NULL,
	`ProcessName` VARCHAR(255) NOT NULL,
	PRIMARY KEY (`ProcessSetID`) USING BTREE
)
ENGINE=InnoDB
;

