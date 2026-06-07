# League Integration Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement integration tests for `LeagueController` and `LeagueManagerController` covering league creation, member management, and full draft execution (both player-side and manager-side).

**Architecture:** Nine controller actions are missing `[ProducesResponseType<T>(Status200OK)]` attributes, causing NSwag to generate untyped `Task` (void) returns for those methods. Task 1 fixes those annotations and regenerates the client. Subsequent tasks build a layered helper library (`LeagueScenario`, `LeagueTestHelpers`, `MockedLivePlayer`, `DraftSimulator`) and three test fixture groups (`LeagueSetupTests`, `LeagueMemberTests`, `StandardLeagueDraftTests`/`ManagerDraftTests`).

**Tech Stack:** C# 13 / .NET 10, NUnit 3, NSwag 14.7.1, `FantasyCritic.ApiClient` (auto-generated), `FantasyCritic.IntegrationTests` (test project)

---

## Reference — Generated Client Method Signatures After Task 1

Once the `[ProducesResponseType]` attributes are added and the client is regenerated, these methods will have the following typed signatures. All later tasks assume these signatures:

| Client | Method | Return type |
|---|---|---|
| `LeagueManagerClient` | `CreateLeagueAsync(CreateLeagueRequest)` | `Task<Guid>` |
| `LeagueManagerClient` | `InviteLinksAsync(Guid leagueID)` | `Task<ICollection<LeagueInviteLinkViewModel>>` |
| `LeagueManagerClient` | `ManagerDraftGameAsync(ManagerDraftGameRequest)` | `Task<ManagerClaimResultViewModel>` |
| `LeagueClient` | `GetLeagueAsync(Guid id, Guid? inviteCode)` | `Task<LeagueViewModel>` |
| `LeagueClient` | `GetLeagueYearAsync(Guid? leagueID, int? year, Guid? inviteCode)` | `Task<LeagueYearViewModel>` |
| `LeagueClient` | `GetLeagueYearOptionsAsync(Guid? leagueID, int? year)` | `Task<LeagueYearSettingsViewModel>` |
| `LeagueClient` | `DraftGameAsync(DraftGameRequest)` | `Task<PlayerClaimResultViewModel>` |
| `LeagueClient` | `TopAvailableGamesAsync(int? year, Guid? leagueID, Guid? publisherID, string? slotInfo)` | `Task<ICollection<PossibleMasterGameYearViewModel>>` |
| `LeagueClient` | `PossibleCounterPicksAsync(Guid? publisherID)` | `Task<ICollection<PublisherGameViewModel>>` |

New ViewModel types generated in `FantasyCriticClients.cs` (usable from tests):

| Type | Key properties |
|---|---|
| `LeagueViewModel` | `LeagueID`, `LeagueName`, `IsManager`, `Players`, `Years`, `ActiveYear`, `PublicLeague`, `TestLeague` |
| `LeagueYearViewModel` | `LeagueID`, `Year`, `Settings`, `Publishers`, `PlayStatus`, `Players` |
| `PlayStatusViewModel` | `PlayStatus`, `DraftIsActive`, `DraftFinished`, `DraftingCounterPicks`, `ReadyToDraft`, `StartDraftErrors` |
| `PublisherViewModel` | `PublisherID`, `UserID`, `PublisherName`, `Games`, `NextToDraft`, `DraftPosition` |
| `ManagerClaimResultViewModel` | `Success`, `Errors`, `Overridable`, `ShowAsWarning` |
| `LeagueInviteLinkViewModel` | `InviteID`, `LeagueID`, `InviteCode`, `FullInviteLink` |

---

## File Map

| Action | File |
|---|---|
| Modify | `src/FantasyCritic.Web/Controllers/API/LeagueController.cs` |
| Modify | `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs` |
| Regenerate | `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/LeagueSetupTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/LeagueMemberTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/LeagueDraftTestBase.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/StandardLeagueDraftTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/LeagueManager/ManagerDraftTests.cs` |

---

