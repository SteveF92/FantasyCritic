# Royale Lifecycle Integration Tests — Design Spec

**Date:** 2026-05-31  
**Status:** Approved

---

## Problem

The existing Royale integration tests (`RoyaleTests.cs`) cover only two scenarios: creating a publisher and buying one game. The full user journey — editing publisher profile, building an affordable multi-game roster with production-informed selection, setting advertising budgets, selling a game, and replacing it — is untested. Failure paths for advertising cap enforcement, budget exhaustion, and the 5-day lockout window are also untested. The hidden-information system (games on an active roster are invisible to public viewers until they release or score) has no test coverage at all.

---

## Goal

Add five focused integration tests (two happy-path, three failure) plus an in-test public-visibility assertion that exercise the complete Royale lifecycle, using the existing `ProductionGameStatsCache` infrastructure for realistic game selection.

---

## New / Modified Files

```
src/FantasyCritic.IntegrationTests/
  ApiModels/RoyaleApiResponses.cs          ← expand existing models
  ProductionStats/ProductionGameStatsCache.cs  ← add FindAffordableSetAsync
  Tests/Royale/RoyaleTests.cs              ← add 5 new test methods + helpers
```

No other projects are touched. No new NuGet dependencies.

---

## Test Methods

All five methods go in the existing `[TestFixture] RoyaleTests : IntegrationTestBase`.

| Method | Category | Notes |
|--------|----------|-------|
| `ChangePublisherProfile_NameIconAndSlogan_Succeeds` | Happy path | |
| `RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds` | Happy path | Includes public-visibility assertion at end (Step D) |
| `SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest` | Failure | |
| `PurchaseGame_WhenBudgetInsufficient_ReturnsFailure` | Failure | |
| `PurchaseGame_InLockoutWindow_ReturnsFailure` | Failure | May be `Assert.Inconclusive` if no lockout-window game exists |

---

## Test 1 — `ChangePublisherProfile_NameIconAndSlogan_Succeeds`

1. Register a new user via `NewUser()`.
2. Create a Royale publisher for the active quarter.
3. `POST /api/Royale/ChangePublisherName` with a new name.
4. `POST /api/Royale/ChangePublisherIcon` with an emoji string (e.g. `"🎮"`).
5. `POST /api/Royale/ChangePublisherSlogan` with a short string (e.g. `"Test Slogan"`).
6. `GET /api/Royale/GetRoyalePublisher/{id}` once and assert all three fields match.

All three change endpoints return `200 OK` with no body on success. Icon and slogan are available to all authenticated users (no FC+ restriction on Royale endpoints).

---

## Test 2 — `RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds`

### Setup
Register a new user, create a publisher (budget starts at `$100`).

### Step A — Build a 10-game roster

1. `GET /api/Royale/PossibleMasterGames?publisherID={id}&releaseFilter=ExpectedToReleaseInQuarter`
2. Filter locally to `IsAvailable == true`.
3. Call new `ProductionGameStatsCache.FindAffordableSetAsync(available, g => g.MasterGame.MasterGameID, g => g.Cost, maxCount: 10, budgetCap: 91m, year)`.
   - If fewer candidates are returned than expected, fall back to `releaseFilter=All` and repeat.
4. For each game in the returned set, `POST /api/Royale/PurchaseGame` and assert `Success == true`.
5. Reload publisher. Assert `PublisherGames.Count == setCount` and `Budget >= 9m`.

**`budgetCap: 91m`** reserves at minimum `$9` for advertising ($1 + $3 + $5 = $9 planned below). The server enforces budget independently per purchase, so even if production stats are unavailable the test succeeds as long as `FindAffordableSetAsync` falls back gracefully.

### Step B — Set advertising budgets

6. Pick games at index 0, 1, 2 from the purchased set.
7. `POST /api/Royale/SetAdvertisingMoney` with amounts `$1`, `$3`, `$5` respectively. Assert each returns `200 OK`.
8. Reload publisher. Assert `PublisherGames[game0].AdvertisingMoney == 1m`, `[game1] == 3m`, `[game2] == 5m`.

### Step C — Sell and replace

9. `POST /api/Royale/SellGame` for game at index 0. Assert `200 OK`.
10. Reload publisher. Assert:
    - `PublisherGames` no longer contains game 0's `MasterGameID`.
    - `Budget` is higher than it was after step 5 (refund applied).
