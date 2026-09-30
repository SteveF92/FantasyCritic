-- Null until the update is sent, then the number of channels it went to, which can be 0. Sent rows are kept as history.
-- The pending read is WHERE SentCount IS NULL ORDER BY QueuedTimestamp, so the index leads on SentCount.
ALTER TABLE `tbl_discord_mastergameupdate`
  ADD COLUMN `SentCount` int DEFAULT NULL AFTER `QueuedTimestamp`,
  DROP INDEX `IX_tbl_discord_mastergameupdate_queuedtimestamp`,
  ADD KEY `IX_tbl_discord_mastergameupdate_sentcount_queuedtimestamp` (`SentCount`, `QueuedTimestamp`);
