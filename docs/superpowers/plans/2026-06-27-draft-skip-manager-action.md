# Manager SkipCurrentDraftPick Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a pause-gated league manager action to skip the current draft pick, extend undo to reverse either the last pick or the last skip, and improve undo action logging from the generic "Publisher Game Removed" to draft-specific action types.

**Architecture:** A new `SkipCurrentDraftPick` service method inserts into `tbl_league_draftpickskip` using the coordinates from `DraftStatus.NextPick`. Undo is extended with a skip branch (deletes the row) and the pick branch is rerouted to log "Draft Pick Undone" instead of "Publisher Game Removed". The frontend adds a sidebar entry and confirmation modal mirroring the existing undo/reset patterns.

**Tech Stack:** C# / ASP.NET Core, Dapper / MySQL, Vue 2 (Options API), NUnit integration tests, NSwag-generated API client.

---

## File Map

| File | Change |
|---|---|
| `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs` | Add `AddDraftPickSkip` and `RemoveDraftPickSkip` signatures |
| `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs` | Implement both repo methods |
| `src/FantasyCritic.Lib/Services/DraftService.cs` | Add `SkipCurrentDraftPick`; refactor `UndoLastDraftAction` |
| `src/FantasyCritic.Lib/Services/PublisherService.cs` | Add `UndoDraftPick` method (draft-undo path, different action type) |
| `src/FantasyCritic.Web/Models/Requests/LeagueManager/SkipCurrentDraftPickRequest.cs` | New request record |
| `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs` | Add `NextPickPublisherName`, `NextPickRoundNumber`, `NextPickIsCounterPick` |
| `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs` | Add `SkipCurrentDraftPick` action |
| `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue` | Add sidebar entry + import |
| `src/FantasyCritic.Web/ClientApp/src/components/modals/skipCurrentDraftPickModal.vue` | New modal (mirrors `resetDraftModal.vue`) |
| `src/FantasyCritic.IntegrationTests/Tests/League/Draft/EdgeCases/DraftSkipManagerActionTests.cs` | New integration test fixture |
| `scripts/Regenerate-ApiClient.ps1` (run, not edited) | Regenerate NSwag client after controller change |

---

## Task 1: Add repo interface + MySQL implementation

**Files:**
- Modify: `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`
- Modify: `src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs`

- [ ] **Step 1: Add the two method signatures to `IFantasyCriticRepo`**

Open `src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs`. After the `SetDraftOrder` line (currently around line 120) add:

```csharp
Task AddDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, LeagueManagerAction action);
Task RemoveDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, LeagueManagerAction action);
```

- [ ] **Step 2: Implement `AddDraftPickSkip` in `MySQLFantasyCriticRepo`**

Add this method. Place it near the other draft-related methods (after `SetDraftPause`, around line 1054):

```csharp
public async Task AddDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, LeagueManagerAction action)
{
    const string sql = "INSERT INTO tbl_league_draftpickskip (DraftID, PublisherID, CounterPick, PickNumber) " +
                       "VALUES (@draftID, @publisherID, @counterPick, @pickNumber);";
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    await connection.ExecuteAsync(sql,
        new { draftID = draft.DraftID, publisherID = publisher.PublisherID, counterPick, pickNumber },
        transaction);
    await AddLeagueManagerAction(action, connection, transaction);
    await transaction.CommitAsync();
}
```

- [ ] **Step 3: Implement `RemoveDraftPickSkip` in `MySQLFantasyCriticRepo`**

Add immediately after `AddDraftPickSkip`:

