ALTER TABLE tbl_league_year
    ADD COLUMN BidsOnlyBeforeNextScheduledDraft TINYINT(1) NOT NULL DEFAULT 0;

ALTER TABLE tbl_league_year
    ALTER COLUMN BidsOnlyBeforeNextScheduledDraft DROP DEFAULT;
