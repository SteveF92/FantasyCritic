# Eligibility-Change Integration Test Design

**Date:** 2026-06-10  
**Status:** Approved, ready for implementation

## Summary

Add a new integration test fixture `EligibilityChangeTests` that verifies bids and drops which were valid when placed are correctly rejected at processing time when the target game gains a critic score in the intervening window. A prerequisite server-side change (`CriticScore` field on `EditMasterGameRequest`) and a typed client for `GetLeagueActionSets` are also required.

---

## Prerequisites

### 1. `EditMasterGameRequest` — inject a critic score (already committed)

`src/FantasyCritic.Web/Models/Requests/Admin/EditMasterGameRequest.cs`

Added `public decimal? CriticScore { get; init; }` alongside the existing `ClearCriticScore` bool.

`ToDomain` logic:
```csharp
var criticScore = ClearCriticScore ? null : (CriticScore ?? existingMasterGame.RawCriticScore);
```
- `ClearCriticScore = true` → clears (unchanged behavior)
- `CriticScore = 85.0m` → injects that score
- Neither set → preserves existing score (unchanged behavior)

The NSwag-generated client picks this up automatically on next regeneration.

### 2. `GetLeagueActionSets` — add typed `ProducesResponseType`

`src/FantasyCritic.Web/Controllers/API/LeagueController.cs`, action `GetLeagueActionSets`

Add:
```csharp
[ProducesResponseType<List<LeagueActionProcessingSetViewModel>>(StatusCodes.Status200OK)]
```

This allows NSwag to generate `Task<ICollection<LeagueActionProcessingSetViewModel>>` so the test can use the typed response. After adding this attribute, regenerate the API client.

---

## New `LeagueScenario`

**Name:** `FourPlayerEligibilityChange`  
**File:** `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs`

| Setting | Value | Reason |
|---|---|---|
| `StandardGames` | 6 | Room for 3 drafted + 3 open bid slots |
| `GamesToDraft` | 3 | Leaves 3 open standard bid slots per player |
| `CounterPicks` | 2 | 1 drafted, 1 open for CP bid |
| `CounterPicksToDraft` | 1 | |
| `WillReleaseDroppableGames` | 1 | One drop allowance per player |
| `WillNotReleaseDroppableGames` | 0 | |
| `DropOnlyDraftGames` | `true` | Drops must be of drafted games |
| `CounterPicksBlockDrops` | `true` | Standard behavior |
| Everything else | Same as `FourPlayerBidding` | |

---

## Test Fixture: `EligibilityChangeTests`

**File:** `src/FantasyCritic.IntegrationTests/Tests/League/EligibilityChangeTests.cs`  
**Base:** `IntegrationTestBase`  
**Scenario:** `LeagueScenarios.FourPlayerEligibilityChange`  
**Players:** P1 (manager), P2, P3, P4

### Player Assignments

| Player | Action | Target gets score? | Expected result |
|---|---|---|---|
| P1 | Standard bid on **Game A** | No | Bid succeeds — P1 gets Game A, budget reduced |
| P2 | Standard bid on **Game B** | **Yes** | Bid fails — P2 doesn't get Game B, budget unchanged |
| P3 | Standalone drop of drafted **Game C** | No | Drop succeeds — Game C leaves roster, enters FormerGames |
| P4 | Standalone drop of drafted **Game D** | **Yes** | Drop fails — Game D stays on roster |
| P3 | Bid on **Game E** with conditional drop of drafted **Game F** | No (Game F stays clean) | Conditional drop + bid succeeds — P3 gets Game E, Game F leaves roster |
| P4 | Bid on **Game G** with conditional drop of drafted **Game H** | **Yes** (Game H gets score) | Conditional drop fails → bid also fails — P4 doesn't get Game G, Game H stays on roster |
| P2 | Counter-pick bid on **a game on P1's roster** (Game I) | **Yes** | CP bid fails — P2 has no new CP game, budget unchanged for CP |

### Setup Phases (`OneTimeSetUp`)

1. **Clock** → `2025-01-06 12:00 UTC` (Monday, safely before Saturday 20:00 ET window)
2. Register 4 users; create league with `FourPlayerEligibilityChange`; invite/accept; create publishers; set draft order (P1=1…P4=4); start draft; run `DraftSimulator`
3. Capture post-draft budgets for P1–P4
4. Identify target games:
   - `_gameAMasterGameID` — available, not released, not taken (P1 bids, stays clean)
   - `_gameBMasterGameID` — available, not released, not taken, ≠ A (P2 bids, gets score)
   - `_p1DropGamePublisherGameID` / `_p1DropGameMasterGameID` — P1's drafted game, droppable (P1 drops, stays clean)
   - `_p4DropGamePublisherGameID` / `_p4DropGameMasterGameID` — P4's drafted game, droppable, ≠ P4's conditional-drop game (P4 drops, gets score)
   - `_gameEMasterGameID` — available, not released, not taken, ≠ A/B (P3 conditional-drop bid target, stays clean)
   - `_p3ConditionalDropPublisherGameID` / `_p3ConditionalDropMasterGameID` — P3's drafted game, droppable (P3 conditionally drops, stays clean)
   - `_gameGMasterGameID` — available, not released, not taken, ≠ A/B/E (P4 conditional-drop bid target, stays clean)
   - `_p4ConditionalDropPublisherGameID` / `_p4ConditionalDropMasterGameID` — P4's drafted game, droppable, ≠ P4's standalone-drop game D (P4 conditionally drops; gets score → condrop fails). Note: since Game D's standalone drop fails (score added), P4's single `WillReleaseDroppableGames` allowance is not consumed by it, so the conditional-drop bid can still attempt to use it.
   - `_p2CounterPickTargetMasterGameID` — a game on P1's roster, not delay-contention, not released (P2 CP bids; gets score)
