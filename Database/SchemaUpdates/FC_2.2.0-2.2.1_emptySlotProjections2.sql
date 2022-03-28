CREATE TABLE `tbl_caching_averagepositionpoints` (
	`PickPosition` INT(10) UNSIGNED NOT NULL,
	`AveragePoints` DECIMAL(12,9) NOT NULL,
	PRIMARY KEY (`PickPosition`) USING BTREE
)
ENGINE=InnoDB
;
