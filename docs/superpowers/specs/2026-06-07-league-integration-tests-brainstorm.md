# League / League Manager Integration Tests — Brainstorm Notes

**Date:** 2026-06-07  
**Status:** In progress — decisions locked below, open questions noted

This document captures the design discussion for the largest integration test block: league
creation, member management, the draft, and post-draft operations. It is a working notes file,
not yet the final spec. See `2026-06-07-league-integration-tests-design.md` (not yet written)
for the approved spec once decisions are finalized.

---

## Decisions already locked

### Test categories (in scope for initial spec)

1. **League setup** — create league, verify settings, edit settings.
   Requires only the league manager.

2. **Members & publishers** — invite players, accept invites, create publishers,
   remove players, rescind invites, invite links.

3. **Draft** — set draft order, start draft, simulate all picks (standard + counter-picks),
   verify final publisher rosters.

4. **Post-draft** — pickup bids, drops.  
   *(Deferred for a future pass; noted here for context.)*

---

### Game selection for the draft

**Decision: Dynamic selection** — call `TopAvailableGames` / `PossibleMasterGames` to find
unreleased, eligible games at the time the test runs. Same pattern as the existing Royale tests.

- Never hardcode game names or IDs.
- Games are picked from the top of the availability list (highest hype via `ProductionGameStatsCache`
  where the cache is warm, or just `FirstOrDefault` otherwise).
- Counter-picks use `PossibleCounterPicks(publisherID)`.

---

### Draft endpoint coverage

**Decision: Both endpoints** — the primary lifecycle test has each player drive their own picks
via the player-facing `DraftGame` endpoint (`LeagueController`). A separate test exercises the
manager-override `ManagerDraftGame` endpoint (`LeagueManagerController`).

---

### Fixture structure

**Decision: Layered fixtures (Option C from discussion)**

| Fixture | Setup style | What it tests |
|---|---|---|
| `LeagueSetupTests` | Self-contained (each test creates its own league) | League creation, settings editing, league options |
| `LeagueMemberTests` | Self-contained | Invites, publisher creation, player management |
| `DraftTests` | `[OneTimeSetUp]` builds a complete 4-player drafted league | Post-draft assertions: rosters, game counts, CP associations |
| `ManagerDraftTests` | `[OneTimeSetUp]` | `ManagerDraftGame` override, draft order, StartDraft edge cases |

Within the `[OneTimeSetUp]` fixtures, test method execution order is pinned with NUnit `[Order(n)]`
where individual test methods need a guaranteed sequence.

---

### League settings for the standard test league

Dynamically obtain the open year via `session.League.LeagueOptionsAsync()` — pick the first year
where `OpenForCreation == true`.

| Setting | Value |
|---|---|
| StandardGames | 6 |
| GamesToDraft | 6 |
| CounterPicks | 1 |
| CounterPicksToDraft | 1 |
| DraftSystem | `"Flexible"` |
| PickupSystem | `"SemiPublicBiddingSecretCounterPicks"` |
| ScoringSystem | `"LinearPositive"` |
| TradingSystem | `"Standard"` |
| TiebreakSystem | `"LowestProjectedPoints"` |
| ReleaseSystem | `"MustBeReleased"` |
| IneligibleGameSystem | `"CaseByCase"` |
| CounterPickDeadline | Dec 1 of the open year |
| Tags | none banned / none required |
| SpecialGameSlots | empty |
| TestLeague | `true` |
| PublicLeague | `false` |

Drop settings for the standard test league (simple, non-unlimited):

| Setting | Value |
|---|---|
| UnrestrictedReleaseStatusDroppableGames | 1 |
| WillNotReleaseDroppableGames | 1 |
| WillReleaseDroppableGames | 1 |
| DropOnlyDraftGames | true |
| GrantSuperDrops | false |
| CounterPicksBlockDrops | true |
| AllowMoveIntoIneligible | false |
| MinimumBidAmount | 1 |

---

### Draft player count and slots

- **4 players** (manager + 3 invited users), each with their own `ApiSession`.
- **6 standard picks + 1 counter-pick per publisher** = 28 total picks.
- Counter-picks are in scope for the initial draft tests.

