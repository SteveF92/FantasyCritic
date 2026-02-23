ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `SendWeeklyReleasesMessage` BIT(1) NOT NULL DEFAULT FALSE AFTER `ShowIneligibleGameNews`;
