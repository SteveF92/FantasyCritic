ALTER TABLE `tbl_discord_leaguechannel`
	ADD COLUMN `SendWeeklyReleasesMessage` BIT(1) NULL DEFAULT NULL AFTER `ShowIneligibleGameNews`;
