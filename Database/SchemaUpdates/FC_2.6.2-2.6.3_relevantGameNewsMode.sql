ALTER TABLE `tbl_discord_leaguechannel`
	CHANGE COLUMN `IsGameNewsEnabled` `GameNewsSetting` VARCHAR(50) NOT NULL DEFAULT 'Relevant' AFTER `ChannelID`;
ALTER TABLE `tbl_discord_leaguechannel`
	CHANGE COLUMN `GameNewsSetting` `GameNewsSetting` VARCHAR(50) NOT NULL AFTER `ChannelID`;
UPDATE tbl_discord_leaguechannel SET GameNewsSetting = 'Relevant';

CREATE TABLE `tbl_discord_gamenewsoptions` (
	`Name` VARCHAR(50) NOT NULL,
	PRIMARY KEY (`Name`)
);

INSERT INTO `tbl_discord_gamenewsoptions` (`Name`) VALUES ('Off');
INSERT INTO `tbl_discord_gamenewsoptions` (`Name`) VALUES ('On');
INSERT INTO `tbl_discord_gamenewsoptions` (`Name`) VALUES ('Relevant');

ALTER TABLE `tbl_discord_leaguechannel`
	ADD CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_discord_gamenewsoptions` FOREIGN KEY (`GameNewsSetting`) REFERENCES `tbl_discord_gamenewsoptions` (`Name`) ON UPDATE NO ACTION ON DELETE NO ACTION;
