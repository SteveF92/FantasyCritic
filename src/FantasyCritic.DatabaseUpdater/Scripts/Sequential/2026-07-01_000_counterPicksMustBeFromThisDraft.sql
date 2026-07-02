ALTER TABLE tbl_league_draft
  ADD COLUMN CounterPicksMustBeFromThisDraft bit(1) NOT NULL DEFAULT b'1';

UPDATE tbl_league_draft SET CounterPicksMustBeFromThisDraft = b'1';

ALTER TABLE tbl_league_draft
  MODIFY COLUMN CounterPicksMustBeFromThisDraft bit(1) NOT NULL;
