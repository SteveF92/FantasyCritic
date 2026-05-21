---
name: Multi Draft Leagues
overview: Introduce `tbl_league_yeardraft` and `tbl_league_yeardraftpublisher` so each league year can have multiple draft sessions. Draft-related columns migrate off `tbl_league_year`; year-level `EnableBids` controls pickups. Drafts load with `LeagueYear` (no separate repo getter). `PublisherDraftInfo` is on both `Publisher` and `LeagueYearDraft`.
todos:
  - id: db-migration
    content: "DbUp script: tbl_league_yeardraft, tbl_league_yeardraftpublisher, migrate data, EnableBids on tbl_league_year, DraftID on publishergame, drop old columns"
    status: pending
  - id: domain-types
    content: "PublisherDraftInfo, LeagueYearDraft, Publisher.DraftInfos, LeagueYear.Drafts/CurrentDraft, LeagueOptions.EnableBids"
    status: pending
  - id: domain-parameters
    content: Update LeagueYearParameters (add EnableBids); create CreateDraftParameters request type
    status: pending
  - id: league-year-entity
    content: Update LeagueYearEntity + MySQL GetLeagueYear mapping (drafts on LeagueYear, DraftInfos on publishers; no public GetDrafts)
    status: pending
  - id: repo-interface
    content: "IFantasyCriticRepo: CreateDraft + draft lifecycle on yeardraft tables; no GetDraftsForLeagueYear"
    status: pending
  - id: stored-procs
    content: "Update sp_getleagueyear, sp_getconferenceyeardata, sp_getleaguesforuser for new tables and EnableBids"
    status: pending
  - id: mysql-impl
    content: "MySQLFantasyCriticRepo: draft entities, SetDraftOrder to yeardraftpublisher, LeagueYear construction with drafts"
    status: pending
  - id: service-draft
    content: "DraftService/DraftFunctions: CurrentDraft, PublisherDraftInfo for order, PublisherGame.DraftID, CreateNextDraft"
    status: pending
  - id: service-fc
    content: "FantasyCriticService.EditLeague: Draft 1 guards, EnableBids, OneShotMode on LeagueYear"
    status: pending
  - id: service-acquisition
    content: "GameAcquisitionService: reject bids when !EnableBids; OneShotMode via leagueYear"
    status: pending
  - id: web-controllers
    content: "LeagueManagerController/LeagueController: OneShotMode, createNextDraft endpoint"
    status: pending
  - id: web-viewmodels
    content: LeagueYearDraftViewModel; settings include enableBids and draft list
    status: pending
  - id: frontend
    content: leagueYearSettings.vue EnableBids toggle; Create Draft button; draft status labels
    status: pending
isProject: false
---

# Multi Draft Leagues

## Architecture Overview

```mermaid
flowchart TD
    LeagueYear -->|"has many"| LeagueYearDraft
    LeagueYearDraft -->|"status per draft"| PlayStatus
    LeagueYearDraft --> PublisherDraftInfos["PublisherDraftInfos[]"]
    Publisher --> DraftInfos["DraftInfos: PublisherDraftInfo[]"]
    PublisherGame -->|"nullable FK"| LeagueYearDraft
    LeagueYear --> EnableBids["Options.EnableBids"]
    LeagueYear -->|"computed from drafts"| ComputedPlayStatus["PlayStatus (computed)"]
    LeagueYear -->|"computed from drafts"| OneShotMode["OneShotMode (computed)"]
```

## 1. Database Schema — new migration script

File: `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/YYYY-MM-DD_NNN_multiDraftLeagues.sql`

**New table `tbl_league_yeardraft`:**

- `DraftID` char(36) PK
- `LeagueID` char(36) FK → `tbl_league`
- `Year` year
- `DraftNumber` tinyint (1-based, UNIQUE per LeagueID+Year)
- `GamesToDraft` int
- `CounterPicksToDraft` int
- `PlayStatus` varchar(50)
- `DraftStartedTimestamp` timestamp NULL

**New table `tbl_league_yeardraftpublisher`** (draft order per publisher per draft):

- `DraftID` char(36) FK → `tbl_league_yeardraft`
- `PublisherID` char(36) FK → `tbl_league_publisher`
- `DraftPosition` tinyint
- PRIMARY KEY (`DraftID`, `PublisherID`)

