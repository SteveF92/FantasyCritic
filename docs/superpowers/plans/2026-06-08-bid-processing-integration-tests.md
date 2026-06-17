# Bid Processing Integration Tests — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `BidProcessingTests` integration-test fixture that drafts a 4-player league, places six bids covering four scenarios (uncontested, contested, tiebreaker, counter-pick), advances the clock past the Saturday processing threshold, runs `ProcessActions`, and asserts on the post-processing publisher rosters and budgets.

**Architecture:** New `FourPlayerBidding` `LeagueScenario` (6 standard slots / 3 drafted, 2 counter-pick slots / 1 drafted) is added alongside existing scenarios. `BidProcessingTests` extends `IntegrationTestBase`, uses a single shared `OneTimeSetUp` to build all state (clock, draft, bid placement, time advance, action processing), and exposes individual `[Test]` methods that only assert — no state changes during tests. Three `ActionRunnerController` endpoints currently generate `Task<FileResponse>` (NSwag fallback); adding `[ProducesResponseType(StatusCodes.Status200OK)]` to them regenerates as `Task` (void). Similarly, `MakePickupBid` needs `[ProducesResponseType<PickupBidResultViewModel>(Status200OK)]` so the result is inspectable in tests.

**Tech Stack:** C# 13 / .NET 10, NUnit 3, NSwag 14.7.1, `FantasyCritic.ApiClient` (auto-generated typed client), `FantasyCritic.IntegrationTests`

---

## Prerequisites

- MySQL Docker container running: `docker compose -f infrastructure/docker-compose-mysql.yaml up -d`
- Build passes: `dotnet build`
- Run command for integration tests: `dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release`
- Filter to one class: append `--filter "FullyQualifiedName~ClassName"`

---

## File Map

| Action | File |
|---|---|
| Modify | `src/FantasyCritic.Web/Controllers/API/LeagueController.cs` |
| Modify | `src/FantasyCritic.Web/Controllers/API/ActionRunnerController.cs` |
| Regenerate | `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` |
| Modify | `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/BidProcessingTests.cs` |

---

