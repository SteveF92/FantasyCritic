ALTER TABLE `tbl_user`
	ADD COLUMN `PatreonDonorNameOverride` VARCHAR(255) NULL DEFAULT NULL AFTER `DisplayName`;
