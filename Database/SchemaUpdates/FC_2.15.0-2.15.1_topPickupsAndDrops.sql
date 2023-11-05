ALTER TABLE `tbl_league_pickupbid`
	ADD CONSTRAINT `FK_tbl_league_pickupbid_tbl_meta_actionprocessingset` FOREIGN KEY (`ProcessSetID`) REFERENCES `tbl_meta_actionprocessingset` (`ProcessSetID`) ON UPDATE NO ACTION ON DELETE NO ACTION;

CREATE TABLE `tbl_caching_topbidsanddrops` (
	`ProcessDate` DATE NOT NULL,
	`MasterGameID` CHAR(36) NOT NULL COLLATE 'utf8mb4_general_ci',
	`Year` INT(10) NOT NULL,
	`TotalStandardBidCount` INT(10) NOT NULL,
	`SuccessfulStandardBids` INT(10) NOT NULL,
	`FailedStandardBids` INT(10) NOT NULL,
	`TotalStandardBidLeagues` INT(10) NOT NULL,
	`TotalStandardBidAmount` INT(10) NOT NULL,
	`TotalCounterPickBidCount` INT(10) NOT NULL,
	`SuccessfulCounterPickBids` INT(10) NOT NULL,
	`FailedCounterPickBids` INT(10) NOT NULL,
	`TotalCounterPickBidLeagues` INT(10) NOT NULL,
	`TotalCounterPickBidAmount` INT(10) NOT NULL,
	`TotalDropCount` INT(10) NOT NULL,
	`SuccessfulDrops` INT(10) NOT NULL,
	`FailedDrops` INT(10) NOT NULL,
	PRIMARY KEY (`ProcessDate`, `MasterGameID`, `Year`) USING BTREE,
	INDEX `FK_tbl_caching_topbidsanddrops_tbl_mastergame` (`MasterGameID`) USING BTREE,
	CONSTRAINT `FK_tbl_caching_topbidsanddrops_tbl_mastergame` FOREIGN KEY (`MasterGameID`) REFERENCES `tbl_mastergame` (`MasterGameID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_general_ci'
ENGINE=InnoDB
;
