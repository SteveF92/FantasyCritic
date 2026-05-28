# Multi-Draft Leagues — Phase 1 Complete

## What Phase 1 Is

Phase 1 is the "engine" half of multi-draft support. The invariant is:
**the app must behave identically to before.** Nothing changes from the user's perspective. What changes is the underlying data model and every layer of code that touches it, so that Phase 2 (adding a second draft) requires only additive changes rather than another structural migration.

---

## Database Schema Changes

**Script:** `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-05-21_000_multiDraftLeagues.sql`

### New tables

**`tbl_league_draft`** — one row per draft session per league year.

| Column | Type | Notes |
|---|---|---|
| `DraftID` | char(36) PK | |
| `LeagueID` | char(36) FK | |
| `Year` | year | |
| `DraftNumber` | tinyint | 1-based; `1` = first (and currently only) draft |
| `Name` | varchar(255) | Backfilled as `'InitialDraft'` for all existing rows |
| `ScheduledDate` | date NULL | Backfilled from `DATE(DraftStartedTimestamp)` |
| `GamesToDraft` | tinyint | Moved from `tbl_league_year` |
| `CounterPicksToDraft` | tinyint | Moved from `tbl_league_year` |
| `DraftOrderSet` | bit | Moved from `tbl_league_year` |
| `PlayStatus` | varchar | Moved from `tbl_league_year` |
| `DraftStartedTimestamp` | timestamp NULL | Moved from `tbl_league_year` |

**`tbl_league_draftpublisher`** — draft order per publisher per draft session.

| Column | Type | Notes |
|---|---|---|
| `DraftID` | char(36) PK, FK → `tbl_league_draft` | |
| `PublisherID` | char(36) PK, FK → `tbl_league_publisher` | |
| `DraftPosition` | tinyint NOT NULL | Moved from `tbl_league_publisher.DraftPosition` |

### Columns removed from existing tables

- `tbl_league_year`: `GamesToDraft`, `CounterPicksToDraft`, `PlayStatus`, `DraftOrderSet`, `DraftStartedTimestamp`
- `tbl_league_publisher`: `DraftPosition`

### New column on existing tables

- `tbl_league_year.EnableBids` (bit NOT NULL) — replaces the implicit "one-shot" detection that previously used `StandardGames == GamesToDraft`. Backfilled `0` (false) for leagues that were already in one-shot mode, `1` (true) for all others.
- `tbl_league_publishergame.DraftID` (char(36) NULL FK) — nullable link from a publisher game to the draft it was acquired in.
- `tbl_league_formerpublishergame.DraftID` (char(36) NULL FK) — same for former games.

### Data migration order

The migration script performs in this order:
1. Create `tbl_league_draft` and `tbl_league_draftpublisher`.
2. Add `EnableBids` with a temporary default of `1`.
3. Backfill `EnableBids = 0` for all existing one-shot leagues (the five-condition check matching `OneShotMode`).
4. Drop the default, making `EnableBids` truly NOT NULL.
5. Backfill `tbl_league_draft` from `tbl_league_year` (one row per year, `DraftNumber = 1`).
6. Backfill `tbl_league_draftpublisher` from `tbl_league_publisher.DraftPosition` (only where non-null).
7. Drop the migrated columns from `tbl_league_year` and `DraftPosition` from `tbl_league_publisher`.

---

## Domain Layer (`FantasyCritic.Lib`)

### New types

**`LeagueDraft`** (`Domain/LeagueDraft.cs`)  
Represents a single draft session. Carries `DraftID`, `DraftNumber`, `Name`, `ScheduledDate`, `GamesToDraft`, `CounterPicksToDraft`, `DraftOrderSet`, `PlayStatus`, `DraftStartedTimestamp`, and `PublisherDraftInfos`.

Key method: `UpdateDraft(gamesToDraft, counterPicksToDraft)` — returns a new instance with updated counts (used by `EditLeagueYear`).

Static method: `ValidateDraftCounts(...)` — the count-validation logic that previously lived inline in services.

**`PublisherDraftInfo`** (`Domain/PublisherDraftInfo.cs`)  
`(DraftID, DraftNumber, PublisherID, DraftPosition)` — the per-publisher per-draft position record.

### Modified types

**`LeagueYear`**
- Constructor now takes `IEnumerable<LeagueDraft> drafts` (required, non-empty; throws if empty).
- `Drafts` property: `IReadOnlyList<LeagueDraft>`, ordered by `DraftNumber`.
- `FirstDraft` property: `Drafts.First()` — non-nullable, always valid.
- `PlayStatus`, `DraftOrderSet`, `DraftStartedTimestamp` are now computed properties that **delegate to `FirstDraft`**.
- `OneShotMode` is now computed on `LeagueYear` (not `LeagueOptions`): `!Options.EnableBids && StandardGames == Drafts.Sum(d => d.GamesToDraft) && CounterPicks == Drafts.Sum(d => d.CounterPicksToDraft) && [drop conditions]`. This makes it multi-draft-aware by design.