```csharp
public async Task RemoveDraftPickSkip(LeagueDraft draft, Publisher publisher, bool counterPick, int pickNumber, LeagueManagerAction action)
{
    const string sql = "DELETE FROM tbl_league_draftpickskip " +
                       "WHERE DraftID = @draftID AND PublisherID = @publisherID AND CounterPick = @counterPick AND PickNumber = @pickNumber;";
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    await using var transaction = await connection.BeginTransactionAsync();
    var deleted = await connection.ExecuteAsync(sql,
        new { draftID = draft.DraftID, publisherID = publisher.PublisherID, counterPick, pickNumber },
        transaction);
    if (deleted != 1)
    {
        await transaction.RollbackAsync();
        throw new Exception($"RemoveDraftPickSkip failed: row not found for DraftID={draft.DraftID}, PublisherID={publisher.PublisherID}");
    }
    await AddLeagueManagerAction(action, connection, transaction);
    await transaction.CommitAsync();
}
```

- [ ] **Step 4: Check that `AddLeagueManagerAction(action, connection, transaction)` overload exists**

Search `MySQLFantasyCriticRepo.cs` for `private async Task AddLeagueManagerAction`. The private overload that accepts a connection and transaction should already exist (same pattern used by `ResetDraft`). If not, add it following the existing `AddLeagueAction` private overload pattern.

- [ ] **Step 5: Build to confirm no compile errors**

```powershell
dotnet build src/FantasyCritic.MySQL/FantasyCritic.MySQL.csproj -c Release
```

Expected: `Build succeeded.  0 Error(s)`

- [ ] **Step 6: Commit**

```
git add src/FantasyCritic.Lib/Interfaces/IFantasyCriticRepo.cs src/FantasyCritic.MySQL/MySQLFantasyCriticRepo.cs
git commit -m "Add AddDraftPickSkip and RemoveDraftPickSkip repo methods."
```

---

## Task 2: Add `UndoDraftPick` to `PublisherService`

This is a new method that removes a publisher game during draft undo and logs the draft-specific action type "Draft Pick Undone" — it does not call `RemovePublisherGame`, which always logs "Publisher Game Removed".

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/PublisherService.cs`

- [ ] **Step 1: Add `UndoDraftPick` method**

Open `src/FantasyCritic.Lib/Services/PublisherService.cs`. Add after the existing `RemovePublisherGame` method (around line 68):

```csharp
public async Task UndoDraftPick(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame)
{
    var now = _clock.GetCurrentInstant();
    var formerPublisherGame = publisherGame.GetFormerPublisherGame(now, "Removed by league manager (draft undo)");
    var gameName = publisherGame.GameName ?? "Unknown";
    var leagueAction = new LeagueAction(publisher, now, "Draft Pick Undone",
        $"Undid draft pick: '{gameName}'.", managerAction: true);
    await _fantasyCriticRepo.ManagerRemovePublisherGame(leagueYear, publisher, publisherGame, formerPublisherGame, leagueAction);
}
```

Note: unlike `RemovePublisherGame`, this method does **not** call `_discordPushService.SendLeagueManagerManualPublisherGameMessage`.

- [ ] **Step 2: Build to confirm**

```powershell
dotnet build src/FantasyCritic.Lib/FantasyCritic.Lib.csproj -c Release
```

Expected: `Build succeeded.  0 Error(s)`

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.Lib/Services/PublisherService.cs
git commit -m "Add UndoDraftPick to PublisherService with Draft Pick Undone action type."
```

---

## Task 3: Add `SkipCurrentDraftPick` and extend `UndoLastDraftAction` in `DraftService`

**Files:**
- Modify: `src/FantasyCritic.Lib/Services/DraftService.cs`

- [ ] **Step 1: Add `SkipCurrentDraftPick` method**

Open `src/FantasyCritic.Lib/Services/DraftService.cs`. Add after `UndoLastDraftAction` (after line 99):

