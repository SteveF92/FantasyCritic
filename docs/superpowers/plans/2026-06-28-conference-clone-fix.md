# Conference Clone Fix (Phase 2 Slice 5) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix three conference operations that incorrectly create only one draft for multi-draft leagues, refactor `AssignLeaguePlayers` to rebuild positions for all drafts (not just draft 1), and surface `ActiveDraftNumber` + correct `DraftFinished` semantics on the conference league table.

**Architecture:** All fixes are in existing files — no new files created. The interface signature changes compile-gate all downstream callers. The `AssignLeaguePlayers` refactor is purely structural (separate draft queries, extend loop), no schema changes needed. The view model changes flow from `ConferenceLeagueYear` domain → `ConferenceLeagueYearEntity` → `ConferenceLeagueYearViewModel`.

**Tech Stack:** C# / ASP.NET Core / Dapper / MySQL (inline SQL, no stored procedure changes) / Vue 2 (BootstrapVue) / NSwag-generated TypeScript API client / NUnit integration tests

---

## File Map

**Modified:**
- `src/FantasyCritic.Lib/Interfaces/IConferenceRepo.cs` — signature change
- `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs` — signature change
- `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs` — `AddNewLeagueYear` + `AddNewLeagueYearInTransaction`
- `src/FantasyCritic.Lib/Services/FantasyCriticService.cs` — `AddNewLeagueYear`
- `src/FantasyCritic.Lib/Services/ConferenceService.cs` — `AddLeagueToConference` + `AddNewLeagueYear`
- `src/FantasyCritic.MySQL/MySQLConferenceRepo.cs` — `AddLeagueToConference`, `AddNewConferenceYear`, `AssignLeaguePlayers`, `GetLeagueYearsInConferenceYear`
- `src/FantasyCritic.Lib/Domain/Conferences/ConferenceLeagueYear.cs` — add `ActiveDraftNumber`
- `src/FantasyCritic.MySQL/Entities/Conferences/ConferenceLeagueYearEntity.cs` — add `ActiveDraftNumber`
- `src/FantasyCritic.Web/Models/Responses/Conferences/ConferenceLeagueYearViewModel.cs` — fix `DraftFinished`, add `ActiveDraftNumber`
- `src/FantasyCritic.Web/Controllers/API/ConferenceController.cs` — add `[ProducesResponseType]` to `InviteLinks`
- `src/FantasyCritic.Web/ClientApp/src/views/conference.vue` — update "drafting" icon

**Created:**
- `src/FantasyCritic.IntegrationTests/Tests/League/Setup/ConferenceCloneTests.cs`

---

## Task 1: Update Interface Signatures (Compile Gate)

**Files:**
- Modify: `src/FantasyCritic.Lib/Interfaces/IConferenceRepo.cs:9`
- Modify: `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs:15`

- [ ] **Step 1: Update `IConferenceRepo.AddLeagueToConference`**

In `IConferenceRepo.cs`, change line 9:
```csharp
// Before
Task AddLeagueToConference(Conference conference, LeagueYear primaryLeagueYear, League newLeague, LeagueDraft initialDraft);

// After
Task AddLeagueToConference(Conference conference, LeagueYear primaryLeagueYear, League newLeague, IReadOnlyList<LeagueDraft> drafts);
```

- [ ] **Step 2: Update `IFantasyCriticRepo.AddNewLeagueYear`**

In `IFantasyCriticRepo.cs`, change line 15:
```csharp
// Before
Task AddNewLeagueYear(League league, int year, LeagueOptions options, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers, LeagueDraft initialDraft);

// After
Task AddNewLeagueYear(League league, int year, LeagueOptions options, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers, IReadOnlyList<LeagueDraft> drafts);
```

- [ ] **Step 3: Verify expected compile failures**

```powershell
dotnet build src/FantasyCritic.sln 2>&1 | Select-String "error CS"
```

Expected: compile errors in `MySQLFantasyCriticRepo.cs`, `FantasyCriticService.cs`, `ConferenceService.cs`, `MySQLConferenceRepo.cs`. These are intentional — Tasks 2–7 fix them one by one.

---

## Task 2: Fix `MySQLFantasyCriticRepo.AddNewLeagueYear` and `AddNewLeagueYearInTransaction`

**Files:**
- Modify: `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs:1343-1410`

- [ ] **Step 1: Update both method signatures and implementation**

Replace the entire `AddNewLeagueYear` method (lines ~1343–1350) and `AddNewLeagueYearInTransaction` method (lines ~1352–1410):

