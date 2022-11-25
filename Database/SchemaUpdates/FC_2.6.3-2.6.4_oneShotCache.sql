CREATE TABLE `tbl_caching_leagueyear` (
	`LeagueID` CHAR(36) NOT NULL,
	`Year` YEAR NOT NULL,
	`OneShotMode` BIT NOT NULL,
	PRIMARY KEY (`LeagueID`, `Year`)
)
;
