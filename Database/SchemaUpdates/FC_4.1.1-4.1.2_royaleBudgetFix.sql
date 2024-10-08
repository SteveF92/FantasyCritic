ALTER TABLE `tbl_royale_publisher`
	CHANGE COLUMN `Budget` `Budget` DECIMAL(11,2) UNSIGNED NOT NULL DEFAULT '0.0' AFTER `PublisherSlogan`;