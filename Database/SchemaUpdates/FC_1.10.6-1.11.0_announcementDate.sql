ALTER TABLE `tbl_mastergame`
	ADD COLUMN `AnnouncementDate` DATE NULL DEFAULT NULL AFTER `InternationalReleaseDate`;
ALTER TABLE `tbl_caching_mastergameyear`
	ADD COLUMN `AnnouncementDate` DATE NULL DEFAULT NULL AFTER `InternationalReleaseDate`;

UPDATE `fantasycritic`.`tbl_mastergame_tag` SET `HasCustomCode`=b'1' WHERE  `Name`='UnannouncedGame';