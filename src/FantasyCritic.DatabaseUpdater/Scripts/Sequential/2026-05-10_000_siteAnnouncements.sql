CREATE TABLE `tbl_meta_siteannouncements` (
	`ID` CHAR(36) NOT NULL,
	`HtmlID` VARCHAR(255) NOT NULL,
	`Title` VARCHAR(255) NOT NULL,
	`Body` TEXT NOT NULL,
	`PostedAt` TIMESTAMP NOT NULL,
	`LinkAddress` TEXT NULL,
	`LinkLabel` VARCHAR(255) NULL,
	PRIMARY KEY (`ID`)
)
COLLATE='utf8mb4_0900_ai_ci'
;

INSERT INTO `tbl_meta_siteannouncements` (`ID`, `HtmlID`, `Title`, `Body`, `PostedAt`, `LinkAddress`, `LinkLabel`)
VALUES (
	'a8f5f167-f44e-4d86-9e7f-fddc86ac34f7',
	'royale-groups-and-charts',
	'Royale Groups, Charts, and More!',
	'The biggest site update in months! Critic''s Royale received a lot of attention this time, with new features for forming groups, seeing a player''s past performance, and charts to view a quarter in more detail.',
	'2026-04-21 12:00:00',
	'https://www.youtube.com/watch?v=wOjiejePfRU',
	'Watch the DevLog!'
);
