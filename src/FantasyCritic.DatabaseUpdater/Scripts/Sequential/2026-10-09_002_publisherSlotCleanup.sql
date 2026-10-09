-- Publisher slots as data, part 3 of 3: lock in the new model and drop the old one (docs/publisher-slot-plan.md,
-- Step 1). Runs only after 2026-10-09_001_publisherSlotData.cs has linked every publisher game to a slot.

ALTER TABLE `tbl_league_publishergame`
	MODIFY COLUMN `LeagueSlotID` CHAR(36) NOT NULL;

-- UNQ_PublisherSlot (added with the schema) now backs the PublisherID foreign key, so UNQ_Slot can go.
ALTER TABLE `tbl_league_publishergame`
	DROP INDEX `UNQ_Slot`,
	DROP COLUMN `SlotNumber`;

ALTER TABLE `tbl_league_year`
	DROP COLUMN `StandardGames`,
	DROP COLUMN `CounterPicks`;

DROP TABLE `tbl_league_specialgameslot`;
