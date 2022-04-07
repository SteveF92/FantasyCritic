ALTER TABLE `tbl_royale_supportedquarter`
	CHANGE COLUMN `OpenForPlay` `OpenForPlay` BIT(1) NOT NULL AFTER `Quarter`,
	CHANGE COLUMN `Finished` `Finished` BIT(1) NOT NULL AFTER `OpenForPlay`;