5. Place all bids and drops:
   - `PlaceBidAsync(P1, GameA, $10, false, null)`
   - `PlaceBidAsync(P2, GameB, $10, false, null)`
   - `PlaceDropAsync(P1, p1DropGame)`
   - `PlaceDropAsync(P4, p4DropGame)`
   - `PlaceBidAsync(P3, GameE, $10, false, p3ConditionalDrop)`
   - `PlaceBidAsync(P4, GameG, $10, false, p4ConditionalDrop)`
   - `PlaceBidAsync(P2, P2CPTarget, $5, true, null)`
6. **Clock** → `2025-01-10 20:00 UTC` (Friday — a few hours before the Saturday ET window)
7. **Admin calls `EditMasterGame`** with `CriticScore = 85.0m` for:
   - Game B
   - P4's drop game (Game D)
   - P4's conditional-drop game (Game H)
   - P2's CP target (Game I)
8. **Clock** → `2025-01-12 01:01 UTC` (Saturday 20:01 ET)
9. `TurnOnActionProcessingMode` → `ProcessActions` → `TurnOffActionProcessingMode`
10. Fetch `_postProcessingSnapshot` = `GetLeagueYearAsync`
11. Fetch `_actionSet` = single entry from `GetLeagueActionSetsAsync`

### Helper: `EditMasterGameToAddScoreAsync`

A private helper that fetches the full current master game state (to preserve all existing fields) and calls `FactChecker.EditMasterGameAsync` with `CriticScore = 85.0m`. Avoids having to reconstruct every field inline four times.

### Test Methods (18 total)

**Roster state:**
1. `Bid_EligibleGame_Succeeds` — P1 has Game A on roster
2. `Bid_EligibleGame_BudgetDeducted` — P1 budget reduced by $10
3. `Bid_ScoredGame_Fails_NotOnRoster` — P2 does NOT have Game B
4. `Bid_ScoredGame_Fails_BudgetUnchanged` — P2 budget unchanged
5. `Drop_EligibleGame_Succeeds_NotOnRoster` — P3 doesn't have drop game
6. `Drop_EligibleGame_Succeeds_InFormerGames` — P3 drop game in FormerGames
7. `Drop_ScoredGame_Fails_StaysOnRoster` — P4 still has drop game
8. `ConditionalDrop_EligibleGame_Succeeds_BidGameOnRoster` — P3 has Game E
9. `ConditionalDrop_EligibleGame_Succeeds_DroppedGameGone` — P3 conditional-drop game no longer on roster
10. `ConditionalDrop_ScoredGame_Fails_BidGameNotOnRoster` — P4 does NOT have Game G
11. `ConditionalDrop_ScoredGame_Fails_GameStaysOnRoster` — P4 still has conditional-drop game (Game H)
12. `CounterPickBid_ScoredGame_Fails_NotOnRoster` — P2 has no CP game matching P2's CP target

**History (action set):**
13. `History_Bid_EligibleGame_Successful` — action set bid for Game A has `Successful == true`
14. `History_Bid_ScoredGame_Unsuccessful_WithScoreOutcome` — action set bid for Game B has `Successful == false` and `Outcome` contains `"score"`
15. `History_Drop_EligibleGame_Successful` — action set drop for P3's game has `Successful == true`
16. `History_Drop_ScoredGame_Unsuccessful` — action set drop for P4's game has `Successful == false`
17. `History_ConditionalDropBid_ScoredGame_Unsuccessful_WithScoreOutcome` — action set bid for Game G has `Successful == false` and `Outcome` contains `"score"`
18. `History_CounterPickBid_ScoredGame_Unsuccessful_WithScoreOutcome` — action set CP bid for Game I has `Successful == false` and `Outcome` contains `"score"`

---

## `EditMasterGameToAddScoreAsync` helper — design sketch

```csharp
private async Task EditMasterGameToAddScoreAsync(Guid masterGameID)
{
    var game = await _adminSession.Game.MasterGameAsync(masterGameID);
    await _adminSession.FactChecker.EditMasterGameAsync(new EditMasterGameRequest
    {
        MasterGameID   = masterGameID,
        GameName       = game.GameName,
        EstimatedReleaseDate = game.EstimatedReleaseDate,
        MinimumReleaseDate   = game.MinimumReleaseDate.ToDateTimeOffset(),
        Tags           = game.Tags.ToList(),
        CriticScore    = 85.0m,
        // all other bool/nullable fields left at defaults (false/null)
    });
}
```

---

## Files Changed / Created

| File | Change |
|---|---|
| `src/FantasyCritic.Web/Models/Requests/Admin/EditMasterGameRequest.cs` | Add `CriticScore?` field + update `ToDomain` (**already committed**) |
| `src/FantasyCritic.Web/Controllers/API/LeagueController.cs` | Add `[ProducesResponseType<List<LeagueActionProcessingSetViewModel>>(StatusCodes.Status200OK)]` to `GetLeagueActionSets` |
| `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` | Regenerated (gitignored, not committed) |
| `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` | Add `FourPlayerEligibilityChange` static scenario |
| `src/FantasyCritic.IntegrationTests/Tests/League/EligibilityChangeTests.cs` | New test fixture (18 test methods) |

---

## Eligibility Rule Being Tested

From `GameEligibilityFunctions.GetGenericSlotMasterGameErrors`:

- **Bid:** `hasScore` → `"That game already has a score."` (blocks acquisition)
- **Drop:** `hasScore` (and game is not "willNotRelease + MustRelease") → `"That game already has a score."` (blocks drop)

Both bid and drop eligibility are re-checked at processing time, so a score added after placement but before processing correctly rejects the action.