---

## Open question: test harness / scenario reuse

This is the biggest unresolved design question. The goal is to define league configurations
(player count, slots, special slots, etc.) and have the test framework automatically run all
the standard assertions for each config, with minimal duplicated code.

Three options are being evaluated:

---

### Option A — Static helper methods + one fixture class per scenario

Write a static `LeagueTestHelpers` class with reusable methods:

```csharp
static Task<Guid> CreateLeagueAsync(ApiSession managerSession, int year, LeagueSettings settings);
static Task InviteAndAcceptAsync(ApiSession managerSession, IReadOnlyList<ApiSession> playerSessions, Guid leagueID);
static Task CreatePublishersAsync(IReadOnlyList<ApiSession> sessions, Guid leagueID, int year);
static Task RunDraftAsync(ApiSession managerSession, IReadOnlyList<ApiSession> playerSessions, Guid leagueID, int year);
```

Each test scenario is its own `[TestFixture]` class that calls these helpers in its
`[OneTimeSetUp]` and then defines its own `[Test]` methods.

**Pros:**
- Simple, no NUnit magic
- Each fixture is self-documenting
- IDE navigation is straightforward

**Cons:**
- Adding a new scenario requires a new fixture class file
- Shared test method bodies must be manually copied or extracted to a base class
- At 10+ scenarios this becomes repetitive

**Best fit for:** 2–4 scenarios that differ meaningfully in their test assertions.

---

### Option B — `[TestFixtureSource]` with a `LeagueScenario` class

NUnit can parameterize entire fixture classes. A single fixture class is instantiated
once per entry in a source collection:

```csharp
public class LeagueScenario
{
    public string Name { get; init; }
    public int PlayerCount { get; init; }
    public int StandardGames { get; init; }
    public int GamesToDraft { get; init; }
    public int CounterPicks { get; init; }
    public int CounterPicksToDraft { get; init; }
    public bool HasSpecialSlots { get; init; }
    public IReadOnlyList<SpecialGameSlotViewModel> SpecialGameSlots { get; init; } = [];
    // … other settings …

    // Expected outcome values, used in assertions
    public int ExpectedGamesPerPublisher => StandardGames;
    public int ExpectedCounterPicksPerPublisher => CounterPicks;

    public override string ToString() => Name; // drives test output label
    public LeagueYearSettingsViewModel BuildSettings(Guid leagueID, int year) { … }
}

public static class LeagueScenarios
{
    public static readonly LeagueScenario[] All =
    [
        new LeagueScenario
        {
            Name = "Standard",
            PlayerCount = 4,
            StandardGames = 6,
            GamesToDraft = 6,
            CounterPicks = 1,
            CounterPicksToDraft = 1,
        },
        new LeagueScenario
        {
            Name = "SpecialSlots",
            PlayerCount = 4,
            StandardGames = 6,
            GamesToDraft = 6,
            CounterPicks = 1,
            CounterPicksToDraft = 1,
            HasSpecialSlots = true,
            SpecialGameSlots = [ /* ... */ ],
        },
        new LeagueScenario
        {
            Name = "NoCounterPicks",
            PlayerCount = 4,
            StandardGames = 8,
            GamesToDraft = 8,
            CounterPicks = 0,
            CounterPicksToDraft = 0,
        },
    ];
}

[TestFixtureSource(typeof(LeagueScenarios), nameof(LeagueScenarios.All))]
public class LeagueDraftTests : IntegrationTestBase
{
    private readonly LeagueScenario _scenario;

    // state populated in OneTimeSetUp
    private Guid _leagueID;
    private int _year;
    // …

    public LeagueDraftTests(LeagueScenario scenario) => _scenario = scenario;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        // use _scenario.PlayerCount, _scenario.BuildSettings(), etc.
        // run the full draft
    }

    [Test]
    public void AllPublishers_HaveCorrectNumberOfStandardGames()
    {
        // _scenario.ExpectedGamesPerPublisher
    }

    [Test]
    public void SpecialSlots_ArePopulated()
    {
        Assume.That(_scenario.HasSpecialSlots, "Only runs for special-slot leagues");
        // …
    }

    [Test]
    public void CounterPicks_AreFromOtherPublishers()
    {
        Assume.That(_scenario.CounterPicks > 0, "Only runs when counter-picks are enabled");
        // …
    }
}
```