**`LeagueOptions`**
- `GamesToDraft` and `CounterPicksToDraft` removed (they live on `LeagueDraft` now).
- `OneShotMode` removed (it lives on `LeagueYear` now).
- `EnableBids` added (bool).

**`Publisher`**
- `DraftPosition` removed.
- `DraftInfos: IReadOnlyList<PublisherDraftInfo>` added — ordered by `DraftNumber`.
- `FirstDraftInfo` property: `DraftInfos.First()`.
- `GetDraftPosition(Guid draftID)` helper — returns nullable int.
- Constructor enforces non-empty `DraftInfos` (same invariant as `LeagueYear` and `Drafts`).

**`LeagueYearParameters`**
- `EnableBids` added.
- `GamesToDraft` and `CounterPicksToDraft` kept — they still describe the first draft's configuration at create/edit time and are forwarded to `LeagueDraft` construction in the service layer.

---

## Persistence Layer (`FantasyCritic.MySQL`)

### New entity classes (`SharedSerialization/Database/`)

**`LeagueDraftEntity`** — maps `tbl_league_draft`; `ToDomain(IEnumerable<PublisherDraftInfo>)` builds a `LeagueDraft`.

**`LeagueDraftPublisherEntity`** — maps `tbl_league_draftpublisher`; used to build `PublisherDraftInfo` list.

### `sp_getleagueyear` result set additions

Two new result sets were added to the stored procedure (and coordinated reads in `QueryMultiple`):
1. `SELECT * FROM tbl_league_draft WHERE LeagueID = … AND Year = …  ORDER BY DraftNumber`
2. `SELECT dp.* FROM tbl_league_draftpublisher dp JOIN tbl_league_draft d … WHERE d.LeagueID = … AND d.Year = …`

These are read in `MySQLFantasyCriticRepo.GetLeagueYear` to build `LeagueDraft` objects (with their `PublisherDraftInfos`) and to attach `PublisherDraftInfo` lists to each `Publisher`.

### Draft lifecycle SQL

Every mutation that previously did `UPDATE tbl_league_year SET PlayStatus/DraftOrderSet/DraftStartedTimestamp` now targets `tbl_league_draft` by `DraftID` (always using `leagueYear.FirstDraft.DraftID`):

- `StartDraft` → `UPDATE tbl_league_draft SET PlayStatus = 'Drafting', DraftStartedTimestamp = … WHERE DraftID = @draftID`
- `PauseDraft` / `ResumeDraft` → same table, status toggles
- `CompleteDraft` → `PlayStatus = 'DraftFinal'`
- `ResetDraft` → clears status and timestamp
- `SetDraftOrder` → **DELETE + INSERT** on `tbl_league_draftpublisher` (not a column update on `tbl_league_publisher`)

### `EditLeagueYear`

Game count changes (GamesToDraft, CounterPicksToDraft) now issue `UPDATE tbl_league_draft SET GamesToDraft = …, CounterPicksToDraft = … WHERE DraftID = @draftID` in addition to the main `tbl_league_year` update.

### `CreatePublisher`

Inserts a row into `tbl_league_draftpublisher` for every draft in the league year (currently always one) and clears `DraftOrderSet` on those drafts, preserving the "draft order is now dirty" signal.

### `DeletePublisher`

After deleting the publisher, clears `tbl_league_draftpublisher` for the first draft, then re-inserts the remaining publishers with renumbered `DraftPosition` values.

### Conference: `AssignLeaguePlayers`

When publishers are moved between conference leagues (pre-draft only), the draft position fixup now:
1. Queries `DraftID` and `DraftPosition` from `tbl_league_draftpublisher` (via the first draft join) alongside each publisher.
2. For every affected league year, clears all `tbl_league_draftpublisher` rows for that draft's `DraftID`.
3. Re-inserts renumbered rows ordered by original `DraftPosition` (publishers moving in are placed first since they have no prior position in the destination league).

---

## Stored Procedures (Idempotent)

All idempotent stored procedures in `Scripts/Idempotent/Stored Procedures/` that previously read from the now-dropped columns on `tbl_league_year` have been updated.

### Pattern used everywhere

```sql
JOIN tbl_league_draft ld
  ON ld.LeagueID = tbl_league_year.LeagueID
 AND ld.Year     = tbl_league_year.Year
 AND ld.DraftNumber = 1
```

Then `ld.PlayStatus`, `ld.GamesToDraft`, etc. are used instead of the now-dropped `tbl_league_year.*` columns.

### Files updated

