# Counter Picks Must Be From This Draft — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add per-draft `CounterPicksMustBeFromThisDraft` (default `true`) so live counter-pick phases only allow targeting standard games drafted in the same draft.

**Architecture:** Store the flag on `tbl_league_draft`, thread through domain/API/UI, and enforce at two points during drafting: filter `GetAvailableCounterPicks` and validate in `CanClaimGame` via `activeDraftID`. Single-draft leagues hide the UI; post-draft bidding is unchanged.

**Tech Stack:** .NET 10 / C#, MySQL (DbUp), Vue 2 + TypeScript ClientApp, NUnit integration tests, NSwag API client.

**Spec:** `docs/superpowers/specs/2026-07-01-counter-picks-must-be-from-this-draft-design.md`

---

## File map

| File | Responsibility |
| ---- | -------------- |
| `DatabaseUpdater/Scripts/Sequential/2026-07-01_000_counterPicksMustBeFromThisDraft.sql` | Add column, backfill, drop default |
| `Lib/Domain/LeagueDraft.cs` | Domain property, update helpers, `GetDifferences` |
| `Lib/SharedSerialization/Database/LeagueDraftEntity.cs` | DB row shape + `ToDomain` |
| `Lib/Domain/Requests/DraftParameters.cs` | Create/edit draft parameter |
| `Lib/Domain/Requests/EditLeagueDraftParameters.cs` | Edit draft parameter |
| `Lib/Domain/Requests/CreateLeagueDraftParameters.cs` | Add-draft parameter |
| `Lib/Services/DraftService.cs` | `GetAvailableCounterPicks` filter, `EditDraft`/`CreateDraft` |
| `Lib/BusinessLogicFunctions/GameEligibilityFunctions.cs` | `CanClaimGame` draft-ID check |
| `Lib/Services/GameAcquisitionService.cs` | Pass `draftID` into `CanClaimGame` |
| `Lib/Services/FantasyCriticService.cs` | Create league draft construction |
| `Lib/Services/ConferenceService.cs` | Conference create + clone paths |
| `MySQL/MySQLFantasyCriticRepo.cs` | INSERT/UPDATE `tbl_league_draft` |
| `MySQL/MySQLConferenceRepo.cs` | Clone draft construction |
| `FakeRepo/TestUtilities/LeagueDraftEntityMap.cs` | CSV test data default `true` |
| `Web/Models/Requests/LeagueManager/DraftSettingsRequest.cs` | API request |
| `Web/Models/Requests/LeagueManager/EditLeagueDraftRequest.cs` | API request |
| `Web/Models/Requests/LeagueManager/CreateLeagueDraftRequest.cs` | API request |
| `Web/Models/Requests/League/LeagueDraftViewModel.cs` | API response |
| `Web/Controllers/API/LeagueController.cs` | Pass `ActiveDraft` to counter-pick list |
| `Test/CounterPickDraftRestrictionTests.cs` | Unit tests (new) |
| `IntegrationTests/Tests/League/MultiDraft/CounterPicksMustBeFromThisDraftTests.cs` | Integration tests (new) |
| `ClientApp/src/utilities/leagueCreationPresets.ts` | Default on `DraftSettings` |
| `ClientApp/src/components/DraftCreationSettings.vue` | Checkbox (multi-draft only) |
| `ClientApp/src/views/manageDrafts.vue` | Read + edit checkbox |

Stored procedures use `SELECT *` on `tbl_league_draft` — no proc changes needed.

---

### Task 1: Database migration

**Files:**
- Create: `src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-07-01_000_counterPicksMustBeFromThisDraft.sql`

- [ ] **Step 1: Add migration script**

```sql
ALTER TABLE tbl_league_draft
  ADD COLUMN CounterPicksMustBeFromThisDraft bit(1) NOT NULL DEFAULT b'1';

UPDATE tbl_league_draft SET CounterPicksMustBeFromThisDraft = b'1';

ALTER TABLE tbl_league_draft
  MODIFY COLUMN CounterPicksMustBeFromThisDraft bit(1) NOT NULL;
```

- [ ] **Step 2: Run DatabaseUpdater against local MySQL**