```csharp
public async Task<Result> SkipCurrentDraftPick(LeagueYear leagueYear)
{
    if (leagueYear.ActiveDraft is null)
    {
        throw new Exception($"No active draft found for league: {leagueYear.Key}");
    }

    var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
    if (draftStatus is null)
    {
        return Result.Failure("Draft is already complete.");
    }

    var nextPick = draftStatus.NextPick;
    bool alreadySkipped = nextPick.Publisher
        .GetDraftInfo(leagueYear.ActiveDraft.DraftID)
        ?.PickSkips
        .Any(s => s.CounterPick == nextPick.CounterPick && s.PickNumber == nextPick.RoundNumber)
        ?? false;

    if (alreadySkipped)
    {
        return Result.Failure("That turn has already been skipped.");
    }

    var slotType = nextPick.CounterPick ? "counter-pick" : "standard game";
    var publisherName = nextPick.Publisher.GetPublisherAndUserDisplayName();
    var description = $"{publisherName} was skipped for round {nextPick.RoundNumber} ({slotType}).";
    var action = new LeagueManagerAction(leagueYear.Key, _clock.GetCurrentInstant(), "Draft Pick Skipped", description);

    await _fantasyCriticRepo.AddDraftPickSkip(leagueYear.ActiveDraft, nextPick.Publisher, nextPick.CounterPick, nextPick.RoundNumber, action);
    return Result.Success();
}
```

- [ ] **Step 2: Find `GetDraftInfo` helper on `Publisher`**

Check `Publisher.cs` for a method that returns `PublisherDraftInfo` for a specific `DraftID`. If it does not exist, use this alternative for the already-skipped check:

```csharp
bool alreadySkipped = nextPick.Publisher.DraftInfos
    .Where(i => i.DraftID == leagueYear.ActiveDraft.DraftID)
    .SelectMany(i => i.PickSkips)
    .Any(s => s.CounterPick == nextPick.CounterPick && s.PickNumber == nextPick.RoundNumber);
```

Look at `Publisher.cs` to find the correct property name for the collection of `PublisherDraftInfo` items. Search the file for `PublisherDraftInfo` property.

- [ ] **Step 3: Extend `UndoLastDraftAction` with skip branch**

Replace the entire `UndoLastDraftAction` method body (currently lines 65–99) with:

```csharp
public async Task<Result> UndoLastDraftAction(LeagueYear leagueYear)
{
    if (leagueYear.ActiveDraft is null)
    {
        throw new Exception($"No active draft found for league: {leagueYear.Key}");
    }

    if (!leagueYear.ActiveDraft.PlayStatus.PlayStarted || leagueYear.ActiveDraft.PlayStatus.DraftFinished)
    {
        return Result.Failure("Cannot undo a draft action when the draft is not active.");
    }

    var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
    var previousPick = draftStatus?.PreviousPick
        ?? GetDraftStatusForUndo(leagueYear);

    if (previousPick is null)
    {
        return Result.Failure("There is nothing to undo.");
    }

    if (previousPick.Skipped)
    {
        // Undo a manager skip
        var slotType = previousPick.CounterPick ? "counter-pick" : "standard game";
        var publisherName = previousPick.Publisher.GetPublisherAndUserDisplayName();
        var description = $"Undid skip for {publisherName}, round {previousPick.RoundNumber} ({slotType}).";
        var action = new LeagueManagerAction(leagueYear.Key, _clock.GetCurrentInstant(), "Draft Skip Undone", description);
        await _fantasyCriticRepo.RemoveDraftPickSkip(leagueYear.ActiveDraft, previousPick.Publisher, previousPick.CounterPick, previousPick.RoundNumber, action);
        return Result.Success();
    }

    // Undo a drafted game pick
    var publisherGamesForDraft = leagueYear.Publishers
        .SelectMany(x => x.PublisherGames.Where(g => g.DraftID == leagueYear.ActiveDraft.DraftID))
        .ToList();

    if (!publisherGamesForDraft.Any())
    {
        return Result.Failure("Cannot undo a draft pick when no games have been drafted yet.");
    }

    var counterPicks = publisherGamesForDraft.Where(x => x.CounterPick).ToList();
    PublisherGame publisherGameToUndo = counterPicks.Any()
        ? counterPicks.MaxBy(x => x.OverallDraftPosition)!
        : publisherGamesForDraft.MaxBy(x => x.OverallDraftPosition)!;

    var publisher = leagueYear.Publishers.Single(x => x.PublisherID == publisherGameToUndo.PublisherID);
    await _publisherService.UndoDraftPick(leagueYear, publisher, publisherGameToUndo);
    return Result.Success();
}
```

