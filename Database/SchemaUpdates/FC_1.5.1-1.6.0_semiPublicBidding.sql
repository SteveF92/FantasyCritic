DELETE FROM `fantasycritic`.`tbl_settings_pickupsystem` WHERE  `PickupSystem`='Manual';

INSERT INTO `tbl_settings_pickupsystem` (`PickupSystem`) VALUES ('SecretBidding');
INSERT INTO `tbl_settings_pickupsystem` (`PickupSystem`) VALUES ('SemiPublicBidding');

UPDATE tbl_league_year SET PickupSystem = 'SecretBidding';

DELETE FROM `fantasycritic`.`tbl_settings_pickupsystem` WHERE  `PickupSystem`='Budget';