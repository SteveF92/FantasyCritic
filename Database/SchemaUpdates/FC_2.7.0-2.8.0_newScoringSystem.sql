INSERT INTO `tbl_settings_scoringsystem` (`ScoringSystem`) VALUES ('LinearPositive');
INSERT INTO `tbl_settings_scoringsystem` (`ScoringSystem`) VALUES ('Legacy');

START TRANSACTION;
UPDATE tbl_league_year SET ScoringSystem = "Legacy" WHERE ScoringSystem = "Standard";
UPDATE tbl_league_year SET ScoringSystem = "Standard" WHERE ScoringSystem = "Diminishing";
COMMIT;

DELETE FROM `fantasycritic`.`tbl_settings_scoringsystem` WHERE  `ScoringSystem`='Diminishing';