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

    private sealed class MasterGameYearApiJson
    {
        public Guid MasterGameID { get; set; }
        public string GameName { get; set; } = "";
        public double HypeFactor { get; set; }
    }
}