**Migrate existing data:**
- `INSERT INTO tbl_league_yeardraft … SELECT LeagueID, Year, 1, GamesToDraft, CounterPicksToDraft, PlayStatus, DraftStartedTimestamp FROM tbl_league_year`
- `INSERT INTO tbl_league_yeardraftpublisher … SELECT <new DraftID>, PublisherID, DraftPosition FROM tbl_league_publisher WHERE DraftPosition IS NOT NULL` (join via LeagueID+Year to find the matching DraftID=1 row)
- `UPDATE tbl_league_publishergame SET DraftID = <DraftID for that year> WHERE DraftPosition IS NOT NULL` (link drafted games to their Draft 1 row)

**Alter `tbl_league_year`:**
- DROP `GamesToDraft`, `CounterPicksToDraft`, `PlayStatus`, `DraftOrderSet`, `DraftStartedTimestamp`
- ADD **`EnableBids`** bit(1) NOT NULL DEFAULT 1

**Alter `tbl_league_publisher`:**
- DROP `DraftPosition` (now lives in `tbl_league_yeardraftpublisher`)

**Alter `tbl_league_publishergame`:**
- ADD `DraftID` char(36) NULL FK → `tbl_league_yeardraft`

## 2. Domain Types

**`PublisherDraftInfo`** — `src/FantasyCritic.Lib/Domain/PublisherDraftInfo.cs`

```csharp
public class PublisherDraftInfo
{
    public Guid DraftID { get; }
    public Guid PublisherID { get; }
    public int DraftPosition { get; }
}
```

**`LeagueYearDraft`** — `src/FantasyCritic.Lib/Domain/LeagueYearDraft.cs`

```csharp
public class LeagueYearDraft
{
    public Guid DraftID { get; }
    public LeagueYearKey LeagueYearKey { get; }
    public int DraftNumber { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public PlayStatus PlayStatus { get; }
    public IReadOnlyList<PublisherDraftInfo> PublisherDraftInfos { get; }
    public Instant? DraftStartedTimestamp { get; }
    public bool DraftOrderSet => PublisherDraftInfos.Any();
}
```

**[`Publisher.cs`](src/FantasyCritic.Lib/Domain/Publisher.cs)**

- Remove scalar **`DraftPosition`** from constructor and properties.
- Add **`IReadOnlyList<PublisherDraftInfo> DraftInfos`** (one entry per draft this publisher participates in for the year).
- Optional helper: `int? GetDraftPosition(Guid draftID)` via lookup in `DraftInfos`.

Draft order can be resolved from either side: `leagueYear.CurrentDraft.PublisherDraftInfos` or `publisher.GetDraftPosition(currentDraft.DraftID)`.

## 3. Changes to `LeagueOptions` — [`src/FantasyCritic.Lib/Domain/LeagueOptions.cs`](src/FantasyCritic.Lib/Domain/LeagueOptions.cs)

- **Remove** `GamesToDraft`, `CounterPicksToDraft` properties and constructor parameters
- **Add** **`EnableBids`** bool property (general bids-on/off; not tied to multi-draft naming)
- **Remove** `OneShotMode` (moves to `LeagueYear` — see below)
- Update `Validate()` to remove GamesToDraft/CounterPicksToDraft checks (those move to per-draft validation)

## 4. Changes to `LeagueYear` — [`src/FantasyCritic.Lib/Domain/LeagueYear.cs`](src/FantasyCritic.Lib/Domain/LeagueYear.cs)

- **Remove** constructor params: `playStatus`, `draftOrderSet`, `draftStartedTimestamp`
- **Add** constructor param: `IEnumerable<LeagueYearDraft> drafts`
- **Add** computed properties:
  - `IReadOnlyList<LeagueYearDraft> Drafts`
  - `LeagueYearDraft? CurrentDraft` → the draft that is active/paused, or if none active, the highest-numbered DraftFinal draft, or the highest-numbered NotStartedDraft draft
  - `PlayStatus PlayStatus` → `CurrentDraft?.PlayStatus ?? PlayStatus.NotStartedDraft`
  - `bool DraftOrderSet` → `CurrentDraft?.DraftOrderSet ?? false`
  - `Instant? DraftStartedTimestamp` → `CurrentDraft?.DraftStartedTimestamp`
  - `bool OneShotMode` → `!Options.EnableBids && Options.StandardGames == Drafts.Sum(d => d.GamesToDraft) && Options.CounterPicks == Drafts.Sum(d => d.CounterPicksToDraft) && <existing drop/trade conditions on Options>`

