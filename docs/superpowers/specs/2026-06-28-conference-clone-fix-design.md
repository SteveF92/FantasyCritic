# Conference Clone Fix — Design Spec (Phase 2 Slice 5)

**Date:** 2026-06-28  
**Status:** Approved for implementation  
**Branch:** `multi-draft-leagues`  
**Prerequisite:** Phase 2 Slices 1–4 complete (draft CRUD, read path, execution, create-league presets)

---

## Overview

When a multi-draft league exists inside a conference, three conference operations incorrectly create only a single draft instead of cloning all drafts from the primary league:

1. Adding a new league to an existing conference year
2. Adding a new league year to a league inside a conference (when that conference year already exists)
3. Creating a new conference year (for the primary league)

In addition, `AssignLeaguePlayers` — the operation that moves publishers between conference leagues before drafting begins — only rebuilds draft positions for Draft 1. For multi-draft leagues it must rebuild positions for every draft.

Finally, the conference league table on the frontend incorrectly reports `draftFinished` based only on the first draft's status, and has no way to indicate which specific draft is currently in progress.

This spec covers all three of these fixes together.

---

## Area 1: Draft Cloning

### Problem

Three callsites each build a single `initialDraft` from `primaryLeagueYear.FirstDraft` and pass it as a single object to downstream methods. For multi-draft leagues the other drafts are silently dropped.

| Callsite | File |
|---|---|
| `ConferenceService.AddLeagueToConference` | `FantasyCritic.Lib/Services/ConferenceService.cs` |
| `ConferenceService.AddNewLeagueYear` | `FantasyCritic.Lib/Services/ConferenceService.cs` |
| `MySQLConferenceRepo.AddNewConferenceYear` | `FantasyCritic.MySQL/MySQLConferenceRepo.cs` |

### Interface Changes

Two interface methods change their last parameter from a single draft to a list:

**`IConferenceRepo` (Lib/Interfaces/IConferenceRepo.cs)**
```csharp
// Before
Task AddLeagueToConference(Conference conference, LeagueYear primaryLeagueYear, League newLeague, LeagueDraft initialDraft);

// After
Task AddLeagueToConference(Conference conference, LeagueYear primaryLeagueYear, League newLeague, IReadOnlyList<LeagueDraft> drafts);
```

**`IFantasyCriticRepo` (Lib/Interfaces/IFantasyCriticRepo.cs)**
```csharp
// Before
Task AddNewLeagueYear(League league, int year, LeagueOptions options, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers, LeagueDraft initialDraft);

// After
Task AddNewLeagueYear(League league, int year, LeagueOptions options, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers, IReadOnlyList<LeagueDraft> drafts);
```

### Cloning Logic

Each callsite replaces the single-draft construction with a loop over all of the primary league year's drafts, creating each with a fresh `DraftID`, the new `LeagueID`/`Year`, and all other settings copied verbatim (`DraftNumber`, `Name`, `ScheduledDate`, `GamesToDraft`, `CounterPicksToDraft`). `PlayStatus` is always set to `NotStartedDraft` and `DraftOrderSet` to `false`.

```csharp
var clonedDrafts = primaryLeagueYear.Drafts
    .Select(d => new LeagueDraft(
        Guid.NewGuid(),
        new LeagueYearKey(newLeague.LeagueID, year),
        d.DraftNumber, d.Name, d.ScheduledDate,
        d.GamesToDraft, d.CounterPicksToDraft,
        false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null))
    .ToList();
```

### Non-Conference `AddNewLeagueYear` (No Behavior Change)

`FantasyCriticService.AddNewLeagueYear` (for standalone leagues) still creates a single initial draft using the most-recent year's settings; it now passes that draft wrapped in a single-element list `[initialDraft]`. No semantic change.

### Implementation Chain

| Layer | Change |
|---|---|
| `ConferenceService.AddLeagueToConference` | Build `clonedDrafts` list; pass to `_conferenceRepo.AddLeagueToConference` |
| `ConferenceService.AddNewLeagueYear` | Build `clonedDrafts` list; pass to `_fantasyCriticRepo.AddNewLeagueYear` |
| `MySQLConferenceRepo.AddNewConferenceYear` | Build `clonedDrafts` list from `primaryLeaguePreviousLeagueYear.Drafts`; pass to `AddNewLeagueYearInTransaction` |
| `MySQLConferenceRepo.AddLeagueToConference` | Accept `IReadOnlyList<LeagueDraft>`; pass directly to `CreateLeagueInTransaction` |
| `MySQLFantasyCriticRepo.AddNewLeagueYear` | Accept `IReadOnlyList<LeagueDraft>`; create each draft row in the transaction |
| `FakeRepo` implementations | Update signatures and implementations to match |

