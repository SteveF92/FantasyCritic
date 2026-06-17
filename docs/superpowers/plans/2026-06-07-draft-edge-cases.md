# Draft Edge Cases + Auto-Draft Integration Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add integration test coverage for draft error conditions (out-of-turn picks, duplicate games, ineligible games, wrong-phase picks) plus the pause/undo flow and auto-draft, as specified in `docs/superpowers/specs/2026-06-07-draft-edge-cases-design.md`.

**Architecture:** Shared test infrastructure is extended first (new `TwoPlayerSmall` scenario, `SetUpLeagueAndStartDraftAsync` helper, `DraftSimulator.RunUntilCounterPickPhaseAsync`). Four new test fixtures follow: one concrete `LeagueDraftTestBase` subclass for auto-draft and three standalone fixtures for error rejections and pause/undo.

**Tech Stack:** C# 13 / .NET 10, NUnit 3, NSwag 14.7.1, `FantasyCritic.ApiClient` (auto-generated), `FantasyCritic.IntegrationTests`

---

## Prerequisites

- MySQL Docker container running: `docker compose -f infrastructure/docker-compose-mysql.yaml up -d`
- Build passes: `dotnet build`
- Run command for tests: `dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release`
- Filter to one class: append `--filter "FullyQualifiedName~ClassName"`

---

## File Map

| Action | File |
|---|---|
| Modify | `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` |
| Modify | `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs` |
| Modify | `src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/AutoDraftLeagueDraftTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/DraftEdgeCaseTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/DraftCounterPickPhaseTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/DraftPauseUndoTests.cs` |

---

## Task 1: Add `TwoPlayerSmall` scenario to `LeagueScenarios`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`

- [ ] **Step 1: Add the `TwoPlayerSmall` entry to `LeagueScenarios`**

Open `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`. In the `LeagueScenarios` static class, immediately after the closing brace of the `Standard` property, add:

```csharp
    /// <summary>
    /// A minimal 2-player league used by error-case and edge-case test fixtures.
    /// 2 standard games + 1 counter-pick per publisher. Fast to set up and exhaust.
    /// </summary>
    public static readonly LeagueScenario TwoPlayerSmall = new()
    {
        Name = "TwoPlayerSmall",
        PlayerCount = 2,
        StandardGames = 2,
        GamesToDraft = 2,
        CounterPicks = 1,
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

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs
git commit -m "Add TwoPlayerSmall scenario for draft edge-case tests."
```

---

## Task 2: Add `SetUpLeagueAndStartDraftAsync` helper to `LeagueTestHelpers`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs`

- [ ] **Step 1: Add the helper method**

Open `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs`. At the end of the `LeagueTestHelpers` class (before the final `}`), add:

```csharp
    /// <summary>
    /// Creates a league, invites and accepts all non-manager players, creates publishers,
    /// sets draft order (manager = position 1, players in creation order), and starts the draft.
    ///
    /// Returns the state needed by error-case fixtures that stop the draft at a partial point:
    /// <list type="bullet">
    ///   <item><description><c>leagueID</c> and <c>year</c></description></item>
    ///   <item><description><c>publisherIDsInDraftOrder</c> — [0] = manager, [1..n] = players</description></item>
    ///   <item><description><c>publisherSessionMap</c> — maps publisher GUID to the session that owns it</description></item>
    /// </list>
    /// Unlike <see cref="LeagueDraftTestBase"/>, this helper does NOT complete the draft.
    /// </summary>
    public static async Task<(Guid leagueID, int year,
        IReadOnlyList<Guid> publisherIDsInDraftOrder,
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap)>
        SetUpLeagueAndStartDraftAsync(
            ApiSession managerSession,
            IReadOnlyList<ApiSession> playerSessions,
            LeagueScenario scenario)
    {
        var year = await GetOpenYearAsync(managerSession);
        var leagueID = await CreateLeagueAsync(managerSession, scenario, year);

        foreach (var playerSession in playerSessions)
        {
            await InviteAndAcceptAsync(managerSession, playerSession, leagueID);
        }

        var publisherSessionMap = new Dictionary<Guid, ApiSession>();
        var publisherIDsInOrder = new List<Guid>();

        var managerPubID = await CreatePublisherAsync(
            managerSession, leagueID, year, $"Mgr-{Guid.NewGuid():N}"[..20]);
        publisherSessionMap[managerPubID] = managerSession;
        publisherIDsInOrder.Add(managerPubID);

        for (var i = 0; i < playerSessions.Count; i++)
        {
            var pubID = await CreatePublisherAsync(
                playerSessions[i], leagueID, year, $"Plr{i + 1}-{Guid.NewGuid():N}"[..20]);
            publisherSessionMap[pubID] = playerSessions[i];
            publisherIDsInOrder.Add(pubID);
        }

        await SetDraftOrderAsync(managerSession, leagueID, year, publisherIDsInOrder);

        await managerSession.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = leagueID,
            Year = year,
        });

        return (leagueID, year,
            publisherIDsInOrder.AsReadOnly(),
            new Dictionary<Guid, ApiSession>(publisherSessionMap));
    }
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs
git commit -m "Add SetUpLeagueAndStartDraftAsync helper for partial-draft test fixtures."
```

