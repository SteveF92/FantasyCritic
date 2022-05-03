ALTER TABLE `tbl_mastergame`
	ADD COLUMN `HasAnyReviews` BIT NOT NULL DEFAULT 0 AFTER `CriticScore`;
ALTER TABLE `tbl_mastergame`
	CHANGE COLUMN `HasAnyReviews` `HasAnyReviews` BIT(1) NOT NULL AFTER `CriticScore`;
ALTER TABLE `tbl_caching_mastergameyear`
	ADD COLUMN `HasAnyReviews` BIT NOT NULL AFTER `CriticScore`;

UPDATE tbl_mastergame SET HasAnyReviews = 1 WHERE CriticScore IS NOT NULL;