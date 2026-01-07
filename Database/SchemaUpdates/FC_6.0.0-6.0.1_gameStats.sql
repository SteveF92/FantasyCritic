CREATE TABLE `tbl_caching_mastergameyearstatistics` (
	`Year` YEAR NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`Date` DATE NOT NULL,
	`PercentStandardGame` DOUBLE NOT NULL,
	`PercentCounterPick` DOUBLE NOT NULL,
	`EligiblePercentStandardGame` DOUBLE NOT NULL,
	`AdjustedPercentCounterPick` DOUBLE NULL DEFAULT NULL,
	`NumberOfBids` INT(10) NOT NULL,
	`TotalBidAmount` INT(10) NOT NULL,
	`BidPercentile` DOUBLE NOT NULL DEFAULT '0',
	`AverageDraftPosition` DOUBLE NULL DEFAULT NULL,
	`AverageWinningBid` DOUBLE NULL DEFAULT NULL,
	`HypeFactor` DOUBLE NOT NULL,
	`DateAdjustedHypeFactor` DOUBLE NOT NULL,
	`PeakHypeFactor` DOUBLE NOT NULL,
	`LinearRegressionHypeFactor` DOUBLE NOT NULL,
	PRIMARY KEY (`Year`, `MasterGameID`, `Date`) USING BTREE
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;

CREATE TABLE `tbl_royale_publisherstatistics` (
	`PublisherID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`Date` DATE NOT NULL,
	`FantasyPoints` DECIMAL(12,4) NOT NULL,
	PRIMARY KEY (`PublisherID`, `Date`) USING BTREE,
	CONSTRAINT `FK_tbl_royale_publisherstatistics_tbl_royale_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_royale_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;