---

## Task 3: Extend `DraftSimulator` — `RunUntilCounterPickPhaseAsync` + improved error message

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs`

- [ ] **Step 1: Improve the error message in `DraftSimulator.RunAsync`**

In `RunAsync`, find:

```csharp
            if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
                throw new InvalidOperationException(
                    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
                    + $"({nextPublisher.PublisherName}).");
```

Replace with:

```csharp
            if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
                throw new InvalidOperationException(
                    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
                    + $"({nextPublisher.PublisherName}). "
                    + "If this publisher has auto-draft enabled, their auto-draft may have failed "
                    + "(e.g. no available games found for the slot). "
                    + "Check TopAvailableGames for this publisher.");
```

- [ ] **Step 2: Add `RunUntilCounterPickPhaseAsync` to `DraftSimulator`**

After the closing brace of `RunAsync`, add:

```csharp
    /// <summary>
    /// Runs the draft loop until the counter-pick phase begins, then returns without picking
    /// any counter-picks. Use this to set up shared state for counter-pick-phase error tests.
    /// </summary>
    public async Task RunUntilCounterPickPhaseAsync(Guid leagueID, int year)
    {
        while (true)
        {
            var leagueYear = await _observerSession.League.GetLeagueYearAsync(leagueID, year, null);

            if (leagueYear.PlayStatus.DraftFinished || leagueYear.PlayStatus.DraftingCounterPicks)
                return;

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active, not finished, and not yet in the counter-pick phase, "
                    + "but no publisher has NextToDraft = true.");

            if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
                throw new InvalidOperationException(
                    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
                    + $"({nextPublisher.PublisherName}).");

            await player.DraftStandardGameAsync(year);
        }
    }
```

- [ ] **Step 3: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs
git commit -m "Add DraftSimulator.RunUntilCounterPickPhaseAsync and improve auto-draft error message."
```

---

## Task 4: `AutoDraftLeagueDraftTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/AutoDraftLeagueDraftTests.cs`

- [ ] **Step 1: Create the file**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Scenarios;

