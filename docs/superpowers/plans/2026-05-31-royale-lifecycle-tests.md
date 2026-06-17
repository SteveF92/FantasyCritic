# Royale Lifecycle Integration Tests — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add five integration tests and supporting infrastructure that exercise the complete Royale publisher lifecycle — profile editing, multi-game purchase, advertising budgets, sell/replace, public visibility, and three purchase-failure paths.

**Architecture:** All changes are confined to `FantasyCritic.IntegrationTests`. We expand the existing Newtonsoft-friendly JSON model file (`RoyaleApiResponses.cs`) with new fields, add a greedy budget-aware selection strategy to `ProductionGameStatsCache`, add pure-unit tests for that strategy, then add five test methods plus one helper to `RoyaleTests.cs`.

**Tech Stack:** C# 12 / .NET 10, NUnit 4, `Microsoft.AspNetCore.Mvc.Testing`, Newtonsoft.Json, local Docker MySQL (port 3307 — must be running before tests execute).

---

## File Map

| File | Change |
|------|--------|
| `src/FantasyCritic.IntegrationTests/ApiModels/RoyaleApiResponses.cs` | Add `Budget`, `PublisherIcon`, `PublisherSlogan` to `RoyalePublisherJson`; add `GameHidden`, `AdvertisingMoney`, `AmountSpent` to `RoyalePublisherGameJson`; add `Cost`, `Status` to `PossibleRoyaleMasterGameJson` |
| `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionGameStatsCache.cs` | Add `internal static SelectAffordableSet` + `public static async FindAffordableSetAsync` |
| `src/FantasyCritic.IntegrationTests/Tests/ProductionStats/ProductionGameStatsCacheTests.cs` | Add four unit tests for `SelectAffordableSet` |
| `src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs` | Add `FindAffordableSetViaApiAsync` helper; add five test methods |

No other files are touched. No new NuGet packages. No production code changes.

---

## Task 1: Expand API response models

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/ApiModels/RoyaleApiResponses.cs`

These models are Newtonsoft-deserialized test shapes. They mirror Web ViewModels but are not generated — property names must exactly match the JSON keys the server emits.

- [ ] **Step 1: Replace the full file contents**

```csharp
using System;
using System.Collections.Generic;

namespace FantasyCritic.IntegrationTests.ApiModels;

/// <summary>
/// JSON shapes for Royale API responses. Web ViewModels in this area are built from
/// domain types and are not suitable for Newtonsoft deserialization in tests.
/// Request types remain <c>FantasyCritic.Web.Models.Requests.Royale</c>.
/// </summary>
internal sealed class RoyaleYearQuarterJson
{
    public int Year { get; set; }
    public int Quarter { get; set; }
    public bool OpenForPlay { get; set; }
    public bool Finished { get; set; }
}

internal sealed class RoyalePublisherJson
{
    public Guid PublisherID { get; set; }
    public string PublisherName { get; set; } = "";
    public string? PublisherIcon { get; set; }
    public string? PublisherSlogan { get; set; }
    public RoyaleYearQuarterJson YearQuarter { get; set; } = null!;
    public List<RoyalePublisherGameJson> PublisherGames { get; set; } = [];
    public decimal Budget { get; set; }
}

internal sealed class RoyalePublisherGameJson
{
    public MasterGameYearJson? MasterGame { get; set; }
    public bool GameHidden { get; set; }
    public decimal AdvertisingMoney { get; set; }
    public decimal? AmountSpent { get; set; }
}

internal sealed class MasterGameYearJson
{
    public Guid MasterGameID { get; set; }
}

internal sealed class PossibleRoyaleMasterGameJson
{
    public MasterGameYearJson MasterGame { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; } = "";
}