| File | What changed |
|---|---|
| `sp_getleagueyear.sql` | Added two new result sets (drafts + draft publishers); `PlayStatus` now from `ld.*` |
| `sp_getleaguesforuser.sql` | `MostRecentYearOneShot` computed inline with `tbl_league_year.EnableBids` + `ld.GamesToDraft`; `PlayStatus` from `ld.*` in both UNION legs |
| `sp_getconferenceyeardata.sql` | Conference draft-started detection aggregated over `tbl_league_draft` |
| `sp_getleagueyearsforconferenceyear.sql` | `PlayStatus` from `ld.*` |
| `sp_gethomepagedata.sql` | Public leagues block: `PlayStatus` from `ld.*` |
| `sp_getleague.sql` | `PlayStatus` from `ld.*` |
| `sp_getusersinleague.sql` | `PlayStatus` from `ld.*` |
| `sp_getcombinedleagueyearuserstatus.sql` | `PlayStatus` from `ld.*` |

---

## Test Layer (`FantasyCritic.FakeRepo` / `FantasyCritic.Test`)

The test CSV files (`LeagueYears.csv`) retain the old columns (including `GamesToDraft`, `PlayStatus`, etc.) because they were written before the migration. A CsvHelper adapter layer reads `LeagueYears.csv` and **projects the draft-related columns into a `LeagueDraftEntity`** rather than requiring a separate `LeagueDraft.csv`.

`TestLeagueYearDefaults.DeriveEnableBids` computes `EnableBids` from the same five-condition rule used in the DB migration (inverse of one-shot mode), so test data does not need to be updated.

A `PublisherGameEntityMap` ignores the missing `DraftID` column on test CSVs (it doesn't exist there yet).

---

## Finding All Phase 2 Deferral Points

Every place in the codebase where Phase 2 assumptions are baked in is marked with:

```
// TODO(Phase2-MultiDraft): <description>
```

(SQL files use `-- TODO(Phase2-MultiDraft):`.)

A project-wide search for `TODO(Phase2-MultiDraft)` will find every deferred decision. As of Phase 1 completion, the hits fall into these categories:

### "Always uses first draft only" (the main class of assumption)

Most deferral comments say some variation of "Updates only the first draft's status" or "Uses only the first draft's PlayStatus." These are in:
- `MySQLFantasyCriticRepo.cs` — draft lifecycle methods (`StartDraft`, `PauseDraft`, `ResumeDraft`, `CompleteDraft`, `ResetDraft`, `EditLeagueYear`, `DeletePublisher`, `CreatePublisher`, `SetDraftOrder`, `MinimalLeagueYearInfo` check)
- `MySQLConferenceRepo.cs` — `AddNewLeagueYear`, `GetLeagueYearsInConferenceYear`
- `DraftService.cs` — `SetDraftOrder`
- `FantasyCriticService.cs` — `AddNewLeagueYear`
- `ConferenceService.cs` — `AddNewConferenceYear`

### "DraftNumber = 1 in SQL" (stored procedures)

Every stored procedure that joins `tbl_league_draft` uses `AND ld.DraftNumber = 1`. These are noted with the TODO comment in:
- `sp_getleagueyear.sql`
- `sp_getleaguesforuser.sql`
- `sp_getleagueyearsforconferenceyear.sql`
- `sp_getconferenceyeardata.sql`
- `sp_gethomepagedata.sql`
- `sp_getleague.sql`
- `sp_getusersinleague.sql`
- `sp_getcombinedleagueyearuserstatus.sql`

### Frontend (not yet wired)

- `leagueYearSettings.vue` — `EnableBids` field exists in the ViewModel round-trip but the league settings UI does not yet expose it as a real option.

---

## What Phase 1 Enables for Phase 2

The structural work is done. Adding a second draft in Phase 2 requires only **additive** changes:

1. **New domain logic** in `FantasyCriticService` / `DraftService`: `CreateNextDraft` validates that the current draft is `DraftFinal`, creates a new `tbl_league_draft` row with `DraftNumber = 2`, and inserts `tbl_league_draftpublisher` rows.

2. **`CurrentDraft` on `LeagueYear`**: a property that returns the draft currently in progress (or the next pending one). Phase 1 always used `FirstDraft`; Phase 2 routes through `CurrentDraft` for active operations.

3. **`EnableBids` enforcement**: bids are already gated by `leagueYear.OneShotMode` in some places. Phase 2 should also gate on `!Options.EnableBids` explicitly, since `EnableBids = false` can now mean "no bids between Draft 1 and Draft 2" even if there are unfilled slots.

4. **UI**: Create Draft button, `StandardGames` expansion, Delete Draft (only for `NotStartedDraft` + `DraftNumber > 1`), and a draft-selector if the user is viewing history.

5. **Stored procedures**: replace the `AND ld.DraftNumber = 1` filter in all procedures with a more sophisticated "current draft" or aggregate-across-drafts approach, depending on what each query needs.