Note: `GetDraftStatusForUndo` is a private helper explained in step 4.

- [ ] **Step 4: Understand `PreviousPick` when draft completes**

`DraftFunctions.GetDraftStatus` returns `null` when the draft is complete. But `UndoLastDraftAction` requires `DraftPaused` status, so the draft cannot be complete. Therefore `draftStatus` will never be null here, and the `PreviousPick` can come directly from it.

Simplify the previous-pick logic — remove the `GetDraftStatusForUndo` helper reference from step 3 and use:

```csharp
var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
if (draftStatus?.PreviousPick is null)
{
    // Draft just started with no picks and no skips yet
    return Result.Failure("There is nothing to undo.");
}
var previousPick = draftStatus.PreviousPick;
```

Update the method body accordingly.

- [ ] **Step 5: Build**

```powershell
dotnet build src/FantasyCritic.Lib/FantasyCritic.Lib.csproj -c Release
```

Expected: `Build succeeded.  0 Error(s)`

- [ ] **Step 6: Commit**

```
git add src/FantasyCritic.Lib/Services/DraftService.cs
git commit -m "Add SkipCurrentDraftPick; extend UndoLastDraftAction with skip branch."
```

---

## Task 4: Add `Publisher.GetDraftInfo` helper (if needed)

**Files:**
- Possibly modify: `src/FantasyCritic.Lib/Domain/Publisher.cs`

- [ ] **Step 1: Check if a `GetDraftInfo(Guid draftID)` or equivalent already exists**

Search `Publisher.cs` for:
- `GetDraftInfo`
- `PublisherDraftInfo`
- `DraftInfos`

If `Publisher` has a public property (e.g. `PublisherDraftInfos`) returning `IReadOnlyList<PublisherDraftInfo>`, use that directly in `SkipCurrentDraftPick` (step 2 of Task 3) instead of adding a helper.

- [ ] **Step 2: If no clean accessor exists, add a helper**

Add to `Publisher.cs`:

```csharp
public PublisherDraftInfo? GetDraftInfo(Guid draftID) =>
    PublisherDraftInfos.SingleOrDefault(i => i.DraftID == draftID);
```

(Use the correct property name found in step 1.)

- [ ] **Step 3: Fix any compile error in `DraftService.SkipCurrentDraftPick` that referenced the wrong property name**

Build again:

```powershell
dotnet build src/FantasyCritic.Lib/FantasyCritic.Lib.csproj -c Release
```

---

## Task 5: Add `SkipCurrentDraftPickRequest` and controller action

**Files:**
- Create: `src/FantasyCritic.Web/Models/Requests/LeagueManager/SkipCurrentDraftPickRequest.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs`

- [ ] **Step 1: Create the request record**

```csharp
namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record SkipCurrentDraftPickRequest(Guid LeagueID, int Year, Guid DraftID);
```

- [ ] **Step 2: Add the controller action**

Open `LeagueManagerController.cs`. Find `UndoLastDraftAction` (around line 1130). Add the new action immediately after it (after the closing `}`):

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> SkipCurrentDraftPick([FromBody] SkipCurrentDraftPickRequest request)
{
    var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.LeagueManager, RequiredYearStatus.DraftPaused);
    if (leagueYearRecord.FailedResult is not null)
    {
        return leagueYearRecord.FailedResult;
    }
    var validResult = leagueYearRecord.ValidResult!;
    var leagueYear = validResult.LeagueYear;

    if (leagueYear.ActiveDraft is null || leagueYear.ActiveDraft.DraftID != request.DraftID)
    {
        return BadRequest("DraftID does not match the active draft.");
    }

    var result = await _draftService.SkipCurrentDraftPick(leagueYear);
    if (result.IsFailure)
    {
        return BadRequest(result.Error);
    }

    await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

    return Ok();
}
```

- [ ] **Step 3: Build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release
```