## 5. Changes to `LeagueYearParameters` — [`src/FantasyCritic.Lib/Domain/Requests/LeagueYearParameters.cs`](src/FantasyCritic.Lib/Domain/Requests/LeagueYearParameters.cs)

- Keep `GamesToDraft` and `CounterPicksToDraft` — they represent Draft 1 configuration when creating/editing a year before Draft 1 starts
- Add **`EnableBids`** bool

## 6. New `CreateDraftParameters` Request Type

New file: `src/FantasyCritic.Lib/Domain/Requests/CreateDraftParameters.cs`

Used by the manager to add a subsequent draft to a league year:

- `LeagueYearKey`
- `int GamesToDraft`
- `int CounterPicksToDraft`

## 7. Repo Interface — [`src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`](src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs)

Drafts are **not** fetched via a separate public method. They are loaded whenever `GetLeagueYear` (and related loaders) run, and attached to `LeagueYear.Drafts`.

**Public changes:**

- **Do not add** `GetDraftsForLeagueYear`.
- `Task CreateDraft(LeagueYearDraft draft)` — adds a new draft row (for subsequent drafts)
- `Task StartDraft(LeagueYear leagueYear)` — updates `tbl_league_yeardraft` via `leagueYear.CurrentDraft`
- `Task CompleteDraft(LeagueYear leagueYear)` — same
- `Task SetDraftPause(LeagueYear leagueYear, bool pause)` — same
- `Task ResetDraft(LeagueYear leagueYear)` — targets current draft row; if `DraftNumber > 1`, delete the row rather than reset Draft 1
- `Task EditLeagueYear(LeagueYear leagueYear)` — updates `tbl_league_year` (including `EnableBids`); updates Draft 1 row when draft hasn't started

Private mapping helpers inside `MySQLFantasyCriticRepo` are fine for assembling drafts from SP result sets.

## 8. Stored Procedures — `Scripts/Idempotent/Stored Procedures/`

These are idempotent `DROP … CREATE` files, updated directly (no sequential script needed):

**`sp_getleagueyear.sql`** — primary loader, needs two new result sets added:
- Add `SELECT * FROM tbl_league_yeardraft WHERE LeagueID = P_LeagueID AND Year = P_Year`
- Add `SELECT dp.* FROM tbl_league_yeardraftpublisher dp JOIN tbl_league_yeardraft d ON dp.DraftID = d.DraftID WHERE d.LeagueID = P_LeagueID AND d.Year = P_Year`
- Fix the mid-procedure result set that selects `ly.PlayStatus` — join `tbl_league_yeardraft` instead
- The `SELECT * FROM tbl_league_year` result set omits migrated columns and includes `EnableBids`

**`sp_getconferenceyeardata.sql`** — fix the `CASE WHEN ly.PlayStatus <> 'NotStartedDraft'` expression:
- LEFT JOIN `tbl_league_yeardraft` and check draft status there instead of `tbl_league_year.PlayStatus`

**`sp_getleaguesforuser.sql`** — inline one-shot detection currently reads `GamesToDraft` / `CounterPicksToDraft` from `tbl_league_year`:
- After migration, JOIN `tbl_league_yeardraft` and sum draft counts per year; keep other predicates on `tbl_league_year` (including `EnableBids` where relevant)

## 9. MySQL Implementation — `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs`

- All `UPDATE tbl_league_year SET PlayStatus = …` → `UPDATE tbl_league_yeardraft SET PlayStatus = … WHERE DraftID = @draftID`
- New entity classes: `LeagueYearDraftEntity`, `LeagueYearDraftPublisherEntity` (DB mapping only; domain uses `PublisherDraftInfo`)
- In **`GetLeagueYear`** / `QueryMultiple`: read draft + draft-publisher result sets; build `LeagueYearDraft` with `PublisherDraftInfos`; build each `Publisher` with **`DraftInfos`** aggregated across drafts for that `PublisherID`
- `SetDraftOrder`: writes to `tbl_league_yeardraftpublisher` (DELETE existing for this DraftID, INSERT new rows)
- Update `PublisherGameEntity` to include nullable `DraftID` field

