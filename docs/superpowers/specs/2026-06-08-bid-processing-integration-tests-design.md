# Bid Processing Integration Tests — Design Spec

**Date:** 2026-06-08
**Status:** Approved

---

## Overview

This spec covers a new integration test fixture — `BidProcessingTests` — that exercises the full post-draft pickup-bid lifecycle:

1. Draft a 4-player league to completion (3 standard games drafted per publisher, leaving 3 open bid slots and 1 open counter-pick slot per publisher).
2. Use `SetInitialTime` (adjustable clock) to place the app at a known Monday, safely outside the Thu→Sat public bidding window.
3. Place six bids covering four distinct scenarios: uncontested, contested (clear winner), tiebreaker (same amount), and counter-pick.
4. Advance the clock past the Saturday 20:00 ET action-processing threshold using `SetTime`.
5. Turn on action processing mode, call `ProcessActions`, turn it off.
6. Assert on the post-processing `GetLeagueYear` snapshot.

---

## 1. League Scenario: `FourPlayerBidding`

New entry added to the `LeagueScenarios` static class in `LeagueScenario.cs`.

| Setting | Value | Rationale |
|---|---|---|
| `PlayerCount` | 4 | Enough for all bid scenarios |
| `StandardGames` | 6 | Total per-publisher standard slots |
| `GamesToDraft` | 3 | Draft fills 3 slots; 3 remain open for bids |
| `CounterPicks` | 2 | Total per-publisher counter-pick slots |
| `CounterPicksToDraft` | 1 | Draft fills 1; 1 remains open for counter-pick bids |
| `PickupSystem` | `"SemiPublicBiddingSecretCounterPicks"` | Matches production default |
| `TiebreakSystem` | `"LowestProjectedPoints"` | Matches production default; needed for tiebreaker test |
| `DraftSystem` | `"Flexible"` | Standard test default |
| `ScoringSystem` | `"LinearPositive"` | Standard test default |
| `TradingSystem` | `"Standard"` | Standard test default |
| `ReleaseSystem` | `"MustBeReleased"` | Standard test default |
| `IneligibleGameSystem` | `"DroppableAsWillNotRelease"` | Standard test default |
| `UnrestrictedReleaseStatusDroppableGames` | 0 | Drops deferred to future fixture |
| `WillNotReleaseDroppableGames` | 0 | Drops deferred |
| `WillReleaseDroppableGames` | 0 | Drops deferred |
| `DropOnlyDraftGames` | `true` | Standard |
| `GrantSuperDrops` | `false` | Standard |
| `CounterPicksBlockDrops` | `true` | Standard |
| `AllowMoveIntoIneligible` | `false` | Standard |
| `MinimumBidAmount` | 0 | No minimum |

---

## 2. Test Fixture: `BidProcessingTests`

**Location:** `src/FantasyCritic.IntegrationTests/Tests/League/BidProcessingTests.cs`

Inherits `IntegrationTestBase`. Uses `[OneTimeSetUp]` to build all shared state; individual `[Test]` methods only assert — no mutations.

### Sessions

| Variable | Who |
|---|---|
| `_adminSession` | Local admin (controls clock + action processing) |
| `_managerSession` | P1 (league manager, draft position 1) |
| `_p2Session` | P2 (draft position 2) |
| `_p3Session` | P3 (draft position 3) |
| `_p4Session` | P4 (draft position 4) |

### State captured in `OneTimeSetUp`

```
_leagueID, _year
_p1PublisherID, _p2PublisherID, _p3PublisherID, _p4PublisherID
_p1StartBudget, _p2StartBudget, _p3StartBudget, _p4StartBudget  (post-draft, pre-bid)
_gameAMasterGameID   // P1 bids this (uncontested)
_gameBMasterGameID   // P2 ($20) and P3 ($10) contest; P2 wins
_gameCMasterGameID   // P3 ($15) and P4 ($15) tiebreaker
_p1CounterPickTargetID  // MasterGameID of P1's first drafted standard game; P2 counter-picks it
_postProcessingSnapshot  // LeagueYearViewModel after ProcessActions
_tiebreakerWinnerID  // whichever of P3/P4 ends up with Game C
_tiebreakerLoserID   // the other one
```

