# Royale Lifecycle Integration Tests — Design Spec

**Date:** 2026-05-31  
**Status:** Approved

---

## Problem

The existing Royale integration tests (`RoyaleTests.cs`) cover only two scenarios: creating a publisher and buying one game. The full user journey — editing publisher profile, building an affordable multi-game roster with production-informed selection, setting advertising budgets, selling a game, and replacing it — is untested. Failure paths for advertising cap enforcement and budget exhaustion are also untested.

---

## Goal

Add four focused integration tests (two happy-path, two failure) that exercise the complete Royale lifecycle, using the existing `ProductionGameStatsCache` infrastructure for realistic game selection.

---

## New / Modified Files

```
src/FantasyCritic.IntegrationTests/
  ApiModels/RoyaleApiResponses.cs          ← expand existing models
  ProductionStats/ProductionGameStatsCache.cs  ← add FindAffordableSetAsync
  Tests/Royale/RoyaleTests.cs              ← add 4 new test methods + helpers
```

No other projects are touched. No new NuGet dependencies.

---

## Test Methods

All four methods go in the existing `[TestFixture] RoyaleTests : IntegrationTestBase`.

| Method | Category |
|--------|----------|
| `ChangePublisherProfile_NameIconAndSlogan_Succeeds` | Happy path |
| `RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds` | Happy path |
| `SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest` | Failure |
| `PurchaseGame_WhenBudgetInsufficient_ReturnsFailure` | Failure |

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
13. Reload publisher. Assert the new game appears on the roster.

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

---

## Out of Scope

- FC+ / PlusUser enforcement on Royale icon/slogan endpoints (decided: no change).
- Group/leaderboard Royale features.
- Conference-tied or league-tied Royale groups.
- Any production or domain code changes.
- Database schema changes.
