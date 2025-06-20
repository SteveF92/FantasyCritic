CREATE TABLE `tbl_discord_notablemissoptions` (
	`Name` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`Name`)
)
;

INSERT INTO `tbl_discord_notablemissoptions` (`Name`) VALUES ('InitialScore');
INSERT INTO `tbl_discord_notablemissoptions` (`Name`) VALUES ('ScoreUpdates');
INSERT INTO `tbl_discord_notablemissoptions` (`Name`) VALUES ('None');

ALTER TABLE `tbl_discord_gamenewschannel`
	ADD COLUMN `ShowAlreadyReleasedNews` BIT NULL DEFAULT NULL AFTER `GameNewsSetting`,
	ADD COLUMN `ShowWillReleaseInYearNews` BIT NULL DEFAULT NULL AFTER `ShowAlreadyReleasedNews`,
	ADD COLUMN `ShowMightReleaseInYearNews` BIT NULL DEFAULT NULL AFTER `ShowWillReleaseInYearNews`,
	ADD COLUMN `ShowWillNotReleaseInYearNews` BIT NULL DEFAULT NULL AFTER `ShowMightReleaseInYearNews`,
	ADD COLUMN `ShowJustReleasedAnnouncements` BIT NULL DEFAULT NULL AFTER `ShowWillNotReleaseInYearNews`,
	ADD COLUMN `ShowNewGameAnnouncements` BIT NULL DEFAULT NULL AFTER `ShowJustReleasedAnnouncements`,
	ADD COLUMN `ShowScoreGameNews` BIT NULL DEFAULT NULL AFTER `ShowNewGameAnnouncements`,
	ADD COLUMN `ShowEditedGameNews` BIT NULL DEFAULT NULL AFTER `ShowScoreGameNews`;

ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `ShowPickedGameNews` BIT NULL DEFAULT NULL AFTER `BidAlertRoleID`,
	ADD COLUMN `ShowEligibleGameNews` BIT NULL DEFAULT NULL AFTER `ShowPickedGameNews`,
	ADD COLUMN `ShowIneligibleGameNews` BIT NULL DEFAULT NULL AFTER `ShowEligibleGameNews`,
	ADD COLUMN `NotableMissSetting` VARCHAR(50) NULL DEFAULT NULL AFTER `ShowIneligibleGameNews`,
	ADD CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_discord_notablemissoptions` FOREIGN KEY (`NotableMissSetting`) REFERENCES `tbl_discord_notablemissoptions` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION;


-- Apply transformations

UPDATE tbl_discord_gamenewschannel g
LEFT JOIN tbl_discord_leaguechannel l ON g.GuildID = l.GuildID
SET
    g.ShowJustReleasedAnnouncements = 1,
    g.ShowNewGameAnnouncements = 1,
    g.ShowAlreadyReleasedNews = 
        CASE 
            WHEN g.GameNewsSetting = 'All' THEN 1
            WHEN l.GuildID IS NULL OR l.SendLeagueMasterGameUpdates = 0 THEN 1
            ELSE 0
        END,
    g.ShowWillReleaseInYearNews = 1,
    g.ShowMightReleaseInYearNews =
        CASE 
            WHEN g.GameNewsSetting IN ('All', 'MightReleaseInYear') THEN 1
            ELSE 0
        END,
    g.ShowWillNotReleaseInYearNews =
        CASE 
            WHEN g.GameNewsSetting = 'All' THEN 1
            ELSE 0
        END,
    g.ShowScoreGameNews = 1,
    g.ShowEditedGameNews = 1;


UPDATE tbl_discord_leaguechannel l
LEFT JOIN tbl_discord_gamenewschannel g ON l.GuildID = g.GuildID
SET
    l.ShowPickedGameNews = l.SendLeagueMasterGameUpdates,
    l.ShowEligibleGameNews =
        CASE 
            WHEN g.GuildID IS NOT NULL THEN 1
            ELSE 0
        END,
    l.ShowIneligibleGameNews =
        CASE 
            WHEN g.GameNewsSetting = 'All' THEN 1
            ELSE 0
        END,
    l.NotableMissSetting =
        CASE 
            WHEN l.SendNotableMisses = 1 THEN 'ScoreUpdates'
            ELSE 'None'
        END;

ALTER TABLE `tbl_discord_gamenewschannel`
	MODIFY COLUMN `ShowAlreadyReleasedNews` BIT NOT NULL,
	MODIFY COLUMN `ShowWillReleaseInYearNews` BIT NOT NULL,
	MODIFY COLUMN `ShowMightReleaseInYearNews` BIT NOT NULL,
	MODIFY COLUMN `ShowWillNotReleaseInYearNews` BIT NOT NULL,
	MODIFY COLUMN `ShowJustReleasedAnnouncements` BIT NOT NULL,
	MODIFY COLUMN `ShowNewGameAnnouncements` BIT NOT NULL,
	MODIFY COLUMN `ShowScoreGameNews` BIT NOT NULL,
	MODIFY COLUMN `ShowEditedGameNews` BIT NOT NULL;

ALTER TABLE `tbl_discord_leaguechannel`
	DROP FOREIGN KEY `FK_tbl_discord_leaguechannel_tbl_discord_notablemissoptions`;

ALTER TABLE `tbl_discord_leaguechannel`
	MODIFY COLUMN `ShowPickedGameNews` BIT NOT NULL,
	MODIFY COLUMN `ShowEligibleGameNews` BIT NOT NULL,
	MODIFY COLUMN `ShowIneligibleGameNews` BIT NOT NULL,
	MODIFY COLUMN `NotableMissSetting` VARCHAR(50) NOT NULL;

ALTER TABLE `tbl_discord_leaguechannel`
	ADD CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_discord_notablemissoptions` FOREIGN KEY (`NotableMissSetting`) REFERENCES `tbl_discord_notablemissoptions` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_discord_leaguechannel`
	DROP COLUMN `SendLeagueMasterGameUpdates`,
	DROP COLUMN `SendNotableMisses`;

ALTER TABLE `tbl_discord_gamenewschannel`
	DROP COLUMN `GameNewsSetting`;