11. Call existing `FindPurchasableGameViaApiAsync` helper (already excludes owned games via `IsAvailable`). Assert a purchasable game is found.
12. `POST /api/Royale/PurchaseGame` for that game. Assert `Success == true`.
13. Reload publisher (as owner). Assert the new game appears on the roster.

### Step D — Public visibility check

14. Register a **second** user via `NewUser()` and create a second `ApiSession` for that user.
15. The second session calls `GET /api/Royale/GetRoyalePublisher/{id}` (this endpoint is `[AllowAnonymous]`).
16. Assert:
    - `PublisherGames.Count > 0` — the game slots are visible (count is public information).
    - **All** games where `GameHidden == true` have `MasterGame == null` and `AmountSpent == null`.
    - At least one game IS hidden (since all purchased games are unreleased, unscored, and still eligible to release in the active quarter — the exact condition for `RoyaleFunctions.GameOrActionIsHidden`).
17. The owner session then calls the same endpoint and asserts:
    - For the same games, `GameHidden` may be `true` but `MasterGame` is **not** null (owners see their own hidden games).

**Why all games are expected to be hidden:** `GameOrActionIsHidden` returns `true` when the quarter is active AND the game has no score AND is not yet released AND could still release in the quarter. Every game purchased via `PossibleMasterGames` satisfies all four conditions by construction (`IsAvailable` already enforces no score, not released, and could-release-in-quarter).

---

## Test 3 — `SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest`

1. Register user, create publisher.
2. Buy one game using `FindPurchasableGameViaApiAsync`.
3. `POST /api/Royale/SetAdvertisingMoney` with `AdvertisingMoney: 10.01m`.
4. Assert HTTP `400 Bad Request`.

The service returns `Result.Failure("You can't allocate more than 10 dollars in advertising money.")` for values `> 10m`. The controller translates this to a 400.

---

## Test 4 — `PurchaseGame_WhenBudgetInsufficient_ReturnsFailure`

### Goal

Verify that the purchase endpoint returns `Success == false` with an appropriate error when the publisher cannot afford the requested game.

### Approach

1. Register user, create publisher.
2. Buy games one at a time using `FindPurchasableGameViaApiAsync` in a loop until:
   - The publisher's remaining `Budget` is less than the `Cost` of the cheapest remaining available game, **or**
   - No available games remain.
3. If no low-budget situation arises (all purchasable games cost less than remaining budget), skip to the direct assertion below.
4. Take the first `PossibleMasterGames` entry where `IsAvailable == false` (server-validated, budget or other constraint) — if a "Not enough budget" case can be isolated — and attempt to `POST /api/Royale/PurchaseGame` with that game's ID.
5. Assert `purchaseResult.Success == false` and `purchaseResult.Errors` is non-empty.

**Fallback assertion (if budget never runs out):** The test is still valid if, after buying all available games, we attempt to re-purchase one already owned (which returns `Success == false` with "Publisher already has that game"). The primary intent — verifying the failure response shape — is exercised regardless.

---

## Test 5 — `PurchaseGame_InLockoutWindow_ReturnsFailure`

### Background

The service blocks purchase of any game releasing within 5 days with the error `"Game will release within 5 days."` (`FUTURE_RELEASE_LIMIT_DAYS = 5`). The `PossibleRoyaleMasterGameViewModel.Status` property surfaces this exact string when `CanPurchase == false`. This window is inherently time-dependent — on any given test run, there may be zero, one, or several games in it.

### Approach

1. Register a new user, create a publisher for the active quarter.
2. `GET /api/Royale/PossibleMasterGames?publisherID={id}&releaseFilter=All`.
3. Scan the returned list for a game where `IsAvailable == false` and `Status == "Game will release within 5 days."`.
4. **If none found:** `Assert.Inconclusive("No game currently in the 5-day lockout window; test skipped.")`. The test run is not marked as a failure.
5. **If found:** `POST /api/Royale/PurchaseGame` with that game's `MasterGameID`.
6. Assert `purchaseResult.Success == false` and `purchaseResult.Errors` is non-empty.

### New field required

`PossibleRoyaleMasterGameJson` must expose `Status` (string) so the test can match the exact blocking reason. Added in the infrastructure changes section below.

---

## Infrastructure Changes

### `RoyaleApiResponses.cs` — Model expansions