Test output in the runner looks like:

```
LeagueDraftTests(Standard) ✓
  AllPublishers_HaveCorrectNumberOfStandardGames ✓
  CounterPicks_AreFromOtherPublishers ✓
  SpecialSlots_ArePopulated — Inconclusive (skipped: Only runs for special-slot leagues)

LeagueDraftTests(SpecialSlots) ✓
  AllPublishers_HaveCorrectNumberOfStandardGames ✓
  CounterPicks_AreFromOtherPublishers ✓
  SpecialSlots_ArePopulated ✓
```

**Pros:**
- Adding a new scenario = one entry in `LeagueScenarios.All`, zero new test code
- Every scenario automatically gets all shared tests
- `Assume.That` guards scenario-specific assertions cleanly
- NUnit names each fixture run with `ToString()` so output is readable

**Cons:**
- Requires understanding of `[TestFixtureSource]` — more NUnit "magic"
- `[OneTimeSetUp]` must handle all scenario variants (can branch on `_scenario.HasSpecialSlots` etc.)
- Scenario-specific tests that only make sense for one config add noise to other fixture runs
  (they show as "Inconclusive" rather than simply not existing)

**Best fit for:** Many scenarios that share the vast majority of assertions.

---

### Option C — Abstract base fixture + concrete subclasses (template method)

```csharp
public abstract class LeagueDraftTestBase : IntegrationTestBase
{
    protected abstract LeagueScenario Scenario { get; }

    // shared state
    protected Guid LeagueID;
    protected int Year;
    // …

    [OneTimeSetUp]
    public async Task SetUp() { /* use Scenario */ }

    // shared tests that run for every subclass
    [Test]
    public void AllPublishers_HaveCorrectNumberOfStandardGames() { … }

    [Test]
    public void CounterPicks_AreFromOtherPublishers()
    {
        Assume.That(Scenario.CounterPicks > 0);
        // …
    }
}

[TestFixture]
public class StandardLeagueDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;
}

[TestFixture]
public class SpecialSlotsLeagueDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.SpecialSlots;

    // Extra tests that ONLY make sense for this scenario — no Assume.That noise
    [Test]
    public void SpecialSlots_ArePopulated() { … }

    [Test]
    public void SpecialSlots_DoNotShareGamesWithStandardSlots() { … }
}
```

**Pros:**
- Scenario-specific tests live in the right class — no `Assume.That` noise for irrelevant tests
- Familiar OOP pattern, very IDE-friendly (jump to definition, find usages, etc.)
- Still shares all common test methods via the base class

**Cons:**
- Each new scenario requires a new concrete class file (though just a few lines)
- Subclass proliferation if scenarios multiply

**Best fit for:** A moderate number of scenarios where some have meaningfully different
assertion sets that warrant their own test class.

---

### Hybrid recommendation (from discussion)

Use **Option B** (`[TestFixtureSource]`) for the core lifecycle tests that apply uniformly
across all configurations (publisher game counts, CP associations, draft order verification).

Use **Option C** (concrete subclasses) for scenarios with meaningfully different assertion
sets — e.g., `SpecialSlotsLeagueDraftTests` inherits the common base but adds its own
special-slot assertions without polluting the shared fixture with `Assume.That` guards.

This is not mutually exclusive: a `SpecialSlotsLeagueDraftTests` concrete class can be its
own `[TestFixture]` while the shared `LeagueDraftTests` runs all common assertions across
`LeagueScenarios.All`.

---

## Next steps (once direction is chosen)

1. Finalize harness option (A, B, C, or hybrid)
2. Write the full spec (`2026-06-07-league-integration-tests-design.md`)
3. Invoke `writing-plans` skill to produce the implementation plan
