ALTER TABLE `tbl_mastergame`
	ADD COLUMN `GGToken` CHAR(6) NULL DEFAULT NULL AFTER `OpenCriticID`,
	ADD COLUMN `GGCoverArtFileName` VARCHAR(255) NULL DEFAULT NULL AFTER `BoxartFileName`;

ALTER TABLE `tbl_caching_mastergameyear`
	ADD COLUMN `GGToken` CHAR(6) NULL DEFAULT NULL AFTER `OpenCriticID`,
	ADD COLUMN `GGCoverArtFileName` VARCHAR(255) NULL DEFAULT NULL AFTER `BoxartFileName`;

ALTER TABLE `tbl_mastergame_request`
	ADD COLUMN `GGToken` CHAR(6) NULL DEFAULT NULL AFTER `OpenCriticID`;

ALTER TABLE `tbl_mastergame_changerequest`
	ADD COLUMN `GGToken` CHAR(6) NULL DEFAULT NULL AFTER `OpenCriticID`;