Expected: `Build succeeded.  0 Error(s)`

- [ ] **Step 4: Commit**

```
git add src/FantasyCritic.Web/Models/Requests/LeagueManager/SkipCurrentDraftPickRequest.cs src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs
git commit -m "Add SkipCurrentDraftPick controller action."
```

---

## Task 6: Enrich `LeagueDraftViewModel` with next-pick info

The `skipCurrentDraftPickModal` needs to display who will be skipped while the draft is paused. `activeDraft.nextPickPublisherName` etc. come from new fields on `LeagueDraftViewModel`.

**Files:**
- Modify: `src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs`

- [ ] **Step 1: Add three nullable fields to the class body**

In `LeagueDraftViewModel.cs`, add after the `DraftingCounterPicks` property declaration:

```csharp
public string? NextPickPublisherName { get; }
public int? NextPickRoundNumber { get; }
public bool? NextPickIsCounterPick { get; }
```

- [ ] **Step 2: Populate them in the constructor**

The existing constructor already calls `DraftFunctions.GetDraftStatus(leagueYear)` inside `if (domain.PlayStatus.DraftIsActiveOrPaused)`. Extend that block:

```csharp
if (domain.PlayStatus.DraftIsActiveOrPaused)
{
    var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
    DraftingCounterPicks = DraftPhase.CounterPicks.Equals(draftStatus?.DraftPhase);
    if (draftStatus is not null)
    {
        NextPickPublisherName = draftStatus.NextDraftPublisher.GetPublisherAndUserDisplayName();
        NextPickRoundNumber = draftStatus.RoundNumber;
        NextPickIsCounterPick = draftStatus.NextPick.CounterPick;
    }
}
```

- [ ] **Step 3: Build**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release
```

- [ ] **Step 4: Commit**

```
git add src/FantasyCritic.Web/Models/Requests/League/LeagueDraftViewModel.cs
git commit -m "Add NextPick display fields to LeagueDraftViewModel."
```

---

## Task 7: Regenerate the NSwag API client

The new controller action must be in the generated client before integration tests can call it.

**Files:**
- Run: `scripts/Regenerate-ApiClient.ps1`

- [ ] **Step 1: Regenerate**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release
scripts/Regenerate-ApiClient.ps1
dotnet build
```

Expected: whole solution builds cleanly after regeneration.

- [ ] **Step 2: Commit the updated generated client**

```
git add src/FantasyCritic.ApiClient/
git commit -m "Regenerate API client: add SkipCurrentDraftPick endpoint."
```

---

## Task 8: Integration tests

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/Draft/EdgeCases/DraftSkipManagerActionTests.cs`

- [ ] **Step 1: Create the fixture file**

```csharp
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

/// <summary>
/// Verifies the manager SkipCurrentDraftPick action and the extended UndoLastDraftAction.
///
/// Three independent fixtures share a common setup helper but each captures its own snapshots.
/// </summary>
[TestFixture]
public class DraftSkipManagerActionTests : IntegrationTestBase
{
    // Fixture A: pause → skip → verify
    private LeagueFixture _skipLeague = null!;
    private LeagueYearViewModel _beforeSkipSnapshot = null!;
    private LeagueYearViewModel _afterSkipSnapshot = null!;

    // Fixture B: pause → skip → undo → verify
    private LeagueFixture _skipUndoLeague = null!;
    private LeagueYearViewModel _afterSkipUndoSnapshot = null!;

    // (Fixture C for undo-pick action type is covered in DraftPauseUndoTests; add assertion there instead)

