ALTER TABLE `tbl_user_hasrole`
	ADD COLUMN `ProgrammaticallyAssigned` BIT NOT NULL AFTER `RoleID`;

INSERT INTO `tbl_user_role` (`RoleID`, `Name`, `NormalizedName`) VALUES (4, 'PlusUser', 'PLUSUSER');

ALTER TABLE `tbl_league_publisher`
	ADD COLUMN `PublisherIcon` VARCHAR(255) NULL AFTER `PublisherName`;
