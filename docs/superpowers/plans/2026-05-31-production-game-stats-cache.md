# Production Game Stats Cache Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a test-only production API stats cache to `FantasyCritic.IntegrationTests` so integration tests can pick locally-available games using real production `HypeFactor` data, starting with Royale purchase tests.

**Architecture:** A static `ProductionGameStatsCache` class fetches `GET /api/Game/MasterGameYear/{year}` from production once per year per test run (in-memory `ConcurrentDictionary` + `Lazy<Task<>>`). Pure selection logic is extracted to an `internal` helper so unit tests can verify ranking without HTTP. `RoyaleTests.FindPurchasableGameViaApiAsync` still calls the local API for candidates, then delegates to `FindHighestHypeAvailableAsync`.

**Tech Stack:** .NET 10, NUnit 4, `HttpClient`, `Newtonsoft.Json` (already referenced by IntegrationTests)

**Design spec:** `docs/superpowers/specs/2026-05-31-production-game-stats-cache-design.md`

---

## File map

| Path | Action | Purpose |
|------|--------|---------|
| `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionMasterGameYearStat.cs` | Create | Internal record for cached stats |
| `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionGameStatsCache.cs` | Create | Fetch, cache, and strategy methods |
| `src/FantasyCritic.IntegrationTests/Tests/ProductionStats/ProductionGameStatsCacheTests.cs` | Create | Unit tests for selection logic (no HTTP) |
| `src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs` | Modify | Wire hype-based game selection |

No new NuGet packages. No changes to Web, Lib, or LocalDatabaseTool.

---

## Key facts (read before coding)

- Production base URL: `https://www.fantasycritic.games` (hard-coded constant).
- Endpoint: `GET /api/Game/MasterGameYear/{year}` — anonymous, returns a JSON array.
- Relevant JSON fields per item: `masterGameID` (Guid), `gameName` (string), `hypeFactor` (double). Use a minimal private DTO for deserialization; do **not** deserialize into `MasterGameYearViewModel` (it has NodaTime types and a constructor-only shape).
- `HypeFactor` on the wire is `double`; store as `decimal` on `ProductionMasterGameYearStat` via cast.
- On fetch failure: write warning to `Console.Error`, return empty list — never throw.
- Empty stats + non-empty candidates: `MaxBy` with hype `0` for all → returns first candidate (fallback behaviour).
- Empty candidates: return `null`.
- Future strategies will be additional `public static async Task<T?>` methods on the same class; no interface/strategy pattern now.

---

## Task 1: `ProductionMasterGameYearStat` record

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionMasterGameYearStat.cs`

- [ ] **Step 1: Create the record**

Create `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionMasterGameYearStat.cs`:

```csharp
namespace FantasyCritic.IntegrationTests.ProductionStats;

/// <summary>
/// Minimal production master-game-year stats used by integration test selection strategies.
/// </summary>
internal sealed record ProductionMasterGameYearStat(
    Guid MasterGameID,
    string GameName,
    decimal HypeFactor);
```

- [ ] **Step 2: Build to verify compilation**

Run:

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/ProductionStats/ProductionMasterGameYearStat.cs
git commit -m "Add ProductionMasterGameYearStat record for integration test stats cache."
```

---

## Task 2: Selection logic unit tests (TDD)

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/ProductionStats/ProductionGameStatsCacheTests.cs`

These tests target `SelectHighestHypeAvailable` — an `internal static` helper added in Task 3. Write the tests first; they will not compile until Task 3.

- [ ] **Step 1: Write failing unit tests**

Create `src/FantasyCritic.IntegrationTests/Tests/ProductionStats/ProductionGameStatsCacheTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using FantasyCritic.IntegrationTests.ProductionStats;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.ProductionStats;

[TestFixture]
public class ProductionGameStatsCacheTests
{
    private sealed record Candidate(Guid MasterGameID, string Label);

    [Test]
    public void SelectHighestHypeAvailable_WhenCandidatesEmpty_ReturnsNull()
    {
        var stats = new List<ProductionMasterGameYearStat>
        {
            new(Guid.NewGuid(), "Game A", 10m)
        };

        var result = ProductionGameStatsCache.SelectHighestHypeAvailable(
            Array.Empty<Candidate>(),
            c => c.MasterGameID,
            stats);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SelectHighestHypeAvailable_WhenStatsEmpty_ReturnsFirstCandidate()
    {
        var id1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var id2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var candidates = new[]
        {
            new Candidate(id1, "first"),
            new Candidate(id2, "second")
        };

        var result = ProductionGameStatsCache.SelectHighestHypeAvailable(
            candidates,
            c => c.MasterGameID,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.MasterGameID, Is.EqualTo(id1));
    }

    [Test]
    public void SelectHighestHypeAvailable_WhenStatsPresent_ReturnsHighestHypeCandidate()
    {
        var lowId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var highId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var missingId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var candidates = new[]
        {
            new Candidate(lowId, "low"),
            new Candidate(highId, "high"),
            new Candidate(missingId, "missing-from-stats")
        };

        var stats = new List<ProductionMasterGameYearStat>
        {
            new(lowId, "Low Hype Game", 1.5m),
            new(highId, "High Hype Game", 99.9m)
        };

        var result = ProductionGameStatsCache.SelectHighestHypeAvailable(
            candidates,
            c => c.MasterGameID,
            stats);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.MasterGameID, Is.EqualTo(highId));
    }
}
```

- [ ] **Step 2: Run tests to verify they fail to compile**

Run:

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~ProductionGameStatsCacheTests" --no-build
```