Run: `dotnet run --project src/FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj`
Expected: Migration applies without error.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.DatabaseUpdater/Scripts/Sequential/2026-07-01_000_counterPicksMustBeFromThisDraft.sql
git commit -m "Add CounterPicksMustBeFromThisDraft column to tbl_league_draft."
```

---

### Task 2: Domain model and entity

**Files:**
- Modify: `src/FantasyCritic.Lib/Domain/LeagueDraft.cs`
- Modify: `src/FantasyCritic.Lib/SharedSerialization/Database/LeagueDraftEntity.cs`
- Modify: `src/FantasyCritic.Lib/Domain/Requests/DraftParameters.cs`
- Modify: `src/FantasyCritic.Lib/Domain/Requests/EditLeagueDraftParameters.cs`
- Modify: `src/FantasyCritic.Lib/Domain/Requests/CreateLeagueDraftParameters.cs`
- Modify: `src/FantasyCritic.FakeRepo/TestUtilities/LeagueDraftEntityMap.cs`

- [ ] **Step 1: Add property to `LeagueDraft`**

Insert `bool counterPicksMustBeFromThisDraft` after `counterPicksToDraft` in the constructor:

```csharp
public LeagueDraft(Guid draftID, LeagueYearKey leagueYearKey, int draftNumber, string name, LocalDate? scheduledDate,
    int gamesToDraft, int counterPicksToDraft, bool counterPicksMustBeFromThisDraft, bool draftOrderSet, PlayStatus playStatus,
    IEnumerable<PublisherDraftInfo> publisherDraftInfo, Instant? draftStartedTimestamp)
{
    // ...existing assignments...
    CounterPicksMustBeFromThisDraft = counterPicksMustBeFromThisDraft;
}

public bool CounterPicksMustBeFromThisDraft { get; }
```

Update both `UpdateDraft` overloads to preserve the flag:

```csharp
public LeagueDraft UpdateDraft(int gamesToDraft, int counterPicksToDraft)
{
    return new LeagueDraft(DraftID, LeagueYearKey, DraftNumber, Name, ScheduledDate, gamesToDraft, counterPicksToDraft,
        CounterPicksMustBeFromThisDraft, DraftOrderSet, PlayStatus, PublisherDraftInfo, DraftStartedTimestamp);
}

public LeagueDraft UpdateDraft(string name, LocalDate? scheduledDate, int gamesToDraft, int counterPicksToDraft, bool counterPicksMustBeFromThisDraft)
{
    return new LeagueDraft(DraftID, LeagueYearKey, DraftNumber, name, scheduledDate, gamesToDraft, counterPicksToDraft,
        counterPicksMustBeFromThisDraft, DraftOrderSet, PlayStatus, PublisherDraftInfo, DraftStartedTimestamp);
}
```

Add to `GetDifferences`:

```csharp
if (CounterPicksMustBeFromThisDraft != existingDraft.CounterPicksMustBeFromThisDraft)
{
    differences.Add($"Counter picks must be from this draft changed from {existingDraft.CounterPicksMustBeFromThisDraft} to {CounterPicksMustBeFromThisDraft}.");
}
```

- [ ] **Step 2: Update `LeagueDraftEntity`**

```csharp
public bool CounterPicksMustBeFromThisDraft { get; set; }

// In ToDomain:
return new LeagueDraft(DraftID, new LeagueYearKey(LeagueID, Year), DraftNumber, Name, ScheduledDate,
    GamesToDraft, CounterPicksToDraft, CounterPicksMustBeFromThisDraft, DraftOrderSet,
    Lib.Enums.PlayStatus.FromValue(PlayStatus), publisherDraftInfos, DraftStartedTimestamp);
```

- [ ] **Step 3: Update request types**

`DraftParameters.cs`:
```csharp
public record DraftParameters(
    string? Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft,
    bool CounterPicksMustBeFromThisDraft = true);
