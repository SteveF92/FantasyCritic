ALTER TABLE tbl_league_draftpickskip
    ADD COLUMN IsManualSkip bit(1) NOT NULL DEFAULT 0;

ALTER TABLE tbl_league_draftpickskip
    ALTER COLUMN IsManualSkip DROP DEFAULT;