internal sealed class PlayerClaimResultJson
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = [];
}
```

- [ ] **Step 2: Build to confirm no compile errors**

```
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: `Build succeeded.` with 0 errors. (Existing tests reference the models; adding fields is non-breaking.)

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.IntegrationTests/ApiModels/RoyaleApiResponses.cs
git commit -m "Add Budget, icon, slogan, GameHidden, AdvertisingMoney, AmountSpent, Cost, Status to Royale JSON models."
```

---

## Task 2: Add `FindAffordableSetAsync` strategy to `ProductionGameStatsCache`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionGameStatsCache.cs`

The existing file already has `SelectHighestHypeAvailable` (internal pure method) and `FindHighestHypeAvailableAsync` (public async wrapper). We follow the same split: an `internal static` pure method (testable without HTTP) plus a `public static async` wrapper.

- [ ] **Step 1: Add `SelectAffordableSet` and `FindAffordableSetAsync` to the class**

Open the file. The class body currently ends with the closing brace of `MasterGameYearApiJson`. Insert the two new members **before** the `MasterGameYearApiJson` private nested class:

```csharp
    public static async Task<IReadOnlyList<T>> FindAffordableSetAsync<T>(
        IEnumerable<T> localCandidates,
        Func<T, Guid> masterGameIdSelector,
        Func<T, decimal> costSelector,
        int maxCount,
        decimal budgetCap,
        int year)
    {
        var stats = await GetStatsForYearAsync(year);
        return SelectAffordableSet(localCandidates, masterGameIdSelector, costSelector, maxCount, budgetCap, stats);
    }

    internal static IReadOnlyList<T> SelectAffordableSet<T>(
        IEnumerable<T> localCandidates,
        Func<T, Guid> masterGameIdSelector,
        Func<T, decimal> costSelector,
        int maxCount,
        decimal budgetCap,
        IReadOnlyList<ProductionMasterGameYearStat> stats)
    {
        var hypeLookup = stats.ToDictionary(s => s.MasterGameID, s => s.HypeFactor);

        var sorted = localCandidates
            .OrderByDescending(c => hypeLookup.GetValueOrDefault(masterGameIdSelector(c), 0m))
            .ToList();

        var result = new List<T>();
        var remainingBudget = budgetCap;

        foreach (var candidate in sorted)
        {
            if (result.Count >= maxCount) break;
            var cost = costSelector(candidate);
            if (cost <= remainingBudget)
            {
                result.Add(candidate);
                remainingBudget -= cost;
            }
        }

        return result;
    }
```

After adding, the full file should look like this (complete replacement for clarity):

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FantasyCritic.IntegrationTests.ProductionStats;

/// <summary>
/// Fetches production master-game-year stats once per year per test run and exposes
/// selection strategies for cross-referencing locally-available game candidates.
/// </summary>
internal static class ProductionGameStatsCache
{
    private const string ProductionBaseUrl = "https://www.fantasycritic.games";

    private static readonly ConcurrentDictionary<int, Lazy<Task<IReadOnlyList<ProductionMasterGameYearStat>>>>
        Cache = new();

    public static async Task<T?> FindHighestHypeAvailableAsync<T>(
        IEnumerable<T> localCandidates,
        Func<T, Guid> masterGameIdSelector,
        int year)
    {
        var stats = await GetStatsForYearAsync(year);
        return SelectHighestHypeAvailable(localCandidates, masterGameIdSelector, stats);
    }

    public static async Task<IReadOnlyList<T>> FindAffordableSetAsync<T>(
        IEnumerable<T> localCandidates,
        Func<T, Guid> masterGameIdSelector,
        Func<T, decimal> costSelector,
        int maxCount,
        decimal budgetCap,
        int year)
    {
        var stats = await GetStatsForYearAsync(year);
        return SelectAffordableSet(localCandidates, masterGameIdSelector, costSelector, maxCount, budgetCap, stats);
    }

    internal static Task<IReadOnlyList<ProductionMasterGameYearStat>> GetStatsForYearAsync(int year) =>
        Cache.GetOrAdd(year, y => new Lazy<Task<IReadOnlyList<ProductionMasterGameYearStat>>>(
            () => FetchStatsForYearAsync(y))).Value;