    [OneTimeSetUp]
    public async Task SetUp()
    {
        // Fixture A -------------------------------------------------------
        _skipLeague = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        await _skipLeague.DraftStandardPicksAsync(1);

        await _skipLeague.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _skipLeague.LeagueID,
            Year = _skipLeague.Year,
            Pause = true,
        });
        _beforeSkipSnapshot = await _skipLeague.GetLeagueYearAsync();

        var pausedDraftId = _beforeSkipSnapshot.Drafts.Single(x => x.DraftIsPaused).DraftID;

        await _skipLeague.Manager.LeagueManager.SkipCurrentDraftPickAsync(new SkipCurrentDraftPickRequest
        {
            LeagueID = _skipLeague.LeagueID,
            Year = _skipLeague.Year,
            DraftID = pausedDraftId,
        });
        _afterSkipSnapshot = await _skipLeague.GetLeagueYearAsync();

        // Fixture B -------------------------------------------------------
        _skipUndoLeague = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        await _skipUndoLeague.DraftStandardPicksAsync(1);

        await _skipUndoLeague.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _skipUndoLeague.LeagueID,
            Year = _skipUndoLeague.Year,
            Pause = true,
        });
        var pausedSnapshot2 = await _skipUndoLeague.GetLeagueYearAsync();
        var pausedDraftId2 = pausedSnapshot2.Drafts.Single(x => x.DraftIsPaused).DraftID;

        await _skipUndoLeague.Manager.LeagueManager.SkipCurrentDraftPickAsync(new SkipCurrentDraftPickRequest
        {
            LeagueID = _skipUndoLeague.LeagueID,
            Year = _skipUndoLeague.Year,
            DraftID = pausedDraftId2,
        });

        await _skipUndoLeague.Manager.LeagueManager.UndoLastDraftActionAsync(new UndoLastDraftActionRequest
        {
            LeagueID = _skipUndoLeague.LeagueID,
            Year = _skipUndoLeague.Year,
            DraftID = pausedDraftId2,
        });
        _afterSkipUndoSnapshot = await _skipUndoLeague.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _skipLeague.DisposeAsync();
        await _skipUndoLeague.DisposeAsync();
    }

    // ── Fixture A: skip ──────────────────────────────────────────────────

    [Test]
    public void Skip_DraftIsStillPaused()
    {
        Assert.That(_afterSkipSnapshot.ActiveDraft()?.DraftIsPaused ?? false, Is.True,
            "Draft should remain paused after SkipCurrentDraftPick.");
    }

    [Test]
    public void Skip_NextPickPublisherChanges()
    {
        var beforePublisher = _beforeSkipSnapshot.ActiveDraft()?.NextPickPublisherName;
        var afterPublisher = _afterSkipSnapshot.ActiveDraft()?.NextPickPublisherName;
        Assert.That(afterPublisher, Is.Not.EqualTo(beforePublisher),
            "NextPickPublisherName should change after a skip advances the turn.");
    }

    [Test]
    public void Skip_ManagerActionLoggedWithCorrectType()
    {
        Assert.That(
            _afterSkipSnapshot.ManagerActions.Any(a => a.ActionType == "Draft Pick Skipped"),
            Is.True,
            "A 'Draft Pick Skipped' manager action should appear after SkipCurrentDraftPick.");
    }

    // ── Fixture B: skip then undo ─────────────────────────────────────────

    [Test]
    public void SkipUndo_NextPickRestoredToOriginalPublisher()
    {
        // After undo the turn should be back to whoever was next before the skip.
        // In TwoPlayerSmall the 2nd pick belongs to publisher[1]; after undo it should be back.
        var afterUndoPublisher = _afterSkipUndoSnapshot.ActiveDraft()?.NextPickPublisherName;
        // The before-skip publisher in Fixture A is the same scenario, so use that as the expected name.
        var expectedPublisher = _beforeSkipSnapshot.ActiveDraft()?.NextPickPublisherName;
        Assert.That(afterUndoPublisher, Is.EqualTo(expectedPublisher),
            "After undoing the skip the turn should revert to the publisher that was skipped.");
    }

    [Test]
    public void SkipUndo_ManagerActionLoggedWithCorrectType()
    {
        Assert.That(
            _afterSkipUndoSnapshot.ManagerActions.Any(a => a.ActionType == "Draft Skip Undone"),
            Is.True,
            "'Draft Skip Undone' manager action should appear after undoing a skip.");
    }
}
```

- [ ] **Step 2: Check that `LeagueYearViewModel.ActiveDraft()` extension and `ManagerActions` property exist**

Look at `src/FantasyCritic.IntegrationTests/Helpers/LeagueYearViewModelExtensions.cs` — `ActiveDraft()` should be defined there. If `LeagueYearViewModel` does not have a `ManagerActions` property, use the `Actions` collection filtered by `a.ManagerAction == true` instead.

- [ ] **Step 3: Build integration tests**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: `Build succeeded.  0 Error(s)`

- [ ] **Step 4: Run new tests only (fast feedback)**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --no-build --filter "FullyQualifiedName~DraftSkipManagerActionTests"
```