---

## 3. `OneTimeSetUp` — Five Phases

### Phase 1: Clock and draft

1. Log in as local admin via `LoginAsLocalAdminAsync`.
2. `SetInitialTime` → **Monday Jan 6, 2025 12:00:00 UTC** (Eastern noon; well outside the Thu→Sat public bidding window, which means any game can be bid on without restriction).
3. Register 4 users, create the `FourPlayerBidding` league, invite all non-manager players, create publishers, set draft order (P1=1, P2=2, P3=3, P4=4), start draft, simulate to completion using `DraftSimulator`.
4. Fetch `GetLeagueYear` snapshot → record publisher IDs in draft order, record each publisher's starting budget.

> **Clock note:** The public bidding window runs from the Thursday `GetNextPublicRevealTime()` through Saturday 20:00 ET. Monday Jan 6 falls safely after the Jan 4 Saturday processing and before the Jan 8 Thursday reveal, so all games are freely biddable.

### Phase 2: Select game targets

```csharp
// Game A — only P1 bids on this
var p1Available = await p1Session.League.TopAvailableGamesAsync(_year, _leagueID, _p1PublisherID, null);
_gameA = p1Available.First(g => g.IsAvailable && !g.Taken);

// Game B — P2 and P3 contest (P2 $20, P3 $10)
var p2Available = await p2Session.League.TopAvailableGamesAsync(_year, _leagueID, _p2PublisherID, null);
_gameB = p2Available.First(g => g.IsAvailable && !g.Taken
    && g.MasterGame.MasterGameID != _gameA.MasterGame.MasterGameID);

// Game C — P3 and P4 tiebreaker ($15 each)
var p3Available = await p3Session.League.TopAvailableGamesAsync(_year, _leagueID, _p3PublisherID, null);
_gameC = p3Available.First(g => g.IsAvailable && !g.Taken
    && g.MasterGame.MasterGameID != _gameA.MasterGame.MasterGameID
    && g.MasterGame.MasterGameID != _gameB.MasterGame.MasterGameID);

// Counter-pick target — P2 counter-picks one of P1's standard games
var p1Snapshot = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
_p1CounterPickTargetID = p1Snapshot.Games.First(g => !g.CounterPick).MasterGame!.MasterGameID;
```

### Phase 3: Place six bids

| Publisher | Session | Game | Amount | `CounterPick` | Expected result |
|---|---|---|---|---|---|
| P1 | `_managerSession` | Game A | $10 | `false` | Queued (will win) |
| P2 | `_p2Session` | Game B | $20 | `false` | Queued (will win) |
| P3 | `_p3Session` | Game B | $10 | `false` | Queued (will lose) |
| P3 | `_p3Session` | Game C | $15 | `false` | Queued (tiebreaker) |
| P4 | `_p4Session` | Game C | $15 | `false` | Queued (tiebreaker) |
| P2 | `_p2Session` | P1's game | $5 | `true` | Queued (will win) |

Each `MakePickupBid` call must return `Success = true` (bid accepted and queued). A `false` result is a test setup failure and should throw immediately — the bid was rejected at the domain level (e.g., game already released, already on roster).

`AllowIneligibleSlot = false` and `ConditionalDropPublisherGameID = null` for all six bids.

### Phase 4: Advance time and run bids

```csharp
// Advance clock to just past the Saturday 20:00 ET processing threshold
await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
{
    NewTime = new DateTimeOffset(2025, 1, 11, 20, 1, 0, TimeSpan.FromHours(-5))  // 20:01 ET = 01:01 UTC next day
});

await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
await _adminSession.ActionRunner.ProcessActionsAsync();
await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();
```

> **Why this works:** `GetNextBidTime()` uses `ActionProcessingDay = Saturday` at `ActionProcessingTime = 20:00`. Bids placed on Monday Jan 6 have timestamps before Jan 11 20:00; when `ProcessActions` runs at Jan 11 20:01, those bids fall within the current processing window. The day-of-week guard in `ProcessActions` is only enforced in production (environment name = "PRODUCTION"); the test factory runs in "Development" mode, so it is bypassed.