## Task 1: Add `[ProducesResponseType<T>]` to Controllers and Regenerate Client

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueController.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs`
- Regenerate: `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs`

**Background:** NSwag infers the 200 OK response type from `ActionResult<T>` only when the action has *no* `[ProducesResponseType]` attributes at all. The moment any `[ProducesResponseType]` error attribute (400, 401, 403) is present, NSwag relies entirely on explicit attributes for *all* response codes — including 200. Without a 200 attribute the generated method returns `Task` (void). The fix is to add `[ProducesResponseType<T>(StatusCodes.Status200OK)]` to each affected action, then rebuild and regenerate.

- [ ] **Step 1: Add success annotation to `GetLeague` in `LeagueController.cs`**

Find this block (around line 120):
```csharp
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueViewModel>> GetLeague(Guid id, Guid? inviteCode)
```
Add one attribute so it becomes:
```csharp
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType<LeagueViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueViewModel>> GetLeague(Guid id, Guid? inviteCode)
```

- [ ] **Step 2: Add success annotation to `GetLeagueYear` in `LeagueController.cs`**

Find this block (around line 162):
```csharp
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueYearViewModel>> GetLeagueYear(Guid leagueID, int year, Guid? inviteCode)
```
Add the 200 OK attribute:
```csharp
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<LeagueYearViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueYearViewModel>> GetLeagueYear(Guid leagueID, int year, Guid? inviteCode)
```

- [ ] **Step 3: Add success annotation to `GetLeagueYearOptions` in `LeagueController.cs`**

Find this block (around line 636):
```csharp
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueYearSettingsViewModel>> GetLeagueYearOptions(Guid leagueID, int year)
```
Add:
```csharp
    [AllowAnonymous]
    [ProducesResponseType<LeagueYearSettingsViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueYearSettingsViewModel>> GetLeagueYearOptions(Guid leagueID, int year)
```

- [ ] **Step 4: Add success annotation to `DraftGame` in `LeagueController.cs`**

Find this block (around line 1098):
```csharp
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlayerClaimResultViewModel>> DraftGame([FromBody] DraftGameRequest request)
```
Add:
```csharp
    [HttpPost]
    [ProducesResponseType<PlayerClaimResultViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlayerClaimResultViewModel>> DraftGame([FromBody] DraftGameRequest request)
```

- [ ] **Step 5: Add success annotation to `TopAvailableGames` in `LeagueController.cs`**

Find this block (around line 1245):
```csharp
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PossibleMasterGameYearViewModel>>> TopAvailableGames(int year, Guid leagueID, Guid publisherID, string? slotInfo)
```
Add:
```csharp
    [HttpGet]
    [ProducesResponseType<List<PossibleMasterGameYearViewModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PossibleMasterGameYearViewModel>>> TopAvailableGames(int year, Guid leagueID, Guid publisherID, string? slotInfo)
```

- [ ] **Step 6: Add success annotation to `PossibleCounterPicks` in `LeagueController.cs`**

Find this block (around line 1317):
```csharp
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PublisherGameViewModel>>> PossibleCounterPicks(Guid publisherID)
```
Add:
```csharp
    [HttpGet]
    [ProducesResponseType<List<PublisherGameViewModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PublisherGameViewModel>>> PossibleCounterPicks(Guid publisherID)
```

- [ ] **Step 7: Add success annotation to `CreateLeague` in `LeagueManagerController.cs`**

Find this block (around line 52):
```csharp
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateLeague([FromBody] CreateLeagueRequest request)
```
Add:
```csharp
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateLeague([FromBody] CreateLeagueRequest request)
```

- [ ] **Step 8: Add success annotation to `InviteLinks` in `LeagueManagerController.cs`**

Find this block (around line 352):
```csharp
    [HttpGet("{leagueID}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<LeagueInviteLinkViewModel>>> InviteLinks(Guid leagueID)
```
Add:
```csharp
    [HttpGet("{leagueID}")]
    [ProducesResponseType<List<LeagueInviteLinkViewModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<LeagueInviteLinkViewModel>>> InviteLinks(Guid leagueID)
```

- [ ] **Step 9: Add success annotation to `ManagerDraftGame` in `LeagueManagerController.cs`**

Find this block (around line 920):
```csharp
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ManagerClaimResultViewModel>> ManagerDraftGame([FromBody] ManagerDraftGameRequest request)
```
Add:
```csharp
    [HttpPost]
    [ProducesResponseType<ManagerClaimResultViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ManagerClaimResultViewModel>> ManagerDraftGame([FromBody] ManagerDraftGameRequest request)
```

- [ ] **Step 10: Build Web project**

Run from repo root:
```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 11: Regenerate the API client**

```powershell
scripts/Regenerate-ApiClient.ps1
```
Expected: "Done." with no errors.

- [ ] **Step 12: Verify typed return types in the generated client**

Open `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` and confirm:
- `Task<Guid> CreateLeagueAsync(CreateLeagueRequest request)` exists (not `Task`)
- `Task<LeagueViewModel> GetLeagueAsync(...)` exists
- `Task<LeagueYearViewModel> GetLeagueYearAsync(...)` exists
- `Task<ManagerClaimResultViewModel> ManagerDraftGameAsync(...)` exists
- `Task<ICollection<PossibleMasterGameYearViewModel>> TopAvailableGamesAsync(...)` exists

- [ ] **Step 13: Build the full solution**

```powershell
dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 14: Commit**

```powershell
git add src/FantasyCritic.Web/Controllers/API/LeagueController.cs
git add src/FantasyCritic.Web/Controllers/API/LeagueManagerController.cs
git add src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs
git commit -m "Add ProducesResponseType success annotations to league controller actions; regenerate API client"
```

---

## Task 2: `LeagueScenario` class + `LeagueTestHelpers` (GetOpenYear, CreateLeague)

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`
- Create: `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs`

`LeagueScenario` is a plain configuration object that holds every setting needed to create a league for a test. Its `BuildSettings(int year)` method converts those settings into the `LeagueYearSettingsViewModel` request body. `LeagueTestHelpers` provides the async helper methods that test fixtures call.

- [ ] **Step 1: Create `LeagueScenario.cs`**

Create `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`:

```csharp
using System;
using System.Collections.Generic;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Encapsulates all settings required to create a test league.
/// Pass an instance to <see cref="LeagueTestHelpers.CreateLeagueAsync"/> and use the
/// scenario's expected counts in test assertions.
/// </summary>
internal sealed class LeagueScenario
{
    public required string Name { get; init; }

    // Player / publisher counts
    public int PlayerCount { get; init; } = 4;

    // Publisher slot counts
    public int StandardGames { get; init; } = 6;
    public int GamesToDraft { get; init; } = 6;
    public int CounterPicks { get; init; } = 1;
    public int CounterPicksToDraft { get; init; } = 1;

    // Droppable game limits — all zero for "one-shot" leagues
    public int UnrestrictedReleaseStatusDroppableGames { get; init; } = 0;
    public int WillNotReleaseDroppableGames { get; init; } = 0;
    public int WillReleaseDroppableGames { get; init; } = 0;

    // Drop / special rules
    public bool DropOnlyDraftGames { get; init; } = true;
    public bool GrantSuperDrops { get; init; } = false;
    public bool CounterPicksBlockDrops { get; init; } = true;
    public bool AllowMoveIntoIneligible { get; init; } = false;

    // Bid rules
    public int MinimumBidAmount { get; init; } = 0;

    // System strings — must match the enum names used by the server
    public string DraftSystem { get; init; } = "Flexible";
    public string PickupSystem { get; init; } = "SemiPublicBiddingSecretCounterPicks";
    public string ScoringSystem { get; init; } = "LinearPositive";
    public string TradingSystem { get; init; } = "Standard";
    public string TiebreakSystem { get; init; } = "LowestProjectedPoints";
    public string ReleaseSystem { get; init; } = "MustBeReleased";
    public string IneligibleGameSystem { get; init; } = "DroppableAsWillNotRelease";

    // Special slots — none by default
    public bool HasSpecialSlots { get; init; } = false;
    public IReadOnlyList<SpecialGameSlotViewModel> SpecialGameSlots { get; init; } = [];

    /// <summary>
    /// Builds the <see cref="LeagueYearSettingsViewModel"/> request body for the given year.
    /// </summary>
    public LeagueYearSettingsViewModel BuildSettings(int year) =>
        new()
        {
            LeagueID = Guid.Empty,
            Year = year,
            LeagueYearName = null,
            StandardGames = StandardGames,
            GamesToDraft = GamesToDraft,
            CounterPicks = CounterPicks,
            CounterPicksToDraft = CounterPicksToDraft,
            UnrestrictedReleaseStatusDroppableGames = UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames = WillNotReleaseDroppableGames,
            WillReleaseDroppableGames = WillReleaseDroppableGames,
            UnlimitedUnrestrictedReleaseStatusDroppableGames = false,
            UnlimitedWillNotReleaseDroppableGames = false,
            UnlimitedWillReleaseDroppableGames = false,
            DropOnlyDraftGames = DropOnlyDraftGames,
            GrantSuperDrops = GrantSuperDrops,
            CounterPicksBlockDrops = CounterPicksBlockDrops,
            AllowMoveIntoIneligible = AllowMoveIntoIneligible,
            MinimumBidAmount = MinimumBidAmount,
            DraftSystem = DraftSystem,
            PickupSystem = PickupSystem,
            ScoringSystem = ScoringSystem,
            TradingSystem = TradingSystem,
            TiebreakSystem = TiebreakSystem,
            ReleaseSystem = ReleaseSystem,
            IneligibleGameSystem = IneligibleGameSystem,
            // December 1st of the draft year is a safe counter-pick deadline for test leagues
            CounterPickDeadline = new DateTimeOffset(year, 12, 1, 0, 0, 0, TimeSpan.Zero),
            MightReleaseDroppableDate = null,
            Tags = new LeagueTagOptionsViewModel { Banned = [], Required = [] },
            HasSpecialSlots = HasSpecialSlots,
            SpecialGameSlots = SpecialGameSlots.ToList(),
        };

    public override string ToString() => Name;
}

/// <summary>
/// Pre-defined scenarios used across test fixtures.
/// </summary>
internal static class LeagueScenarios
{
    /// <summary>
    /// A simple 4-player league: 6 standard games + 1 counter-pick per publisher,
    /// Flexible draft, LinearPositive scoring, no drops or bids (one-shot).
    /// </summary>
    public static readonly LeagueScenario Standard = new()
    {
        Name = "Standard",
        PlayerCount = 4,
        StandardGames = 6,
        GamesToDraft = 6,
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
}
```

- [ ] **Step 2: Create `LeagueTestHelpers.cs` (GetOpenYear and CreateLeague)**

Create `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs`:

```csharp
using System;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Reusable async helpers for setting up leagues in integration tests.
/// All methods build state through the HTTP API — no direct DB access.
/// </summary>
internal static class LeagueTestHelpers
{
    /// <summary>
    /// Returns the first year currently open for league creation.
    /// Throws if no open years are available (seed DB may be misconfigured).
    /// </summary>
    public static async Task<int> GetOpenYearAsync(ApiSession session)
    {
        var options = await session.League.LeagueOptionsAsync();
        if (options.OpenYears.Count == 0)
            throw new InvalidOperationException(
                "LeagueOptions returned no open years. Is the seed DB running?");
        return options.OpenYears.First();
    }

    /// <summary>
    /// Creates a league under <paramref name="managerSession"/> using the given
    /// <paramref name="scenario"/> settings and returns the new league's GUID.
    /// </summary>
    public static async Task<Guid> CreateLeagueAsync(
        ApiSession managerSession,
        LeagueScenario scenario,
        int year)
    {
        var leagueID = await managerSession.LeagueManager.CreateLeagueAsync(
            new CreateLeagueRequest
            {
                LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
                PublicLeague = false,
                TestLeague = true,
                CustomRulesLeague = false,
                LeagueYearSettings = scenario.BuildSettings(year),
            });

        return leagueID;
    }
}
```

- [ ] **Step 3: Build the test project to verify compilation**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs
git add src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs
git commit -m "Add LeagueScenario and initial LeagueTestHelpers (GetOpenYear, CreateLeague)"
```

---

## Task 3: `LeagueSetupTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/LeagueSetupTests.cs`

These are fully independent, stateless tests. Each test creates its own session(s) and tears down when done. They cover league creation, basic settings round-tripping, and the league-options endpoint.

- [ ] **Step 1: Create `LeagueSetupTests.cs`**

Create `src/FantasyCritic.IntegrationTests/Tests/League/LeagueSetupTests.cs`:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

[TestFixture]
public class LeagueSetupTests : IntegrationTestBase
{
    // ---------------------------------------------------------------------------
    // LeagueOptions (no auth required)
    // ---------------------------------------------------------------------------

    [Test]
    public async Task LeagueOptions_ReturnsAtLeastOneOpenYear()
    {
        using var session = new ApiSession(Factory);
        var options = await session.League.LeagueOptionsAsync();

        Assert.That(options, Is.Not.Null);
        Assert.That(options.OpenYears, Is.Not.Empty, "At least one year must be open for creation.");
    }

    [Test]
    public async Task LeagueOptions_DraftSystems_ContainsFlexible()
    {
        using var session = new ApiSession(Factory);
        var options = await session.League.LeagueOptionsAsync();

        Assert.That(options.DraftSystems, Contains.Item("Flexible"),
            "DraftSystems must include 'Flexible'.");
    }

    // ---------------------------------------------------------------------------
    // CreateLeague
    // ---------------------------------------------------------------------------

    [Test]
    public async Task CreateLeague_WithValidSettings_ReturnsNonEmptyGuid()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        Assert.That(leagueID, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task GetLeague_AfterCreate_ReturnsLeagueWithCorrectManagerFlag()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        var league = await session.League.GetLeagueAsync(leagueID, null);

        Assert.That(league, Is.Not.Null);
        Assert.That(league.LeagueID, Is.EqualTo(leagueID));
        Assert.That(league.IsManager, Is.True, "The creator must be the league manager.");
        Assert.That(league.TestLeague, Is.True);
    }

    [Test]
    public async Task GetLeagueYearOptions_AfterCreate_RoundTripsStandardGameCount()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        var settings = await session.League.GetLeagueYearOptionsAsync(leagueID, year);

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.StandardGames, Is.EqualTo(LeagueScenarios.Standard.StandardGames));
        Assert.That(settings.GamesToDraft, Is.EqualTo(LeagueScenarios.Standard.GamesToDraft));
        Assert.That(settings.CounterPicks, Is.EqualTo(LeagueScenarios.Standard.CounterPicks));
        Assert.That(settings.DraftSystem, Is.EqualTo(LeagueScenarios.Standard.DraftSystem));
        Assert.That(settings.ScoringSystem, Is.EqualTo(LeagueScenarios.Standard.ScoringSystem));
        Assert.That(settings.IneligibleGameSystem, Is.EqualTo(LeagueScenarios.Standard.IneligibleGameSystem));
    }
}
```

- [ ] **Step 2: Run the tests and verify they pass**

```powershell
dotnet test src/FantasyCritic.IntegrationTests --filter "FullyQualifiedName~LeagueSetupTests"
```
Expected: 5 tests pass (or fail with a meaningful assertion error, not a compilation error).

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/League/LeagueSetupTests.cs
git commit -m "Add LeagueSetupTests: league creation and settings round-trip"
```

---

## Task 4: `LeagueTestHelpers` (invite/accept/publisher/draft-order) + `LeagueMemberTests`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs`
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/LeagueMemberTests.cs`

`LeagueMemberTests` is an ordered fixture: `[OneTimeSetUp]` builds the complete league-member state and the individual `[Test]` methods assert against that state.

- [ ] **Step 1: Add helpers to `LeagueTestHelpers.cs`**

Add these four static methods to the `LeagueTestHelpers` class:

```csharp
    /// <summary>
    /// Manager invites <paramref name="playerSession"/>'s user by email, then
    /// the player accepts the invite for the given league.
    /// </summary>
    public static async Task InviteAndAcceptAsync(
        ApiSession managerSession,
        ApiSession playerSession,
        Guid leagueID)
    {
        var playerUser = await playerSession.Account.CurrentUserAsync();

        await managerSession.LeagueManager.InvitePlayerAsync(new CreateInviteRequest
        {
            LeagueID = leagueID,
            InviteEmail = playerUser.EmailAddress,
        });

        // The player must be able to see the pending invite
        var pendingInvites = await playerSession.League.MyInvitesAsync();
        var invite = pendingInvites.SingleOrDefault(i => i.LeagueID == leagueID)
            ?? throw new InvalidOperationException(
                $"Player {playerUser.DisplayName} does not see a pending invite for league {leagueID}.");

        await playerSession.League.AcceptInviteAsync(new AcceptInviteRequest
        {
            LeagueID = leagueID,
        });
    }

    /// <summary>
    /// Creates a publisher for <paramref name="playerSession"/> in the given league/year.
    /// Returns the new publisher's GUID.
    /// </summary>
    public static async Task<Guid> CreatePublisherAsync(
        ApiSession playerSession,
        Guid leagueID,
        int year,
        string publisherName)
    {
        // CreatePublisherAsync returns Task (void) — the publisher ID is read back
        // from GetLeagueYear after creation.
        var currentUser = await playerSession.Account.CurrentUserAsync();

        await playerSession.League.CreatePublisherAsync(new CreatePublisherRequest
        {
            LeagueID = leagueID,
            Year = year,
            PublisherName = publisherName,
        });

        var leagueYear = await playerSession.League.GetLeagueYearAsync(leagueID, year, null);
        var publisher = leagueYear.Publishers.SingleOrDefault(p => p.UserID == currentUser.UserID)
            ?? throw new InvalidOperationException(
                $"Publisher not found for user {currentUser.UserID} after CreatePublisher.");

        return publisher.PublisherID;
    }

    /// <summary>
    /// Sets the draft order to the supplied publisher order using the "Manual" type.
    /// </summary>
    public static async Task SetDraftOrderAsync(
        ApiSession managerSession,
        Guid leagueID,
        int year,
        IReadOnlyList<Guid> publisherIDsInOrder)
    {
        await managerSession.LeagueManager.SetDraftOrderAsync(new DraftOrderRequest
        {
            LeagueID = leagueID,
            Year = year,
            DraftOrderType = "Manual",
            ManualPublisherDraftPositions = publisherIDsInOrder.ToList(),
        });
    }
```

- [ ] **Step 2: Build to verify compilation**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Create `LeagueMemberTests.cs`**

Create `src/FantasyCritic.IntegrationTests/Tests/League/LeagueMemberTests.cs`:

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
/// Tests member-management workflows: inviting, accepting, creating publishers,
/// setting draft order, invite links, and player removal.
///
/// The <see cref="SetUpMembers"/> fixture sets up a fully-populated 4-player league
/// (manager + 3 players, all with publishers, draft order set). The individual
/// [Test] methods assert against that state. Tests are ordered so that destructive
/// operations (remove, rescind) run after the read-only checks.
/// </summary>
[TestFixture]
public class LeagueMemberTests : IntegrationTestBase
{
    private ApiSession _managerSession = null!;
    private List<ApiSession> _playerSessions = null!;
    private Guid _leagueID;
    private int _year;
    private List<Guid> _publisherIDs = null!;

    [OneTimeSetUp]
    public async Task SetUpMembers()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        _playerSessions = new List<ApiSession>();
        for (int i = 0; i < LeagueScenarios.Standard.PlayerCount - 1; i++)
        {
            var (email, password, displayName) = NewUser();
            var session = new ApiSession(Factory);
            await session.RegisterAsync(email, password, displayName);
            _playerSessions.Add(session);
        }

        _year = await LeagueTestHelpers.GetOpenYearAsync(_managerSession);
        _leagueID = await LeagueTestHelpers.CreateLeagueAsync(_managerSession, LeagueScenarios.Standard, _year);

        // Invite and accept for all players
        foreach (var playerSession in _playerSessions)
        {
            await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, playerSession, _leagueID);
        }

        // Create publishers
        _publisherIDs = new List<Guid>();
        var managerPubID = await LeagueTestHelpers.CreatePublisherAsync(
            _managerSession, _leagueID, _year, "Manager Publisher");
        _publisherIDs.Add(managerPubID);

        for (int i = 0; i < _playerSessions.Count; i++)
        {
            var pubID = await LeagueTestHelpers.CreatePublisherAsync(
                _playerSessions[i], _leagueID, _year, $"Player{i + 1} Publisher");
            _publisherIDs.Add(pubID);
        }

        // Set draft order
        await LeagueTestHelpers.SetDraftOrderAsync(_managerSession, _leagueID, _year, _publisherIDs);
    }

    [OneTimeTearDown]
    public void TearDownSessions()
    {
        _managerSession?.Dispose();
        if (_playerSessions != null)
            foreach (var s in _playerSessions)
                s.Dispose();
    }

    // ---------------------------------------------------------------------------
    // Read-only checks (run first)
    // ---------------------------------------------------------------------------

    [Test, Order(1)]
    public async Task League_AfterSetup_IsManager_ForCreator()
    {
        var league = await _managerSession.League.GetLeagueAsync(_leagueID, null);
        Assert.That(league.IsManager, Is.True);
    }

    [Test, Order(2)]
    public async Task LeagueYear_AfterSetup_HasExpectedPublisherCount()
    {
        var leagueYear = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        Assert.That(leagueYear.Publishers.Count,
            Is.EqualTo(LeagueScenarios.Standard.PlayerCount),
            "Every player (including manager) must have a publisher.");
    }

    [Test, Order(3)]
    public async Task LeagueYear_AfterSetup_DraftOrderIsSet()
    {
        var leagueYear = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // All publishers should have a non-null DraftPosition
        var positions = leagueYear.Publishers.Select(p => p.DraftPosition).ToList();
        Assert.That(positions, Has.All.Not.Null, "All publishers must have a draft position.");

        // Positions should be 1..N with no duplicates
        var expected = Enumerable.Range(1, LeagueScenarios.Standard.PlayerCount).ToList();
        Assert.That(positions.OrderBy(x => x).ToList(),
            Is.EqualTo(expected),
            "Draft positions must be 1..N.");
    }

    [Test, Order(4)]
    public async Task LeagueYear_AfterSetup_PlayStatusIsReadyToDraft()
    {
        var leagueYear = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        Assert.That(leagueYear.PlayStatus.ReadyToDraft, Is.True,
            "All publishers have been created and draft order set — league should be ready to draft.");
        Assert.That(leagueYear.PlayStatus.StartDraftErrors, Is.Empty,
            $"No start-draft errors expected. Got: {string.Join("; ", leagueYear.PlayStatus.StartDraftErrors ?? [])}");
    }

    [Test, Order(5)]
    public async Task Players_AfterAcceptingInvites_AreVisibleInLeague()
    {
        var league = await _managerSession.League.GetLeagueAsync(_leagueID, null);
        // The Players list on the league view includes all members
        Assert.That(league.Players, Is.Not.Null);
        Assert.That(league.Players!.Count, Is.EqualTo(LeagueScenarios.Standard.PlayerCount));
    }

    // ---------------------------------------------------------------------------
    // Invite link tests
    // ---------------------------------------------------------------------------

    [Test, Order(10)]
    public async Task InviteLink_Created_IsReturnedByInviteLinks()
    {
        await _managerSession.LeagueManager.CreateInviteLinkAsync(
            new CreateInviteLinkRequest { LeagueID = _leagueID });

        var links = await _managerSession.LeagueManager.InviteLinksAsync(_leagueID);

        Assert.That(links, Is.Not.Empty, "At least one invite link must be returned.");
        Assert.That(links.First().LeagueID, Is.EqualTo(_leagueID));
        Assert.That(links.First().InviteCode, Is.Not.EqualTo(Guid.Empty));
    }

    [Test, Order(11)]
    public async Task JoinWithInviteLink_NewUser_JoinsLeague()
    {
        // Ensure an invite link exists
        await _managerSession.LeagueManager.CreateInviteLinkAsync(
            new CreateInviteLinkRequest { LeagueID = _leagueID });
        var links = await _managerSession.LeagueManager.InviteLinksAsync(_leagueID);
        var inviteCode = links.First().InviteCode;

        var (email, password, displayName) = NewUser();
        using var newPlayerSession = new ApiSession(Factory);
        await newPlayerSession.RegisterAsync(email, password, displayName);

        await newPlayerSession.League.JoinWithInviteLinkAsync(new JoinWithInviteLinkRequest
        {
            LeagueID = _leagueID,
            InviteCode = inviteCode,
        });

        // Verify the new user sees the invite (pending accept)
        var invites = await newPlayerSession.League.MyInvitesAsync();
        Assert.That(invites.Any(i => i.LeagueID == _leagueID), Is.True,
            "New player should have a pending invite after joining with invite link.");
    }

    // ---------------------------------------------------------------------------
    // Invite rescind test
    // ---------------------------------------------------------------------------

    [Test, Order(20)]
    public async Task RescindInvite_PendingInvite_RemovesItFromPlayerView()
    {
        // Invite a fresh user but don't have them accept
        var (email, password, displayName) = NewUser();
        using var pendingSession = new ApiSession(Factory);
        await pendingSession.RegisterAsync(email, password, displayName);

        var pendingUser = await pendingSession.Account.CurrentUserAsync();
        await _managerSession.LeagueManager.InvitePlayerAsync(new CreateInviteRequest
        {
            LeagueID = _leagueID,
            InviteEmail = pendingUser.EmailAddress,
        });

        // Verify invite is visible to pending player
        var invitesBefore = await pendingSession.League.MyInvitesAsync();
        var invite = invitesBefore.SingleOrDefault(i => i.LeagueID == _leagueID);
        Assert.That(invite, Is.Not.Null, "Pending invite should be visible to the invitee.");

        // Manager rescinds
        await _managerSession.LeagueManager.RescindInviteAsync(new DeleteInviteRequest
        {
            LeagueID = _leagueID,
            InviteID = invite!.InviteID,
        });

        // Invite should no longer appear for the player
        var invitesAfter = await pendingSession.League.MyInvitesAsync();
        Assert.That(invitesAfter.Any(i => i.LeagueID == _leagueID), Is.False,
            "Invite should be gone after rescind.");
    }
}
```

- [ ] **Step 4: Run the tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests --filter "FullyQualifiedName~LeagueMemberTests"
```
Expected: All tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs
git add src/FantasyCritic.IntegrationTests/Tests/League/LeagueMemberTests.cs
git commit -m "Add invite/accept/publisher/draft-order helpers and LeagueMemberTests"
```

---

## Task 5: `MockedLivePlayer` and `DraftSimulator`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs`

`MockedLivePlayer` encapsulates the logic for a single simulated player picking games during a draft. `DraftSimulator` drives the draft loop: it polls `GetLeagueYear` to find whose turn it is, then delegates to the correct `MockedLivePlayer`.

- [ ] **Step 1: Create `MockedLivePlayer.cs`**

Create `src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Simulates a single live player making picks during a draft.
/// Dynamically selects the first available eligible game each turn via the API
/// (same strategy as the site's own game-search UI).
/// Override <see cref="DraftStandardGameAsync"/> or <see cref="DraftCounterPickAsync"/>
/// to inject custom pick logic in scenario-specific tests.
/// </summary>
internal class MockedLivePlayer
{
    private readonly ApiSession _session;

    public Guid PublisherID { get; }
    public Guid LeagueID { get; }

    public MockedLivePlayer(ApiSession session, Guid publisherID, Guid leagueID)
    {
        _session = session;
        PublisherID = publisherID;
        LeagueID = leagueID;
    }

    /// <summary>
    /// Picks the first available, un-taken eligible game from TopAvailableGames.
    /// </summary>
    public virtual async Task DraftStandardGameAsync(int year)
    {
        var available = await _session.League.TopAvailableGamesAsync(
            year, LeagueID, PublisherID, slotInfo: null);

        var pick = available.FirstOrDefault(g => g.IsAvailable && !g.Taken)
            ?? throw new InvalidOperationException(
                $"TopAvailableGames returned no available games for publisher {PublisherID}.");

        var result = await _session.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = PublisherID,
            MasterGameID = pick.MasterGame.MasterGameID,
            GameName = pick.MasterGame.GameName,
            CounterPick = false,
            AllowIneligibleSlot = false,
        });

        if (!result.Success)
            throw new InvalidOperationException(
                $"DraftGame (standard) failed for publisher {PublisherID}: "
                + string.Join("; ", result.Errors ?? []));
    }

    /// <summary>
    /// Picks the first available counter-pick from PossibleCounterPicks.
    /// </summary>
    public virtual async Task DraftCounterPickAsync(int year)
    {
        var options = await _session.League.PossibleCounterPicksAsync(PublisherID);

        var pick = options.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"PossibleCounterPicks returned no options for publisher {PublisherID}.");

        var result = await _session.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = PublisherID,
            MasterGameID = pick.MasterGame!.MasterGameID,
            GameName = pick.GameName,
            CounterPick = true,
            AllowIneligibleSlot = false,
        });

        if (!result.Success)
            throw new InvalidOperationException(
                $"DraftGame (counter-pick) failed for publisher {PublisherID}: "
                + string.Join("; ", result.Errors ?? []));
    }
}

/// <summary>
/// Drives the draft loop for a full league draft.
/// Polls <see cref="ApiSession.League"/>.GetLeagueYear to find the next publisher,
/// delegates to the correct <see cref="MockedLivePlayer"/>, and returns when the
/// draft is finished.
/// </summary>
internal sealed class DraftSimulator
{
    private readonly ApiSession _observerSession;
    private readonly IReadOnlyDictionary<Guid, MockedLivePlayer> _players;

    /// <param name="observerSession">
    ///   Any authenticated session used only to poll league-year state.
    ///   Typically the manager session.
    /// </param>
    /// <param name="players">One <see cref="MockedLivePlayer"/> per publisher.</param>
    public DraftSimulator(ApiSession observerSession, IEnumerable<MockedLivePlayer> players)
    {
        _observerSession = observerSession;
        _players = players.ToDictionary(p => p.PublisherID);
    }

    /// <summary>
    /// Runs the draft to completion. Throws if a publisher is expected to pick
    /// but has no entry in <see cref="_players"/>.
    /// </summary>
    public async Task RunAsync(Guid leagueID, int year)
    {
        while (true)
        {
            var leagueYear = await _observerSession.League.GetLeagueYearAsync(leagueID, year, null);

            if (leagueYear.PlayStatus.DraftFinished)
                return;

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active and not finished, but no publisher has NextToDraft = true.");

            if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
                throw new InvalidOperationException(
                    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
                    + $"({nextPublisher.PublisherName}).");

            if (leagueYear.PlayStatus.DraftingCounterPicks)
                await player.DraftCounterPickAsync(year);
            else
                await player.DraftStandardGameAsync(year);
        }
    }
}
```

- [ ] **Step 2: Build to verify compilation**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs
git commit -m "Add MockedLivePlayer and DraftSimulator draft helpers"
```

---

## Task 6: `LeagueDraftTestBase` and `StandardLeagueDraftTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/LeagueDraftTestBase.cs`
- Create: `src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/StandardLeagueDraftTests.cs`

`LeagueDraftTestBase` is an abstract NUnit fixture that builds a complete league (create → invite → accept → publishers → draft order → start → simulate), stores a post-draft snapshot, and declares the shared `[Test]` methods. Concrete subclasses provide the `Scenario` property.

- [ ] **Step 1: Create `LeagueDraftTestBase.cs`**

Create `src/FantasyCritic.IntegrationTests/Tests/League/LeagueDraftTestBase.cs`:

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
/// Abstract base for draft scenario tests. Subclasses provide a <see cref="LeagueScenario"/>
/// and optionally override <see cref="SimulateDraftAsync"/> to change how the draft is run
/// (e.g. player-side vs manager-side).
///
/// <see cref="SetUpLeagueAndDraft"/> builds the complete state; the shared <c>[Test]</c> methods
/// assert against the post-draft <see cref="LeagueYearSnapshot"/>.
/// </summary>
public abstract class LeagueDraftTestBase : IntegrationTestBase
{
    protected abstract LeagueScenario Scenario { get; }

    // State filled in by OneTimeSetUp
    protected int Year;
    protected Guid LeagueID;
    protected IReadOnlyList<Guid> PublisherIDs = [];
    protected LeagueYearViewModel LeagueYearSnapshot = null!;
    protected ApiSession ManagerSession = null!;
    protected IReadOnlyList<ApiSession> PlayerSessions = [];

    [OneTimeSetUp]
    public async Task SetUpLeagueAndDraft()
    {
        // 1. Create manager session
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        ManagerSession = new ApiSession(Factory);
        await ManagerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        // 2. Create player sessions (Scenario.PlayerCount - 1 non-manager players)
        var playerSessions = new List<ApiSession>();
        for (int i = 0; i < Scenario.PlayerCount - 1; i++)
        {
            var (email, password, displayName) = NewUser();
            var session = new ApiSession(Factory);
            await session.RegisterAsync(email, password, displayName);
            playerSessions.Add(session);
        }
        PlayerSessions = playerSessions.AsReadOnly();

        // 3. Get open year and create league
        Year = await LeagueTestHelpers.GetOpenYearAsync(ManagerSession);
        LeagueID = await LeagueTestHelpers.CreateLeagueAsync(ManagerSession, Scenario, Year);

        // 4. Invite all players and have them accept
        foreach (var playerSession in PlayerSessions)
        {
            await LeagueTestHelpers.InviteAndAcceptAsync(ManagerSession, playerSession, LeagueID);
        }

        // 5. Create publishers; build publisher-to-session map
        var publisherSessionMap = new Dictionary<Guid, ApiSession>();
        var publisherIDs = new List<Guid>();

        var managerPubID = await LeagueTestHelpers.CreatePublisherAsync(
            ManagerSession, LeagueID, Year, "Manager Publisher");
        publisherSessionMap[managerPubID] = ManagerSession;
        publisherIDs.Add(managerPubID);

        for (int i = 0; i < PlayerSessions.Count; i++)
        {
            var pubID = await LeagueTestHelpers.CreatePublisherAsync(
                PlayerSessions[i], LeagueID, Year, $"Player{i + 1} Publisher");
            publisherSessionMap[pubID] = PlayerSessions[i];
            publisherIDs.Add(pubID);
        }

        PublisherIDs = publisherIDs.AsReadOnly();

        // 6. Set draft order
        await LeagueTestHelpers.SetDraftOrderAsync(ManagerSession, LeagueID, Year, PublisherIDs);

        // 7. Start the draft
        await ManagerSession.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = LeagueID,
            Year = Year,
        });

        // 8. Run the draft simulation (overridable by subclasses)
        await SimulateDraftAsync(publisherSessionMap, LeagueID, Year);

        // 9. Capture the post-draft snapshot
        LeagueYearSnapshot = await ManagerSession.League.GetLeagueYearAsync(LeagueID, Year, null);
    }

    [OneTimeTearDown]
    public void TearDownSessions()
    {
        ManagerSession?.Dispose();
        foreach (var s in PlayerSessions)
            s.Dispose();
    }

    /// <summary>
    /// Runs the draft to completion using <see cref="MockedLivePlayer"/> / <see cref="DraftSimulator"/>.
    /// Override to use the manager-side <c>ManagerDraftGame</c> endpoint instead.
    /// </summary>
    protected virtual async Task SimulateDraftAsync(
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap,
        Guid leagueID,
        int year)
    {
        var players = publisherSessionMap.Select(kvp =>
            new MockedLivePlayer(kvp.Value, kvp.Key, leagueID));
        var simulator = new DraftSimulator(ManagerSession, players);
        await simulator.RunAsync(leagueID, year);
    }

    // ---------------------------------------------------------------------------
    // Shared tests — run for every concrete subclass
    // ---------------------------------------------------------------------------

    [Test]
    public void Draft_Completed_PlayStatusShowsDraftFinished()
    {
        Assert.That(LeagueYearSnapshot.PlayStatus.DraftFinished, Is.True,
            "Draft must be finished after simulation.");
    }

    [Test]
    public void Draft_Completed_DraftIsNoLongerActive()
    {
        Assert.That(LeagueYearSnapshot.PlayStatus.DraftIsActive, Is.False);
    }

    [Test]
    public void Draft_Completed_NoPublisherIsNextToDraft()
    {
        Assert.That(
            LeagueYearSnapshot.Publishers.Any(p => p.NextToDraft),
            Is.False,
            "No publisher should be marked NextToDraft once the draft is finished.");
    }

    [Test]
    public void Draft_Completed_AllPublishersHaveCorrectStandardGameCount()
    {
        foreach (var publisher in LeagueYearSnapshot.Publishers)
        {
            var standardGames = publisher.Games.Count(g => !g.CounterPick);
            Assert.That(standardGames, Is.EqualTo(Scenario.GamesToDraft),
                $"Publisher '{publisher.PublisherName}' should have {Scenario.GamesToDraft} standard games.");
        }
    }

    [Test]
    public void Draft_Completed_AllPublishersHaveCorrectCounterPickCount()
    {
        foreach (var publisher in LeagueYearSnapshot.Publishers)
        {
            var counterPicks = publisher.Games.Count(g => g.CounterPick);
            Assert.That(counterPicks, Is.EqualTo(Scenario.CounterPicksToDraft),
                $"Publisher '{publisher.PublisherName}' should have {Scenario.CounterPicksToDraft} counter-pick(s).");
        }
    }

    [Test]
    public void Draft_Completed_TotalGamesAcrossAllPublishers_EqualsExpectedCount()
    {
        int expectedTotal =
            Scenario.PlayerCount * (Scenario.GamesToDraft + Scenario.CounterPicksToDraft);
        int actualTotal = LeagueYearSnapshot.Publishers.Sum(p => p.Games.Count);

        Assert.That(actualTotal, Is.EqualTo(expectedTotal),
            $"Total games across all publishers should be {expectedTotal}.");
    }
}
```

- [ ] **Step 2: Create `StandardLeagueDraftTests.cs`**

Create `src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/StandardLeagueDraftTests.cs`:

```csharp
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Scenarios;

/// <summary>
/// Exercises a full draft using the player-side <c>DraftGame</c> endpoint.
/// Uses the <see cref="LeagueScenarios.Standard"/> scenario:
/// 4 players, 6 standard + 1 counter-pick, Flexible draft.
/// </summary>
[TestFixture]
public class StandardLeagueDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;
}
```

- [ ] **Step 3: Build to verify compilation**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Run the tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests --filter "FullyQualifiedName~StandardLeagueDraftTests"
```
Expected: All 6 shared tests pass. The one-time setup completes a full 4-player draft (24 standard + 4 counter-picks = 28 total picks).

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/League/LeagueDraftTestBase.cs
git add src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/StandardLeagueDraftTests.cs
git commit -m "Add LeagueDraftTestBase and StandardLeagueDraftTests (player-side draft)"
```

---

## Task 7: `ManagerDraftTests`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/LeagueManager/ManagerDraftTests.cs`

`ManagerDraftTests` inherits all shared draft assertions from `LeagueDraftTestBase` but overrides `SimulateDraftAsync` to use the manager-side `ManagerDraftGame` endpoint instead of player-side `DraftGame`. This verifies that both endpoints produce the same outcome.

- [ ] **Step 1: Create `ManagerDraftTests.cs`**

Create `src/FantasyCritic.IntegrationTests/Tests/LeagueManager/ManagerDraftTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.IntegrationTests.Tests.League;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.LeagueManager;

/// <summary>
/// Exercises a full draft using the manager-side <c>ManagerDraftGame</c> endpoint.
/// Inherits all shared post-draft assertions from <see cref="LeagueDraftTestBase"/>.
/// Uses the same <see cref="LeagueScenarios.Standard"/> scenario as
/// <see cref="Scenarios.StandardLeagueDraftTests"/> so both endpoints are verified to
/// produce the same outcome.
/// </summary>
[TestFixture]
public class ManagerDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;

    /// <summary>
    /// Overrides the default player-side draft with manager-controlled picks
    /// using <c>ManagerDraftGame</c>. The manager session polls league-year state
    /// and makes every pick on behalf of whoever is next.
    /// </summary>
    protected override async Task SimulateDraftAsync(
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap,
        Guid leagueID,
        int year)
    {
        while (true)
        {
            var leagueYear = await ManagerSession.League.GetLeagueYearAsync(leagueID, year, null);

            if (leagueYear.PlayStatus.DraftFinished)
                return;

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active and not finished, but no publisher has NextToDraft = true.");

            if (leagueYear.PlayStatus.DraftingCounterPicks)
            {
                // Counter-pick phase: pick first available counter-pick for the next publisher
                var options = await ManagerSession.League.PossibleCounterPicksAsync(
                    nextPublisher.PublisherID);

                var pick = options.FirstOrDefault()
                    ?? throw new InvalidOperationException(
                        $"PossibleCounterPicks returned no options for publisher {nextPublisher.PublisherID}.");

                var result = await ManagerSession.LeagueManager.ManagerDraftGameAsync(
                    new ManagerDraftGameRequest
                    {
                        PublisherID = nextPublisher.PublisherID,
                        MasterGameID = pick.MasterGame!.MasterGameID,
                        GameName = pick.GameName,
                        CounterPick = true,
                        ManagerOverride = false,
                        AllowIneligibleSlot = false,
                    });

                if (!result.Success)
                    throw new InvalidOperationException(
                        $"ManagerDraftGame (counter-pick) failed for publisher {nextPublisher.PublisherID}: "
                        + string.Join("; ", result.Errors ?? []));
            }
            else
            {
                // Standard game phase: pick first available eligible game
                var available = await ManagerSession.League.TopAvailableGamesAsync(
                    year, leagueID, nextPublisher.PublisherID, slotInfo: null);

                var game = available.FirstOrDefault(g => g.IsAvailable && !g.Taken)
                    ?? throw new InvalidOperationException(
                        $"TopAvailableGames returned no available games for publisher {nextPublisher.PublisherID}.");

                var result = await ManagerSession.LeagueManager.ManagerDraftGameAsync(
                    new ManagerDraftGameRequest
                    {
                        PublisherID = nextPublisher.PublisherID,
                        MasterGameID = game.MasterGame.MasterGameID,
                        GameName = game.MasterGame.GameName,
                        CounterPick = false,
                        ManagerOverride = false,
                        AllowIneligibleSlot = false,
                    });

                if (!result.Success)
                    throw new InvalidOperationException(
                        $"ManagerDraftGame (standard) failed for publisher {nextPublisher.PublisherID}: "
                        + string.Join("; ", result.Errors ?? []));
            }
        }
    }
}
```

- [ ] **Step 2: Build to verify compilation**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Run the tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests --filter "FullyQualifiedName~ManagerDraftTests"
```
Expected: All 6 shared tests pass.

- [ ] **Step 4: Run the full integration test suite**

```powershell
dotnet test src/FantasyCritic.IntegrationTests
```
Expected: All tests pass (existing + new).

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/LeagueManager/ManagerDraftTests.cs
git commit -m "Add ManagerDraftTests (manager-side draft using ManagerDraftGame endpoint)"
```

---

## Self-Review

**Spec coverage check:**

| Spec requirement | Task that implements it |
|---|---|
| `[ProducesResponseType]` annotations prerequisite | Task 1 |
| `LeagueScenario` configuration class | Task 2 |
| `LeagueTestHelpers.GetOpenYearAsync` | Task 2 |
| `LeagueTestHelpers.CreateLeagueAsync` | Task 2 |
| `LeagueSetupTests` — creation + settings round-trip | Task 3 |
| `LeagueTestHelpers` invite/accept/publisher/draft-order helpers | Task 4 |
| `LeagueMemberTests` — all 8 member tests | Task 4 |
| `MockedLivePlayer` encapsulating pick logic | Task 5 |
| `DraftSimulator` orchestrating the draft loop | Task 5 |
| `LeagueDraftTestBase` abstract base with shared tests | Task 6 |
| `StandardLeagueDraftTests` player-side draft | Task 6 |
| `ManagerDraftTests` manager-side draft | Task 7 |

**Placeholder scan:** None found. Every step includes actual code or an exact command with expected output.

**Type consistency check:**
- `LeagueScenario.BuildSettings` returns `LeagueYearSettingsViewModel` from `FantasyCritic.ApiClient` ✓
- `DraftSimulator` uses `LeagueYearViewModel.PlayStatus.DraftFinished` and `.DraftingCounterPicks` ✓
- `MockedLivePlayer` uses `PossibleMasterGameYearViewModel.IsAvailable`, `.Taken`, `.MasterGame.MasterGameID` ✓
- `ManagerDraftTests` uses `PublisherViewModel.NextToDraft` ✓
- `LeagueMemberTests` disposes sessions in `[OneTimeTearDown]` ✓
- `SetDraftOrderAsync` uses `DraftOrderType = "Manual"` and `ManualPublisherDraftPositions` ✓