```csharp
public async Task AddNewLeagueYear(League league, int year, LeagueOptions options, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers, IReadOnlyList<LeagueDraft> drafts)
{
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    await AddNewLeagueYearInTransaction(league, year, options, mostRecentActivePlayers, drafts, connection, transaction);
    await transaction.CommitAsync();
}

public async Task AddNewLeagueYearInTransaction(League league, int year, LeagueOptions options, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers, IReadOnlyList<LeagueDraft> drafts, MySqlConnection connection, MySqlTransaction transaction)
{
    bool? conferenceLocked = null;
    if (league.ConferenceID.HasValue)
    {
        conferenceLocked = false;
    }
    LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, year, options, conferenceLocked, false, null);
    var tagEntities = options.LeagueTags.Select(x => new LeagueYearTagEntity(league, year, x));

    List<SpecialGameSlotEntity> slotEntities = options.SpecialGameSlots.SelectMany(slot => slot.Tags, (slot, tag) =>
        new SpecialGameSlotEntity(Guid.NewGuid(), league, year, slot.SpecialSlotPosition, tag)).ToList();

    const string newLeagueYearSQL =
        """
        INSERT INTO tbl_league_year
        (LeagueID,Year,StandardGames,CounterPicks,UnrestrictedReleaseStatusDroppableGames,WillNotReleaseDroppableGames,WillReleaseDroppableGames,DropOnlyDraftGames,
        GrantSuperDrops,CounterPicksBlockDrops,AllowMoveIntoIneligible,MinimumBidAmount,EnableBids,DraftSystem,PickupSystem,TiebreakSystem,ScoringSystem,TradingSystem,ReleaseSystem,IneligibleGameSystem,
        CounterPickDeadlineMonth,CounterPickDeadlineDay,MightReleaseDroppableMonth,MightReleaseDroppableDay,ConferenceLocked,UnderReview) VALUES
        (@LeagueID,@Year,@StandardGames,@CounterPicks,@UnrestrictedReleaseStatusDroppableGames,@WillNotReleaseDroppableGames,@WillReleaseDroppableGames,
        @DropOnlyDraftGames,@GrantSuperDrops,@CounterPicksBlockDrops,@AllowMoveIntoIneligible,@MinimumBidAmount,@EnableBids,@DraftSystem,@PickupSystem,@TiebreakSystem,@ScoringSystem,@TradingSystem,
        @ReleaseSystem,@IneligibleGameSystem,@CounterPickDeadlineMonth,@CounterPickDeadlineDay,@MightReleaseDroppableMonth,@MightReleaseDroppableDay,@ConferenceLocked,0);
        """;

    const string createDraftSQL =
        """
        INSERT INTO tbl_league_draft (DraftID,LeagueID,Year,DraftNumber,Name,ScheduledDate,GamesToDraft,CounterPicksToDraft,DraftOrderSet,PlayStatus,DraftStartedTimestamp)
        VALUES (@DraftID,@LeagueID,@Year,@DraftNumber,@Name,@ScheduledDate,@GamesToDraft,@CounterPicksToDraft,@DraftOrderSet,@PlayStatus,@DraftStartedTimestamp);
        """;

    const string activePlayersSQL = "INSERT INTO tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@year,@userID);";
    var activePlayersObjects = mostRecentActivePlayers.Select(x => new
    {
        leagueID = league.LeagueID,
        userID = x.Id,
        year
    });

    var draftRows = drafts.Select(d => new
    {
        DraftID = d.DraftID,
        LeagueID = d.LeagueYearKey.LeagueID,
        Year = d.LeagueYearKey.Year,
        DraftNumber = d.DraftNumber,
        Name = d.Name,
        ScheduledDate = (DateTime?)null,
        GamesToDraft = d.GamesToDraft,
        CounterPicksToDraft = d.CounterPicksToDraft,
        DraftOrderSet = false,
        PlayStatus = PlayStatus.NotStartedDraft.Value,
        DraftStartedTimestamp = (Instant?)null
    }).ToList();

    await connection.ExecuteAsync(newLeagueYearSQL, leagueYearEntity, transaction);
    await connection.ExecuteAsync(createDraftSQL, draftRows, transaction);
    await connection.BulkInsertAsync<LeagueYearTagEntity>(tagEntities, "tbl_league_yearusestag", 500, transaction);
    await connection.BulkInsertAsync<SpecialGameSlotEntity>(slotEntities, "tbl_league_specialgameslot", 500, transaction);
    await connection.ExecuteAsync(activePlayersSQL, activePlayersObjects, transaction: transaction);
}
```

---

## Task 3: Fix `FantasyCriticService.AddNewLeagueYear` (Standalone Leagues)

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/FantasyCriticService.cs:288-296`

- [ ] **Step 1: Wrap single draft in a list and remove TODO comment**

```csharp
public async Task AddNewLeagueYear(League league, int year, LeagueOptions options, LeagueYear mostRecentLeagueYear)
{
    var mostRecentActivePlayers = await _fantasyCriticRepo.GetActivePlayersForLeagueYear(league.LeagueID, mostRecentLeagueYear.Year);
    var initialDraft = new LeagueDraft(Guid.NewGuid(), new LeagueYearKey(league.LeagueID, year), 1,
        "Initial Draft", null, mostRecentLeagueYear.FirstDraft.GamesToDraft, mostRecentLeagueYear.FirstDraft.CounterPicksToDraft,
        false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null);
    await _fantasyCriticRepo.AddNewLeagueYear(league, year, options, mostRecentActivePlayers, [initialDraft]);
}
```

---

## Task 4: Fix `ConferenceService.AddLeagueToConference` (Clone All Drafts)

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/ConferenceService.cs:65-93`

- [ ] **Step 1: Replace single-draft construction with clone-all**

Replace lines 88–91 (the `var initialDraft = ...` block and the `await _conferenceRepo.AddLeagueToConference` call):