### Phase 5: Capture post-processing state

```csharp
_postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_leagueID, _year, null);

// Resolve tiebreaker winner/loser dynamically
var p3Publisher = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
var p4Publisher = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID);
bool p3WonGameC = p3Publisher.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameCMasterGameID);
_tiebreakerWinnerID = p3WonGameC ? _p3PublisherID : _p4PublisherID;
_tiebreakerLoserID  = p3WonGameC ? _p4PublisherID : _p3PublisherID;
```

---

## 4. Test Methods

### Uncontested bid (P1 → Game A)

- `Bid_Uncontested_P1_HasGameA_OnRoster`  
  P1's games list contains a non-counter-pick entry whose `MasterGameID == _gameAMasterGameID`.

- `Bid_Uncontested_P1_BudgetDeductedBy10`  
  P1's post-processing budget == `_p1StartBudget - 10`.

### Contested bid — winner (P2 → Game B, $20 beats $10)

- `Bid_Contested_P2_HasGameB_OnRoster`  
  P2's games list contains Game B as a standard game.

- `Bid_Contested_P2_BudgetDeductedBy25`  
  P2's budget == `_p2StartBudget - 25` (=$20 Game B + $5 counter-pick).

### Contested bid — loser (P3 vs Game B)

- `Bid_Contested_P3_DoesNotHaveGameB_OnRoster`  
  P3's games list has no entry with `MasterGameID == _gameBMasterGameID`.

### Counter-pick bid (P2 counter-picks P1's game)

- `Bid_CounterPick_P2_HasP1Game_AsCounterPick`  
  P2's games list contains a counter-pick entry whose `MasterGameID == _p1CounterPickTargetID`.

### Tiebreaker (P3 vs P4 on Game C, $15 each)

- `Bid_Tiebreaker_ExactlyOneOfP3P4_HasGameC`  
  Exactly one of the two publishers has Game C as a standard game.

- `Bid_Tiebreaker_Winner_BudgetDeductedBy15`  
  `_tiebreakerWinnerID` publisher's budget == their starting budget minus $15.

- `Bid_Tiebreaker_Loser_BudgetUnchanged`  
  `_tiebreakerLoserID` publisher's budget == their starting budget (losing bid is not charged).

---

## 5. NSwag Annotation Required

`MakePickupBid` in `LeagueController.cs` is missing its 200 OK annotation. Without it, the NSwag-generated `LeagueClient.MakePickupBidAsync()` returns `Task` (void) instead of `Task<PickupBidResultViewModel>`.

Add before the action:
```csharp
[ProducesResponseType<PickupBidResultViewModel>(StatusCodes.Status200OK)]
```

After adding, rebuild and regenerate the client:
```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build
```

The `ActionRunnerController` methods (`TurnOnActionProcessingMode`, `TurnOffActionProcessingMode`, `ProcessActions`) all return `IActionResult` with no body — they correctly generate as `Task` (void) in the client and need no annotation.

---

## 6. Files Touched

| File | Change |
|---|---|
| `src/FantasyCritic.IntegrationTests/Helpers/LeagueScenario.cs` | Add `FourPlayerBidding` scenario to `LeagueScenarios` |
| `src/FantasyCritic.Web/Controllers/API/LeagueController.cs` | Add `[ProducesResponseType<PickupBidResultViewModel>(Status200OK)]` to `MakePickupBid` |
| `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` | Regenerated (NSwag) |
| `src/FantasyCritic.IntegrationTests/Tests/League/BidProcessingTests.cs` | **New** — `BidProcessingTests` fixture |

---

## 7. Deferred / Future Work

| Item | Notes |
|---|---|
| **Drops and conditional drops** | Separate fixture; requires droppable games configured in the scenario |
| **"Ineligible at processing time" bids** | Requires a test-only admin endpoint that injects critic review scores for a game on a specific date. When a game receives reviews just before its release date, it becomes ineligible to be picked up. The adjustable clock makes the timing controllable; the missing piece is the score-injection endpoint. Flag as future work. |