    internal static T? SelectHighestHypeAvailable<T>(
        IEnumerable<T> localCandidates,
        Func<T, Guid> masterGameIdSelector,
        IReadOnlyList<ProductionMasterGameYearStat> stats)
    {
        var candidates = localCandidates as IList<T> ?? localCandidates.ToList();
        if (candidates.Count == 0)
        {
            return default;
        }

        var hypeLookup = stats.ToDictionary(s => s.MasterGameID, s => s.HypeFactor);
        return candidates.MaxBy(c => hypeLookup.GetValueOrDefault(masterGameIdSelector(c), 0m));
    }

    internal static IReadOnlyList<T> SelectAffordableSet<T>(
        IEnumerable<T> localCandidates,
        Func<T, Guid> masterGameIdSelector,
        Func<T, decimal> costSelector,
        int maxCount,
        decimal budgetCap,
        IReadOnlyList<ProductionMasterGameYearStat> stats)
    {
        var hypeLookup = stats.ToDictionary(s => s.MasterGameID, s => s.HypeFactor);

        var sorted = localCandidates
            .OrderByDescending(c => hypeLookup.GetValueOrDefault(masterGameIdSelector(c), 0m))
            .ToList();

        var result = new List<T>();
        var remainingBudget = budgetCap;

        foreach (var candidate in sorted)
        {
            if (result.Count >= maxCount) break;
            var cost = costSelector(candidate);
            if (cost <= remainingBudget)
            {
                result.Add(candidate);
                remainingBudget -= cost;
            }
        }

        return result;
    }

    private static async Task<IReadOnlyList<ProductionMasterGameYearStat>> FetchStatsForYearAsync(int year)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(ProductionBaseUrl) };
            var json = await client.GetStringAsync($"api/Game/MasterGameYear/{year}");
            var rows = JsonConvert.DeserializeObject<List<MasterGameYearApiJson>>(json);
            if (rows is null)
            {
                Console.Error.WriteLine(
                    $"ProductionGameStatsCache: null response deserializing stats for year {year}.");
                return Array.Empty<ProductionMasterGameYearStat>();
            }

            return rows
                .Select(r => new ProductionMasterGameYearStat(r.MasterGameID, r.GameName, (decimal)r.HypeFactor))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"ProductionGameStatsCache: failed to fetch stats for year {year}: {ex.Message}");
            return Array.Empty<ProductionMasterGameYearStat>();
        }
    }

    private sealed class MasterGameYearApiJson
    {
        public Guid MasterGameID { get; set; }
        public string GameName { get; set; } = "";
        public double HypeFactor { get; set; }
    }
}
```

- [ ] **Step 2: Build to confirm no compile errors**

```
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: `Build succeeded.` with 0 errors.

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.IntegrationTests/ProductionStats/ProductionGameStatsCache.cs
git commit -m "Add FindAffordableSetAsync and SelectAffordableSet to ProductionGameStatsCache."
```

---

## Task 3: Unit tests for `SelectAffordableSet`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/ProductionStats/ProductionGameStatsCacheTests.cs`

These are pure unit tests — no HTTP, no database, no `IntegrationTestBase`. They live in the same file as the existing `SelectHighestHypeAvailable` tests.

- [ ] **Step 1: Add the four new test methods and the `CostCandidate` record**

Append after the closing brace of `SelectHighestHypeAvailable_WhenStatsPresent_ReturnsHighestHypeCandidate`, before the final `}` of the class:

```csharp
    private sealed record CostCandidate(Guid Id, decimal Cost);

    [Test]
    public void SelectAffordableSet_WhenCandidatesEmpty_ReturnsEmpty()
    {
        var result = ProductionGameStatsCache.SelectAffordableSet(
            Array.Empty<CostCandidate>(),
            c => c.Id,
            c => c.Cost,
            maxCount: 10,
            budgetCap: 100m,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void SelectAffordableSet_WhenBudgetCapExcludesCostlyGame_ExcludesThatGame()
    {
        var cheapID    = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var expensiveID = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var candidates = new[]
        {
            new CostCandidate(cheapID, 5m),
            new CostCandidate(expensiveID, 50m),
        };

        // budgetCap 20m: cheap (5m) fits; expensive (50m) does not
        var result = ProductionGameStatsCache.SelectAffordableSet(
            candidates,
            c => c.Id,
            c => c.Cost,
            maxCount: 10,
            budgetCap: 20m,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(cheapID));
    }

    [Test]
    public void SelectAffordableSet_WhenMaxCountReached_StopsAtCount()
    {
        var candidates = Enumerable.Range(0, 5)
            .Select(_ => new CostCandidate(Guid.NewGuid(), 1m))
            .ToList();

        var result = ProductionGameStatsCache.SelectAffordableSet(
            candidates,
            c => c.Id,
            c => c.Cost,
            maxCount: 3,
            budgetCap: 100m,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public void SelectAffordableSet_WhenStatsPresent_PicksHighestHypeFirst()
    {
        var lowHypeID  = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var highHypeID = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var candidates = new[]
        {
            new CostCandidate(lowHypeID, 10m),
            new CostCandidate(highHypeID, 10m),
        };
        var stats = new[]
        {
            new ProductionMasterGameYearStat(lowHypeID,  "Low Hype Game",  1m),
            new ProductionMasterGameYearStat(highHypeID, "High Hype Game", 99m),
        };

        // budgetCap 15m: only one 10m game fits
        var result = ProductionGameStatsCache.SelectAffordableSet(
            candidates,
            c => c.Id,
            c => c.Cost,
            maxCount: 1,
            budgetCap: 15m,
            stats);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(highHypeID),
            "Highest-hype game should be selected first when budget only allows one.");
    }
```

- [ ] **Step 2: Run the unit tests to verify all four pass**

```
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~ProductionGameStatsCacheTests"
```

Expected output (excerpt):
```
Passed  SelectAffordableSet_WhenCandidatesEmpty_ReturnsEmpty
Passed  SelectAffordableSet_WhenBudgetCapExcludesCostlyGame_ExcludesThatGame
Passed  SelectAffordableSet_WhenMaxCountReached_StopsAtCount
Passed  SelectAffordableSet_WhenStatsPresent_PicksHighestHypeFirst
```

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.IntegrationTests/Tests/ProductionStats/ProductionGameStatsCacheTests.cs
git commit -m "Add SelectAffordableSet unit tests to ProductionGameStatsCacheTests."
```

---

## Task 4: Test 1 — `ChangePublisherProfile_NameIconAndSlogan_Succeeds`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs`

This test verifies that all three profile-change endpoints (`ChangePublisherName`, `ChangePublisherIcon`, `ChangePublisherSlogan`) succeed for an authenticated user and that the changes are reflected on `GetRoyalePublisher`. No FC+ restriction exists on Royale publisher endpoints.

`ApiSession.PostJsonAsync<T>` returns `HttpResponseMessage` and is already available — use it for void-response endpoints.

- [ ] **Step 1: Add `using System.Net;` to the using block at the top of `RoyaleTests.cs`**

The file currently starts with:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
```

Add `using System.Net;` after `using System;` so it reads:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
```

- [ ] **Step 2: Add the test method inside the `RoyaleTests` class, after `PurchaseGame_WhenGameIsAvailable_Succeeds`**

