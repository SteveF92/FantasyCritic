CREATE TABLE `fantasycritic`.`tbl_discord_leaguechannel` (
  `LeagueID` CHAR(36) NOT NULL,
  `ChannelID` VARCHAR(30) NOT NULL,
  PRIMARY KEY (`LeagueID`, `ChannelID`),
  CONSTRAINT `FK_tbl_discord_leaguechannel_tbl_league`
    FOREIGN KEY (`LeagueID`)
    REFERENCES `fantasycritic`.`tbl_league` (`LeagueID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
