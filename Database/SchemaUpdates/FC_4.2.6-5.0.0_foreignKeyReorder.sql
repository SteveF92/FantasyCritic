-- Drop the keys

ALTER TABLE `tbl_league_activeplayer`
	DROP FOREIGN KEY `FK_tbl_league_activeplayer_tbl_league_year`;

ALTER TABLE `tbl_league_eligibilityoverride`
	DROP FOREIGN KEY `FK_tbl_league_eligibilityoverride_tbl_league_year`;

ALTER TABLE `tbl_league_manageraction`
	DROP FOREIGN KEY `FK_tbl_league_manageraction_tbl_league_year`;

ALTER TABLE `tbl_league_managermessage`
	DROP FOREIGN KEY `FK__tbl_league_year`;

ALTER TABLE `tbl_league_publisher`
	DROP FOREIGN KEY `FK_tblpublisher_tblleagueyear`;

ALTER TABLE `tbl_league_specialauction`
	DROP FOREIGN KEY `FK_tbl_league_specialauction_tbl_league_year`;

ALTER TABLE `tbl_league_specialgameslot`
	DROP FOREIGN KEY `FK_tbl_league_specialgameslot_tbl_league_year`;

ALTER TABLE `tbl_league_tagoverride`
	DROP FOREIGN KEY `FK_tbl_league_tagoverride_tbl_league_year`;

ALTER TABLE `tbl_league_trade`
	DROP FOREIGN KEY `FK_tbl_league_trade_tbl_league_year`;

ALTER TABLE `tbl_league_yearusestag`
	DROP FOREIGN KEY `FK_tbl_league_yearusestag_tbl_league_year`;

-- Drop all keys from League Year
ALTER TABLE `tbl_league_year`
	DROP FOREIGN KEY `FK_tbl_league_year_tbl_settings_releasesystem`,
	DROP FOREIGN KEY `FK_tbl_league_year_tbl_settings_tiebreaksystem`,
	DROP FOREIGN KEY `FK_tbl_league_year_tbl_settings_tradingsystem`,
	DROP FOREIGN KEY `FK_tbl_league_year_tbl_user`,
	DROP FOREIGN KEY `tbl_league_year_ibfk_1`,
	DROP FOREIGN KEY `tbl_league_year_ibfk_3`,
	DROP FOREIGN KEY `tbl_league_year_ibfk_4`,
	DROP FOREIGN KEY `tbl_league_year_ibfk_5`,
	DROP FOREIGN KEY `tbl_league_year_ibfk_6`,
	DROP FOREIGN KEY `tbl_league_year_ibfk_7`;

-- Change the key
ALTER TABLE `tbl_league_year`
	DROP PRIMARY KEY,
	ADD PRIMARY KEY (`LeagueID`, `Year`) USING BTREE;

-- Re-add the foreign keys on tbl_league_year

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `FK_tbl_league_year_tbl_settings_releasesystem`
	FOREIGN KEY (`ReleaseSystem`)
	REFERENCES `tbl_settings_releasesystem` (`ReleaseSystem`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `FK_tbl_league_year_tbl_settings_tiebreaksystem`
	FOREIGN KEY (`TiebreakSystem`)
	REFERENCES `tbl_settings_tiebreaksystem` (`TiebreakSystem`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `FK_tbl_league_year_tbl_settings_tradingsystem`
	FOREIGN KEY (`TradingSystem`)
	REFERENCES `tbl_settings_tradingsystem` (`TradingSystem`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `FK_tbl_league_year_tbl_user`
	FOREIGN KEY (`WinningUserID`)
	REFERENCES `tbl_user` (`UserID`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `tbl_league_year_ibfk_1`
	FOREIGN KEY (`DraftSystem`)
	REFERENCES `tbl_settings_draftsystem` (`DraftSystem`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `tbl_league_year_ibfk_3`
	FOREIGN KEY (`LeagueID`)
	REFERENCES `tbl_league` (`LeagueID`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `tbl_league_year_ibfk_4`
	FOREIGN KEY (`PlayStatus`)
	REFERENCES `tbl_settings_playstatus` (`Name`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `tbl_league_year_ibfk_5`
	FOREIGN KEY (`ScoringSystem`)
	REFERENCES `tbl_settings_scoringsystem` (`ScoringSystem`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `tbl_league_year_ibfk_6`
	FOREIGN KEY (`Year`)
	REFERENCES `tbl_meta_supportedyear` (`Year`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
	ADD CONSTRAINT `tbl_league_year_ibfk_7`
	FOREIGN KEY (`PickupSystem`)
	REFERENCES `tbl_settings_pickupsystem` (`PickupSystem`)
	ON UPDATE NO ACTION ON DELETE NO ACTION;

-- Re-add the foreign keys against league year

ALTER TABLE `tbl_league_activeplayer`
	ADD CONSTRAINT `FK_tbl_league_activeplayer_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_eligibilityoverride`
	ADD CONSTRAINT `FK_tbl_league_eligibilityoverride_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_manageraction`
	ADD CONSTRAINT `FK_tbl_league_manageraction_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_managermessage`
	ADD CONSTRAINT `FK_tbl_league_managermessage_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_publisher`
	ADD CONSTRAINT `FK_tblpublisher_tblleagueyear`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_specialauction`
	ADD CONSTRAINT `FK_tbl_league_specialauction_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_specialgameslot`
	ADD CONSTRAINT `FK_tbl_league_specialgameslot_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_tagoverride`
	ADD CONSTRAINT `FK_tbl_league_tagoverride_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_trade`
	ADD CONSTRAINT `FK_tbl_league_trade_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);

ALTER TABLE `tbl_league_yearusestag`
	ADD CONSTRAINT `FK_tbl_league_yearusestag_tbl_league_year`
	FOREIGN KEY (`LeagueID`, `Year`)
	REFERENCES `tbl_league_year` (`LeagueID`, `Year`);