**[`LeagueYearEntity`](src/FantasyCritic.Lib/SharedSerialization/Database/LeagueYearEntity.cs)** — remove migrated columns; add `EnableBids`; `ToDomain()` accepts drafts from repo mapping (not from this entity alone).

## 10. Service Changes

**`DraftService`** — [`src/FantasyCritic.Lib/Services/DraftService.cs`](src/FantasyCritic.Lib/Services/DraftService.cs)

- `StartDraft`: uses `leagueYear.CurrentDraft` for all checks
- `CompleteDraft`: count check uses `leagueYear.CurrentDraft.GamesToDraft * publisherCount`
- `DraftGame`: links new `PublisherGame.DraftID = currentDraft.DraftID`
- `CreateNextDraft`: uses `leagueYear.Drafts` already on the loaded year; creates new row with `DraftNumber = maxExisting + 1`

**`DraftFunctions`:** resolve pick order from `CurrentDraft.PublisherDraftInfos` or `publisher.GetDraftPosition(currentDraft.DraftID)` — not `publisher.DraftPosition`.

**`FantasyCriticService.EditLeague`** — [`src/FantasyCritic.Lib/Services/FantasyCriticService.cs`](src/FantasyCritic.Lib/Services/FantasyCriticService.cs)

- `GamesToDraft`/`CounterPicksToDraft` guard logic targets Draft 1: blocked if Draft 1's `PlayStatus.DraftFinished`; adjustable if Draft 1 not yet started
- **`EnableBids`** flows through `LeagueOptions` / `LeagueYearParameters`
- `OneShotMode` references shift from `Options.OneShotMode` → `leagueYear.OneShotMode`

**`GameAcquisitionService`** — bid/pickup guard changes:

- Reject bids when **`!leagueYear.Options.EnableBids`**
- `OneShotMode` check: `leagueYear.OneShotMode` (was `leagueYear.Options.OneShotMode`)

## 11. Web Layer

**Controllers** — `LeagueManagerController.cs`, `LeagueController.cs`

- `OneShotMode` references: `leagueYear.OneShotMode` throughout
- New endpoint: `POST /api/leaguemanager/createNextDraft` → calls `DraftService.CreateNextDraft`
- Existing draft endpoints operate on `leagueYear.CurrentDraft`

**ViewModels**

- `PlayStatusViewModel` — no structural change; fed by computed `leagueYear.PlayStatus`
- Update `LeagueYearSettingsViewModel` / response models to include **`enableBids`** and a list of draft summaries
- New `LeagueYearDraftViewModel` with `DraftID`, `DraftNumber`, `GamesToDraft`, `CounterPicksToDraft`, `PlayStatus`

**`RequiredYearStatus`** — no structural change; still reads `leagueYear.PlayStatus` (now computed)

## 12. Frontend — `src/FantasyCritic.Web/ClientApp/`

- `leagueYearSettings.vue` — **`EnableBids`** toggle (general “allow bids”, not multi-draft-specific); `GamesToDraft` only editable when Draft 1 hasn't started
- League manager page — new "Create Draft #N" button (visible after Draft 1 is final and year not finished)
- Draft status displays — show which draft number is currently active (e.g. "Draft 2 in progress")
- `leagueMixin.js` — `oneShotMode` reads from API as today

## Key Constraints / Risks

- `GamesToDraft` is referenced in ~15+ places across Lib, MySQL, Web, and Discord — the removal from `LeagueOptions` is the biggest ripple change
- `OneShotMode` moving from `LeagueOptions` to `LeagueYear` touches every controller/service that calls `Options.OneShotMode`
- `Publisher.DraftPosition` removal ripples into `DraftFunctions.GetNextDraftPublisher` and `GetDraftPositions` — use `PublisherDraftInfo` instead
- `LeagueYearEntity` in `SharedSerialization/Database/` must be updated before the stack compiles cleanly
- The DB migration must atomically copy draft data and drop old columns — needs careful testing against existing year data
- `sp_getleagueyear` returns multiple result sets in a fixed order; C# `QueryMultiple` read order must match new result sets
- `tbl_caching_leagueyear` is already dropped — no cache table to maintain for one-shot listing
- `GetPublisherSlots` on `Publisher.cs` does not use `GamesToDraft` and is unaffected
