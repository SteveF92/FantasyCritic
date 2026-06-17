# League Integration Test Infrastructure — Design

**Date:** 2026-06-14

## Goal

Refactor the league integration test suite for consistency and lower boilerplate when
adding new fixtures. The work covers:

1. Reorganize `Tests/League/` by lifecycle phase (setup, draft, post-draft actions).
2. Introduce `LeagueFixture` + `LeagueFixtureBuilder` as the single setup path for
   multi-step league scenarios.
3. Slim down `LeagueDraftTestBase` to compose with the fixture instead of duplicating setup.
4. Migrate all existing league scenario fixtures to the new patterns.

**Non-goals:**

- Admin/action-processing wrapper helpers — the typed `ApiSession.Admin` and
  `ActionRunner` clients are sufficient; tests call them directly.
- Changing test behavior or assertions — refactor only; all tests must pass unchanged
  in meaning.

---

## Decisions (from brainstorming)

| Question | Choice |
|----------|--------|
| Primary driver | Refactor existing tests **and** improve add-new-test ergonomics |
| Folder layout | Lifecycle phase: `Setup/`, `Draft/`, `Draft/EdgeCases/`, `Actions/` |
| State encapsulation | Context object (`LeagueFixture`) |
| Fixture scope | League identity + draft lifecycle methods only |
| Domain naming | Avoid `PublisherSlot` (real domain term); use `TestPublisher` |

---

## Folder layout

```
src/FantasyCritic.IntegrationTests/
  Helpers/
    LeagueFixture.cs              # NEW — fixture, builder, TestPublisher
    LeagueScenario.cs             # unchanged
    LeagueTestHelpers.cs          # slimmed; atomic primitives only
    MockedLivePlayer.cs           # unchanged (DraftSimulator stays here)
  Tests/League/
    Setup/
      LeagueSetupTests.cs
      LeagueMemberTests.cs
    Draft/
      LeagueDraftTestBase.cs
      Scenarios/
        StandardLeagueDraftTests.cs
        AutoDraftLeagueDraftTests.cs
      EdgeCases/
        DraftEdgeCaseTests.cs
        DraftCounterPickPhaseTests.cs
        DraftPauseUndoTests.cs
    Actions/
      BidProcessingTests.cs
      DropProcessingTests.cs
      EligibilityChangeTests.cs
```

Namespaces mirror folders (`Tests.League.Draft.Scenarios`, `Tests.League.Actions`, etc.).

---

## Core types

### `TestPublisher`

A test-harness binding between an authenticated session and a publisher in draft order.
**Not** the domain `PublisherSlot` type (roster game slot).

```csharp
public sealed record TestPublisher(
    int DraftPosition,       // 1-based, matches server draft position
    ApiSession Session,
    Guid PublisherID,
    string PublisherName);
```

**Publisher access:** Use `LeagueFixture.Manager` for the manager session. Use
`LeagueFixture.Publishers` indexed by draft order for player publishers — do not assume
a fixed list index is the manager. This replaces paired fields like `_p2Session` +
`_p2PublisherID`.

### `LeagueFixture`

Immutable league state created by the builder. Owns all `ApiSession` instances it
created; implements `IAsyncDisposable`.

```csharp
public sealed class LeagueFixture : IAsyncDisposable
{
    public LeagueScenario Scenario { get; }
    public Guid LeagueID { get; }
    public int Year { get; }
    public ApiSession Manager { get; }
    public IReadOnlyList<TestPublisher> Publishers { get; }
    public IReadOnlyDictionary<Guid, ApiSession> PublisherSessionMap { get; }

    public Task<LeagueYearViewModel> GetLeagueYearAsync();

    public Task DraftToCompletionAsync(IEnumerable<MockedLivePlayer>? players = null);
    public Task DraftUntilCounterPickPhaseAsync(IEnumerable<MockedLivePlayer>? players = null);
    public Task DraftStandardPicksAsync(int count, IEnumerable<MockedLivePlayer>? players = null);

    public ValueTask DisposeAsync();
}
```

When `players` is omitted, draft methods construct a `MockedLivePlayer` for every
publisher in `Publishers`.

### `LeagueFixtureBuilder`

