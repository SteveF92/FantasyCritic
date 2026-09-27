ALTER TABLE `tbl_meta_systemwidesettings`
	ADD COLUMN `WorkerShouldPullNewJobs` BIT(1) NOT NULL DEFAULT b'1';