Expected: all new tests pass.

- [ ] **Step 5: Commit**

```
git add src/FantasyCritic.IntegrationTests/Tests/League/Draft/EdgeCases/DraftSkipManagerActionTests.cs
git commit -m "Add DraftSkipManagerActionTests integration tests."
```

---

## Task 9: Frontend — sidebar entry

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue`

- [ ] **Step 1: Add the import**

In the import block (around line 247 where `UndoLastDraftActionModal` is imported), add:

```js
import SkipCurrentDraftPickModal from '@/components/modals/skipCurrentDraftPickModal.vue';
```

- [ ] **Step 2: Register the component**

In the `components:` object (around line 296 where `UndoLastDraftActionModal` is registered), add:

```js
SkipCurrentDraftPickModal,
```

- [ ] **Step 3: Add the sidebar list items**

In the template, find the undo block (lines 89–95 approximately):

```html
<li v-show="activeDraft?.draftIsPaused" v-b-modal="'undoLastDraftActionModal'" class="fake-link action">Undo Last Drafted Game</li>
<li v-show="!activeDraft?.draftIsPaused">
  Undo Last Drafted Game
  <br />
  <span class="action-note">(Pause Draft First)</span>
</li>
```

Add the skip block **after** the undo block:

```html
<li v-show="activeDraft?.draftIsPaused" v-b-modal="'skipCurrentDraftPickModal'" class="fake-link action">Skip Current Pick</li>
<li v-show="!activeDraft?.draftIsPaused">
  Skip Current Pick
  <br />
  <span class="action-note">(Pause Draft First)</span>
</li>
```

- [ ] **Step 4: Commit**

```
git add src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue
git commit -m "Add Skip Current Pick sidebar entry to Draft Management."
```

---

## Task 10: Frontend — confirmation modal

**Files:**
- Create: `src/FantasyCritic.Web/ClientApp/src/components/modals/skipCurrentDraftPickModal.vue`

- [ ] **Step 1: Create the modal**

```vue
<template>
  <b-modal id="skipCurrentDraftPickModal" ref="skipCurrentDraftPickModalRef" title="Warning!" :ok-disabled="!skipConfirmed" @ok="skipCurrentDraftPick" @hidden="clearData">
    <p>
      Are you sure you want to skip the current pick?
    </p>
    <p v-if="activeDraft && activeDraft.nextPickPublisherName">
      This will skip
      <strong>{{ activeDraft.nextPickPublisherName }}</strong>'s turn
      (round {{ activeDraft.nextPickRoundNumber }},
      {{ activeDraft.nextPickIsCounterPick ? 'counter-pick' : 'standard game' }}).
    </p>
    <p>
      If you are sure, type
      <strong>SKIP TURN</strong>
      into the box below and click the OK button.
    </p>
    <input v-model="skipConfirmation" type="text" class="form-control input" />
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      skipConfirmation: ''
    };
  },
  computed: {
    skipConfirmed() {
      return this.skipConfirmation.toUpperCase() === 'SKIP TURN';
    }
  },
  methods: {
    skipCurrentDraftPick() {
      this.skipConfirmation = '';
      this.$refs.skipCurrentDraftPickModalRef.hide();
      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        draftID: this.activeDraft.draftID
      };
      axios
        .post('/api/leagueManager/SkipCurrentDraftPick', model)
        .then(() => {
          this.notifyAction('Turn was skipped.', false);
        })
        .catch(() => {});
    },
    clearData() {
      this.skipConfirmation = '';
    }
  }
};
</script>
```

- [ ] **Step 2: Commit**

```
git add src/FantasyCritic.Web/ClientApp/src/components/modals/skipCurrentDraftPickModal.vue
git commit -m "Add skipCurrentDraftPickModal Vue component."
```

---

## Task 11: Update undo modal description

The current `undoLastDraftActionModal` says "This will remove the game that was added most recently from it's publisher." — this is now wrong for skip undo.

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/modals/undoLastDraftActionModal.vue`

