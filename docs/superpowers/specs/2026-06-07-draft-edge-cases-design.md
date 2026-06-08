# Draft Edge Cases + Auto-Draft Integration Tests — Design

**Date:** 2026-06-07  
**Builds on:** `2026-06-07-league-integration-tests-design.md`

## Goal

Add integration test coverage for the draft scenarios explicitly left out of scope in the
league integration tests spec:

- **Error rejections** during the standard-game phase (out of turn, duplicate game, ineligible
  game, counter-pick flag in the wrong phase)
- **Error rejections** during the counter-pick phase (standard game in the wrong phase,
  counter-pick of a game not on any roster)
- **Pause and undo** flow (`SetDraftPause` + `UndoLastDraftAction`)
- **Auto-draft** — one publisher has `AutoDraftMode.All` enabled; the server handles their picks
  transparently

---

## New scenario: `TwoPlayerSmall`

Added to `LeagueScenarios` in `Helpers/LeagueScenario.cs`.

```csharp
public static readonly LeagueScenario TwoPlayerSmall = new()
{
    Name        = "TwoPlayerSmall",
    PlayerCount = 2,
    StandardGames       = 2,
    GamesToDraft        = 2,
    CounterPicks        = 1,
    CounterPicksToDraft = 1,
    // All other settings inherit defaults from LeagueScenario
};
```

Using 2 players with 2 standard games each keeps setup fast: 4 total standard picks to exhaust
the standard-game phase, 2 counter-picks to finish the draft. All error-case fixtures use this
scenario.

---

## New helper: `LeagueTestHelpers.SetUpLeagueAndStartDraftAsync`

Added to `Helpers/LeagueTestHelpers.cs`.

```csharp
/// <summary>
/// Creates a league, invites and accepts all players, creates publishers, sets draft order,
/// and starts the draft. Returns the state needed by error-case fixtures.
/// </summary>
/// <param name="managerSession">Session that owns the league.</param>
/// <param name="playerSessions">Non-manager sessions (one per additional player).</param>
/// <param name="scenario">Scenario whose settings drive the league.</param>
/// <returns>
///   leagueID, year, publisherIDsInDraftOrder, publisherSessionMap (publisherID → session)
/// </returns>
internal static Task<(Guid leagueID, int year,
                      IReadOnlyList<Guid> publisherIDsInDraftOrder,
                      IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap)>
    SetUpLeagueAndStartDraftAsync(
        ApiSession managerSession,
        IReadOnlyList<ApiSession> playerSessions,
        LeagueScenario scenario);
```

The method:
1. Calls `GetOpenYearAsync`
2. Calls `CreateLeagueAsync`
3. Calls `InviteAndAcceptAsync` for each player session
4. Calls `CreatePublisherAsync` for the manager first, then each player — builds `publisherSessionMap`
5. Calls `SetDraftOrderAsync` in creation order (so position 1 = manager, position 2 = first player, etc.)
6. Calls `managerSession.LeagueManager.StartDraftAsync`
7. Returns `(leagueID, year, publisherIDsInOrder, publisherSessionMap)`

**Why not inherit `LeagueDraftTestBase`?**
`LeagueDraftTestBase.SetUpLeagueAndDraft` always completes the draft. The error-case fixtures need
a partially-in-progress draft. This helper provides the common preamble without the draft loop.

---

## `AutoDraftLeagueDraftTests`

**File:** `Tests/League/Scenarios/AutoDraftLeagueDraftTests.cs`  
**Extends:** `LeagueDraftTestBase`  
**Scenario:** `LeagueScenarios.Standard` (4 players, 6 standard + 1 counter-pick)

### How auto-draft integrates with `DraftSimulator`

After each successful `DraftGame` call (player-side or manager-side), the server immediately calls
`AutoDraftForLeague`. That loop picks for any auto-draft publisher whose turn it is, then exits
when the next publisher is not on auto-draft. This means the auto-draft player **never appears as
`NextToDraft`** when the `DraftSimulator` polls — their picks happen inside the server's response
to the previous player's pick. The simulator only needs entries for the three non-auto-draft
players.

