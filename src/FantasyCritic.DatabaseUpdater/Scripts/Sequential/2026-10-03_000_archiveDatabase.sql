-- The weekly offsite copy of the database. Production only: MySQLBetaCleaner turns it off in every copy of production.
INSERT INTO `tbl_job_type` (`Name`, `DisplayName`, `Category`, `Severity`, `RunType`) VALUES
	('ArchiveDatabase', 'Archive Database', 'Database', 'Warning', 'ManualOrCron');