---

## Area 2: `AssignLeaguePlayers` Draft Position Fixup

### Problem

When publishers are moved between conference leagues before drafting starts, `AssignLeaguePlayers` must clear and rebuild the draft order for all affected league years. The current implementation queries only `DraftNumber = 1` (via an explicit join filter), so for multi-draft leagues only Draft 1's positions are maintained; Draft 2+ are left stale.

### Approach: Separate Draft Queries

The publisher query currently joins `tbl_league_draft` with `DraftNumber = 1` to get `DraftID` and `DraftPosition` per publisher. These fields drive the position rebuild.

The fix separates concerns:

1. **Publisher query** — remove the draft join entirely. `ConferencePublisherInfo` becomes a 4-field record: `(PublisherID, LeagueID, Year, UserID)`. The publisher-movement detection logic is unchanged.

2. **New: all-drafts query** — load `(DraftID, LeagueID, Year, DraftNumber)` for all drafts in the conference year at transaction start.

3. **New: all-positions query** — load `(DraftID, PublisherID, DraftPosition)` for all publisher-draft pairings in the conference year at transaction start.

4. **Rebuild loop** — after determining which `LeagueYearKey`s need fixup (unchanged logic), iterate over all drafts for each affected league year. For each draft, run the existing clear + re-insert SQL with that draft's ID. The rebuild computes the final ordered publisher list from the loaded position data (same ordering algorithm as today, but applied per-draft rather than only to draft 1).

The `clearDraftPositionsSQL` and `insertDraftPositionSQL` strings are unchanged; only the C# loop driving them is extended.

### `ConferencePublisherInfo` (private record)

```csharp
// Before
private record ConferencePublisherInfo(Guid PublisherID, Guid LeagueID, int Year, Guid UserID, Guid? DraftID, int? DraftPosition);

// After
private record ConferencePublisherInfo(Guid PublisherID, Guid LeagueID, int Year, Guid UserID);
```

New supporting records (also private):
```csharp
private record ConferenceDraftInfo(Guid DraftID, Guid LeagueID, int Year, int DraftNumber);
private record ConferenceDraftPosition(Guid DraftID, Guid PublisherID, int DraftPosition);
```

### New SQL Queries (run inside the transaction)

```sql
-- All drafts for this conference year
SELECT ld.DraftID, ld.LeagueID, ld.Year, ld.DraftNumber
FROM tbl_league_draft ld
JOIN tbl_league l ON ld.LeagueID = l.LeagueID
WHERE l.ConferenceID = @conferenceID AND ld.Year = @year;

-- All draft positions for those drafts
SELECT dp.DraftID, dp.PublisherID, dp.DraftPosition
FROM tbl_league_draftpublisher dp
JOIN tbl_league_draft ld ON dp.DraftID = ld.DraftID
JOIN tbl_league l ON ld.LeagueID = l.LeagueID
WHERE l.ConferenceID = @conferenceID AND ld.Year = @year;
```

### Rebuild Algorithm (extended from current)

For each `leagueYearKey` that needs a position fix:

1. Collect all `ConferenceDraftInfo` rows for that league year.
2. For each draft:
   a. Load current `DraftPosition` values for publishers in this league year (from the pre-loaded positions lookup).
   b. Identify publishers moving out and moving in (same logic as today).
   c. Add the `draftID` to `draftClearsNeeded`.
   d. Compute the final ordered publisher list (remaining + moved-in, sorted by their original `DraftPosition` in this draft, moved-in appended with no position as before).
   e. Add new position rows to `draftPositionInserts`.

The bulk clear and bulk insert happen after the loop, unchanged.

---

## Area 3: DraftStarted / DraftFinished / ActiveDraft Display

### Problem

`ConferenceLeagueYearViewModel` (the primary data shape for the conference frontend) reports `DraftFinished` based only on `domain.FirstDraft.PlayStatus.DraftFinished`. For multi-draft leagues this is false when the first draft is done but the second is not. Additionally, there is no way to tell the frontend which draft number is currently active.

