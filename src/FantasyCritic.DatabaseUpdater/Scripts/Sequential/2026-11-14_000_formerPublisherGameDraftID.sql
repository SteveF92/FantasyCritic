-- Games removed from a roster since 2026-06-27_000_multiDraftLeagues.sql lost their DraftID, because the insert into
-- tbl_league_formerpublishergame left the column out. Restore it from the league year's drafts: a drafted game belongs
-- to the latest draft that had started by the time it was picked, which is the first draft for a single-draft league.
UPDATE `tbl_league_formerpublishergame` fpg
JOIN `tbl_league_publisher` p ON p.`PublisherID` = fpg.`PublisherID`
JOIN `tbl_league_draft` d ON d.`LeagueID` = p.`LeagueID` AND d.`Year` = p.`Year`
SET fpg.`DraftID` = d.`DraftID`
WHERE fpg.`DraftID` IS NULL
  AND fpg.`OverallPickNumber` IS NOT NULL
  AND d.`DraftNumber` = (
      SELECT COALESCE(MAX(d2.`DraftNumber`), 1)
      FROM `tbl_league_draft` d2
      WHERE d2.`LeagueID` = p.`LeagueID`
        AND d2.`Year` = p.`Year`
        AND d2.`DraftStartedTimestamp` <= fpg.`Timestamp`
  );
