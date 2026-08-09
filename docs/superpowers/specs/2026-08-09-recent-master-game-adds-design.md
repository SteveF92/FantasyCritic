# Recent Master Game Adds on Changes Feed

## Goal

Show newly added master games on the existing Recent Master Game Changes page, without changing what is stored in the database. New games are surfaced using `tbl_mastergame.AddedTimestamp` (and `AddedByUserID`), mixed into the same feed as changelog edits.

## Constraints

- No DB schema or storage changes
- Keep the existing API response shape (`CompleteMasterGameChangeViewModel`)
- Keep the Vue page structure (same table); add rows appear via description `"Game added"`
- Final feed is newest-first and capped at 100 rows overall

## Approach

Replace the changelog-only SQL in `MySQLMasterGameRepo.GetRecentMasterGameChanges()` with a single `UNION ALL` query that combines:

1. **Changelog rows** from `tbl_mastergame_changelog` (unchanged columns)
2. **Synthetic add rows** from `tbl_mastergame`:
   - `Timestamp` = `AddedTimestamp`
   - `ChangedByUserID` = `AddedByUserID`
   - `Description` = `'Game added'`
   - `MasterGameChangeID` = `MasterGameID` (stable synthetic id for the shared row shape)

Outer query: `ORDER BY Timestamp DESC LIMIT 100`.

Existing hydration stays the same: load master games and users, map entities to `MasterGameChangeLogEntry`, then the existing controller/VM path.

## Layers touched

| Layer | Change |
| --- | --- |
| `MySQLMasterGameRepo.GetRecentMasterGameChanges` | SQL only (UNION + limit) |
| `IMasterGameRepo` / `InterLeagueService` | No signature changes |
| `GameController.GetRecentMasterGameChanges` | None |
| View models | None |
| `recentMasterGameChanges.vue` | None |

## Testing

- Manual: load Recent Master Game Changes (or `GET /api/game/GetRecentMasterGameChanges`) and confirm `"Game added"` rows appear sorted with edits, total ≤ 100
- No NSwag regen (contract unchanged)
- Automated coverage optional; no existing endpoint tests required for this change

## Out of scope

- Persisting add events into `tbl_mastergame_changelog`
- Separate UI section for newly added games
- Changing the page columns or enabling the commented-out “Changed by” column
