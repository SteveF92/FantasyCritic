ALTER TABLE `tbl_league_pickupbid`
	ADD COLUMN `AllowIneligibleSlot` BIT NOT NULL DEFAULT 0 AFTER `BidAmount`;