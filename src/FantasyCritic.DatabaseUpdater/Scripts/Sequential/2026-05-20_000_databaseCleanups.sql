ALTER TABLE `tbl_user`
	DROP INDEX `Index 3`,
	ADD UNIQUE INDEX `UNQ_EmailAddress` (`NormalizedEmailAddress`) USING BTREE;

ALTER TABLE `tbl_mastergame_subgame`
	CHANGE COLUMN `EstimatedReleaseDate` `EstimatedReleaseDate` VARCHAR(255) NOT NULL COLLATE 'utf8mb4_0900_ai_ci' AFTER `GameName`;

DROP TABLE IF EXISTS `tbl_caching_conferenceyearstandings`;