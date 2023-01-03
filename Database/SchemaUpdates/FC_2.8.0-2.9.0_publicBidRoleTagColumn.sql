ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `PublicBidAlertRoleID` BIGINT(20) NULL DEFAULT NULL AFTER `GameNewsSetting`;
