-- The table keeps sent updates as history from the next script on, so it is no longer only pending ones.
RENAME TABLE `tbl_discord_pendingmastergameupdatetype` TO `tbl_discord_mastergameupdatetype`,
             `tbl_discord_pendingmastergameupdate` TO `tbl_discord_mastergameupdate`;

ALTER TABLE `tbl_discord_mastergameupdate`
  DROP FOREIGN KEY `FK_tbl_discord_pendingmastergameupdate_type`;

ALTER TABLE `tbl_discord_mastergameupdate`
  RENAME COLUMN `PendingUpdateID` TO `MasterGameUpdateID`,
  RENAME INDEX `FK_tbl_discord_pendingmastergameupdate_type` TO `FK_tbl_discord_mastergameupdate_type`,
  RENAME INDEX `IX_tbl_discord_pendingmastergameupdate_queuedtimestamp` TO `IX_tbl_discord_mastergameupdate_queuedtimestamp`;

ALTER TABLE `tbl_discord_mastergameupdate`
  ADD CONSTRAINT `FK_tbl_discord_mastergameupdate_type` FOREIGN KEY (`UpdateType`) REFERENCES `tbl_discord_mastergameupdatetype` (`UpdateType`);