### `SimulateDraftAsync` override

```csharp
protected override async Task SimulateDraftAsync(
    IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap,
    Guid leagueID, int year)
{
    // Last publisher in draft order is the auto-draft player.
    var autoDraftPubID = PublisherIDs.Last();
    var autoDraftSession = publisherSessionMap[autoDraftPubID];

    // Enable auto-draft. If it is already this player's turn, the server auto-picks immediately.
    await autoDraftSession.League.SetAutoDraftAsync(new SetAutoDraftRequest
    {
        PublisherID          = autoDraftPubID,
        Mode                 = "All",
        OnlyDraftFromWatchlist = false,
    });

    // Register only the non-auto-draft players. If auto-draft fails and the auto-draft publisher
    // ends up as NextToDraft, DraftSimulator throws — that is the intended test failure signal.
    var nonAutoDraftPlayers = publisherSessionMap
        .Where(kvp => kvp.Key != autoDraftPubID)
        .Select(kvp => new MockedLivePlayer(kvp.Value, kvp.Key, leagueID));
    var simulator = new DraftSimulator(ManagerSession, nonAutoDraftPlayers);
    await simulator.RunAsync(leagueID, year);
}
```

### Additional test (beyond the five inherited from `LeagueDraftTestBase`)

```csharp
[Test]
public void AutoDraftPlayer_HasAutoDraftModeSetToAll()
{
    var autoDraftPublisher = LeagueYearSnapshot.Publishers
        .Single(p => p.PublisherID == PublisherIDs.Last());
    Assert.That(autoDraftPublisher.AutoDraftMode, Is.EqualTo("All"),
        "Auto-draft publisher's AutoDraftMode should be 'All' in the post-draft snapshot.");
}
```

**Why only one additional test?**  
The six shared tests in `LeagueDraftTestBase` already verify that every publisher — including
the auto-draft player — ends up with the correct game counts and no duplicates. The additional
test here is specific to auto-draft: it proves the setting persisted and the name round-trips
through the API.

---

## `DraftEdgeCaseTests`

**File:** `Tests/League/DraftEdgeCaseTests.cs`  
**Extends:** `IntegrationTestBase`  
**Scenario:** `TwoPlayerSmall`

### `[OneTimeSetUp]`

1. Creates manager session + 1 player session (using `NewUser()`).
2. Calls `LeagueTestHelpers.SetUpLeagueAndStartDraftAsync` → `(leagueID, year, publisherIDs, publisherSessionMap)`.
3. Identifies `P1` (draft position 1 = manager publisher) and `P2` (draft position 2 = player publisher).
4. Makes **exactly one standard pick** on behalf of P1 using a `MockedLivePlayer` call directly:
   ```csharp
   var p1Player = new MockedLivePlayer(P1Session, P1PublisherID, LeagueID);
   await p1Player.DraftStandardGameAsync(Year);
   ```
5. Records `P1DraftedGameID` by reading `GetLeagueYearAsync` after the pick and extracting
   P1's single standard game's `MasterGame.MasterGameID`. This avoids having to predict which game
   `MockedLivePlayer` will pick.
