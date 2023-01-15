ALTER TABLE `tbl_league_trade`
	CHANGE COLUMN `ProposerPublisherID` `ProposerPublisherID` CHAR(36) NULL AFTER `Year`,
	CHANGE COLUMN `CounterPartyPublisherID` `CounterPartyPublisherID` CHAR(36) NULL AFTER `ProposerPublisherID`;