```csharp
    [Test]
    public async Task ChangePublisherProfile_NameIconAndSlogan_Succeeds()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var initialName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, initialName));

        var newName = $"Pub-{Guid.NewGuid():N}"[..20];

        var nameResponse = await session.PostJsonAsync("/api/Royale/ChangePublisherName",
            new ChangeRoyalePublisherNameRequest(publisherID, newName));
        Assert.That(nameResponse.IsSuccessStatusCode, Is.True, "ChangePublisherName should return 2xx.");

        var iconResponse = await session.PostJsonAsync("/api/Royale/ChangePublisherIcon",
            new ChangeRoyalePublisherIconRequest(publisherID, "🎮"));
        Assert.That(iconResponse.IsSuccessStatusCode, Is.True, "ChangePublisherIcon should return 2xx.");

        var sloganResponse = await session.PostJsonAsync("/api/Royale/ChangePublisherSlogan",
            new ChangeRoyalePublisherSloganRequest(publisherID, "Test Slogan"));
        Assert.That(sloganResponse.IsSuccessStatusCode, Is.True, "ChangePublisherSlogan should return 2xx.");

        var publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(publisher.PublisherName, Is.EqualTo(newName));
        Assert.That(publisher.PublisherIcon, Is.EqualTo("🎮"));
        Assert.That(publisher.PublisherSlogan, Is.EqualTo("Test Slogan"));
    }
```

- [ ] **Step 3: Run the new test**

```
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RoyaleTests.ChangePublisherProfile"
```

Expected: `Passed  ChangePublisherProfile_NameIconAndSlogan_Succeeds`

- [ ] **Step 4: Commit**

```
git add src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs
git commit -m "Add ChangePublisherProfile_NameIconAndSlogan_Succeeds integration test."
```

---

## Task 5: Test 2 — `RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs`

This is the largest test. It covers the full lifecycle: multi-game roster build, advertising budgets, sell, replace, and public-visibility check. Add a private `FindAffordableSetViaApiAsync` helper to the class as well.

**How `FindAffordableSetViaApiAsync` works:** It tries `ExpectedToReleaseInQuarter` first (games the user actually wants — expected to release this quarter). If that yields zero candidates it falls back to `All`. It then calls `ProductionGameStatsCache.FindAffordableSetAsync` with the budget cap so the greedy hype-sorted selection happens.

**Why games are hidden from public:** `RoyaleFunctions.GameOrActionIsHidden` returns `true` when: quarter is active AND game has no critic score AND game is not yet released AND game could still release in the quarter. All games purchasable via `IsAvailable == true` satisfy all four conditions by definition — the server's `ValidatePurchaseGame` already enforces "no score" and "not released" as preconditions.

**Owner view:** When the owner calls `GetRoyalePublisher`, `thisPlayerIsViewing = true` is passed to the ViewModel constructor, which populates `MasterGame` even when `GameHidden = true`. The public viewer gets `MasterGame = null` for hidden games.

- [ ] **Step 1: Add `FindAffordableSetViaApiAsync` helper to the `RoyaleTests` class**

Add this private static method after `FindPurchasableGameViaApiAsync`:

```csharp
    /// <summary>
    /// Tries ExpectedToReleaseInQuarter first; falls back to All if no candidates found.
    /// Returns up to <paramref name="maxCount"/> games whose combined cost stays within
    /// <paramref name="budgetCap"/>, ordered by production HypeFactor descending.
    /// </summary>
    private static async Task<IReadOnlyList<PossibleRoyaleMasterGameJson>> FindAffordableSetViaApiAsync(
        ApiSession session,
        Guid publisherID,
        int year,
        int maxCount,
        decimal budgetCap)
    {
        foreach (var releaseFilter in new[] { "ExpectedToReleaseInQuarter", "All" })
        {
            var possibleGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
                $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter={releaseFilter}");

            var available = possibleGames.Where(g => g.IsAvailable).ToList();
            if (available.Count == 0) continue;

            var set = await ProductionGameStatsCache.FindAffordableSetAsync(
                available,
                g => g.MasterGame.MasterGameID,
                g => g.Cost,
                maxCount,
                budgetCap,
                year);

            if (set.Count > 0) return set;
        }

        return Array.Empty<PossibleRoyaleMasterGameJson>();
    }
```

- [ ] **Step 2: Add the lifecycle test method**