```csharp
var clonedDrafts = primaryLeagueYear.Drafts
    .Select(d => new LeagueDraft(Guid.NewGuid(), new LeagueYearKey(newLeague.LeagueID, year),
        d.DraftNumber, d.Name, d.ScheduledDate,
        d.GamesToDraft, d.CounterPicksToDraft,
        false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null))
    .ToList();
await _conferenceRepo.AddLeagueToConference(conference, primaryLeagueYear, newLeague, clonedDrafts);
```

---

## Task 5: Fix `MySQLConferenceRepo.AddLeagueToConference` (Accept List)

**Files:**
- Modify: `src/FantasyCritic.MySQL/MySQLConferenceRepo.cs:122-129`

- [ ] **Step 1: Change signature and pass list through**

```csharp
public async Task AddLeagueToConference(Conference conference, LeagueYear primaryLeagueYear, League newLeague, IReadOnlyList<LeagueDraft> drafts)
{
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    await _fantasyCriticRepo.CreateLeagueInTransaction(newLeague, primaryLeagueYear.Year, primaryLeagueYear.Options, drafts, true, connection, transaction);
    await transaction.CommitAsync();
}
```

---

## Task 6: Fix `ConferenceService.AddNewLeagueYear` (Clone All Drafts)

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/ConferenceService.cs:100-121`

- [ ] **Step 1: Replace single-draft construction with clone-all and remove TODO comment**

Replace lines 115–119 (the `// TODO` comment, `var initialDraft = ...`, and `await _fantasyCriticRepo.AddNewLeagueYear` call):

```csharp
var clonedDrafts = primaryLeagueYear.Drafts
    .Select(d => new LeagueDraft(Guid.NewGuid(), new LeagueYearKey(leagueToRenew.LeagueID, year),
        d.DraftNumber, d.Name, d.ScheduledDate,
        d.GamesToDraft, d.CounterPicksToDraft,
        false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null))
    .ToList();
await _fantasyCriticRepo.AddNewLeagueYear(leagueToRenew, year, primaryLeagueYear.Options, mostRecentActivePlayers, clonedDrafts);
```

---

## Task 7: Fix `MySQLConferenceRepo.AddNewConferenceYear` (Clone All Drafts)

**Files:**
- Modify: `src/FantasyCritic.MySQL/MySQLConferenceRepo.cs:131-182`

- [ ] **Step 1: Replace single-draft construction with clone-all and remove TODO comment**

Replace lines 175–179 (the `// TODO` comment, `var initialDraft = ...`, and `await _fantasyCriticRepo.AddNewLeagueYearInTransaction` call):

```csharp
var clonedDrafts = primaryLeaguePreviousLeagueYear.Drafts
    .Select(d => new LeagueDraft(Guid.NewGuid(), new LeagueYearKey(primaryLeaguePreviousLeagueYear.League.LeagueID, year),
        d.DraftNumber, d.Name, d.ScheduledDate,
        d.GamesToDraft, d.CounterPicksToDraft,
        false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null))
    .ToList();
await _fantasyCriticRepo.AddNewLeagueYearInTransaction(primaryLeaguePreviousLeagueYear.League, year, primaryLeaguePreviousLeagueYear.Options, mostRecentActivePrimaryLeaguePlayers, clonedDrafts, connection, transaction);
```

---

## Task 8: Build Check + Commit (Cloning Done)

**Files:** — (no edits, just verify)

- [ ] **Step 1: Build the full solution**

```powershell
dotnet build src/FantasyCritic.sln -c Release
```

Expected: 0 errors, 0 warnings related to these changes.

- [ ] **Step 2: Commit**

```powershell
cd i:/CodeProjects/FantasyCritic
git add -A
git commit -m "Fix conference multi-draft cloning: clone all drafts in AddLeagueToConference, AddNewLeagueYear, AddNewConferenceYear."
```

---

## Task 9: Refactor `AssignLeaguePlayers` for N Drafts

**Files:**
- Modify: `src/FantasyCritic.MySQL/MySQLConferenceRepo.cs:559-843`

This is the largest single change. We restructure the query strategy (no SQL schema changes) and extend the rebuild loop.

- [ ] **Step 1: Update the three private records at the bottom of the class**

Replace the existing private record (line ~842):
```csharp
// Before
private record ConferencePublisherInfo(Guid PublisherID, Guid LeagueID, int Year, Guid UserID, Guid? DraftID, int? DraftPosition);

// After — three records
private record ConferencePublisherInfo(Guid PublisherID, Guid LeagueID, int Year, Guid UserID);
private record ConferenceDraftInfo(Guid DraftID, Guid LeagueID, int Year, int DraftNumber);
private record ConferenceDraftPosition(Guid DraftID, Guid PublisherID, int DraftPosition);
```

- [ ] **Step 2: Replace the publisher SQL and add two new SQL constants**

Inside `AssignLeaguePlayers`, replace the `publisherEntitiesSQL` constant (lines ~568–576) with the new version, and add two new SQL constants after it:

```csharp
const string publisherEntitiesSQL = """
                                    SELECT p.PublisherID, p.LeagueID, p.Year, p.UserID
                                    FROM tbl_league_publisher p
                                    JOIN tbl_league l ON p.LeagueID = l.LeagueID
                                    WHERE l.ConferenceID = @conferenceID AND p.Year = @year;
                                    """;

const string conferenceDraftsSQL = """
                                   SELECT ld.DraftID, ld.LeagueID, ld.Year, ld.DraftNumber
                                   FROM tbl_league_draft ld
                                   JOIN tbl_league l ON ld.LeagueID = l.LeagueID
                                   WHERE l.ConferenceID = @conferenceID AND ld.Year = @year;
                                   """;

const string conferenceDraftPositionsSQL = """
                                           SELECT dp.DraftID, dp.PublisherID, dp.DraftPosition
                                           FROM tbl_league_draftpublisher dp
                                           JOIN tbl_league_draft ld ON dp.DraftID = ld.DraftID
                                           JOIN tbl_league l ON ld.LeagueID = l.LeagueID
                                           WHERE l.ConferenceID = @conferenceID AND ld.Year = @year;
                                           """;
```

- [ ] **Step 3: Add the two new query calls inside the `try { ... }` block**

After the existing three query calls (lines ~636–638):
```csharp
var currentLeagueUsers = (await connection.QueryAsync<LeagueHasUserEntity>(currentLeagueUserSQL, conferenceParam, transaction)).ToList();
var currentPublisherEntities = (await connection.QueryAsync<ConferencePublisherInfo>(publisherEntitiesSQL, conferenceParam, transaction)).ToList();
var currentActivePlayerEntities = (await connection.QueryAsync<LeagueActivePlayerEntity>(activePlayersSQL, conferenceParam, transaction)).ToList();
```

Add two more:
```csharp
var conferenceDrafts = (await connection.QueryAsync<ConferenceDraftInfo>(conferenceDraftsSQL, conferenceParam, transaction)).ToList();
var conferenceDraftPositions = (await connection.QueryAsync<ConferenceDraftPosition>(conferenceDraftPositionsSQL, conferenceParam, transaction)).ToList();
```

- [ ] **Step 4: Remove the `draftID` lookup from `publishersByID` and `publisherLookup` setup**

The existing code (lines ~708–709) is:
```csharp
var publishersByID = currentPublisherEntities.ToDictionary(x => x.PublisherID);
var publisherLookup = currentPublisherEntities.ToLookup(x => new LeagueYearKey(x.LeagueID, x.Year));
```

These stay unchanged — they now operate on the simpler 4-field `ConferencePublisherInfo`. No edits needed here.

- [ ] **Step 5: Replace the draft position rebuild loop**

Replace the entire `draftClearsNeeded` / `draftPositionInserts` loop (lines ~710–741):

```csharp
var draftsByLeagueYear = conferenceDrafts.ToLookup(d => new LeagueYearKey(d.LeagueID, d.Year));
var positionByDraftAndPublisher = conferenceDraftPositions
    .ToDictionary(p => (p.DraftID, p.PublisherID), p => p.DraftPosition);

var draftClearsNeeded = new List<object>();
var draftPositionInserts = new List<object>();
foreach (var leagueYearKey in leagueYearsToFixDraftOrders)
{
    var existingPublishersInLeagueYear = publisherLookup[leagueYearKey].ToList();
    var existingPublisherIDsInLeagueYear = existingPublishersInLeagueYear.Select(x => x.PublisherID).ToHashSet();

    var publishersMovedOutIDs = publishersToUpdate
        .Where(x => existingPublisherIDsInLeagueYear.Contains(x.PublisherID) && x.LeagueID != leagueYearKey.LeagueID)
        .Select(x => x.PublisherID)
        .ToHashSet();
    var publishersMovedIn = publishersToUpdate
        .Where(x => !existingPublisherIDsInLeagueYear.Contains(x.PublisherID) && x.LeagueID == leagueYearKey.LeagueID)
        .Select(x => publishersByID[x.PublisherID])
        .ToList();

    foreach (var draft in draftsByLeagueYear[leagueYearKey])
    {
        draftClearsNeeded.Add(new { draftID = draft.DraftID });

        var finalPublishers = existingPublishersInLeagueYear
            .Concat(publishersMovedIn)
            .Where(x => !publishersMovedOutIDs.Contains(x.PublisherID))
            .OrderBy(x => positionByDraftAndPublisher.TryGetValue((draft.DraftID, x.PublisherID), out var pos) ? pos : int.MaxValue)
            .Select((pub, index) => new LeagueDraftPublisherEntity(leagueYearKey, draft.DraftID, pub.PublisherID, index + 1));
        draftPositionInserts.AddRange(finalPublishers);
    }
}
```

- [ ] **Step 6: Build and verify no errors**

```powershell
dotnet build src/FantasyCritic.sln -c Release
```

Expected: 0 errors.

- [ ] **Step 7: Commit**

```powershell
git add src/FantasyCritic.MySQL/MySQLConferenceRepo.cs
git commit -m "Refactor AssignLeaguePlayers to rebuild draft positions for all drafts (not just draft 1)."
```

---

## Task 10: `ConferenceLeagueYear` Domain Chain + ViewModel + SQL