```

`EditLeagueDraftParameters.cs` — add constructor param + property `bool CounterPicksMustBeFromThisDraft`.

`CreateLeagueDraftParameters.cs` — add constructor param + property `bool CounterPicksMustBeFromThisDraft`.

- [ ] **Step 4: Fix all `new LeagueDraft(` call sites**

Run: `rg "new LeagueDraft\(" src -g "*.cs"`

Add `counterPicksMustBeFromThisDraft` argument (use `true` as default at clone/create sites, copy `d.CounterPicksMustBeFromThisDraft` at clone sites). Key files:

- `FantasyCriticService.cs` (create league + renew year)
- `ConferenceService.cs` (3 sites)
- `MySQLConferenceRepo.cs`
- `DraftService.cs` (`CreateDraft`, `EditDraft`)
- `LeagueDraft.cs` (`UpdateDraft` internals — done above)
- `LeagueDraftEntity.cs` (`ToDomain` — done above)
- Test files: `EligibilityTests.cs`, `GetDraftStatusTestBuilder.cs`, `BaseGameNewsTests.cs`, any others from ripgrep

- [ ] **Step 5: Update `LeagueDraftEntityMap`**

```csharp
Map(m => m.CounterPicksMustBeFromThisDraft).Constant(true);
```

- [ ] **Step 6: Build Lib + Test projects**

Run: `dotnet build src/FantasyCritic.Lib/FantasyCritic.Lib.csproj`
Expected: Build succeeds (MySQL/Web may still fail until Task 7/9).

- [ ] **Step 7: Commit**

```powershell
git add src/FantasyCritic.Lib src/FantasyCritic.FakeRepo src/FantasyCritic.Test
git commit -m "Add CounterPicksMustBeFromThisDraft to LeagueDraft domain model."
```

---

### Task 3: Unit test — `GetAvailableCounterPicks` filtering

**Files:**
- Create: `src/FantasyCritic.Test/CounterPickDraftRestrictionTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class CounterPickDraftRestrictionTests
{
    private static DraftService CreateDraftService() =>
        new(null!, null!, null!, null!, null!, null!, null!);

    [Test]
    public void GetAvailableCounterPicks_WhenFlagOn_ExcludesGamesFromOtherDrafts()
    {
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var leagueYear = BuildMultiDraftLeagueYear(draftID1, draftID2, counterPicksMustBeFromThisDraft: true);
        var picker = leagueYear.Publishers[0];
        var activeDraft = leagueYear.Drafts.Single(d => d.DraftID == draftID2);

        var results = CreateDraftService().GetAvailableCounterPicks(leagueYear, picker, activeDraft);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].DraftID, Is.EqualTo(draftID2));
    }

    [Test]
    public void GetAvailableCounterPicks_WhenFlagOff_IncludesGamesFromOtherDrafts()
    {
        var draftID1 = Guid.NewGuid();
        var draftID2 = Guid.NewGuid();
        var leagueYear = BuildMultiDraftLeagueYear(draftID1, draftID2, counterPicksMustBeFromThisDraft: false);
        var picker = leagueYear.Publishers[0];
        var activeDraft = leagueYear.Drafts.Single(d => d.DraftID == draftID2);

        var results = CreateDraftService().GetAvailableCounterPicks(leagueYear, picker, activeDraft);

        Assert.That(results, Has.Count.EqualTo(2));
    }

    // BuildMultiDraftLeagueYear: 2 publishers, 2 drafts.
    // Publisher[1] has standard game A (DraftID=draftID1) and standard game B (DraftID=draftID2).
    // Active draft is draft 2 with flag as specified.
}
```

Implement `BuildMultiDraftLeagueYear` helper in the same file using minimal `League`, `LeagueYear`, `Publisher`, `PublisherGame`, `MasterGame` stubs (follow patterns from `EligibilityTests.cs`).

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj -c Release --filter "FullyQualifiedName~CounterPickDraftRestrictionTests"`
Expected: FAIL — method signature or filtering not implemented.

- [ ] **Step 3: Commit failing tests**

```powershell
git add src/FantasyCritic.Test/CounterPickDraftRestrictionTests.cs
git commit -m "Add failing tests for counter-pick draft restriction filtering."
```

---

