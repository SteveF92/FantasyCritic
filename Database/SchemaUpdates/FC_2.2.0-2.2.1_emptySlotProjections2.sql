ALTER TABLE `tbl_caching_systemwidevalues`
	ADD COLUMN `AveragePickupOnlyStandardGamePoints` DECIMAL(12,9) NOT NULL AFTER `AverageStandardGamePoints`;

CREATE TABLE `tbl_caching_positionpoints` (
	`PickPosition` INT(10) UNSIGNED NOT NULL,
	`AveragePoints` DECIMAL(12,9) NOT NULL,
	PRIMARY KEY (`PickPosition`) USING BTREE
)
ENGINE=InnoDB
;