```csharp
public static class LeagueFixtureBuilder
{
    /// <summary>
    /// Registers manager + (Scenario.PlayerCount - 1) players, creates the league,
    /// invites, creates publishers, sets manual draft order, starts the draft.
    /// Does not run any picks.
    /// </summary>
    public static Task<LeagueFixture> CreateAndStartDraftAsync(
        FantasyCriticWebApplicationFactory factory,
        LeagueScenario scenario,
        Func<(string email, string password, string displayName)> newUser);

    /// <summary>
    /// Same as above but stops before StartDraft — for LeagueMemberTests and similar.
    /// </summary>
    public static Task<LeagueFixture> CreateLeagueWithMembersAsync(
        FantasyCriticWebApplicationFactory factory,
        LeagueScenario scenario,
        Func<(string email, string password, string displayName)> newUser);
}
```

**Removed:** `LeagueTestHelpers.SetUpLeagueAndStartDraftAsync` — absorbed by the builder.
Atomic helpers (`CreateLeagueAsync`, `InviteAndAcceptAsync`, `CreatePublisherAsync`,
`SetDraftOrderAsync`, `GetOpenYearAsync`) remain and are called internally by the builder.

---

## `LeagueDraftTestBase` (after refactor)

Thin shell composing fixture + shared completion assertions (~40 lines of setup logic
replaced by builder call):

```csharp
public abstract class LeagueDraftTestBase : IntegrationTestBase
{
    protected abstract LeagueScenario Scenario { get; }
    protected LeagueFixture League = null!;
    protected LeagueYearViewModel LeagueYearSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUpLeagueAndDraft()
    {
        League = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, Scenario, NewUser);
        await SimulateDraftAsync();
        LeagueYearSnapshot = await League.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown() => await League.DisposeAsync();

    protected virtual Task SimulateDraftAsync() => League.DraftToCompletionAsync();

    // existing shared [Test] methods unchanged — reference League.Scenario and LeagueYearSnapshot
}
```

`AutoDraftLeagueDraftTests` overrides `SimulateDraftAsync` to enable auto-draft on the
last publisher and pass a filtered player set to `DraftToCompletionAsync`.

---

## Per-fixture migration

| Fixture | Migration |
|---------|-----------|
| `StandardLeagueDraftTests` | No body change; inherits refactored base |
| `AutoDraftLeagueDraftTests` | Override `SimulateDraftAsync`; use `League.Publishers.Last()` |
| `DraftEdgeCaseTests` | Builder → `DraftStandardPicksAsync(1)`; use `League.Publishers[0/1]` |
| `DraftCounterPickPhaseTests` | Builder → `DraftUntilCounterPickPhaseAsync()` |
| `DraftPauseUndoTests` | Builder → one pick → pause/undo/resume |
| `BidProcessingTests` | Builder → draft → local targets record; admin session separate |
| `DropProcessingTests` | Same as Bid |
| `EligibilityChangeTests` | Same; collapse ~15 publisher fields to `League.Publishers[n]` |
| `LeagueMemberTests` | `CreateLeagueWithMembersAsync` (no draft start) |
| `LeagueSetupTests` | Unchanged |

### Test-specific targets pattern (action fixtures)

Scenario-specific game/drop IDs stay on the fixture class in a private record:

```csharp
private sealed record EligibilityTargets(
    Guid GameA, Guid GameB, Guid GameE, Guid GameG, ...);

private EligibilityTargets _targets = null!;
```

Private helper methods (`PlaceBidAsync`, `EditMasterGameToAddScoreAsync`, etc.) remain
on the test class or move to small private static helpers within the same file if
duplicated across action fixtures — but **not** into a shared admin wrapper layer.

Admin sessions (`ApiSession` + `LoginAsLocalAdminAsync`) are created and disposed by
each action fixture that needs clock control, same as today.

---

## Documentation updates

After implementation, update:

- `.cursor/rules/integration-tests.mdc` — folder layout, `LeagueFixture` usage pattern
- `.cursor/skills/add-integration-test/SKILL.md` — reference builder for league scenarios

---

## Verification

1. All integration tests pass:
   ```powershell
   dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
   ```
2. No references remain to `SetUpLeagueAndStartDraftAsync`.
3. No test file under `Tests/League/` (root) except the new subfolders.
4. Grep confirms no type named `PublisherSlot` in `FantasyCritic.IntegrationTests`.

---

## Implementation order

1. Add `LeagueFixture.cs` (types + builder + draft methods) using existing helpers.
2. Refactor `LeagueDraftTestBase` to use builder; verify `StandardLeagueDraftTests` passes.
3. Migrate `Draft/EdgeCases/` fixtures.
4. Migrate `Actions/` fixtures.
5. Migrate `Setup/LeagueMemberTests` via `CreateLeagueWithMembersAsync`.
6. Move files to new folders; fix namespaces.
7. Remove `SetUpLeagueAndStartDraftAsync`; update docs.

Each step should leave the test suite green.
