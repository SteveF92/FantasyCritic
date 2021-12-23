ALTER TABLE `tbl_mastergame_tag`
	ADD COLUMN `SystemTagOnly` BIT(1) NOT NULL DEFAULT 0 AFTER `HasCustomCode`;

UPDATE `fantasycritic`.`tbl_mastergame_tag` SET `SystemTagOnly`= 1 WHERE  `Name`='Cancelled';

DELETE FROM tbl_league_yearusestag WHERE Tag = "Cancelled";