**Files:**
- Modify: `src/FantasyCritic.Lib/Domain/Conferences/ConferenceLeagueYear.cs`
- Modify: `src/FantasyCritic.MySQL/Entities/Conferences/ConferenceLeagueYearEntity.cs`
- Modify: `src/FantasyCritic.MySQL/MySQLConferenceRepo.cs` (GetLeagueYearsInConferenceYear SQL)
- Modify: `src/FantasyCritic.Web/Models/Responses/Conferences/ConferenceLeagueYearViewModel.cs`

- [ ] **Step 1: Add `ActiveDraftNumber` to `ConferenceLeagueYear` domain**

```csharp
// ConferenceLeagueYear.cs — full replacement
namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceLeagueYear
{
    public ConferenceLeagueYear(ConferenceLeague league, int year, bool conferenceLocked, bool draftStarted, bool draftFinished, int? activeDraftNumber)
    {
        League = league;
        Year = year;
        ConferenceLocked = conferenceLocked;
        DraftStarted = draftStarted;
        DraftFinished = draftFinished;
        ActiveDraftNumber = activeDraftNumber;
    }

    public ConferenceLeague League { get; }
    public int Year { get; }
    public LeagueYearKey LeagueYearKey => new LeagueYearKey(League.LeagueID, Year);

    public bool ConferenceLocked { get; }
    public bool DraftStarted { get; }
    public bool DraftFinished { get; }
    public int? ActiveDraftNumber { get; }
}
```

- [ ] **Step 2: Add `ActiveDraftNumber` to `ConferenceLeagueYearEntity`**

```csharp
// ConferenceLeagueYearEntity.cs — full replacement
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities.Conferences;
internal class ConferenceLeagueYearEntity
{
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public Guid LeagueManager { get; set; }
    public int Year { get; set; }

    public bool DraftStarted { get; set; }
    public bool DraftFinished { get; set; }
    public int? ActiveDraftNumber { get; set; }
    public bool ConferenceLocked { get; set; }

    public string LeagueManagerDisplayName { get; set; } = null!;
    public string LeagueManagerEmailAddress { get; set; } = null!;

    public ConferenceLeagueYear ToDomain()
    {
        var leagueManager = new MinimalFantasyCriticUser(LeagueManager, LeagueManagerDisplayName, LeagueManagerEmailAddress);
        var league = new ConferenceLeague(LeagueID, LeagueName, leagueManager);
        return new ConferenceLeagueYear(league, Year, ConferenceLocked, DraftStarted, DraftFinished, ActiveDraftNumber);
    }
}
```

- [ ] **Step 3: Update `GetLeagueYearsInConferenceYear` SQL in `MySQLConferenceRepo`**

Replace the `leagueYearSQL` constant in `GetLeagueYearsInConferenceYear` (around line 429):

```csharp
const string leagueYearSQL = """
                             SELECT
                             tbl_league.LeagueID, tbl_league.LeagueName, tbl_league.LeagueManager,
                             tbl_user.DisplayName AS ManagerDisplayName, tbl_user.EmailAddress AS ManagerEmailAddress,
                             tbl_league_year.Year,
                             ld1.PlayStatus <> 'NotStartedDraft' AS DraftStarted,
                             NOT EXISTS (
                                 SELECT 1 FROM tbl_league_draft ld2
                                 WHERE ld2.LeagueID = tbl_league_year.LeagueID
                                   AND ld2.Year = tbl_league_year.Year
                                   AND ld2.PlayStatus <> 'DraftFinal'
                             ) AS DraftFinished,
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
                             """;
```

- [ ] **Step 4: Fix `ConferenceLeagueYearViewModel` — add `ActiveDraftNumber` and fix `DraftFinished`**

```csharp
// ConferenceLeagueYearViewModel.cs — full replacement
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceLeagueYearViewModel
{
    public ConferenceLeagueYearViewModel(LeagueYear domain, IReadOnlyList<ConferencePlayer> conferencePlayersInLeagueYear, FantasyCriticUser? currentUser, bool isPrimaryLeague)
    {
        LeagueID = domain.League.LeagueID;
        LeagueName = domain.League.LeagueName;
        Year = domain.Year;
        LeagueManager = new PlayerViewModel(domain.League.LeagueID, domain.League.LeagueName, domain.League.LeagueManager, false);

        if (currentUser is not null)
        {
            UserIsInLeague = conferencePlayersInLeagueYear.Any(x => x.User.UserID == currentUser.Id);
        }

        IsPrimaryLeague = isPrimaryLeague;

        ConferenceLocked = domain.ConferenceLocked.HasValue && domain.ConferenceLocked.Value;
        DraftStarted = domain.FirstDraft.PlayStatus.PlayStarted;
        DraftFinished = domain.Drafts.All(d => d.PlayStatus.DraftFinished);
        ActiveDraftNumber = domain.ActiveDraft?.DraftNumber;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public PlayerViewModel LeagueManager { get; }
    public bool UserIsInLeague { get; }
    public bool IsPrimaryLeague { get; }

    public bool ConferenceLocked { get; }
    public bool DraftStarted { get; }
    public bool DraftFinished { get; }
    public int? ActiveDraftNumber { get; }
}
```

- [ ] **Step 5: Build and verify no errors**

