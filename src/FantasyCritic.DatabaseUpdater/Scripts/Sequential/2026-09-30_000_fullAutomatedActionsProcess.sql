-- PrepareForActionProcessing becomes FullAutomatedActionsProcess, which processes actions itself, so cron only and Danger like ProcessActions.
-- tbl_job.JobType references tbl_job_type.Name without ON UPDATE CASCADE, so the new row goes in first, the jobs move to it, then the old row goes.
INSERT INTO `tbl_job_type` (`Name`, `DisplayName`, `Category`, `Severity`, `RunType`) VALUES
	('FullAutomatedActionsProcess', 'Full Automated Actions Process', 'Bids', 'Danger', 'Cron');

UPDATE `tbl_job` SET `JobType` = 'FullAutomatedActionsProcess' WHERE `JobType` = 'PrepareForActionProcessing';

DELETE FROM `tbl_job_type` WHERE `Name` = 'PrepareForActionProcessing';
