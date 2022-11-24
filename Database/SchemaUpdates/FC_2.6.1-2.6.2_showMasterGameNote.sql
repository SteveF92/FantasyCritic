ALTER TABLE `tbl_mastergame`
	ADD COLUMN `ShowNote` BIT NOT NULL DEFAULT 0 AFTER `DelayContention`;
ALTER TABLE `tbl_mastergame`
	CHANGE COLUMN `ShowNote` `ShowNote` BIT(1) NOT NULL AFTER `DelayContention`;