If the project hasn't been built since adding the test file, run build first:

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: BUILD FAILED — `ProductionGameStatsCache` does not exist yet.

- [ ] **Step 3: Commit test file**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/ProductionStats/ProductionGameStatsCacheTests.cs
git commit -m "Add unit tests for production game stats selection logic."
```

---

## Task 3: `ProductionGameStatsCache` — fetch, cache, and selection

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionGameStatsCache.cs`

- [ ] **Step 1: Implement the cache class**

Create `src/FantasyCritic.IntegrationTests/ProductionStats/ProductionGameStatsCache.cs`:

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

- [ ] **Step 2: Build and run unit tests**

Run:

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~ProductionGameStatsCacheTests"
```

Expected: 3 tests PASSED

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/ProductionStats/ProductionGameStatsCache.cs
git commit -m "Add ProductionGameStatsCache with hype-based selection strategy."
```

---

## Task 4: Wire into `RoyaleTests`

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs`

- [ ] **Step 1: Add using and pass year into helper**

At top of `RoyaleTests.cs`, add:

```csharp
using FantasyCritic.IntegrationTests.ProductionStats;
```

In `PurchaseGame_WhenGameIsAvailable_Succeeds`, change the call:

```csharp
var gameToBuy = await FindPurchasableGameViaApiAsync(session, publisherID, activeQuarter.Year);
```

- [ ] **Step 2: Update `FindPurchasableGameViaApiAsync`**

Replace the method signature and body:

```csharp
/// <summary>
/// Mirrors the purchase-game modal: list top games under each release filter until one is available.
/// Picks the highest-hype candidate using production stats when reachable.
/// </summary>
private static async Task<PossibleRoyaleMasterGameJson?> FindPurchasableGameViaApiAsync(
    ApiSession session,
    Guid publisherID,
    int year)
{
    foreach (var releaseFilter in new[] { "All", "ExpectedToReleaseInQuarter", "ConfirmedReleaseInQuarter" })
    {
        var possibleGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
            $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter={releaseFilter}");

        var available = possibleGames.Where(g => g.IsAvailable).ToList();
        if (available.Count > 0)
        {
            return await ProductionGameStatsCache.FindHighestHypeAvailableAsync(
                available,
                g => g.MasterGame.MasterGameID,
                year);
        }
    }

    return null;
}
```

- [ ] **Step 3: Build**

Run:

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/Royale/RoyaleTests.cs
git commit -m "Use production hype stats when picking Royale games in integration tests."
```

---

## Task 5: Full integration test verification

**Pre-flight:** MySQL Docker must be running with schema migrated and LocalDatabaseTool sync completed (same as existing integration test workflow):

```powershell
docker compose -f infrastructure/docker-compose-mysql.yaml up -d
```

Wait for `fantasycritic-database-updater` to exit 0. Run LocalDatabaseTool if master games are not yet synced locally.

- [ ] **Step 1: Run all integration tests**

Run:

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: All tests PASSED (including `PurchaseGame_WhenGameIsAvailable_Succeeds`). First run hits production once for the active year; subsequent tests in the same run reuse the in-memory cache.

- [ ] **Step 2: Confirm production fetch is non-fatal**

Optional sanity check — temporarily break network or use an invalid base URL in a local experiment, re-run `PurchaseGame_WhenGameIsAvailable_Succeeds`, confirm test still passes (falls back to first available candidate). Revert before committing anything from the experiment.

---

## Spec coverage checklist

| Spec requirement | Task |
|---|---|
| `ProductionMasterGameYearStat` record | Task 1 |
| In-memory per-year cache with `Lazy<Task<>>` | Task 3 |
| Hard-coded production base URL | Task 3 |
| Fetch via `GET /api/Game/MasterGameYear/{year}` | Task 3 |
| Graceful failure (warn + empty list) | Task 3 |
| `GetStatsForYearAsync` internal helper | Task 3 |
| `FindHighestHypeAvailableAsync` strategy | Task 3 |
| Future strategies as additional methods (documented, not implemented) | Key facts section |
| RoyaleTests still calls local API for candidates | Task 4 |
| Year passed by caller | Task 4 |
| No changes to factory/base/ApiSession | N/A (no tasks) |

---

## Out of scope (do not implement)

- File-based cache persistence
- Authenticated production API calls
- Additional strategy methods (`FindMostPickedAvailableAsync`, etc.)
- Changes to LocalDatabaseTool or production code
