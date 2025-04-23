-- --------------------------------------------------------
-- Host:                         fantasy-critic-rds.cldutembgs4w.us-east-1.rds.amazonaws.com
-- Server version:               8.0.31 - Source distribution
-- Server OS:                    Linux
-- HeidiSQL Version:             12.3.0.6589
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Dumping data for table fantasycritic.tbl_caching_systemwidevalues: ~1 rows (approximately)
INSERT INTO `tbl_caching_systemwidevalues` (`AverageStandardGamePoints`, `AveragePickupOnlyStandardGamePoints`, `AverageCounterPickPoints`) VALUES
	(8.746645870, 7.551299977, -4.313712975);

-- Dumping data for table fantasycritic.tbl_mastergame_tag: ~18 rows (approximately)
INSERT INTO `tbl_mastergame_tag` (`Name`, `ReadableName`, `ShortName`, `TagType`, `HasCustomCode`, `SystemTagOnly`, `Description`, `Examples`, `BadgeColor`) VALUES
	('Cancelled', 'Cancelled', 'CNCL', 'Other', b'0', b'1', 'A game that is not actively in development. Includes games that are on \'hiatus\'.', '[]', 'EE352E'),
	('CurrentlyInEarlyAccess', 'Currently in Early Access', 'C-EA', 'Other', b'1', b'0', 'A game that is playable right now in some form of public early access or general beta.', '[]', 'A7A9AC'),
	('DirectorsCut', 'Director\'s Cut', 'DC', 'RemakeLevel', b'0', b'0', 'A game released not long after an original release, with some new content.', '["Persona 5 Royale", "Super Mario 3D World + Bowser\'s Fury"]', 'CE8E00'),
	('ExpansionPack', 'Expansion Pack', 'EXP', 'Other', b'0', b'0', 'A game that is an addon to an existing game.', '["Doom Eternal: The Ancient Gods", "Monster Hunter World: Iceborne", "World of Warcraft: Shadowlands", "Shovel Knight: King of Cards"]', '6CBE45'),
	('FreeToPlay', 'Free to Play', 'FTP', 'Other', b'0', b'0', 'A game that is free to play.', '[]', 'FF6319'),
	('NewGame', 'New Game', 'NG', 'RemakeLevel', b'0', b'0', 'A definitively new game.', '["Spider Man (2018)", "Red Dead Redemption 2", "Detroit: Become Human", "Call of Duty: Black Ops 4", "Assassin\'s Creed Odyssey"]', '0039A6'),
	('NewGamingFranchise', 'New Gaming Franchise', 'NGF', 'Other', b'0', b'0', 'A game that begins a new series, or a standalone game. Not a sequel. Can be from an existing IP if the IP has never been represented in video games before.', '["Celeste", "Death Loop", "Elden Ring", "Back 4 Blood", "Metro 2033"]', '036BFC'),
	('PartialRemake', 'Partial Remake', 'P-RMKE', 'RemakeLevel', b'0', b'0', 'This category is hard to define. There are a lot of games that are a little "too remade" to be a remaster but "too similar" to be a remake. They live here.', '["Demon\'s Souls (2020)", "Shadow of the Colossus (2018)", "Diablo 2: Resurrected", "Grand Theft Auto: The Trilogy", "The Legend of Zelda: Skyward Sword HD"]', 'FD9C11'),
	('PlannedForEarlyAccess', 'Planned for Early Access', 'P-EA', 'Other', b'1', b'0', 'A game that is planned to release in early access, but is not playable yet.', '[]', 'A7A9AC'),
	('Port', 'Port', 'PRT', 'RemakeLevel', b'0', b'0', 'A game being released on a platform it was not originally on.', '["Doom 2016 (Switch)", "Death Stranding (PC)", "Red Dead Redemption 2 (PC)"]', 'EE352E'),
	('Reimagining', 'Reimagining', 'RIMG', 'RemakeLevel', b'0', b'0', 'A game that drastically overhauls the game it\'s based on.', '["Resident Evil 2 Remake", "Final Fantasy 7 Remake"]', '00933C'),
	('ReleasedInternationally', 'Released Internationally', 'R-INT', 'Other', b'1', b'0', 'A game that is already released outside of North America.', '[]', '00A1DE'),
	('Remake', 'Remake', 'RMKE', 'RemakeLevel', b'0', b'0', 'A game that modernizes gameplay without fundamentally changing it.', '["The Legend of Zelda: Link\'s Awakening (Switch)", "Crash N-Sane Trilogy", "Pokemon Brilliant Diamond and Shining Pearl"]', 'FCCC0A'),
	('Remaster', 'Remaster', 'RMSTR', 'RemakeLevel', b'0', b'0', 'A game that updates graphics or adds very minor features to an existing release.', '["Gears of War: Ultimate Edition", "The Legend of Zelda: The Windwaker HD", "Uncharted: The Nathan Drake Collection"]', 'FF6319'),
	('UnannouncedGame', 'Unannounced Game', 'UNA', 'Other', b'1', b'0', 'A game that is not formally announced, just rumored.', '[]', 'B933AD'),
	('VirtualReality', 'Virtual Reality', 'VR', 'Other', b'0', b'0', 'A game designed for a VR platform.', '["Superhot VR", "Lone Echo 2", "Resident Evil 4 VR"]', '1C1E20'),
	('WillReleaseInternationallyFirst', 'Will Release Internationally First', 'W-INT', 'Other', b'1', b'0', 'A game that will release outside of North America before it is released in North America, but has not released yet.', '[]', '00A1DE'),
	('YearlyInstallment', 'Yearly Installment', 'YI', 'Other', b'0', b'0', 'A game part of a series that releases yearly or close to yearly, with only incremental changes. Mostly sports games that have the year in the title. Does not include games like Call of Duty and Jackbox Party Pack, which are more different year to year than these games.', '["Madden", "FIFA", "Football Manager", "F1 (Racing Series)"]', '996633');