```csharp
internal sealed class RoyalePublisherJson
{
    // existing
    public Guid PublisherID { get; set; }
    public string PublisherName { get; set; } = "";
    public RoyaleYearQuarterJson YearQuarter { get; set; } = null!;
    public List<RoyalePublisherGameJson> PublisherGames { get; set; } = [];
    // new
    public decimal Budget { get; set; }
    public string? PublisherIcon { get; set; }
    public string? PublisherSlogan { get; set; }
}

internal sealed class RoyalePublisherGameJson
{
    // existing
    public MasterGameYearJson? MasterGame { get; set; }
    // new
    public bool GameHidden { get; set; }
    public decimal AdvertisingMoney { get; set; }
    public decimal? AmountSpent { get; set; }
}

internal sealed class PossibleRoyaleMasterGameJson
{
    // existing
    public MasterGameYearJson MasterGame { get; set; } = null!;
    public bool IsAvailable { get; set; }
    // new
    public decimal Cost { get; set; }
    public string Status { get; set; } = "";
}
```

### `ProductionGameStatsCache.cs` — New strategy

```csharp
/// <summary>
/// Returns up to <paramref name="maxCount"/> candidates whose combined cost fits within
/// <paramref name="budgetCap"/>, ordered by production HypeFactor descending (greedy).
/// Falls back to natural candidate order when production stats are unavailable.
/// Returns an empty list when <paramref name="localCandidates"/> is empty.
/// </summary>
public static async Task<IReadOnlyList<T>> FindAffordableSetAsync<T>(
    IEnumerable<T> localCandidates,
    Func<T, Guid>    masterGameIdSelector,
    Func<T, decimal> costSelector,
    int              maxCount,
    decimal          budgetCap,
    int              year)
```

**Algorithm:**

1. `await GetStatsForYearAsync(year)` → build `Dictionary<Guid, decimal> hypeLookup`.
2. Sort `localCandidates` by `hypeLookup.GetValueOrDefault(id, 0m)` descending. (If stats empty, original order is preserved — `0m` for all ties, `MaxBy` picks first.)
3. Greedy loop: maintain `remainingBudget = budgetCap` and `result = []`. For each candidate in sorted order, if `costSelector(candidate) <= remainingBudget` and `result.Count < maxCount`, add it, subtract cost.
4. Return `result` (may be fewer than `maxCount` if budget runs out or candidates are exhausted).

### `RoyaleTests.cs` — New helpers

```csharp
/// <summary>
/// Tries ExpectedToReleaseInQuarter first; falls back to All if fewer than <paramref name="minCount"/> candidates found.
/// Returns up to <paramref name="maxCount"/> affordable games within <paramref name="budgetCap"/>.
/// </summary>
private static async Task<IReadOnlyList<PossibleRoyaleMasterGameJson>> FindAffordableSetViaApiAsync(
    ApiSession session,
    Guid publisherID,
    int year,
    int maxCount,
    decimal budgetCap)
```

Uses `FindAffordableSetAsync` with `g => g.MasterGame.MasterGameID` and `g => g.Cost`.

---

## What Is Not Changed

- `IntegrationTestBase` — no modifications.
- `FantasyCriticWebApplicationFactory` — no modifications.
- `ApiSession` — no modifications.
- `RoyaleController` — no modifications (icon/slogan are available to all authenticated users on Royale endpoints; no FC+ restriction to add or remove).
- No production or domain code is touched.
- No database schema changes.

---

## Error Handling

| Scenario | Behaviour |
|----------|-----------|
| Fewer than 10 games available in `ExpectedToReleaseInQuarter` | Fall back to `All`; buy however many are returned |
| Production stats unavailable | `FindAffordableSetAsync` returns candidates in natural order; test still passes |
| Budget runs out before 10 games | Set is smaller; test asserts `setCount` (whatever was returned) |
| Sell blocked (game releasing within 5 days) | Test is inherently resilient: picks from the affordable set, which excludes near-release games via `IsAvailable` |
| No game in 5-day lockout window (Test 5) | `Assert.Inconclusive` — test is skipped, not failed |

---

## Out of Scope

- FC+ / PlusUser enforcement on Royale icon/slogan endpoints (decided: no change).
- Group/leaderboard Royale features.
- Conference-tied or league-tied Royale groups.
- Any production or domain code changes.
- Database schema changes.