```csharp
    [Test]
    public async Task RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds()
    {
        // ── Setup ──────────────────────────────────────────────────────────────
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        // ── Step A: Build up to a 10-game roster ───────────────────────────────
        // Keep $9 in reserve ($1 + $3 + $5 advertising allocated below).
        const decimal adReserve = 9m;
        const decimal budgetCap = 100m - adReserve;

        var gameSet = await FindAffordableSetViaApiAsync(
            session, publisherID, activeQuarter.Year, maxCount: 10, budgetCap);

        Assert.That(gameSet.Count, Is.GreaterThan(0),
            "Expected at least one purchasable game for the active quarter. "
            + "Verify the local DB is synced from production (run LocalDatabaseTool).");

        var purchasedIDs = new List<Guid>();
        foreach (var game in gameSet)
        {
            var result = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
                "/api/Royale/PurchaseGame",
                new PurchaseRoyaleGameRequest(publisherID, game.MasterGame.MasterGameID));
            Assert.That(result.Success, Is.True,
                $"PurchaseGame failed for {game.MasterGame.MasterGameID}: {string.Join("; ", result.Errors)}");
            purchasedIDs.Add(game.MasterGame.MasterGameID);
        }

        var publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(publisher.PublisherGames.Count, Is.EqualTo(gameSet.Count),
            "Roster count should match the number of successful purchases.");
        Assert.That(publisher.Budget, Is.GreaterThanOrEqualTo(adReserve),
            "Budget should have at least $9 remaining for advertising.");

        // ── Step B: Advertising budgets ────────────────────────────────────────
        Assert.That(purchasedIDs.Count, Is.GreaterThanOrEqualTo(3),
            "Need at least 3 games to set advertising money on games 0, 1, 2.");

        var game0ID = purchasedIDs[0];
        var game1ID = purchasedIDs[1];
        var game2ID = purchasedIDs[2];

        var ad0 = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game0ID, 1m));
        Assert.That(ad0.IsSuccessStatusCode, Is.True, "SetAdvertisingMoney $1 on game0 should succeed.");

        var ad1 = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game1ID, 3m));
        Assert.That(ad1.IsSuccessStatusCode, Is.True, "SetAdvertisingMoney $3 on game1 should succeed.");

        var ad2 = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game2ID, 5m));
        Assert.That(ad2.IsSuccessStatusCode, Is.True, "SetAdvertisingMoney $5 on game2 should succeed.");

        // Owner reloads — hidden games still have MasterGame populated for the owner.
        publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(
            publisher.PublisherGames.Single(g => g.MasterGame!.MasterGameID == game0ID).AdvertisingMoney,
            Is.EqualTo(1m), "game0 should have $1 advertising money.");
        Assert.That(
            publisher.PublisherGames.Single(g => g.MasterGame!.MasterGameID == game1ID).AdvertisingMoney,
            Is.EqualTo(3m), "game1 should have $3 advertising money.");
        Assert.That(
            publisher.PublisherGames.Single(g => g.MasterGame!.MasterGameID == game2ID).AdvertisingMoney,
            Is.EqualTo(5m), "game2 should have $5 advertising money.");

        // ── Step C: Sell game0 and buy a replacement ───────────────────────────
        var budgetBeforeSell = publisher.Budget;

        var sellResponse = await session.PostJsonAsync("/api/Royale/SellGame",
            new SellRoyaleGameRequest(publisherID, game0ID));
        Assert.That(sellResponse.IsSuccessStatusCode, Is.True, "SellGame should return 2xx.");

        publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(
            publisher.PublisherGames.All(g => g.MasterGame?.MasterGameID != game0ID),
            Is.True,
            "Sold game (game0) should not appear on the roster after selling.");
        Assert.That(publisher.Budget, Is.GreaterThan(budgetBeforeSell),
            "Budget should increase after selling (refund applied).");

        var replacement = await FindPurchasableGameViaApiAsync(session, publisherID, activeQuarter.Year);
        Assert.That(replacement, Is.Not.Null,
            "A purchasable replacement game should exist after selling game0.");

        var replaceResult = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, replacement!.MasterGame.MasterGameID));
        Assert.That(replaceResult.Success, Is.True,
            $"Replacement purchase failed: {string.Join("; ", replaceResult.Errors)}");

        publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");
        Assert.That(
            publisher.PublisherGames.Any(g => g.MasterGame?.MasterGameID == replacement.MasterGame.MasterGameID),
            Is.True,
            "Replacement game should appear on the roster.");

        // ── Step D: Public visibility check ────────────────────────────────────
        // A different authenticated user viewing this publisher sees hidden-game entries
        // with GameHidden=true but MasterGame=null (game identity concealed).
        // The owner, by contrast, sees MasterGame populated even for hidden games.
        var (email2, password2, displayName2) = NewUser();
        using var publicSession = new ApiSession(Factory);
        await publicSession.RegisterAsync(email2, password2, displayName2);

        var publicView = await publicSession.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(publicView.PublisherGames.Count, Is.GreaterThan(0),
            "Public viewer should be able to see how many games the publisher has.");

        var hiddenFromPublic = publicView.PublisherGames.Where(g => g.GameHidden).ToList();
        Assert.That(hiddenFromPublic.Count, Is.GreaterThan(0),
            "At least one game should be hidden from a public viewer. "
            + "All purchased games are unreleased and unscored, so all should be hidden.");

        foreach (var hidden in hiddenFromPublic)
        {
            Assert.That(hidden.MasterGame, Is.Null,
                "Public viewers must not see the MasterGame identity of hidden games.");
            Assert.That(hidden.AmountSpent, Is.Null,
                "Public viewers must not see the AmountSpent of hidden games.");
        }

        // Owner still sees MasterGame for their own hidden games.
        var ownerView = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        foreach (var ownerHidden in ownerView.PublisherGames.Where(g => g.GameHidden))
        {
            Assert.That(ownerHidden.MasterGame, Is.Not.Null,
                "Owner must see MasterGame even for games flagged as hidden.");
        }
    }
```