-- Dumping data for table fantasycritic.tbl_mastergame_tagtype: ~2 rows (approximately)
INSERT INTO `tbl_mastergame_tagtype` (`Name`) VALUES
	('Other'),
	('Platform'),
	('RemakeLevel');

-- Dumping data for table fantasycritic.tbl_meta_quarters: ~4 rows (approximately)
INSERT INTO `tbl_meta_quarters` (`Quarter`) VALUES
	(1),
	(2),
	(3),
	(4);

-- Dumping data for table fantasycritic.tbl_meta_supportedyear: ~3 rows (approximately)
INSERT INTO `tbl_meta_supportedyear` (`Year`, `OpenForCreation`, `OpenForPlay`, `OpenForBetaUsers`, `StartDate`, `Finished`) VALUES
	('2018', b'0', b'0', b'0', '2017-12-08', b'1'),
	('2019', b'0', b'0', b'0', '2018-12-07', b'1'),
	('2020', b'0', b'0', b'0', '2019-12-13', b'1'),
	('2021', b'0', b'0', b'0', '2020-12-11', b'1'),
	('2022', b'0', b'0', b'0', '2021-12-15', b'1'),
	('2023', b'0', b'0', b'0', '2022-12-09', b'1'),
	('2024', b'0', b'0', b'0', '2023-12-07', b'1'),
	('2025', b'1', b'1', b'0', '2024-12-12', b'0');

-- Dumping data for table fantasycritic.tbl_meta_systemwidesettings: ~1 rows (approximately)
INSERT INTO `tbl_meta_systemwidesettings` (`ActionProcessingMode`, `RefreshOpenCritic`) VALUES
	(b'0', b'1');

-- Dumping data for table fantasycritic.tbl_meta_tradestatus: ~6 rows (approximately)
INSERT INTO `tbl_meta_tradestatus` (`Status`) VALUES
	('Accepted'),
	('Executed'),
	('Expired'),
	('Proposed'),
	('RejectedByCounterParty'),
	('RejectedByManager'),
	('Rescinded');

-- Dumping data for table fantasycritic.tbl_meta_tradingparty: ~2 rows (approximately)
INSERT INTO `tbl_meta_tradingparty` (`Name`) VALUES
	('CounterParty'),
	('Proposer');

