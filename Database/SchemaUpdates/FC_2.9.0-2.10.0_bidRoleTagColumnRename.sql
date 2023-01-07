ALTER TABLE `tbl_discord_leaguechannel`
	CHANGE COLUMN `PublicBidAlertRoleID` `BidAlertRoleID` BIGINT(19) NULL DEFAULT NULL AFTER `GameNewsSetting`;
