-- Publisher slots as data, part 3 of 3: lock in the new model and drop the old one (docs/publisher-slot-plan.md,
-- Step 1). Runs only after 2026-10-09_001_publisherSlotData.cs has linked every publisher game to a slot.

-- A game always has a slot, a publisher has one game per slot, and a counter pick sits in a counter-pick slot.
-- Each of these fails if the backfill left anything inconsistent, and nothing has been dropped at that point.
-- One statement on purpose: this table has a million rows and every ALTER here rebuilds it, so the constraints
-- and the drops share a single rebuild. UNQ_PublisherSlot takes over from UNQ_Slot as the index behind the
-- PublisherID foreign key, which MySQL accepts within the same statement.
ALTER TABLE `tbl_league_publishergame`
	MODIFY COLUMN `LeagueSlotID` CHAR(36) NOT NULL,
	ADD UNIQUE INDEX `UNQ_PublisherSlot` (`PublisherID`, `LeagueSlotID`) USING BTREE,
	ADD INDEX `FK_tbl_league_publishergame_tbl_league_slot` (`LeagueSlotID`, `CounterPick`) USING BTREE,
	ADD CONSTRAINT `FK_tbl_league_publishergame_tbl_league_slot` FOREIGN KEY (`LeagueSlotID`, `CounterPick`) REFERENCES `tbl_league_slot` (`LeagueSlotID`, `CounterPick`) ON UPDATE NO ACTION ON DELETE NO ACTION,
	DROP INDEX `UNQ_Slot`,
	DROP COLUMN `SlotNumber`;

ALTER TABLE `tbl_league_year`
	DROP COLUMN `StandardGames`,
	DROP COLUMN `CounterPicks`;

DROP TABLE `tbl_league_specialgameslot`;