- [ ] **Step 3: Run the lifecycle test**

```
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RoyaleTests.RoyaleLifecycle"
```

Expected: `Passed  RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds`

This test makes real HTTP calls to `https://www.fantasycritic.games` for production hype stats and real DB calls to the local MySQL. Both must be available. If production is unreachable the test still passes (fallback to natural order).

- [ ] **Step 4: Commit**

```
git add src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs
git commit -m "Add RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds with FindAffordableSetViaApiAsync helper."
```

---

## Task 6: Test 3 — `SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs`

The service enforces a `$10` maximum per game: `if (advertisingMoney > 10m) return Result.Failure("You can't allocate more than 10 dollars in advertising money.")`. The controller returns `400 Bad Request` when `setAdvertisingMoneyResult.IsFailure`. We verify the HTTP status code directly using `session.PostJsonAsync`.

- [ ] **Step 1: Add the test method**

```csharp
    [Test]
    public async Task SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        var game = await FindPurchasableGameViaApiAsync(session, publisherID, activeQuarter.Year);
        Assert.That(game, Is.Not.Null,
            "Need a purchasable game to set advertising money on. "
            + "Verify the local DB is synced from production.");

        var purchaseResult = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, game!.MasterGame.MasterGameID));
        Assert.That(purchaseResult.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors)}");

        // Attempt to set $10.01 — one cent above the $10 cap
        var overCapResponse = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game.MasterGame.MasterGameID, 10.01m));

        Assert.That(overCapResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Setting advertising money above $10 must return HTTP 400 Bad Request.");
    }
```

- [ ] **Step 2: Run the test**

```
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RoyaleTests.SetAdvertisingMoney"
```

Expected: `Passed  SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest`

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs
git commit -m "Add SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest integration test."
```

---

## Task 7: Tests 4 & 5 — Budget insufficient and lockout window failure tests

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs`

Two more failure tests are added together in one commit since both are compact.

**Test 4 — `PurchaseGame_WhenBudgetInsufficient_ReturnsFailure`:** Depletes budget by buying the most expensive available game in a loop, then attempts to buy something no longer affordable (ideal) or re-buys an already-owned game (fallback). Either way the response must have `Success == false`.