`GetLeagueYearsInConferenceYear` has the same `DraftFinished` issue in its SQL (though this method's `DraftStarted`/`DraftFinished` fields are not currently read by any caller — it is still fixed for data correctness).

### `ConferenceLeagueYearViewModel` Changes

| Property | Before | After |
|---|---|---|
| `DraftStarted` | `domain.FirstDraft.PlayStatus.PlayStarted` | Unchanged — first draft started is the right signal for player assignment locking |
| `DraftFinished` | `domain.FirstDraft.PlayStatus.DraftFinished` | `domain.Drafts.All(d => d.PlayStatus.DraftFinished)` |
| `ActiveDraftNumber` | *(not present)* | `domain.ActiveDraft?.DraftNumber` (nullable `int?`) — the 1-based number of the draft currently in progress, or `null` |

### `ConferenceLeagueYear` Domain + Entity Chain

`ConferenceLeagueYear` gains an `ActiveDraftNumber` property (`int?`). The chain of changes:
- `ConferenceLeagueYear.cs` — add constructor parameter and property
- `ConferenceLeagueYearEntity.cs` — add `ActiveDraftNumber` property (`int?`)
- `ConferenceLeagueYearEntity.ToDomain()` — pass through
- `GetLeagueYearsInConferenceYear` SQL — add `DraftFinished` aggregate fix + `ActiveDraftNumber` subquery (see below)

### `GetLeagueYearsInConferenceYear` SQL

```sql
SELECT
  tbl_league.LeagueID, tbl_league.LeagueName, tbl_league.LeagueManager,
  tbl_user.DisplayName AS ManagerDisplayName, tbl_user.EmailAddress AS ManagerEmailAddress,
  tbl_league_year.Year,
  ld1.PlayStatus <> 'NotStartedDraft' AS DraftStarted,
  -- DraftFinished = true only when every draft for this league year is DraftFinal
  NOT EXISTS (
      SELECT 1 FROM tbl_league_draft ld2
      WHERE ld2.LeagueID = tbl_league_year.LeagueID
        AND ld2.Year = tbl_league_year.Year
        AND ld2.PlayStatus <> 'DraftFinal'
  ) AS DraftFinished,
  -- ActiveDraftNumber = the DraftNumber of the currently in-progress draft, or NULL
  (SELECT ld3.DraftNumber FROM tbl_league_draft ld3
   WHERE ld3.LeagueID = tbl_league_year.LeagueID
     AND ld3.Year = tbl_league_year.Year
     AND ld3.PlayStatus IN ('Draft', 'DraftPaused')
   LIMIT 1) AS ActiveDraftNumber,
  ConferenceLocked
FROM tbl_league_year
JOIN tbl_league ON tbl_league.LeagueID = tbl_league_year.LeagueID
JOIN tbl_user ON tbl_league.LeagueManager = tbl_user.UserID
JOIN tbl_league_draft ld1 ON ld1.LeagueID = tbl_league_year.LeagueID
  AND ld1.Year = tbl_league_year.Year AND ld1.DraftNumber = 1
WHERE ConferenceID = @conferenceID AND Year = @year;
```

### Frontend: `conference.vue`

The three bucket counts (Not Started / Drafting / Done) use the same filter predicates and remain correct with the updated semantics.

The "currently drafting" icon on each row (currently `<font-awesome-icon v-if="data.item.draftStarted && !data.item.draftFinished">`) gains a tooltip/label update:

- When `activeDraftNumber` is `1`: tooltip stays as today ("This league is currently drafting.")
- When `activeDraftNumber > 1`: tooltip shows "This league is drafting (Draft `activeDraftNumber`)."
- The icon is shown whenever `activeDraftNumber` is not null (equivalent to `draftStarted && !draftFinished` for single-draft leagues, and correctly tracks which draft is live for multi-draft).

No other frontend components need changes for this slice.

---

## Integration Tests

| Scenario | Assertions |
|---|---|
| Add new league to conference with 2-draft primary league | New league has 2 `tbl_league_draft` rows; both names, counts, and DraftNumbers match primary |
| Add new league year to conference league (primary has 2 drafts) | New league year has 2 draft rows cloned from primary |
| Add new conference year (primary had 2 drafts in previous year) | Primary league's new year has 2 draft rows |
| `AssignLeaguePlayers`: move publisher between leagues in 2-draft conference | `tbl_league_draftpublisher` rows correct for both drafts in source and destination |
| Conference league year `DraftFinished` flag after first draft only | `DraftFinished = false`; `ActiveDraftNumber = null` (between drafts) |
| Conference league year during second draft | `DraftStarted = true`, `DraftFinished = false`, `ActiveDraftNumber = 2` |
| Conference league year after all drafts done | `DraftFinished = true`, `ActiveDraftNumber = null` |

---

## Out of Scope

- `AddNewLeagueYear` for standalone (non-conference) leagues — still creates one initial draft; no change in semantics
- Redesigning the conference league table UI beyond the tooltip/label update
- Fixing the `leagueMixin.js` `firstDraftFinished` computed property (that is for individual league pages, not conference)
- The `GetLeagueYearsInConferenceYear` `ConferenceLocked`-based checks in `AssignLeaguePlayers` — unchanged