6. Records `IneligibleGame` = first game from `TopAvailableGamesAsync` (called for P2's publisher)
   where `IsEligible == false && !Taken`. If none exists, stores `null`.

**State after setup:**
- Draft active, standard-game phase
- P1 has 1 game; P2 has 0 games
- P2 is next to draft

### Tests

#### `DraftGame_OutOfTurn_Throws`
P1's turn is done; it is now P2's turn. P1 calls `DraftGame` with their own publisher ID.

```
Expected: ApiException thrown (server returns 400 "It is not your turn to draft.")
```

#### `DraftGame_CounterPickFlagDuringStandardPhase_Throws`
P2 calls `DraftGame` with `CounterPick = true` during the standard-game phase.

```
Expected: ApiException thrown (server returns 400 "Not drafting counterPicks now.")
```

#### `DraftGame_DuplicateGame_ReturnsFailureResult`
P2 calls `DraftGame` with `MasterGameID = P1DraftedGameID` (game already on P1's roster).

```
Expected: result.Success == false (200 response, errors non-empty)
```

#### `DraftGame_IneligibleGame_ReturnsFailureResult`
P2 calls `DraftGame` with the `MasterGameID` of `IneligibleGame` and `AllowIneligibleSlot = false`.

```
Precondition (Assume): IneligibleGame != null
Expected: result.Success == false (200 response, errors non-empty)
```

The `Assume.That(IneligibleGame, Is.Not.Null, "...")` guard causes NUnit to mark the test as
inconclusive (skipped) if no ineligible games appear in `TopAvailableGames` for this slot. In
practice this should not happen; the test seed data includes many unreleased games which are
ineligible under `ReleaseSystem = "MustBeReleased"`.

---

## `DraftCounterPickPhaseTests`

**File:** `Tests/League/DraftCounterPickPhaseTests.cs`  
**Extends:** `IntegrationTestBase`  
**Scenario:** `TwoPlayerSmall`

### `[OneTimeSetUp]`

1. Creates manager session + 1 player session.
2. Calls `SetUpLeagueAndStartDraftAsync`.
3. Runs `DraftSimulator` with both players until `DraftingCounterPicks == true`:

   ```csharp
   var players = publisherSessionMap.Select(kvp => new MockedLivePlayer(kvp.Value, kvp.Key, leagueID));
   var simulator = new DraftSimulator(ManagerSession, players);
   await simulator.RunUntilCounterPickPhaseAsync(leagueID, year);
   ```

   `RunUntilCounterPickPhaseAsync` is a new variant of `RunAsync` that exits the loop as soon as
   `PlayStatus.DraftingCounterPicks == true` rather than waiting for `DraftFinished`. The standard
   `RunAsync` loop structure is the same; only the exit condition changes.

4. Records `AvailableUndraftedGame` = first game from `TopAvailableGamesAsync` (for P1's publisher)
   where `!Taken` — a game that is not on any publisher's standard roster.

**State after setup:**
- Draft active, counter-pick phase
- Both publishers have 2 standard games; 0 counter-picks
- P1 is next to counter-pick

### Tests

#### `DraftGame_StandardGameDuringCounterPickPhase_Throws`
P1 calls `DraftGame` with `CounterPick = false` during the counter-pick phase.

```
Expected: ApiException thrown (server returns 400 "Not drafting standard games now.")
```

#### `DraftGame_CounterPickOfUnownedGame_ReturnsFailureResult`
P1 calls `DraftGame` with `CounterPick = true` and `MasterGameID = AvailableUndraftedGame.MasterGameID`.
The game is not on any publisher's standard roster, making it an invalid counter-pick target.

```
Expected: result.Success == false (200 response, errors non-empty)
```

### `DraftSimulator.RunUntilCounterPickPhaseAsync`

New method added to `DraftSimulator` in `Helpers/MockedLivePlayer.cs`:

```csharp
/// <summary>
/// Runs the draft until the counter-pick phase begins, then returns.
/// Useful for tests that need state at the start of the counter-pick phase.
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
                "Draft is active and not in counter-pick phase, but no publisher has NextToDraft = true.");

        if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
            throw new InvalidOperationException(
                $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID}.");

        await player.DraftStandardGameAsync(year);
    }
}
```

---

## `DraftPauseUndoTests`

**File:** `Tests/League/DraftPauseUndoTests.cs`  
**Extends:** `IntegrationTestBase`  
**Scenario:** `TwoPlayerSmall`

### `[OneTimeSetUp]`

1. Creates manager session + 1 player session.
2. Calls `SetUpLeagueAndStartDraftAsync`.
3. P1 makes 1 standard pick (via `MockedLivePlayer`).
4. **Pause:** Manager calls `SetDraftPauseAsync(pause: true)`.  
   Captures `PausedSnapshot` via `GetLeagueYearAsync`.
5. **Undo:** Manager calls `UndoLastDraftActionAsync`.  
   Captures `AfterUndoSnapshot` via `GetLeagueYearAsync` (draft is still paused).
6. **Resume:** Manager calls `SetDraftPauseAsync(pause: false)`.  
   Captures `ResumedSnapshot` via `GetLeagueYearAsync`.

### Snapshots and tests

#### `PausedSnapshot` tests

| Test | Assertion |
|---|---|
| `Pause_DraftIsNotActive` | `PausedSnapshot.PlayStatus.DraftIsActive == false` |
| `Pause_NoPublisherIsNextToDraft` | No publisher in `PausedSnapshot.Publishers` has `NextToDraft == true` |

The second assertion reflects the intended behavior: `NextToDraft` is derived from
`DraftFunctions.GetDraftStatus`, which returns `null` when the draft is paused (no active turn).
If implementation reveals a different behavior, the assertion should be updated to match the
actual intended contract rather than assumed behavior.

#### `AfterUndoSnapshot` tests (still paused)

| Test | Assertion |
|---|---|
| `Undo_P1HasNoGames` | P1's publisher in `AfterUndoSnapshot` has 0 games |
| `Undo_NoPublisherIsNextToDraft` | No publisher has `NextToDraft == true` (still paused) |

#### `ResumedSnapshot` tests

| Test | Assertion |
|---|---|
| `Resume_DraftIsActive` | `ResumedSnapshot.PlayStatus.DraftIsActive == true` |
| `Resume_P1IsNextToDraft` | P1's publisher has `NextToDraft == true` (undo put them back to draft position 1, with 0 games) |

**Note:** `UndoLastDraftAction` requires `RequiredYearStatus.DraftPaused` — calling it on an
active draft returns 400. The sequence (pause → undo → resume) in `[OneTimeSetUp]` is mandatory
and matches the only valid call order.

---

## `MockedLivePlayer` — error message improvement

The existing throw in `DraftSimulator.RunAsync` when a publisher has no registered player:

```csharp
// Before
throw new InvalidOperationException(
    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
    + $"({nextPublisher.PublisherName}).");

// After
throw new InvalidOperationException(
    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
    + $"({nextPublisher.PublisherName}). "
    + "If this publisher is on auto-draft, their auto-draft may have failed "
    + "(e.g. no available games for the slot). Check TopAvailableGames for this publisher.");
```

---

## File map

| Action | File |
|---|---|
| Modify | `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` |
| Modify | `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs` |
| Modify | `src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/DraftEdgeCaseTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/DraftCounterPickPhaseTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/DraftPauseUndoTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/League/Scenarios/AutoDraftLeagueDraftTests.cs` |

---

## Test data strategy (unchanged from parent spec)

- All user accounts via `NewUser()` — never hardcoded credentials.
- Open year discovered via `LeagueOptionsAsync()` — never hardcoded.
- Game picks chosen dynamically via `TopAvailableGamesAsync` / `PossibleCounterPicksAsync`.
- No direct DB access; all state built and read through the API.
- GUID-based unique names prevent collisions across parallel or repeated runs.

---

## Out of scope

- **Ineligible slot override** (`AllowIneligibleSlot = true`) — the current tests verify that
  `AllowIneligibleSlot = false` rejects an ineligible game; the override path is not covered.
- **`ResetDraft`** — not covered in this pass.
- **Auto-draft from watchlist** (`OnlyDraftFromWatchlist = true`) — the `"All"` mode test does not
  constrain to watchlist; that variant is out of scope.
- **Manager-side auto-draft** (`LeagueManagerController.SetAutoDraft`) — only the player-side
  endpoint (`LeagueController.SetAutoDraft`) is tested here.