## Task 1: Add `[ProducesResponseType]` annotations + regenerate the typed client

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueController.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/ActionRunnerController.cs`
- Regenerate: `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs`

**Background:** NSwag generates a typed return only when the action declares an explicit success-response attribute. Without one, any `ActionResult<T>` with other error attributes becomes `Task` (void), and any `IActionResult` becomes `Task<FileResponse>`.

Two issues to fix here:

1. `MakePickupBid` in `LeagueController` returns `ActionResult<PickupBidResultViewModel>` but has no 200 OK attribute → NSwag generates `Task` (void). Fix: add `[ProducesResponseType<PickupBidResultViewModel>(StatusCodes.Status200OK)]`.

2. `ProcessActions`, `TurnOnActionProcessingMode`, and `TurnOffActionProcessingMode` in `ActionRunnerController` return `IActionResult` with no attribute → NSwag generates `Task<FileResponse>`. Fix: add `[ProducesResponseType(StatusCodes.Status200OK)]` (non-generic, no body).

- [ ] **Step 1: Add 200 OK attribute to `MakePickupBid`**

Open `src/FantasyCritic.Web/Controllers/API/LeagueController.cs`.

Find this block (around line 897):
```csharp
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PickupBidResultViewModel>> MakePickupBid([FromBody] PickupBidRequest request)
```

Replace with:
```csharp
    [HttpPost]
    [ProducesResponseType<PickupBidResultViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PickupBidResultViewModel>> MakePickupBid([FromBody] PickupBidRequest request)
```

- [ ] **Step 2: Add 200 OK attributes to `ActionRunnerController`**

Open `src/FantasyCritic.Web/Controllers/API/ActionRunnerController.cs`.

Find `ProcessActions` (around line 106):
```csharp
    [HttpPost]
    public async Task<IActionResult> ProcessActions()
```
Replace with:
```csharp
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessActions()
```

Find `TurnOnActionProcessingMode` (around line 150):
```csharp
    [HttpPost]
    public async Task<IActionResult> TurnOnActionProcessingMode()
```
Replace with:
```csharp
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TurnOnActionProcessingMode()
```

Find `TurnOffActionProcessingMode` (around line 157):
```csharp
    [HttpPost]
    public async Task<IActionResult> TurnOffActionProcessingMode()
```
Replace with:
```csharp
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TurnOffActionProcessingMode()
```

- [ ] **Step 3: Build the Web project**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Regenerate the typed API client**

```powershell
scripts/Regenerate-ApiClient.ps1
```

Expected: Script completes with no errors.

- [ ] **Step 5: Verify the generated client has the expected signatures**

Open `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` and confirm:

- `Task<PickupBidResultViewModel> MakePickupBidAsync(PickupBidRequest request)` — NOT `Task` (void)
- `Task ProcessActionsAsync()` — NOT `Task<FileResponse>`
- `Task TurnOnActionProcessingModeAsync()` — NOT `Task<FileResponse>`
- `Task TurnOffActionProcessingModeAsync()` — NOT `Task<FileResponse>`

- [ ] **Step 6: Build the full solution**

```powershell
dotnet build
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```powershell
git add src/FantasyCritic.Web/Controllers/API/LeagueController.cs
git add src/FantasyCritic.Web/Controllers/API/ActionRunnerController.cs
git add src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs
git commit -m "Add ProducesResponseType annotations to MakePickupBid and ActionRunner endpoints; regenerate API client."
```

---

## Task 2: Add `FourPlayerBidding` scenario

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`

This scenario has more standard game slots (6) than games to draft (3), leaving 3 open bid slots per publisher post-draft. Similarly 2 counter-pick slots total vs 1 drafted, leaving 1 open counter-pick bid slot.

- [ ] **Step 1: Add the scenario**

Open `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`. In the `LeagueScenarios` static class, after the closing brace of `TwoPlayerSmall`, add:

```csharp
    /// <summary>
    /// A 4-player league with 6 standard slots (3 drafted, 3 open for bids) and
    /// 2 counter-pick slots (1 drafted, 1 open for counter-pick bids).
    /// Used by bid-processing tests that exercise uncontested, contested,
    /// tiebreaker, and counter-pick bid scenarios.
    /// </summary>
    public static readonly LeagueScenario FourPlayerBidding = new()
    {
        Name = "FourPlayerBidding",
        PlayerCount = 4,
        StandardGames = 6,
        GamesToDraft = 3,
        CounterPicks = 2,
        CounterPicksToDraft = 1,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
    };
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs
git commit -m "Add FourPlayerBidding scenario for bid processing tests."
```

---

## Task 3: Create `BidProcessingTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/BidProcessingTests.cs`

**Test scenarios covered:**

| Scenario | Publishers | Game | Amounts | Expected |
|---|---|---|---|---|
| Uncontested standard | P1 | Game A | $10 | P1 wins |
| Contested standard | P2 vs P3 | Game B | $20 vs $10 | P2 wins |
| Tiebreaker standard | P3 vs P4 | Game C | $15 vs $15 | One wins by projected-points tiebreaker |
| Uncontested counter-pick | P2 | P1's first drafted game | $5 | P2 wins |

**Clock setup:** `SetInitialTime` to Monday Jan 6 2025 12:00:00 UTC (= noon ET; safely after the last Saturday processing window and before the Thursday public-bidding reveal — no bid restrictions apply). `SetTime` advances to Jan 12 2025 01:01:00 UTC (= Saturday Jan 11 20:01 ET, just past the 20:00 ET action-processing threshold).

**Budget note:** Publishers start with $100. P1 spends $10 (Game A). P2 spends $25 ($20 Game B + $5 counter-pick). P3 spends $0 if losing Game B and losing Game C, or $15 if winning Game C. P4 spends $15 if winning Game C, or $0 if losing.

- [ ] **Step 1: Create the fixture file**

Create `src/FantasyCritic.IntegrationTests/Tests/League/BidProcessingTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Exercises the full post-draft pickup-bid lifecycle:
///   1. Set clock to a known Monday (outside public-bidding window).
///   2. Draft a 4-player FourPlayerBidding league to completion.
///   3. Place six bids: uncontested, contested (clear winner), tiebreaker, counter-pick.
///   4. Advance clock past Saturday 20:00 ET action-processing threshold.
///   5. Turn on action processing mode, run ProcessActions, turn it off.
///   6. Assert on post-processing rosters and budgets.
///
/// All tests share the OneTimeSetUp snapshot and are read-only.
/// </summary>
[TestFixture]
public class BidProcessingTests : IntegrationTestBase
{
    // ── Sessions ───────────────────────────────────────────────────────────────
    private ApiSession _adminSession = null!;
    private ApiSession _managerSession = null!;  // P1, draft position 1
    private ApiSession _p2Session = null!;
    private ApiSession _p3Session = null!;
    private ApiSession _p4Session = null!;

    // ── League ─────────────────────────────────────────────────────────────────
    private Guid _leagueID;
    private int _year;

    // ── Publisher IDs ──────────────────────────────────────────────────────────
    private Guid _p1PublisherID;
    private Guid _p2PublisherID;
    private Guid _p3PublisherID;
    private Guid _p4PublisherID;

    // ── Starting budgets (post-draft, pre-bid) ─────────────────────────────────
    private int _p1StartBudget;
    private int _p2StartBudget;
    private int _p3StartBudget;
    private int _p4StartBudget;

    // ── Bid target game IDs ────────────────────────────────────────────────────
    private Guid _gameAMasterGameID;       // P1 uncontested bid
    private Guid _gameBMasterGameID;       // P2 ($20) vs P3 ($10) — P2 wins
    private Guid _gameCMasterGameID;       // P3 ($15) vs P4 ($15) — tiebreaker
    private Guid _p1CounterPickTargetID;   // P2 counter-picks this from P1's roster

    // ── Post-processing state ──────────────────────────────────────────────────
    private LeagueYearViewModel _postProcessingSnapshot = null!;
    private Guid _tiebreakerWinnerID;
    private Guid _tiebreakerLoserID;
    private int _tiebreakerWinnerStartBudget;
    private int _tiebreakerLoserStartBudget;

    [OneTimeSetUp]
    public async Task SetUpBidProcessingScenario()
    {
        // ── Phase 1: Clock + draft ──────────────────────────────────────────────

        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        // Monday Jan 6 2025 12:00 UTC = noon Eastern Standard Time.
        // This falls after the Jan 4 Saturday processing window and before
        // the Jan 8 Thursday public-bidding reveal — any game may be freely bid on.
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        var (mgrEmail, mgrPwd, mgrName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPwd, mgrName);

        var (p2Email, p2Pwd, p2Name) = NewUser();
        _p2Session = new ApiSession(Factory);
        await _p2Session.RegisterAsync(p2Email, p2Pwd, p2Name);

        var (p3Email, p3Pwd, p3Name) = NewUser();
        _p3Session = new ApiSession(Factory);
        await _p3Session.RegisterAsync(p3Email, p3Pwd, p3Name);

        var (p4Email, p4Pwd, p4Name) = NewUser();
        _p4Session = new ApiSession(Factory);
        await _p4Session.RegisterAsync(p4Email, p4Pwd, p4Name);

        _year = await LeagueTestHelpers.GetOpenYearAsync(_managerSession);
        _leagueID = await LeagueTestHelpers.CreateLeagueAsync(
            _managerSession, LeagueScenarios.FourPlayerBidding, _year);

        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p2Session, _leagueID);
        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p3Session, _leagueID);
        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p4Session, _leagueID);

        _p1PublisherID = await LeagueTestHelpers.CreatePublisherAsync(
            _managerSession, _leagueID, _year, $"P1-{Guid.NewGuid():N}"[..20]);
        _p2PublisherID = await LeagueTestHelpers.CreatePublisherAsync(
            _p2Session, _leagueID, _year, $"P2-{Guid.NewGuid():N}"[..20]);
        _p3PublisherID = await LeagueTestHelpers.CreatePublisherAsync(
            _p3Session, _leagueID, _year, $"P3-{Guid.NewGuid():N}"[..20]);
        _p4PublisherID = await LeagueTestHelpers.CreatePublisherAsync(
            _p4Session, _leagueID, _year, $"P4-{Guid.NewGuid():N}"[..20]);

        var draftOrder = new List<Guid>
        {
            _p1PublisherID, _p2PublisherID, _p3PublisherID, _p4PublisherID
        };
        await LeagueTestHelpers.SetDraftOrderAsync(_managerSession, _leagueID, _year, draftOrder);

        await _managerSession.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _leagueID,
            Year = _year,
        });

        var players = new[]
        {
            new MockedLivePlayer(_managerSession, _p1PublisherID, _leagueID),
            new MockedLivePlayer(_p2Session,       _p2PublisherID, _leagueID),
            new MockedLivePlayer(_p3Session,       _p3PublisherID, _leagueID),
            new MockedLivePlayer(_p4Session,       _p4PublisherID, _leagueID),
        };
        var simulator = new DraftSimulator(_managerSession, players);
        await simulator.RunAsync(_leagueID, _year);

        // Record post-draft budgets before any bids alter them.
        var postDraftSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        _p1StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID).Budget;
        _p2StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID).Budget;
        _p3StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID).Budget;
        _p4StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID).Budget;

        // ── Phase 2: Select distinct bid targets ────────────────────────────────

        // Game A — P1's uncontested bid (first available game)
        var p1Available = await _managerSession.League.TopAvailableGamesAsync(
            _year, _leagueID, _p1PublisherID, null);
        var gameA = p1Available.First(g => g.IsAvailable && !g.Taken);
        _gameAMasterGameID = gameA.MasterGame.MasterGameID;

        // Game B — P2 vs P3 (different from Game A)
        var p2Available = await _p2Session.League.TopAvailableGamesAsync(
            _year, _leagueID, _p2PublisherID, null);
        var gameB = p2Available.First(g => g.IsAvailable && !g.Taken
            && g.MasterGame.MasterGameID != _gameAMasterGameID);
        _gameBMasterGameID = gameB.MasterGame.MasterGameID;

        // Game C — P3/P4 tiebreaker (different from Game A and B)
        var p3Available = await _p3Session.League.TopAvailableGamesAsync(
            _year, _leagueID, _p3PublisherID, null);
        var gameC = p3Available.First(g => g.IsAvailable && !g.Taken
            && g.MasterGame.MasterGameID != _gameAMasterGameID
            && g.MasterGame.MasterGameID != _gameBMasterGameID);
        _gameCMasterGameID = gameC.MasterGame.MasterGameID;

        // Counter-pick target — P2 counter-picks P1's first standard drafted game
        var p1Publisher = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        _p1CounterPickTargetID = p1Publisher.Games
            .First(g => !g.CounterPick)
            .MasterGame!.MasterGameID;

        // ── Phase 3: Place six bids ─────────────────────────────────────────────
        // Each call returns a PickupBidResultViewModel. Success = true means the bid
        // was validated and queued; it does not mean the bid will WIN at processing time.
        // A Success = false here means the bid was rejected at the domain level
        // (e.g. game already released) — that is a test-setup failure, not a test failure.

        var bidA = await _managerSession.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = _p1PublisherID,
            MasterGameID = _gameAMasterGameID,
            CounterPick = false,
            BidAmount = 10,
            AllowIneligibleSlot = false,
        });
        if (!bidA.Success)
            throw new InvalidOperationException(
                "P1 uncontested bid on Game A failed to queue: " + string.Join("; ", bidA.Errors));

        var bidB_P2 = await _p2Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = _p2PublisherID,
            MasterGameID = _gameBMasterGameID,
            CounterPick = false,
            BidAmount = 20,
            AllowIneligibleSlot = false,
        });
        if (!bidB_P2.Success)
            throw new InvalidOperationException(
                "P2 contested bid on Game B ($20) failed to queue: " + string.Join("; ", bidB_P2.Errors));

        var bidB_P3 = await _p3Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = _p3PublisherID,
            MasterGameID = _gameBMasterGameID,
            CounterPick = false,
            BidAmount = 10,
            AllowIneligibleSlot = false,
        });
        if (!bidB_P3.Success)
            throw new InvalidOperationException(
                "P3 contested bid on Game B ($10) failed to queue: " + string.Join("; ", bidB_P3.Errors));

        var bidC_P3 = await _p3Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = _p3PublisherID,
            MasterGameID = _gameCMasterGameID,
            CounterPick = false,
            BidAmount = 15,
            AllowIneligibleSlot = false,
        });
        if (!bidC_P3.Success)
            throw new InvalidOperationException(
                "P3 tiebreaker bid on Game C ($15) failed to queue: " + string.Join("; ", bidC_P3.Errors));

        var bidC_P4 = await _p4Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = _p4PublisherID,
            MasterGameID = _gameCMasterGameID,
            CounterPick = false,
            BidAmount = 15,
            AllowIneligibleSlot = false,
        });
        if (!bidC_P4.Success)
            throw new InvalidOperationException(
                "P4 tiebreaker bid on Game C ($15) failed to queue: " + string.Join("; ", bidC_P4.Errors));

        var bidCP_P2 = await _p2Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = _p2PublisherID,
            MasterGameID = _p1CounterPickTargetID,
            CounterPick = true,
            BidAmount = 5,
            AllowIneligibleSlot = false,
        });
        if (!bidCP_P2.Success)
            throw new InvalidOperationException(
                "P2 counter-pick bid on P1's game ($5) failed to queue: " + string.Join("; ", bidCP_P2.Errors));

        // ── Phase 4: Advance time and run bids ─────────────────────────────────
        // Target: Saturday Jan 11 2025 20:01 ET.
        // Eastern Standard Time (January) = UTC-5, so 20:01 ET = 01:01 UTC on Jan 12.
        // ActionProcessingDay = Saturday, ActionProcessingTime = 20:00 ET.
        // The clock is now past the threshold, so all bids placed on Monday Jan 6
        // fall within the current processing window.
        // The day-of-week guard in ProcessActions is only enforced in production
        // (environment name "PRODUCTION"); the test factory runs in "Development".
        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });

        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        await _adminSession.ActionRunner.ProcessActionsAsync();
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        // ── Phase 5: Capture post-processing state ──────────────────────────────

        _postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // Resolve tiebreaker winner/loser dynamically — we don't predict which publisher
        // wins because projected points depend on which games were auto-drafted. The test
        // asserts the *invariants* (exactly one winner, correct budget change) rather than
        // the specific winner's identity.
        var p3Post = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        bool p3WonGameC = p3Post.Games.Any(
            g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameCMasterGameID);
        _tiebreakerWinnerID          = p3WonGameC ? _p3PublisherID : _p4PublisherID;
        _tiebreakerLoserID           = p3WonGameC ? _p4PublisherID : _p3PublisherID;
        _tiebreakerWinnerStartBudget = p3WonGameC ? _p3StartBudget : _p4StartBudget;
        _tiebreakerLoserStartBudget  = p3WonGameC ? _p4StartBudget : _p3StartBudget;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _adminSession?.Dispose();
        _managerSession?.Dispose();
        _p2Session?.Dispose();
        _p3Session?.Dispose();
        _p4Session?.Dispose();
    }

    // ── Uncontested bid (P1 → Game A at $10) ───────────────────────────────────

    [Test]
    public void Bid_Uncontested_P1_HasGameA_OnRoster()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(
            p1.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameAMasterGameID),
            Is.True,
            "P1 should have Game A as a standard game after the uncontested bid processes.");
    }

    [Test]
    public void Bid_Uncontested_P1_BudgetDeductedBy10()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.Budget, Is.EqualTo(_p1StartBudget - 10),
            "P1's budget should be reduced by the $10 bid amount.");
    }

    // ── Contested bid — winner (P2 beats P3, $20 > $10) ───────────────────────

    [Test]
    public void Bid_Contested_P2_HasGameB_OnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(
            p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameBMasterGameID),
            Is.True,
            "P2 should have Game B after winning the contested bid with a higher amount ($20 > $10).");
    }

    [Test]
    public void Bid_Contested_P2_BudgetDeductedBy25()
    {
        // P2 placed two winning bids: Game B ($20) + counter-pick ($5) = $25.
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(p2.Budget, Is.EqualTo(_p2StartBudget - 25),
            "P2's budget should be reduced by $20 (Game B) + $5 (counter-pick) = $25.");
    }

    // ── Contested bid — loser (P3 lost Game B at $10) ──────────────────────────

    [Test]
    public void Bid_Contested_P3_DoesNotHaveGameB_OnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameBMasterGameID),
            Is.False,
            "P3 should NOT have Game B after losing the contested bid to P2 ($10 < $20).");
    }

    // ── Counter-pick bid (P2 counter-picks P1's first drafted game at $5) ──────

    [Test]
    public void Bid_CounterPick_P2_HasP1Game_AsCounterPick()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(
            p2.Games.Any(g => g.CounterPick && g.MasterGame?.MasterGameID == _p1CounterPickTargetID),
            Is.True,
            "P2 should have P1's game as a counter-pick after the uncontested counter-pick bid processes.");
    }

    // ── Tiebreaker (P3 vs P4 on Game C, $15 each) ──────────────────────────────

    [Test]
    public void Bid_Tiebreaker_ExactlyOneOfP3P4_HasGameC()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID);
        bool p3HasC = p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameCMasterGameID);
        bool p4HasC = p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameCMasterGameID);
        Assert.That(p3HasC ^ p4HasC, Is.True,
            "Exactly one of P3 or P4 should have Game C after the projected-points tiebreaker resolves.");
    }

    [Test]
    public void Bid_Tiebreaker_Winner_BudgetDeductedBy15()
    {
        var winner = _postProcessingSnapshot.Publishers.Single(
            p => p.PublisherID == _tiebreakerWinnerID);
        Assert.That(winner.Budget, Is.EqualTo(_tiebreakerWinnerStartBudget - 15),
            "The tiebreaker winner's budget should be reduced by the $15 bid amount.");
    }

    [Test]
    public void Bid_Tiebreaker_Loser_BudgetUnchanged()
    {
        var loser = _postProcessingSnapshot.Publishers.Single(
            p => p.PublisherID == _tiebreakerLoserID);
        Assert.That(loser.Budget, Is.EqualTo(_tiebreakerLoserStartBudget),
            "The tiebreaker loser's budget should be unchanged — losing bids are never charged.");
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Run only `BidProcessingTests`**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~BidProcessingTests"
```

Expected: 10 tests pass. The `OneTimeSetUp` will take longer than draft-only fixtures (draft + 6 bid placements + action processing), so allow up to 2 minutes.

If any bid placement throws in `OneTimeSetUp`, the error message includes the bid's `Errors` list, which will identify the domain-level rejection reason (e.g., "That game will release before bids are processed" → choose a different game by increasing the index in the `First(...)` call, or check the seeded game data).

- [ ] **Step 4: Run the full integration test suite to confirm no regressions**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: All tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/League/BidProcessingTests.cs
git commit -m "Add BidProcessingTests: uncontested, contested, tiebreaker, and counter-pick bid lifecycle."
```

---

## Self-Review

**Spec coverage check:**

| Spec requirement | Task that implements it |
|---|---|
| `FourPlayerBidding` scenario (6 standard / 3 drafted, 2 CP / 1 drafted) | Task 2 |
| `SetInitialTime` to Monday (outside public-bidding window) | Task 3, Phase 1 |
| Full draft to completion | Task 3, Phase 1 |
| Uncontested bid queued and validated | Task 3, Phase 3 |
| Contested bid (P2 $20 vs P3 $10) | Task 3, Phase 3 |
| Tiebreaker (P3 $15 vs P4 $15) | Task 3, Phase 3 |
| Counter-pick bid | Task 3, Phase 3 |
| `SetTime` past Saturday 20:00 ET threshold | Task 3, Phase 4 |
| `TurnOnActionProcessingMode` → `ProcessActions` → `TurnOff` | Task 3, Phase 4 |
| `[ProducesResponseType]` on `MakePickupBid` | Task 1 |
| `[ProducesResponseType]` on ActionRunner methods (void returns) | Task 1 |
| Uncontested: P1 has Game A, budget −$10 | Task 3 tests |
| Contested winner: P2 has Game B, budget −$25 | Task 3 tests |
| Contested loser: P3 does not have Game B | Task 3 tests |
| Counter-pick: P2 has P1's game as counter-pick | Task 3 tests |
| Tiebreaker: exactly one winner, correct budget deductions | Task 3 tests |

**Placeholder scan:** No TBDs, TODOs, or vague steps. Every step has exact file paths, exact code, and expected output.

**Type consistency check:**
- `PickupBidRequest.BidAmount` is `int` in the generated client — literal values `10`, `20`, `15`, `5` are int-compatible ✓
- `PublisherViewModel.Budget` is `int` — arithmetic comparisons are valid ✓
- `PublisherGameViewModel.CounterPick` is `bool` — `!g.CounterPick` and `g.CounterPick` are correct ✓
- `PublisherGameViewModel.MasterGame` is nullable — `g.MasterGame?.MasterGameID` is correct ✓
- `PossibleMasterGameYearViewModel.IsAvailable`, `.Taken`, `.MasterGame.MasterGameID` match `MockedLivePlayer` usage ✓
- `LeagueYearViewModel.Publishers` → `PublisherViewModel.PublisherID`, `.Games`, `.Budget` all verified in generated client ✓
- After Task 1 regeneration: `MakePickupBidAsync` → `Task<PickupBidResultViewModel>` with `.Success` and `.Errors` ✓
- After Task 1 regeneration: `ProcessActionsAsync`, `TurnOnActionProcessingModeAsync`, `TurnOffActionProcessingModeAsync` → `Task` (void) ✓
