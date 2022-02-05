ALTER TABLE `tbl_mastergame`
	ADD COLUMN `DelayContention` BIT(1) NOT NULL DEFAULT 0 AFTER `EligibilityChanged`;

ALTER TABLE `tbl_caching_mastergameyear`
	CHANGE COLUMN `EligibilityChanged` `EligibilityChanged` BIT(1) NOT NULL AFTER `GGCoverArtFileName`,
	ADD COLUMN `DelayContention` BIT(1) NOT NULL AFTER `EligibilityChanged`;
