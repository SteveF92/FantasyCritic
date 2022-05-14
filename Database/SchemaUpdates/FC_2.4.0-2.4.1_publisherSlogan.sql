ALTER TABLE `tbl_league_publisher`
	ADD COLUMN `PublisherSlogan` VARCHAR(255) NULL DEFAULT NULL AFTER `PublisherIcon`;
ALTER TABLE `tbl_royale_publisher`
	ADD COLUMN `PublisherSlogan` VARCHAR(255) NULL DEFAULT NULL AFTER `PublisherIcon`;
