-- Replaces the synchronous PushYearEndDiscordMessages endpoint, for sending the last finished year's standings again by hand.
INSERT INTO `tbl_job_type` (`Name`, `DisplayName`, `Category`, `Severity`, `RunType`) VALUES
	('SendFinalYearStandings', 'Send Final Year Standings', 'Other', 'Danger', 'Manual');
