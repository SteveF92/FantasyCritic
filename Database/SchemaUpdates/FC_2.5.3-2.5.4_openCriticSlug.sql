ALTER TABLE `tbl_mastergame`
	ADD COLUMN `OpenCriticSlug` VARCHAR(255) NULL DEFAULT NULL AFTER `HasAnyReviews`;

ALTER TABLE `tbl_caching_mastergameyear`
	ADD COLUMN `OpenCriticSlug` VARCHAR(255) NULL DEFAULT NULL AFTER `HasAnyReviews`;