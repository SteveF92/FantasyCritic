# Production Game Stats Cache — Design Spec

**Date:** 2026-05-31  
**Status:** Approved

---

## Problem

Integration tests always start from a blank database. When a test needs to pick a game (Royale purchase, league draft, etc.), it currently takes the first `IsAvailable == true` result from the local API — an arbitrary choice that has nothing to do with real player behaviour. As the test suite grows, many fixtures will need to pick games, and picking a high-quality, high-demand game is a better simulation of real usage.

The production Fantasy Critic API (`https://www.fantasycritic.games`) has exactly this signal: per-year master game stats including `HypeFactor`, which measures predicted community interest regardless of how much real pick data exists yet — the same situation integration tests are always in.

---

## Goal

Build a lightweight, in-memory, test-only utility that:

1. Calls the production API once per year per test run to fetch master game stats.
2. Exposes named selection-strategy methods that cross-reference a caller-supplied list of locally-available game candidates against those stats and return the best match.
3. Works generically across Royale and league scenarios.
4. Fails gracefully when production is unreachable (falls back to first candidate; no test breakage).

---

## New Files

```
src/FantasyCritic.IntegrationTests/
  ProductionStats/
    ProductionGameStatsCache.cs      ← cache + strategy methods
    ProductionMasterGameYearStat.cs  ← internal data record
```

No other projects are touched. No new NuGet dependencies (Newtonsoft.Json + NodaTime are already referenced by `FantasyCritic.IntegrationTests`).

---

## `ProductionMasterGameYearStat`

A minimal internal record containing only the fields selection strategies need:

```csharp
internal sealed record ProductionMasterGameYearStat(
    Guid   MasterGameID,
    string GameName,
    decimal HypeFactor);
```

Mapped from the `GET /api/Game/MasterGameYear/{year}` JSON response. All other fields in the response are ignored.

---

## `ProductionGameStatsCache`

### State

```csharp
private static readonly ConcurrentDictionary<int, Lazy<Task<IReadOnlyList<ProductionMasterGameYearStat>>>>
    _cache = new();
```

Using `Lazy<Task<T>>` ensures that concurrent first-callers for the same year issue exactly one HTTP request. The result is stored forever for the lifetime of the test process (in-memory only, no file persistence).

### Base URL

Read from configuration in priority order:

1. `appsettings.Testing.Local.json` key `"ProductionApiBaseUrl"`
2. `appsettings.Testing.json` key `"ProductionApiBaseUrl"`
3. Hard-coded default: `"https://www.fantasycritic.games"`

This mirrors the `LocalDatabaseTool` `baseAddress` pattern and allows overriding in unusual CI environments.

### Fetch

```
GET {baseUrl}/api/Game/MasterGameYear/{year}
```

- Plain `HttpClient` (no auth, no Polly — same as `LocalDatabaseTool`).
- Deserialised with `JsonConvert.DeserializeObject` + `ConfigureForNodaTime`.
- On any exception (network error, non-2xx status, deserialisation failure): log a warning via `Console.Error`, return an empty list. The test run continues.

### Internal helper

```csharp
internal static Task<IReadOnlyList<ProductionMasterGameYearStat>> GetStatsForYearAsync(int year)
```

`internal` so tests can access raw stats directly if a future strategy needs it without duplicating the fetch logic.

---

## Selection Strategies

All strategy methods share the same signature shape:

```csharp
public static async Task<T?> Find___AvailableAsync<T>(
    IEnumerable<T> localCandidates,
    Func<T, Guid>  masterGameIdSelector,
    int            year)
```

Callers:
- Still call the local API themselves to get their candidate list (and apply their own `IsAvailable` / release-filter logic — unchanged from today).
- Pass the filtered candidates plus the ID selector and year.
- Receive the "best" candidate by the chosen strategy, or `null` if the candidate list is empty.

### `FindHighestHypeAvailableAsync` (initial implementation)

Algorithm:
1. Await `GetStatsForYearAsync(year)`.
2. Build `Dictionary<Guid, decimal> hypeLookup` from the stats.
3. Return `localCandidates.MaxBy(c => hypeLookup.GetValueOrDefault(masterGameIdSelector(c), 0m))`.
4. If stats are empty (fallback path), `MaxBy` still returns the first candidate by natural order — same behaviour as today.

### Future strategies (not implemented now)

Additional methods will be added to the same class as new test scenarios require them. Candidates include:

- `FindMostPickedAvailableAsync` — sort by `EligiblePercentStandardGame`
- `FindCheapestAvailableAsync` — sort by Royale cost (caller would pass cost in the candidate type)
- `FindHighestRatedAvailableAsync` — sort by OpenCritic score once games have released

Each new strategy is a standalone `public static async Task<T?>` method that calls `GetStatsForYearAsync` and applies its own ordering. No interface or strategy-pattern abstraction is needed at this stage.

---

## Changes to `RoyaleTests`

`FindPurchasableGameViaApiAsync` is updated (not replaced):

**Before:**
```csharp
var available = possibleGames.FirstOrDefault(g => g.IsAvailable);
if (available is not null) return available;
```

**After:**
```csharp
var available = possibleGames.Where(g => g.IsAvailable).ToList();
if (available.Count > 0)
    return await ProductionGameStatsCache.FindHighestHypeAvailableAsync(
        available, g => g.MasterGame.MasterGameID, year);
```

`year` is passed in from `activeQuarter.Year` (already present in the test body). The rest of the test — publisher creation, the purchase POST, the roster assertion — is untouched.

---

## What Is Not Changed

- `IntegrationTestBase` — no modifications.
- `FantasyCriticWebApplicationFactory` — no modifications.
- `ApiSession` — no modifications.
- All existing `ApiModels/` types — no modifications.
- No production or domain code is touched.

---

## Error Handling Summary

| Failure mode | Behaviour |
|---|---|
| Production unreachable (network) | Warning to `Console.Error`; empty stats; test uses first candidate |
| Non-2xx HTTP response | Same as above |
| JSON deserialisation failure | Same as above |
| `localCandidates` is empty | Returns `null` (same as before) |
| Game in candidates has no production stats entry | `HypeFactor` treated as `0`; game sorts to the bottom |

---

## Out of Scope

- File-based cache persistence across runs.
- Authenticated production API calls.
- Changes to `LocalDatabaseTool`.
- Any production or domain code.