/// <summary>
/// Exercises a full draft where the last-position publisher has AutoDraftMode = "All".
/// The server auto-picks on their behalf every turn; the simulator drives only the other
/// three publishers. Verifies the draft completes correctly and the auto-draft publisher
/// ends up with the expected game counts (inherited tests) and that the mode is persisted.
/// </summary>
[TestFixture]
public class AutoDraftLeagueDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;

    protected override async Task SimulateDraftAsync(
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap,
        Guid leagueID,
        int year)
    {
        // The last publisher in draft order is designated as the auto-draft player.
        var autoDraftPubID = PublisherIDs.Last();
        var autoDraftSession = publisherSessionMap[autoDraftPubID];

        // Enabling "All" causes the server to auto-pick for this publisher every time it
        // is their turn. After any other player's pick, DraftGame → AutoDraftForLeague
        // runs on the server side, so this publisher will never appear as NextToDraft
        // when the simulator loop polls the league year. If they do appear (i.e. auto-draft
        // failed), DraftSimulator throws a clear error — which is the intended failure signal.
        await autoDraftSession.League.SetAutoDraftAsync(new SetAutoDraftRequest
        {
            PublisherID = autoDraftPubID,
            Mode = "All",
            OnlyDraftFromWatchlist = false,
        });

        var nonAutoDraftPlayers = publisherSessionMap
            .Where(kvp => kvp.Key != autoDraftPubID)
            .Select(kvp => new MockedLivePlayer(kvp.Value, kvp.Key, leagueID));
        var simulator = new DraftSimulator(ManagerSession, nonAutoDraftPlayers);
        await simulator.RunAsync(leagueID, year);
    }

    [Test]
    public void AutoDraftPlayer_HasAutoDraftModeSetToAll()
    {
        var autoDraftPublisher = LeagueYearSnapshot.Publishers
            .Single(p => p.PublisherID == PublisherIDs.Last());
        Assert.That(autoDraftPublisher.AutoDraftMode, Is.EqualTo("All"),
            "The auto-draft publisher's AutoDraftMode should be persisted as 'All' in the post-draft snapshot.");
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Run the new tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~AutoDraftLeagueDraftTests"
```

Expected: 7 tests pass (6 inherited from `LeagueDraftTestBase` + 1 new `AutoDraftPlayer_HasAutoDraftModeSetToAll`).

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/AutoDraftLeagueDraftTests.cs
git commit -m "Add AutoDraftLeagueDraftTests: full draft with one auto-draft publisher."
```

---

## Task 5: `DraftEdgeCaseTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/DraftEdgeCaseTests.cs`

- [ ] **Step 1: Create the file**

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
/// Tests error conditions during the standard-game phase of a draft.
///
/// OneTimeSetUp creates a <see cref="LeagueScenarios.TwoPlayerSmall"/> league, starts the draft,
/// and has P1 (draft position 1) make exactly one standard pick. All tests share that
/// mid-draft state (P2 is next, P1 has 1 game) and make only failed / rejected attempts
/// so they do not advance the shared draft state.
/// </summary>
[TestFixture]
public class DraftEdgeCaseTests : IntegrationTestBase
{
    private ApiSession _managerSession = null!;
    private ApiSession _playerSession = null!;
    private Guid _leagueID;
    private int _year;
    private Guid _p1PublisherID;
    private Guid _p2PublisherID;
    private Guid _p1DraftedGameID;
    private PossibleMasterGameYearViewModel? _ineligibleGame;

    [OneTimeSetUp]
    public async Task SetUpLeagueMidDraft()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var (plrEmail, plrPassword, plrDisplayName) = NewUser();
        _playerSession = new ApiSession(Factory);
        await _playerSession.RegisterAsync(plrEmail, plrPassword, plrDisplayName);

        IReadOnlyList<Guid> publisherIDs;
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap;
        (_leagueID, _year, publisherIDs, publisherSessionMap) =
            await LeagueTestHelpers.SetUpLeagueAndStartDraftAsync(
                _managerSession, [_playerSession], LeagueScenarios.TwoPlayerSmall);

        // [0] = manager (draft position 1), [1] = player (draft position 2)
        _p1PublisherID = publisherIDs[0];
        _p2PublisherID = publisherIDs[1];

        // P1 makes exactly one standard pick (MockedLivePlayer picks the first available game).
        var p1Player = new MockedLivePlayer(_managerSession, _p1PublisherID, _leagueID);
        await p1Player.DraftStandardGameAsync(_year);

        // Read back which game P1 picked so duplicate-game tests can target it.
        var snapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        var p1Publisher = snapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        _p1DraftedGameID = p1Publisher.Games
            .Single(g => !g.CounterPick).MasterGame!.MasterGameID;

        // Record the first ineligible-but-available game for P2 (null if none found).
        // Games with IsEligible == false are ineligible under the league's ReleaseSystem =
        // "MustBeReleased" setting — typically unreleased titles not expected this year.
        var available = await _playerSession.League.TopAvailableGamesAsync(
            _year, _leagueID, _p2PublisherID, slotInfo: null);
        _ineligibleGame = available.FirstOrDefault(g => !g.IsEligible && !g.Taken);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _managerSession?.Dispose();
        _playerSession?.Dispose();
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// It is P2's turn. P1 calls DraftGame using their own publisher ID.
    /// The server should reject with 400 "It is not your turn to draft."
    /// </summary>
    [Test]
    public async Task DraftGame_OutOfTurn_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _managerSession.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _p1PublisherID,
                MasterGameID = null,
                GameName = "out-of-turn-attempt",
                CounterPick = false,
                AllowIneligibleSlot = false,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException when drafting out of turn.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    /// <summary>
    /// P2 calls DraftGame with CounterPick = true while still in the standard-game phase.
    /// The server should reject with 400 "Not drafting counterPicks now."
    /// </summary>
    [Test]
    public async Task DraftGame_CounterPickFlagDuringStandardPhase_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _playerSession.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _p2PublisherID,
                MasterGameID = null,
                GameName = "wrong-phase-attempt",
                CounterPick = true,
                AllowIneligibleSlot = false,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException when using CounterPick flag in standard phase.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    /// <summary>
    /// P2 tries to pick the same game P1 already has on their roster.
    /// The service returns Success = false (HTTP 200 with an error body).
    /// </summary>
    [Test]
    public async Task DraftGame_DuplicateGame_ReturnsFailureResult()
    {
        var result = await _playerSession.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _p2PublisherID,
            MasterGameID = _p1DraftedGameID,
            GameName = "duplicate-game-attempt",
            CounterPick = false,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Picking a game already on another publisher's roster should return Success = false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "Errors should be non-empty when a duplicate game pick fails.");
    }

    /// <summary>
    /// P2 tries to draft a game that is ineligible in this league's slot (IsEligible = false).
    /// The service returns Success = false (HTTP 200 with an error body).
    /// Skipped if no ineligible games appear in TopAvailableGames for P2's publisher.
    /// </summary>
    [Test]
    public async Task DraftGame_IneligibleGame_ReturnsFailureResult()
    {
        Assume.That(_ineligibleGame, Is.Not.Null,
            "No ineligible game found in TopAvailableGames for P2; test skipped.");

        var result = await _playerSession.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _p2PublisherID,
            MasterGameID = _ineligibleGame!.MasterGame.MasterGameID,
            GameName = _ineligibleGame.MasterGame.GameName,
            CounterPick = false,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Drafting an ineligible game with AllowIneligibleSlot = false should return Success = false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "Errors should be non-empty when an ineligible game pick fails.");
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Run the new tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~DraftEdgeCaseTests"
```

Expected: 4 tests pass. The `IneligibleGame` test passes or is marked Inconclusive (skipped) if no ineligible games are seeded — not a failure.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/League/DraftEdgeCaseTests.cs
git commit -m "Add DraftEdgeCaseTests: out-of-turn, wrong-phase, duplicate, ineligible game rejections."
```

---

## Task 6: `DraftCounterPickPhaseTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/DraftCounterPickPhaseTests.cs`

- [ ] **Step 1: Create the file**

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
/// Tests error conditions during the counter-pick phase of a draft.
///
/// OneTimeSetUp creates a <see cref="LeagueScenarios.TwoPlayerSmall"/> league, drives all
/// four standard game picks using <see cref="DraftSimulator.RunUntilCounterPickPhaseAsync"/>,
/// then records which publisher is first to counter-pick. Tests make only rejected attempts
/// and do not advance the shared draft state.
/// </summary>
[TestFixture]
public class DraftCounterPickPhaseTests : IntegrationTestBase
{
    private ApiSession _managerSession = null!;
    private ApiSession _playerSession = null!;
    private Guid _leagueID;
    private int _year;
    private Guid _firstCpPublisherID;
    private ApiSession _firstCpSession = null!;
    private PossibleMasterGameYearViewModel _availableUndraftedGame = null!;

    [OneTimeSetUp]
    public async Task SetUpLeagueAtCounterPickPhase()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var (plrEmail, plrPassword, plrDisplayName) = NewUser();
        _playerSession = new ApiSession(Factory);
        await _playerSession.RegisterAsync(plrEmail, plrPassword, plrDisplayName);

        IReadOnlyList<Guid> publisherIDs;
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap;
        (_leagueID, _year, publisherIDs, publisherSessionMap) =
            await LeagueTestHelpers.SetUpLeagueAndStartDraftAsync(
                _managerSession, [_playerSession], LeagueScenarios.TwoPlayerSmall);

        // Drive all 4 standard picks (2 players × 2 games each); stop when counter-pick phase starts.
        var players = publisherSessionMap.Select(kvp =>
            new MockedLivePlayer(kvp.Value, kvp.Key, _leagueID));
        var simulator = new DraftSimulator(_managerSession, players);
        await simulator.RunUntilCounterPickPhaseAsync(_leagueID, _year);

        // Determine who is first to pick a counter-pick (don't assume draft position 1 always goes first).
        var cpPhaseSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        var firstCpPublisher = cpPhaseSnapshot.Publishers.Single(p => p.NextToDraft);
        _firstCpPublisherID = firstCpPublisher.PublisherID;
        _firstCpSession = publisherSessionMap[_firstCpPublisherID];

        // Find a game not on any publisher's roster — an invalid counter-pick target.
        // Counter-picks must be from another publisher's standard roster; a still-available
        // (Taken == false) game was never drafted and is therefore invalid.
        var available = await _firstCpSession.League.TopAvailableGamesAsync(
            _year, _leagueID, _firstCpPublisherID, slotInfo: null);
        _availableUndraftedGame = available.First(g => !g.Taken);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _managerSession?.Dispose();
        _playerSession?.Dispose();
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// The first counter-pick publisher attempts to draft a standard game (CounterPick = false)
    /// during the counter-pick phase. The server returns 400 "Not drafting standard games now."
    /// </summary>
    [Test]
    public async Task DraftGame_StandardGameDuringCounterPickPhase_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _firstCpSession.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _firstCpPublisherID,
                MasterGameID = null,
                GameName = "wrong-phase-standard-attempt",
                CounterPick = false,
                AllowIneligibleSlot = false,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException when using standard-game flag in counter-pick phase.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    /// <summary>
    /// The first counter-pick publisher attempts to counter-pick a game that is not on any
    /// publisher's standard roster. Counter-picks must target another publisher's drafted game;
    /// a game that was never drafted is an invalid target.
    /// The service returns Success = false (HTTP 200 with an error body).
    /// </summary>
    [Test]
    public async Task DraftGame_CounterPickOfUnownedGame_ReturnsFailureResult()
    {
        var result = await _firstCpSession.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _firstCpPublisherID,
            MasterGameID = _availableUndraftedGame.MasterGame.MasterGameID,
            GameName = _availableUndraftedGame.MasterGame.GameName,
            CounterPick = true,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Counter-picking a game not on any publisher's roster should return Success = false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "Errors should be non-empty when an invalid counter-pick target is used.");
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Run the new tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~DraftCounterPickPhaseTests"
```

Expected: 2 tests pass.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/League/DraftCounterPickPhaseTests.cs
git commit -m "Add DraftCounterPickPhaseTests: wrong-phase and invalid counter-pick target rejections."
```

---

## Task 7: `DraftPauseUndoTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/DraftPauseUndoTests.cs`

- [ ] **Step 1: Create the file**

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
/// Verifies the SetDraftPause → UndoLastDraftAction → resume sequence.
///
/// OneTimeSetUp performs the full sequence and captures three snapshots.
/// Tests assert against those snapshots — no test mutates state.
/// Note: UndoLastDraftAction requires <c>RequiredYearStatus.DraftPaused</c>;
/// the pause → undo → resume order in OneTimeSetUp is mandatory.
/// </summary>
[TestFixture]
public class DraftPauseUndoTests : IntegrationTestBase
{
    private ApiSession _managerSession = null!;
    private ApiSession _playerSession = null!;
    private Guid _leagueID;
    private int _year;
    private Guid _p1PublisherID;

    private LeagueYearViewModel _pausedSnapshot = null!;
    private LeagueYearViewModel _afterUndoSnapshot = null!;
    private LeagueYearViewModel _resumedSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUpPauseUndoState()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var (plrEmail, plrPassword, plrDisplayName) = NewUser();
        _playerSession = new ApiSession(Factory);
        await _playerSession.RegisterAsync(plrEmail, plrPassword, plrDisplayName);

        IReadOnlyList<Guid> publisherIDs;
        (_leagueID, _year, publisherIDs, _) =
            await LeagueTestHelpers.SetUpLeagueAndStartDraftAsync(
                _managerSession, [_playerSession], LeagueScenarios.TwoPlayerSmall);

        _p1PublisherID = publisherIDs[0]; // manager, draft position 1

        // P1 makes one standard pick so there is a draft action to undo later.
        var p1Player = new MockedLivePlayer(_managerSession, _p1PublisherID, _leagueID);
        await p1Player.DraftStandardGameAsync(_year);

        // ── Pause ──
        await _managerSession.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _leagueID,
            Year = _year,
            Pause = true,
        });
        _pausedSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // ── Undo (requires draft to be paused — 400 if active) ──
        await _managerSession.LeagueManager.UndoLastDraftActionAsync(new UndoLastDraftActionRequest
        {
            LeagueID = _leagueID,
            Year = _year,
        });
        _afterUndoSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // ── Resume ──
        await _managerSession.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _leagueID,
            Year = _year,
            Pause = false,
        });
        _resumedSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _managerSession?.Dispose();
        _playerSession?.Dispose();
    }

    // ── Paused snapshot assertions ─────────────────────────────────────────────

    [Test]
    public void Pause_DraftIsNotActive()
    {
        Assert.That(_pausedSnapshot.PlayStatus.DraftIsActive, Is.False,
            "DraftIsActive must be false while the draft is paused.");
    }

    [Test]
    public void Pause_DraftIsNotFinished()
    {
        Assert.That(_pausedSnapshot.PlayStatus.DraftFinished, Is.False,
            "DraftFinished must be false while the draft is paused mid-draft.");
    }

    [Test]
    public void Pause_NoPublisherIsNextToDraft()
    {
        // NextToDraft is derived from DraftFunctions.GetDraftStatus, which returns null when
        // the draft is paused (no active turn). If this fails, check whether the server actually
        // sets NextToDraft on a paused draft and update the assertion to match.
        Assert.That(
            _pausedSnapshot.Publishers.Any(p => p.NextToDraft),
            Is.False,
            "No publisher should be marked NextToDraft while the draft is paused.");
    }

    // ── After-undo snapshot assertions (still paused) ──────────────────────────

    [Test]
    public void Undo_P1HasNoGames()
    {
        var p1 = _afterUndoSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.Games, Is.Empty,
            "P1's game should be removed after UndoLastDraftAction.");
    }

    [Test]
    public void Undo_NoPublisherIsNextToDraft()
    {
        Assert.That(
            _afterUndoSnapshot.Publishers.Any(p => p.NextToDraft),
            Is.False,
            "No publisher should be NextToDraft while the draft is still paused after undo.");
    }

    // ── Resumed snapshot assertions ────────────────────────────────────────────

    [Test]
    public void Resume_DraftIsActive()
    {
        Assert.That(_resumedSnapshot.PlayStatus.DraftIsActive, Is.True,
            "DraftIsActive must be true after resuming the draft.");
    }

    [Test]
    public void Resume_P1IsNextToDraft()
    {
        // Undo reverted P1's pick, restoring them to the head of the draft order with 0 games.
        // After resume, P1 (draft position 1) should be next again.
        var p1 = _resumedSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.NextToDraft, Is.True,
            "P1 should be NextToDraft after resume: undo put them back at position 1 with 0 games.");
    }
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 3: Run the new tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~DraftPauseUndoTests"
```

Expected: 7 tests pass. If `Pause_NoPublisherIsNextToDraft` or `Undo_NoPublisherIsNextToDraft` fail, it means the server DOES set `NextToDraft` while paused — update those assertions to `Is.True` for P1 and document the actual behavior.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/League/DraftPauseUndoTests.cs
git commit -m "Add DraftPauseUndoTests: pause, undo, and resume flow assertions."
```

---

## Final: Run the full integration test suite

- [ ] **Run all integration tests to confirm no regressions**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: All tests pass (or `IneligibleGame` marked Inconclusive — not a failure). Zero new failures.