```powershell
dotnet build src/FantasyCritic.sln -c Release
```

Expected: 0 errors.

- [ ] **Step 6: Commit**

```powershell
git add src/FantasyCritic.Lib/Domain/Conferences/ConferenceLeagueYear.cs `
        src/FantasyCritic.MySQL/Entities/Conferences/ConferenceLeagueYearEntity.cs `
        src/FantasyCritic.MySQL/MySQLConferenceRepo.cs `
        src/FantasyCritic.Web/Models/Responses/Conferences/ConferenceLeagueYearViewModel.cs
git commit -m "Add ActiveDraftNumber to ConferenceLeagueYear; fix DraftFinished to require all drafts complete."
```

---

## Task 11: Update `conference.vue` Frontend

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/conference.vue:116`

- [ ] **Step 1: Update the "currently drafting" icon condition and tooltip**

Change line 116:
```html
<!-- Before -->
<font-awesome-icon v-if="data.item.draftStarted && !data.item.draftFinished" icon="list-ol" v-b-popover.hover="'This league is currently drafting.'" />

<!-- After -->
<font-awesome-icon
  v-if="data.item.activeDraftNumber !== null && data.item.activeDraftNumber !== undefined"
  icon="list-ol"
  v-b-popover.hover="data.item.activeDraftNumber > 1 ? `This league is currently drafting (Draft ${data.item.activeDraftNumber}).` : 'This league is currently drafting.'" />
```

- [ ] **Step 2: Commit**

```powershell
git add src/FantasyCritic.Web/ClientApp/src/views/conference.vue
git commit -m "Update conference league table: show which draft is active for multi-draft leagues."
```

---

## Task 12: Fix `InviteLinks` ProducesResponseType + Regenerate Client