- [ ] **Step 1: Update the modal body**

Replace the inner `<p>`:

```html
<!-- BEFORE -->
<p>
  Do you wish to undo the last draft action?
  <br />
  This will remove the game that was added most recently from it's publisher.
  <br />
  Only do this if there was a mistake.
</p>

<!-- AFTER -->
<p>
  Do you wish to undo the last draft action?
  <br />
  This will reverse the most recent action — either removing the last drafted game, or restoring a skipped turn.
  <br />
  Only do this if there was a mistake.
</p>
```

- [ ] **Step 2: Commit**

```
git add src/FantasyCritic.Web/ClientApp/src/components/modals/undoLastDraftActionModal.vue
git commit -m "Update undo modal description to cover skip undo."
```

---

## Task 12: Full integration test run and baseline check

- [ ] **Step 1: Build everything**

```powershell
dotnet build -c Release
```

Expected: `Build succeeded.  0 Error(s)`

- [ ] **Step 2: Run all integration tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --no-build
```

Expected: new tests pass; no previously-passing tests newly fail. (Some tests are intentionally `[Ignore]`'d — that is fine.)

- [ ] **Step 3: If any non-ignored test newly fails, investigate before proceeding**

---

## Self-Review

### Spec coverage check

| Spec requirement | Task |
|---|---|
| `SkipCurrentDraftPick` endpoint, `DraftPaused` gate | Task 5 |
| Targets `DraftStatus.NextPick` only | Task 3 step 1 |
| Voluntary skip (no slot check) | Task 3 step 1 (no `ShouldSkipPublisher` call) |
| Duplicate skip check → 400 | Task 3 step 1 |
| `LeagueManagerAction` logged as "Draft Pick Skipped" | Task 3 step 1 |
| `AddDraftPickSkip` repo method | Task 1 step 2 |
| `RemoveDraftPickSkip` repo method | Task 1 step 3 |
| Undo skip branch → delete row, log "Draft Skip Undone" | Task 3 step 3 |
| Undo pick branch → log "Draft Pick Undone" not "Publisher Game Removed" | Task 2, Task 3 step 3 |
| No Discord push for draft undo | Task 2 step 1 |
| `NextPickPublisherName/RoundNumber/IsCounterPick` on `LeagueDraftViewModel` | Task 6 |
| Sidebar entry (pause-gated) | Task 9 |
| `skipCurrentDraftPickModal` with `SKIP TURN` confirmation | Task 10 |
| Undo modal description updated | Task 11 |
| NSwag regeneration | Task 7 |
| Integration tests: skip, skip→undo, action types | Task 8 |

### Notes for implementer

- `Publisher.GetPublisherAndUserDisplayName()` — verify this method name in `Publisher.cs`; it may be `GetDisplayName()` or a property. Search before use.
- `LeagueYearViewModel.ManagerActions` — verify this exists on the view model returned by the API client. If not, filter `Actions` by `ManagerAction == true`.
- The `AddLeagueManagerAction(action, connection, transaction)` private overload — verify it exists in `MySQLFantasyCriticRepo.cs`; if not, add it following the same pattern as `AddLeagueAction(action, connection, transaction)`.
- Task 4 (Publisher helper) — complete before Task 3 compiles cleanly; both tasks may be done in the same editing pass.
