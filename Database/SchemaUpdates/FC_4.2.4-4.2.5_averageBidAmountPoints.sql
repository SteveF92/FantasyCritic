CREATE TABLE `tbl_caching_averagebidamountpoints` (
	`BidAmount` INT(10) UNSIGNED NOT NULL,
	`DataPoints` INT(10) UNSIGNED NOT NULL,
	`AveragePoints` DECIMAL(12,9) NOT NULL,
	PRIMARY KEY (`BidAmount`) USING BTREE
)
ENGINE=InnoDB
;
