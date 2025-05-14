CREATE TABLE `tbl_discord_notablemissoptions` (
	`Name` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`Name`)
)
;

INSERT INTO `tbl_discord_notablemissoptions` (`Name`) VALUES ('InitialScore');
INSERT INTO `tbl_discord_notablemissoptions` (`Name`) VALUES ('ScoreUpdates');
INSERT INTO `tbl_discord_notablemissoptions` (`Name`) VALUES ('None');

ALTER TABLE `tbl_discord_gamenewschannel`
	ADD COLUMN `ShowWillReleaseInYearNews` BIT NULL DEFAULT NULL AFTER `GameNewsSetting`,
	ADD COLUMN `ShowMightReleaseInYearNews` BIT NULL AFTER `ShowWillReleaseInYearNews`,
	ADD COLUMN `ShowWillNotReleaseInYearNews` BIT NULL DEFAULT NULL AFTER `ShowMightReleaseInYearNews`,
	ADD COLUMN `ShowScoreGameNews` BIT NULL DEFAULT NULL AFTER `ShowWillNotReleaseInYearNews`,
	ADD COLUMN `ShowReleasedGameNews` BIT NULL DEFAULT NULL AFTER `ShowScoreGameNews`,
	ADD COLUMN `ShowNewGameNews` BIT NULL DEFAULT NULL AFTER `ShowReleasedGameNews`,
	ADD COLUMN `ShowEditedGameNews` BIT NULL DEFAULT NULL AFTER `ShowNewGameNews`;

UPDATE tbl_discord_gamenewschannel SET
ShowWillReleaseInYearNews = true,
ShowMightReleaseInYearNews = true,
ShowWillNotReleaseInYearNews = true,
ShowScoreGameNews = true,
ShowReleasedGameNews = true,
ShowNewGameNews = true,
ShowEditedGameNews = true
WHERE GameNewsSetting = 'All';

UPDATE tbl_discord_gamenewschannel SET
ShowWillReleaseInYearNews = true,
ShowMightReleaseInYearNews = true,
ShowWillNotReleaseInYearNews = false,
ShowScoreGameNews = true,
ShowReleasedGameNews = true,
ShowNewGameNews = true,
ShowEditedGameNews = true
WHERE GameNewsSetting = 'MightReleaseInYear';

UPDATE tbl_discord_gamenewschannel SET
ShowWillReleaseInYearNews = true,
ShowMightReleaseInYearNews = false,
ShowWillNotReleaseInYearNews = false,
ShowScoreGameNews = true,
ShowReleasedGameNews = true,
ShowNewGameNews = true,
ShowEditedGameNews = true
WHERE GameNewsSetting = 'WillReleaseInYear';

ALTER TABLE `tbl_discord_gamenewschannel`
	CHANGE COLUMN `ShowWillReleaseInYearNews` `ShowWillReleaseInYearNews` BIT(1) NOT NULL AFTER `ChannelID`,
	CHANGE COLUMN `ShowMightReleaseInYearNews` `ShowMightReleaseInYearNews` BIT(1) NOT NULL AFTER `ShowWillReleaseInYearNews`,
	CHANGE COLUMN `ShowWillNotReleaseInYearNews` `ShowWillNotReleaseInYearNews` BIT(1) NOT NULL AFTER `ShowMightReleaseInYearNews`,
	CHANGE COLUMN `ShowScoreGameNews` `ShowScoreGameNews` BIT(1) NOT NULL AFTER `ShowWillNotReleaseInYearNews`,
	CHANGE COLUMN `ShowReleasedGameNews` `ShowReleasedGameNews` BIT(1) NOT NULL AFTER `ShowScoreGameNews`,
	CHANGE COLUMN `ShowNewGameNews` `ShowNewGameNews` BIT(1) NOT NULL AFTER `ShowReleasedGameNews`,
	CHANGE COLUMN `ShowEditedGameNews` `ShowEditedGameNews` BIT(1) NOT NULL AFTER `ShowNewGameNews`,
	DROP COLUMN `GameNewsSetting`;

ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `ShowPickedGameNews` BIT NULL DEFAULT NULL AFTER `BidAlertRoleID`,
	ADD COLUMN `ShowEligibleGameNews` BIT NULL DEFAULT NULL AFTER `ShowPickedGameNews`,
	ADD COLUMN `NotableMissSetting` VARCHAR(50) NULL DEFAULT NULL AFTER `ShowEligibleGameNews`,
	ADD CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_discord_notablemissoptions` FOREIGN KEY (`NotableMissSetting`) REFERENCES `tbl_discord_notablemissoptions` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION;

UPDATE tbl_discord_leaguechannel SET
ShowPickedGameNews = false,
ShowEligibleGameNews = false,
NotableMissSetting = 'None'
WHERE 
SendLeagueMasterGameUpdates = 0 AND SendNotableMisses = 0;

UPDATE tbl_discord_leaguechannel SET
ShowPickedGameNews = false,
ShowEligibleGameNews = false,
NotableMissSetting = 'ScoreUpdates'
WHERE 
SendLeagueMasterGameUpdates = 0 AND SendNotableMisses = 1;

UPDATE tbl_discord_leaguechannel SET
ShowPickedGameNews = true,
ShowEligibleGameNews = true,
NotableMissSetting = 'None'
WHERE 
SendLeagueMasterGameUpdates = 1 AND SendNotableMisses = 0;

UPDATE tbl_discord_leaguechannel SET
ShowPickedGameNews = true,
ShowEligibleGameNews = true,
NotableMissSetting = 'ScoreUpdates'
WHERE 
SendLeagueMasterGameUpdates = 1 AND SendNotableMisses = 1;

ALTER TABLE `tbl_discord_leaguechannel`
	CHANGE COLUMN `ShowPickedGameNews` `ShowPickedGameNews` BIT(1) NOT NULL AFTER `BidAlertRoleID`,
	CHANGE COLUMN `ShowEligibleGameNews` `ShowEligibleGameNews` BIT(1) NOT NULL AFTER `ShowPickedGameNews`,
	CHANGE COLUMN `NotableMissSetting` `NotableMissSetting` VARCHAR(50) NOT NULL AFTER `ShowCurrentYearGameNewsOnly`,
	DROP COLUMN `SendLeagueMasterGameUpdates`,
	DROP COLUMN `SendNotableMisses`;