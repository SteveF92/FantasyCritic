ALTER TABLE `tbl_mastergame`
	CHANGE COLUMN `DoNotRefreshAnything` `SyncWithExternalAPIs` BIT(1) NOT NULL AFTER `FirstCriticScoreTimestamp`,
	DROP COLUMN `DoNotRefreshDate`;

UPDATE tbl_mastergame
	SET `SyncWithExternalAPIs` = `SyncWithExternalAPIs` ^ 1;