### Task 4: Implement `GetAvailableCounterPicks` filtering

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/DraftService.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueController.cs`

- [ ] **Step 1: Change method signature and add filter**

```csharp
public IReadOnlyList<PublisherGame> GetAvailableCounterPicks(LeagueYear leagueYear, Publisher publisherMakingCounterPick, LeagueDraft? activeDraft)
{
    IReadOnlyList<Publisher> otherPublishers = leagueYear.GetAllPublishersExcept(publisherMakingCounterPick);
    IReadOnlyList<PublisherGame> gamesForYear = leagueYear.Publishers.SelectMany(x => x.PublisherGames).ToList();
    IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).Where(x => !x.CounterPick).ToList();

    if (activeDraft?.CounterPicksMustBeFromThisDraft == true)
    {
        otherPlayersGames = otherPlayersGames.Where(x => x.DraftID == activeDraft.DraftID).ToList();
    }

    // ...existing loop unchanged...
}
```

- [ ] **Step 2: Update call sites**

`DraftService.cs` auto-draft path (~line 307):
```csharp
var availableCounterPicks = GetAvailableCounterPicks(updatedLeagueYear, nextPublisher, updatedLeagueYear.ActiveDraft)
```

`LeagueController.cs` `PossibleCounterPicks`:
```csharp
var availableCounterPicks = _draftService.GetAvailableCounterPicks(leagueYear, publisher, leagueYear.ActiveDraft);
```

- [ ] **Step 3: Run unit tests**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj -c Release --filter "FullyQualifiedName~CounterPickDraftRestrictionTests"`
Expected: PASS

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.Lib/Services/DraftService.cs src/FantasyCritic.Web/Controllers/API/LeagueController.cs
git commit -m "Filter available counter picks by draft when restriction is enabled."
```

---

### Task 5: Unit test — `CanClaimGame` draft restriction

**Files:**
- Modify: `src/FantasyCritic.Test/CounterPickDraftRestrictionTests.cs`

- [ ] **Step 1: Add failing tests**

```csharp
[Test]
public void CanClaimGame_DraftingCounterPick_WhenFlagOn_RejectsGameFromOtherDraft()
{
    var draftID1 = Guid.NewGuid();
    var draftID2 = Guid.NewGuid();
    var leagueYear = BuildMultiDraftLeagueYear(draftID1, draftID2, counterPicksMustBeFromThisDraft: true);
    var picker = leagueYear.Publishers[0];
    var gameFromDraft1 = leagueYear.Publishers[1].PublisherGames.Single(g => g.DraftID == draftID1);

    var request = new ClaimGameDomainRequest(leagueYear, picker, gameFromDraft1.MasterGame!.MasterGame,
        gameFromDraft1.GameName, true, false, false, null, null);

    var result = GameEligibilityFunctions.CanClaimGame(
        request, null, null, true, drafting: true, partOfSpecialAuction: false,
        counterPickWillBeConditionallyDropped: false, currentDate: new LocalDate(2026, 6, 1),
        allowIneligibleSlot: false, allTags: [], activeDraftID: draftID2);

    Assert.That(result.Success, Is.False);
    Assert.That(result.Errors.Any(e => e.ErrorMessage.Contains("not drafted in this draft")), Is.True);
}

