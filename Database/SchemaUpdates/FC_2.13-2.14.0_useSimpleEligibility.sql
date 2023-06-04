ALTER TABLE `tbl_mastergame`
	CHANGE COLUMN `EligibilityChanged` `UseSimpleEligibility` BIT(1) NOT NULL DEFAULT 0 AFTER `DoNotRefreshAnything`;

ALTER TABLE `tbl_caching_mastergameyear`
	CHANGE COLUMN `EligibilityChanged` `UseSimpleEligibility` BIT(1) NOT NULL AFTER `GGCoverArtFileName`;