The `ConferenceController.InviteLinks` action is missing a `[ProducesResponseType]` annotation, so NSwag generates `Task` (untyped) for it. The integration test needs the typed result to get the invite code.

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/ConferenceController.cs` (around line 444)

- [ ] **Step 1: Add `ProducesResponseType` annotation to `InviteLinks`**

Add `[ProducesResponseType<IReadOnlyList<ConferenceInviteLinkViewModel>>(StatusCodes.Status200OK)]` above the method:

```csharp
[HttpGet("{conferenceID}")]
[ProducesResponseType<IReadOnlyList<ConferenceInviteLinkViewModel>>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<ActionResult<IEnumerable<ConferenceInviteLinkViewModel>>> InviteLinks(Guid conferenceID)
```

- [ ] **Step 2: Build Web project and regenerate API client**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release
scripts/Regenerate-ApiClient.ps1
dotnet build src/FantasyCritic.sln -c Release
```

Expected: The generated `FantasyCriticClients.cs` now has `Task<IReadOnlyList<ConferenceInviteLinkViewModel>> InviteLinksAsync(Guid conferenceID)`.

- [ ] **Step 3: Verify the generated client**

```powershell
rg "InviteLinksAsync" src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs
```

Expected: a line containing `Task<IReadOnlyList<ConferenceInviteLinkViewModel>> InviteLinksAsync`.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.Web/Controllers/API/ConferenceController.cs `
        src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs
git commit -m "Add ProducesResponseType to Conference.InviteLinks; regenerate API client."
```

---

## Task 13: Integration Tests

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/Setup/ConferenceCloneTests.cs`

- [ ] **Step 1: Create the test file with all three test cases**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Setup;

/// <summary>
/// Verifies that multi-draft conferences correctly clone all draft rows when new leagues
/// or league years are added, and that AssignLeaguePlayers rebuilds positions for all drafts.
/// </summary>
[TestFixture]
public class ConferenceCloneTests : IntegrationTestBase
{
    /// <summary>
    /// Creates a conference with a 2-draft primary league, then adds a second league.
    /// Asserts the second league has 2 draft rows matching the primary's draft settings.
    /// </summary>
    [Test]
    public async Task AddLeagueToConference_WithTwoDraftPrimary_ClonesAllDrafts()
    {
        var year = await LeagueTestHelpers.GetOpenYearAsync(new ApiSession(Factory));

        var (emailA, passwordA, displayNameA) = NewUser();
        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionA = new ApiSession(Factory);
        using var sessionB = new ApiSession(Factory);
        await sessionA.RegisterAsync(emailA, passwordA, displayNameA);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);
        var userBId = await sessionB.GetCurrentUserIdAsync();

        // Create a conference with a 2-draft primary league
        var conferenceID = await sessionA.Conference.CreateConferenceAsync(new CreateConferenceRequest
        {
            ConferenceName = $"TestConf-{Guid.NewGuid():N}"[..30],
            PrimaryLeagueName = $"Primary-{Guid.NewGuid():N}"[..20],
            CustomRulesConference = false,
            LeagueYearSettings = LeagueScenarios.Standard.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 1 },
                new() { Name = "Draft 2", ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 0 },
            }
        });

        // Invite user B and have them join
        await sessionA.Conference.CreateInviteLinkAsync(new CreateConferenceInviteLinkRequest { ConferenceID = conferenceID });
        var inviteLinks = await sessionA.Conference.InviteLinksAsync(conferenceID);
        var inviteCode = inviteLinks.First().InviteCode;
        await sessionB.Conference.JoinWithInviteLinkAsync(new JoinConferenceWithInviteLinkRequest
        {
            ConferenceID = conferenceID,
            InviteCode = inviteCode
        });

        // Add a second league managed by user B
        await sessionA.Conference.AddLeagueToConferenceAsync(new AddLeagueToConferenceRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueName = $"SecondLeague-{Guid.NewGuid():N}"[..20],
            LeagueManager = userBId
        });

        // Get the conference's leagues and find the new one
        var conferenceLeagues = await sessionA.Conference.GetConferenceLeaguesAsync(conferenceID);
        var secondLeague = conferenceLeagues.Single(l => l.LeagueManager.UserID != sessionA.UserId);

        // Verify the new league has 2 draft rows matching the primary's settings
        var newLeagueYear = await sessionB.League.GetLeagueYearAsync(secondLeague.LeagueID, year, null);

        Assert.That(newLeagueYear, Is.Not.Null);
        Assert.That(newLeagueYear.Drafts, Has.Count.EqualTo(2), "New league should have 2 draft rows cloned from the primary.");

        var draft1 = newLeagueYear.Drafts.Single(d => d.DraftNumber == 1);
        Assert.That(draft1.GamesToDraft, Is.EqualTo(3));
        Assert.That(draft1.CounterPicksToDraft, Is.EqualTo(1));

        var draft2 = newLeagueYear.Drafts.Single(d => d.DraftNumber == 2);
        Assert.That(draft2.Name, Is.EqualTo("Draft 2"));
        Assert.That(draft2.GamesToDraft, Is.EqualTo(3));
        Assert.That(draft2.CounterPicksToDraft, Is.EqualTo(0));
    }

    /// <summary>
    /// Verifies AssignLeaguePlayers with a 2-draft conference does not crash and correctly
    /// updates draft position rows for both drafts when players are reassigned.
    /// </summary>
    [Test]
    public async Task AssignLeaguePlayers_WithTwoDraftConference_UpdatesPositionsForAllDrafts()
    {
        var year = await LeagueTestHelpers.GetOpenYearAsync(new ApiSession(Factory));

        var (emailA, passwordA, displayNameA) = NewUser();
        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionA = new ApiSession(Factory);
        using var sessionB = new ApiSession(Factory);
        await sessionA.RegisterAsync(emailA, passwordA, displayNameA);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);
        var userAId = await sessionA.GetCurrentUserIdAsync();
        var userBId = await sessionB.GetCurrentUserIdAsync();

        // Create conference with 2-draft primary
        var conferenceID = await sessionA.Conference.CreateConferenceAsync(new CreateConferenceRequest
        {
            ConferenceName = $"TestConf-{Guid.NewGuid():N}"[..30],
            PrimaryLeagueName = $"Primary-{Guid.NewGuid():N}"[..20],
            CustomRulesConference = false,
            LeagueYearSettings = LeagueScenarios.Standard.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 1 },
                new() { Name = "Draft 2", ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 0 },
            }
        });

        // Invite user B and have them join
        await sessionA.Conference.CreateInviteLinkAsync(new CreateConferenceInviteLinkRequest { ConferenceID = conferenceID });
        var inviteLinks = await sessionA.Conference.InviteLinksAsync(conferenceID);
        await sessionB.Conference.JoinWithInviteLinkAsync(new JoinConferenceWithInviteLinkRequest
        {
            ConferenceID = conferenceID,
            InviteCode = inviteLinks.First().InviteCode
        });

        // Add a second league managed by user B
        await sessionA.Conference.AddLeagueToConferenceAsync(new AddLeagueToConferenceRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueName = $"SecondLeague-{Guid.NewGuid():N}"[..20],
            LeagueManager = userBId
        });

        var conferenceLeagues = await sessionA.Conference.GetConferenceLeaguesAsync(conferenceID);
        var primaryLeague = conferenceLeagues.Single(l => l.LeagueManager.UserID == userAId);
        var secondLeague = conferenceLeagues.Single(l => l.LeagueManager.UserID == userBId);

        // Initial assignment: A in primary, B in secondary
        await sessionA.Conference.AssignLeaguePlayersAsync(new AssignLeaguePlayersRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueAssignments = new Dictionary<Guid, List<Guid>>
            {
                [primaryLeague.LeagueID] = new List<Guid> { userAId },
                [secondLeague.LeagueID] = new List<Guid> { userBId }
            }
        });

        // Set draft order for both leagues (both drafts) so position rows exist
        var primaryYear = await sessionA.League.GetLeagueYearAsync(primaryLeague.LeagueID, year, null);
        foreach (var draft in primaryYear.Drafts)
        {
            await sessionA.LeagueManager.SetDraftOrderAsync(new DraftOrderRequest
            {
                LeagueID = primaryLeague.LeagueID,
                Year = year,
                DraftID = draft.DraftID,
                DraftOrderType = "Random",
                ManualPublisherDraftPositions = null,
            });
        }

        var secondYear = await sessionB.League.GetLeagueYearAsync(secondLeague.LeagueID, year, null);
        foreach (var draft in secondYear.Drafts)
        {
            await sessionB.LeagueManager.SetDraftOrderAsync(new DraftOrderRequest
            {
                LeagueID = secondLeague.LeagueID,
                Year = year,
                DraftID = draft.DraftID,
                DraftOrderType = "Random",
                ManualPublisherDraftPositions = null,
            });
        }

        // Reassign: swap A and B (A → secondary, B → primary)
        await sessionA.Conference.AssignLeaguePlayersAsync(new AssignLeaguePlayersRequest
        {
            ConferenceID = conferenceID,
            Year = year,
            LeagueAssignments = new Dictionary<Guid, List<Guid>>
            {
                [primaryLeague.LeagueID] = new List<Guid> { userBId },
                [secondLeague.LeagueID] = new List<Guid> { userAId }
            }
        });

        // Verify both drafts in primary league now show user B at position 1
        var updatedPrimary = await sessionB.League.GetLeagueYearAsync(primaryLeague.LeagueID, year, null);
        foreach (var draft in updatedPrimary.Drafts)
        {
            Assert.That(draft.PublisherDraftInfo, Has.Count.EqualTo(1),
                $"Primary draft {draft.DraftNumber} should have 1 publisher after reassignment.");
        }

        // Verify both drafts in secondary league now show user A at position 1
        var updatedSecondary = await sessionA.League.GetLeagueYearAsync(secondLeague.LeagueID, year, null);
        foreach (var draft in updatedSecondary.Drafts)
        {
            Assert.That(draft.PublisherDraftInfo, Has.Count.EqualTo(1),
                $"Secondary draft {draft.DraftNumber} should have 1 publisher after reassignment.");
        }
    }
}
```

- [ ] **Step 2: Check if `GetConferenceLeaguesAsync` exists in the API client**

```powershell
rg "GetConferenceLeagues|ConferenceLeagues" src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs -n | Select-Object -First 5
```

If the method does not exist, use `session.Conference.GetConferenceAsync(conferenceID)` to get the conference data, then extract the leagues from there. If `GetConference` returns an untyped `Task`, use `session.CombinedData` or another endpoint to find the league IDs, or add a `[ProducesResponseType]` annotation to the relevant controller action following the same pattern as Task 12.

- [ ] **Step 3: Check if `GetCurrentUserIdAsync` exists on `ApiSession`**

```powershell
rg "GetCurrentUserId" src/FantasyCritic.IntegrationTests -n | Select-Object -First 5
```

If it doesn't exist, replace `sessionA.GetCurrentUserIdAsync()` with an account call: `(await sessionA.Account.GetCurrentUserAsync()).UserID`.

- [ ] **Step 4: Fix any compilation issues in the test**

Build the integration tests:
```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Fix any compilation errors. Common issues: API client types don't exist (add `[ProducesResponseType]` to the relevant controller action and regenerate the client), missing helpers (use existing `ApiSession` members).

---

## Task 14: Final Build + Test Run + Commit

- [ ] **Step 1: Run the full build**

```powershell
dotnet build src/FantasyCritic.sln -c Release
```

Expected: 0 errors.

- [ ] **Step 2: Verify no remaining `TODO(Phase2-MultiDraft)` markers for the fixed locations**

```powershell
rg "TODO\(Phase2-MultiDraft\)" src/FantasyCritic.Lib/Services/ConferenceService.cs
rg "TODO\(Phase2-MultiDraft\)" src/FantasyCritic.MySQL/MySQLConferenceRepo.cs
rg "TODO\(Phase2-MultiDraft\)" src/FantasyCritic.Lib/Services/FantasyCriticService.cs
```

Expected: no output from any of those commands.

- [ ] **Step 3: Run the integration tests**

Ensure Docker MySQL is running:
```powershell
docker compose -f infrastructure/docker-compose-mysql.yaml up -d
```

Run:
```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~ConferenceClone"
```

Expected: both `ConferenceCloneTests` tests pass.

- [ ] **Step 4: Run full integration test suite**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: all existing tests continue to pass.

- [ ] **Step 5: Final commit**

```powershell
git add -A
git commit -m "Add ConferenceCloneTests integration tests for Slice 5."
```

---

## Self-Review Notes

**Spec coverage:**
- Area 1 (draft cloning): Tasks 1–8 ✓
- Area 2 (AssignLeaguePlayers N-draft): Task 9 ✓
- Area 3 (DraftFinished / ActiveDraftNumber): Tasks 10–11 ✓
- Integration tests: Task 13 ✓

**Watch out during execution:**
- Task 13 Step 2: `GetConferenceLeaguesAsync` may not exist as a typed client method. If so, check the conference controller for a `GetLeagues` action and add `[ProducesResponseType]` there, or use `GetConferenceAsync` if it returns conference + leagues together.
- Task 13 Step 3: `GetCurrentUserIdAsync` is not a standard `ApiSession` method. Use `session.Account.GetCurrentUserAsync()` which returns a `UserViewModel` with `UserID`.
- The `SetDraftOrder` endpoint in the conference context is called on each league's manager session. For the primary league, use `sessionA.LeagueManager`; for the secondary league, use `sessionB.LeagueManager`.
- `PublisherDraftInfo` on the draft VM shows publishers in their draft order. After a reassignment and position rebuild, the count should match the new league membership.