-- Dumping data for table fantasycritic.tbl_royale_supportedquarter: ~23 rows (approximately)
INSERT INTO `tbl_royale_supportedquarter` (`Year`, `Quarter`, `OpenForPlay`, `Finished`, `WinningUser`) VALUES
	('2019', 4, b'1', b'1', '447c0876-ce09-4e02-8da7-3209195b11d2'),
	('2020', 1, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2020', 2, b'1', b'1', 'bcb5e308-608b-4826-adf3-303401f8db51'),
	('2020', 3, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2020', 4, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2021', 1, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2021', 2, b'1', b'1', '8227fcf5-fcb9-460e-9f37-09c8a678b5f7'),
	('2021', 3, b'1', b'1', '1bae9094-97ec-4996-8642-6f927dd109da'),
	('2021', 4, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2022', 1, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2022', 2, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2022', 3, b'1', b'1', 'e2ac7032-a4ff-49af-8b88-ca2e7e5737bc'),
	('2022', 4, b'1', b'1', 'e2ac7032-a4ff-49af-8b88-ca2e7e5737bc'),
	('2023', 1, b'1', b'1', '2bfe3033-cf37-4687-8592-c05ee5ac86ad'),
	('2023', 2, b'1', b'1', 'b517ac79-27fe-4517-9bc4-6b8db91514e2'),
	('2023', 3, b'1', b'1', '8ec3befb-6d2e-4a8c-97c7-ac30c281b81a'),
	('2023', 4, b'1', b'1', '4238b013-7bc2-40a1-adad-8b216fb087be'),
	('2024', 1, b'1', b'1', '692befde-a737-40df-9fb9-4f4274087428'),
	('2024', 2, b'1', b'1', '053e2f95-5614-48de-b3be-4b9a04c59e92'),
	('2024', 3, b'1', b'1', 'e9267b1f-9d06-4e9a-bdd9-ab9d330f8921'),
	('2024', 4, b'1', b'1', '61d1d913-b0f9-4f17-82c1-30c8e71213e1'),
	('2025', 1, b'1', b'1', '61d1d913-b0f9-4f17-82c1-30c8e71213e1'),
	('2025', 2, b'1', b'0', NULL);

-- Dumping data for table fantasycritic.tbl_settings_draftsystem: ~2 rows (approximately)
INSERT INTO `tbl_settings_draftsystem` (`DraftSystem`) VALUES
	('Flexible'),
	('Manual');

-- Dumping data for table fantasycritic.tbl_settings_pickupsystem: ~2 rows (approximately)
INSERT INTO `tbl_settings_pickupsystem` (`PickupSystem`) VALUES
	('SecretBidding'),
	('SemiPublicBidding'),
	('SemiPublicBiddingSecretCounterPicks');

-- Dumping data for table fantasycritic.tbl_settings_playstatus: ~4 rows (approximately)
INSERT INTO `tbl_settings_playstatus` (`Name`) VALUES
	('DraftFinal'),
	('Drafting'),
	('DraftPaused'),
	('NotStartedDraft');

-- Dumping data for table fantasycritic.tbl_settings_releasesystem: ~2 rows (approximately)
INSERT INTO `tbl_settings_releasesystem` (`ReleaseSystem`) VALUES
	('MustBeReleased'),
	('OnlyNeedsScore');

-- Dumping data for table fantasycritic.tbl_settings_scoringsystem: ~5 rows (approximately)
INSERT INTO `tbl_settings_scoringsystem` (`ScoringSystem`) VALUES
	('HalfBonus'),
	('Legacy'),
	('LinearPositive'),
	('Manual'),
	('Standard');

-- Dumping data for table fantasycritic.tbl_settings_tagstatus: ~2 rows (approximately)
INSERT INTO `tbl_settings_tagstatus` (`Status`) VALUES
	('Banned'),
	('Required');

-- Dumping data for table fantasycritic.tbl_settings_tiebreaksystem: ~2 rows (approximately)
INSERT INTO `tbl_settings_tiebreaksystem` (`TiebreakSystem`) VALUES
	('EarliestBid'),
	('LowestProjectedPoints');

-- Dumping data for table fantasycritic.tbl_settings_tradingsystem: ~2 rows (approximately)
INSERT INTO `tbl_settings_tradingsystem` (`TradingSystem`) VALUES
	('NoTrades'),
	('Standard');

-- Dumping data for table fantasycritic.tbl_user_emailtype: ~0 rows (approximately)
INSERT INTO `tbl_user_emailtype` (`EmailType`, `ReadableName`) VALUES
	('PublicBids', 'Public Bids');

-- Dumping data for table fantasycritic.tbl_user_role: ~3 rows (approximately)
INSERT INTO `tbl_user_role` (`RoleID`, `Name`, `NormalizedName`) VALUES
	(1, 'User', 'USER'),
	(2, 'Admin', 'ADMIN'),
	(3, 'BetaTester', 'BETATESTER'),
	(4, 'PlusUser', 'PLUSUSER'),
	(5, 'FactChecker', 'FACTCHECKER'),
	(6, 'ActionRunner', 'ACTIONRUNNER');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
