CREATE TABLE `tbl_league_publisherstatistics` (
	`PublisherID` CHAR(36) NOT NULL,
	`Date` DATE NOT NULL,
	`FantasyPoints` DECIMAL(12,9) NOT NULL,
	`ProjectedPoints` DECIMAL(12,9) NOT NULL,
	`RemainingBudget` SMALLINT(5) UNSIGNED NOT NULL,
	`NumberOfStandardGames` TINYINT(3) UNSIGNED NOT NULL,
	`NumberOfStandardGamesReleased` TINYINT(3) UNSIGNED NOT NULL,
	`NumberOfStandardGamesExpectedToRelease` TINYINT(3) UNSIGNED NOT NULL,
	`NumberOfStandardGamesNotExpectedToRelease` TINYINT(3) UNSIGNED NOT NULL,
	`NumberOfCounterPicks` TINYINT(3) UNSIGNED NOT NULL,
	`NumberOfCounterPicksReleased` TINYINT(3) UNSIGNED NOT NULL,
	`NumberOfCounterPicksExpectedToRelease` TINYINT(3) UNSIGNED NOT NULL,
	`NumberOfCounterPicksNotExpectedToRelease` TINYINT(3) UNSIGNED NOT NULL,
	PRIMARY KEY (`PublisherID`, `Date`) USING BTREE,
	CONSTRAINT `FK_tbl_league_publisherstatistics_tbl_league_publisher` FOREIGN KEY (`PublisherID`) REFERENCES `tbl_league_publisher` (`PublisherID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
ENGINE=InnoDB
;
