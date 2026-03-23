CREATE TABLE `tbl_user_supportticket` (
	`SupportTicketID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`UserID` CHAR(36) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`VerificationCode` CHAR(8) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`OpenedAt` TIMESTAMP NOT NULL,
	`IssueDescription` TEXT NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
	`ClosedAt` TIMESTAMP NULL DEFAULT NULL,
	`ResolutionNotes` TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
	PRIMARY KEY (`SupportTicketID`) USING BTREE,
	INDEX `FK_tbl_user_supportticket_tbl_user` (`UserID`) USING BTREE,
	CONSTRAINT `FK_tbl_user_supportticket_tbl_user` FOREIGN KEY (`UserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION
)
COLLATE='utf8mb4_0900_ai_ci'
ENGINE=InnoDB
;
