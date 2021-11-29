ALTER TABLE `tbl_league_publishergame`
	ADD COLUMN `SlotNumber` INT NOT NULL AFTER `CurrentlyIneligible`;

-- Run the Utility now

ALTER TABLE `tbl_league_publishergame`
	ADD UNIQUE INDEX `UNQ_Slot` (`PublisherID`, `CounterPick`, `SlotNumber`);