**Test 5 — `PurchaseGame_InLockoutWindow_ReturnsFailure`:** Finds a game whose `Status` is exactly `"Game will release within 5 days."` and tries to buy it. This test is time-dependent; if no such game exists in the local DB right now it calls `Assert.Inconclusive` (NUnit marks it as skipped, not failed).

- [ ] **Step 1: Add both test methods**

```csharp
    [Test]
    public async Task PurchaseGame_WhenBudgetInsufficient_ReturnsFailure()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        // Buy the most expensive available game repeatedly to deplete budget quickly.
        var boughtIDs = new List<Guid>();
        while (true)
        {
            var all = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
                $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter=All");

            var next = all.Where(g => g.IsAvailable).MaxBy(g => g.Cost);
            if (next is null) break; // No more affordable games

            var result = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
                "/api/Royale/PurchaseGame",
                new PurchaseRoyaleGameRequest(publisherID, next.MasterGame.MasterGameID));
            if (!result.Success) break;
            boughtIDs.Add(next.MasterGame.MasterGameID);
        }

        if (boughtIDs.Count == 0)
        {
            Assert.Inconclusive("No games were purchasable; cannot verify purchase failure path.");
            return;
        }

        // Best target: a game blocked by "Not enough budget." if one exists;
        // otherwise fall back to re-purchasing an already-owned game.
        var allGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
            $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter=All");

        var budgetBlocked = allGames.FirstOrDefault(g => !g.IsAvailable && g.Status == "Not enough budget.");
        var targetID = budgetBlocked?.MasterGame.MasterGameID ?? boughtIDs[0];

        var failureResult = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, targetID));

        Assert.That(failureResult.Success, Is.False,
            "Purchasing a game the publisher cannot afford (or already owns) must return Success=false.");
        Assert.That(failureResult.Errors, Is.Not.Empty,
            "A failed purchase must include at least one error message.");
    }

    [Test]
    public async Task PurchaseGame_InLockoutWindow_ReturnsFailure()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        var allGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
            $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter=All");

        // The service sets Status = "Game will release within 5 days." for games in the lockout window.
        // RoyaleService.FUTURE_RELEASE_LIMIT_DAYS == 5.
        const string lockoutStatus = "Game will release within 5 days.";
        var lockoutGame = allGames.FirstOrDefault(g => !g.IsAvailable && g.Status == lockoutStatus);

        if (lockoutGame is null)
        {
            Assert.Inconclusive(
                $"No game currently has Status == \"{lockoutStatus}\". "
                + "This test only runs when a game is releasing within 5 days. Skipping.");
            return;
        }

        var result = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, lockoutGame.MasterGame.MasterGameID));

        Assert.That(result.Success, Is.False,
            "Purchasing a game in the 5-day lockout window must return Success=false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "A failed purchase must include at least one error message.");
    }
```

- [ ] **Step 2: Run all Royale tests**

```
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RoyaleTests"
```

Expected outcomes:
- `Passed  PurchaseGame_WhenBudgetInsufficient_ReturnsFailure`
- `Passed  PurchaseGame_InLockoutWindow_ReturnsFailure` **or** `Inconclusive` (if no lockout-window game exists today — not a failure)
- All previously-passing Royale tests still pass

- [ ] **Step 3: Commit**

```
git add src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs
git commit -m "Add PurchaseGame_WhenBudgetInsufficient and PurchaseGame_InLockoutWindow failure tests."
```

---

## Task 8: Final full-suite run

- [ ] **Step 1: Run all integration tests**

```
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: All 19 tests pass (10 existing + 4 new `SelectAffordableSet` unit tests + 5 new Royale integration tests). The lockout-window test may be `Inconclusive` if no game is releasing within 5 days — that is not a failure.

- [ ] **Step 2: Done**

No further changes. All test infrastructure, strategy methods, API model fields, and test methods are committed.