[Test]
public void CanClaimGame_DraftingCounterPick_WhenFlagOff_AllowsGameFromOtherDraft()
{
    // Same setup with counterPicksMustBeFromThisDraft: false on draft 2
    // Assert result does NOT contain the draft restriction error (may still fail other checks — assert on absence of that specific error, or full success if helper builds valid game)
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj -c Release --filter "FullyQualifiedName~CanClaimGame_DraftingCounterPick"`
Expected: FAIL — `activeDraftID` parameter does not exist.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.Test/CounterPickDraftRestrictionTests.cs
git commit -m "Add failing tests for CanClaimGame draft counter-pick restriction."
```

---

### Task 6: Implement `CanClaimGame` restriction and wire `ClaimGame`

**Files:**
- Modify: `src/FantasyCritic.Lib/BusinessLogicFunctions/GameEligibilityFunctions.cs`
- Modify: `src/FantasyCritic.Lib/Services/GameAcquisitionService.cs`

- [ ] **Step 1: Add `activeDraftID` parameter to `CanClaimGame`**

```csharp
public static ClaimResult CanClaimGame(ClaimGameDomainRequest request, Instant? nextBidTime, int? validDropSlot, bool acquiringNow, bool drafting,
    bool partOfSpecialAuction, bool counterPickWillBeConditionallyDropped, LocalDate currentDate, bool allowIneligibleSlot,
    IReadOnlyList<MasterGameTag> allTags, Guid? activeDraftID = null)
```

Inside the `if (request.CounterPick)` block, after existing checks:

```csharp
if (drafting && activeDraftID.HasValue && request.MasterGame is not null)
{
    var activeDraft = request.LeagueYear.Drafts.SingleOrDefault(d => d.DraftID == activeDraftID.Value);
    if (activeDraft?.CounterPicksMustBeFromThisDraft == true)
    {
        bool otherPlayerHasGameFromThisDraft = gameSet.OtherPlayerStandardGames
            .Any(pg => pg.MasterGame is not null
                && pg.MasterGame.MasterGame.MasterGameID == request.MasterGame.MasterGameID
                && pg.DraftID == activeDraft.DraftID);
        if (!otherPlayerHasGameFromThisDraft)
        {
            claimErrors.Add(new ClaimError("That game was not drafted in this draft.", false));
        }
    }
}
```

- [ ] **Step 2: Update all `CanClaimGame` callers to pass `activeDraftID: null`**

Run: `rg "CanClaimGame\(" src -g "*.cs"`

Default `null` handles bid paths. Update `GameAcquisitionService.ClaimGame`:

```csharp
ClaimResult claimResult = GameEligibilityFunctions.CanClaimGame(
    request, null, null, true, draftID.HasValue, false, false, _clock.GetToday(), allowIneligibleSlot, allTags, draftID);
```

Other callers (`ActionProcessor`, bid paths) pass nothing extra — default `null` is correct.

- [ ] **Step 3: Run unit tests**

Run: `dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj -c Release --filter "FullyQualifiedName~CounterPickDraftRestrictionTests"`
Expected: PASS

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.Lib/BusinessLogicFunctions/GameEligibilityFunctions.cs src/FantasyCritic.Lib/Services/GameAcquisitionService.cs
git commit -m "Reject draft counter picks targeting games from other drafts."
```

---

### Task 7: MySQL persistence

**Files:**
- Modify: `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs`

- [ ] **Step 1: Update INSERT statements**

Find all `INSERT INTO tbl_league_draft` statements. Add `CounterPicksMustBeFromThisDraft` column and `@CounterPicksMustBeFromThisDraft` value from `draft.CounterPicksMustBeFromThisDraft`.

- [ ] **Step 2: Update UPDATE statements**

`EditLeagueDraft` UPDATE (~line 1308):
```sql
SET Name = @name, ScheduledDate = @scheduledDate, GamesToDraft = @gamesToDraft,
    CounterPicksToDraft = @counterPicksToDraft, CounterPicksMustBeFromThisDraft = @counterPicksMustBeFromThisDraft
```

Any other UPDATE on `tbl_league_draft` that sets draft settings — add the column if applicable.

- [ ] **Step 3: Build solution**

Run: `dotnet build src/FantasyCritic.sln -c Release`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs
git commit -m "Persist CounterPicksMustBeFromThisDraft in tbl_league_draft."
```

---

### Task 8: Service layer — create, edit, clone

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/DraftService.cs`
- Modify: `src/FantasyCritic.Lib/Services/FantasyCriticService.cs`
- Modify: `src/FantasyCritic.Lib/Services/ConferenceService.cs`
- Modify: `src/FantasyCritic.MySQL/MySQLConferenceRepo.cs`

- [ ] **Step 1: `DraftService.CreateDraft`**

Pass `domainRequest.CounterPicksMustBeFromThisDraft` into `new LeagueDraft(...)`.

- [ ] **Step 2: `DraftService.EditDraft`**

Add to started-draft guard:
```csharp
(domainRequest.CounterPicksMustBeFromThisDraft != draft.CounterPicksMustBeFromThisDraft)
```

Build `updatedDraft` with `domainRequest.CounterPicksMustBeFromThisDraft`.

- [ ] **Step 3: `FantasyCriticService` create league**

```csharp
var drafts = parameters.Drafts.Select((d, i) => new LeagueDraft(
    Guid.NewGuid(), leagueYearKey, i + 1,
    d.Name ?? (i == 0 ? "Initial Draft" : $"Draft {i + 1}"),
    d.ScheduledDate, d.GamesToDraft, d.CounterPicksToDraft,
    d.CounterPicksMustBeFromThisDraft,
    false, PlayStatus.NotStartedDraft, new List<PublisherDraftInfo>(), null))
```

Same for `initialDraft` in renew-year path (~line 291).

- [ ] **Step 4: Conference clone paths**

`ConferenceService.cs` (3 `new LeagueDraft` sites) and `MySQLConferenceRepo.cs` — copy `d.CounterPicksMustBeFromThisDraft`.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.Lib/Services src/FantasyCritic.MySQL/MySQLConferenceRepo.cs
git commit -m "Thread CounterPicksMustBeFromThisDraft through create, edit, and clone flows."
```

---

### Task 9: Web API request/response types

**Files:**
- Modify: `src/FantasyCritic.Web/Models/Requests/LeagueManager/DraftSettingsRequest.cs`
- Modify: `src/FantasyCritic.Web/Models/Requests/LeagueManager/EditLeagueDraftRequest.cs`
- Modify: `src/FantasyCritic.Web/Models/Requests/LeagueManager/CreateLeagueDraftRequest.cs`
- Modify: `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs`

- [ ] **Step 1: Add property to request types**

`DraftSettingsRequest`:
```csharp
public record DraftSettingsRequest(
    string? Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft,
    bool CounterPicksMustBeFromThisDraft = true)
{
    public DraftParameters ToDomain(int draftIndex)
    {
        string resolvedName = Name ?? (draftIndex == 0 ? "Initial Draft" : $"Draft {draftIndex + 1}");
        return new DraftParameters(resolvedName, ScheduledDate, GamesToDraft, CounterPicksToDraft, CounterPicksMustBeFromThisDraft);
    }
}
```

`EditLeagueDraftRequest` — add `bool CounterPicksMustBeFromThisDraft = true` to record; pass through in `ToDomain()`.

`CreateLeagueDraftRequest` — add `bool CounterPicksMustBeFromThisDraft = true`; pass through in `ToDomain()`.

- [ ] **Step 2: Add to `LeagueDraftViewModel`**

```csharp
CounterPicksMustBeFromThisDraft = domain.CounterPicksMustBeFromThisDraft;
// property:
public bool CounterPicksMustBeFromThisDraft { get; }
```

- [ ] **Step 3: Build Web project**

Run: `dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.Web/Models
git commit -m "Expose CounterPicksMustBeFromThisDraft on draft API types."
```

---

### Task 10: Regenerate API client

**Files:**
- Modify: `src/FantasyCritic.ApiClient/` (generated)

- [ ] **Step 1: Regenerate client**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build src/FantasyCritic.sln -c Release
```

Expected: `CreateLeagueDraftRequest`, `EditLeagueDraftRequest`, `DraftSettingsRequest`, `LeagueDraftViewModel` include `CounterPicksMustBeFromThisDraft`.

- [ ] **Step 2: Commit**

```powershell
git add src/FantasyCritic.ApiClient
git commit -m "Regenerate API client for CounterPicksMustBeFromThisDraft."
```

---

### Task 11: Integration tests

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/MultiDraft/CounterPicksMustBeFromThisDraftTests.cs`
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` (optional — `BuildDraftSettings` can rely on default `true`)

- [ ] **Step 1: Write integration test fixture**

```csharp
[TestFixture]
public class CounterPicksMustBeFromThisDraftTests : IntegrationTestBase
{
    // OneTimeSetUp:
    // 1. Create league with 2 drafts (Draft 1: 2 std + 1 cpk, Draft 2: 2 std + 1 cpk) via CreateLeague with 2 DraftSettingsRequest entries
    // 2. Set publishers, draft order, start Draft 1, complete Draft 1 (std + cpk)
    // 3. Start Draft 2, complete standard rounds only (leave counter-pick phase open)
    //
    // Test: PossibleCounterPicks returns only games with DraftID == draft2
    // Test: Attempting to counter-pick a Draft 1 game via DraftGame API fails
    // Test: Edit Draft 2 to set CounterPicksMustBeFromThisDraft = false (before start would be needed — instead create second fixture with flag off before draft 2 starts)
}
```

Use `LeagueFixtureBuilder`, `MockedLivePlayer`, and `DraftSimulator` patterns from `MultiDraftCrudTests.cs` and `LeagueDraftTestBase.cs`.

For the flag-off test: create a separate league, set `CounterPicksMustBeFromThisDraft = false` on Draft 2 via `EditLeagueDraftAsync` before starting Draft 2, then assert Draft 1 games appear in `PossibleCounterPicks`.

- [ ] **Step 2: Run integration tests**

```powershell
docker compose -f infrastructure/docker-compose-mysql.yaml up -d
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~CounterPicksMustBeFromThisDraftTests"
```

Expected: PASS

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests
git commit -m "Add integration tests for counter-pick draft restriction."
```

---

### Task 12: Frontend — types and defaults

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts`

- [ ] **Step 1: Extend `DraftSettings` interface**

```typescript
export interface DraftSettings {
  name: string | null;
  scheduledDate: string | null;
  gamesToDraft: number;
  counterPicksToDraft: number;
  counterPicksMustBeFromThisDraft: boolean;
}
```

- [ ] **Step 2: Default `true` in all draft builders**

Update `buildOneShotDraft`, `getDefaultDraft`, and multi-draft preset in `computePreset`:

```typescript
counterPicksMustBeFromThisDraft: true
```

For multi-draft preset second draft with `counterPicksToDraft: 0`, the property can still be `true` (UI won't show it).

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.Web/ClientApp/src/utilities/leagueCreationPresets.ts
git commit -m "Default counterPicksMustBeFromThisDraft to true in league creation presets."
```

---

### Task 13: Frontend — `DraftCreationSettings.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/DraftCreationSettings.vue`

- [ ] **Step 1: Add checkbox after counter picks field**

Show when `gameMode === 'Multi Draft' && draft.counterPicksToDraft > 0`:

```vue
<div v-if="gameMode === 'Multi Draft' && draft.counterPicksToDraft > 0" class="form-group">
  <b-form-checkbox v-model="draft.counterPicksMustBeFromThisDraft" @input="emitUpdate">
    <span class="checkbox-label">Lorem ipsum dolor sit amet</span>
    <p>Consectetur adipiscing elit, sed do eiusmod tempor incididunt.</p>
  </b-form-checkbox>
</div>
```

- [ ] **Step 2: Normalize in watcher**

When mapping drafts in the `value` watcher, default missing field:

```typescript
counterPicksMustBeFromThisDraft: d.counterPicksMustBeFromThisDraft ?? true
```

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.Web/ClientApp/src/components/DraftCreationSettings.vue
git commit -m "Add counter-pick draft restriction checkbox to draft creation settings."
```

---

### Task 14: Frontend — `manageDrafts.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue`

- [ ] **Step 1: Read view**

Show when `leagueYear.drafts.length > 1 && draft.counterPicksToDraft > 0`:

```vue
<div v-if="leagueYear.drafts.length > 1 && draft.counterPicksToDraft > 0">
  <strong>Counter Picks From This Draft Only:</strong>
  {{ draft.counterPicksMustBeFromThisDraft ? 'Yes' : 'No' }}
</div>
```

- [ ] **Step 2: Edit form**

Add to `editForm` data, `startEdit`, and `submitEdit` model. Checkbox with same visibility rule, disabled when `draft.playStatus !== 'NotStartedDraft'`.

- [ ] **Step 3: New draft form**

Add checkbox to "Add Another Draft" section when `newDraft.counterPicksToDraft > 0`, default `true` in `newDraft` data and `resetNewDraftForm`. Include in `submitNewDraft` model.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue
git commit -m "Add counter-pick draft restriction to manage drafts UI."
```

---

### Task 15: Final verification

- [ ] **Step 1: Run full unit test suite**

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj -c Release
```

Expected: PASS

- [ ] **Step 2: Run integration tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: PASS

- [ ] **Step 3: Manual smoke test**

1. Create multi-draft league with counter picks on Draft 2 — checkbox visible and checked by default.
2. Complete Draft 1, start Draft 2 standard rounds, enter counter-pick phase — picker shows only Draft 2 games.
3. Uncheck flag on Draft 3 (before start) — after Draft 3 standard rounds, counter-pick list includes earlier draft games.
