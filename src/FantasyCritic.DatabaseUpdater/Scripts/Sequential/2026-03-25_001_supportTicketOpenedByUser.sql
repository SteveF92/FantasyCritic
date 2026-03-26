ALTER TABLE `tbl_user_supportticket`
	ADD COLUMN `OpenedByUser` BIT(1) NOT NULL DEFAULT b'0' AFTER `IssueDescription`;
