using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class TopBidsAndDropsFunctions
{
    public static bool IsBidDropProcessingWeekEnd(ActionProcessingSetMetadata actionProcessingSet) =>
        actionProcessingSet.ProcessName.StartsWith("Drop/Bid Processing")
        || actionProcessingSet.ProcessName.StartsWith("Drop Processing")
        || actionProcessingSet.ProcessName.StartsWith("Bid Processing");

    public static IReadOnlyList<ActionProcessingWeek> GetActionProcessingWeeks(IReadOnlyList<ActionProcessingSetMetadata> actionProcessingSets)
    {
        var weekEndingSets = actionProcessingSets
            .Where(IsBidDropProcessingWeekEnd)
            .OrderBy(x => x.ProcessTime)
            .ToList();

        List<ActionProcessingWeek> weeks = [];
        for (var index = 0; index < weekEndingSets.Count; index++)
        {
            var weekEndingSet = weekEndingSets[index];
            Instant startRange = index > 0 ? weekEndingSets[index - 1].ProcessTime : Instant.MinValue;
            Instant endRange = weekEndingSet.ProcessTime;

            var processingSetsToInclude = actionProcessingSets
                .Where(x => x.ProcessTime > startRange && x.ProcessTime <= endRange)
                .ToList();

            weeks.Add(new ActionProcessingWeek(weekEndingSet.ProcessTime.ToEasternDate(), processingSetsToInclude));
        }

        // Multiple bid/drop week-ending runs on the same Eastern calendar day share one cache row
        // (ProcessDate, MasterGameID, Year). Merge their processing sets into a single week.
        return weeks
            .GroupBy(week => week.ProcessDate)
            .OrderBy(group => group.Key)
            .Select(group => new ActionProcessingWeek(
                group.Key,
                group.SelectMany(week => week.ProcessingSets)
                    .DistinctBy(set => set.ProcessSetID)
                    .OrderBy(set => set.ProcessTime)
                    .ToList()))
            .ToList();
    }

    public static IReadOnlyList<TopBidsAndDropsGame> CalculateTopBidsAndDrops(LocalDate processDate, BidsAndDropsSet bidsAndDrops, IEnumerable<int> relevantYears, IReadOnlyList<MasterGameYear> masterGameYears)
    {
        List<TopBidsAndDropsGame> results = [];
        foreach (var year in relevantYears)
        {
            var masterGameYearDictionaryForYear = masterGameYears.Where(x => x.Year == year).ToDictionary(x => x.MasterGame);
            var standardBidsForYear = bidsAndDrops.Bids.Where(x => !x.CounterPick && x.LeagueYear.Year == year).ToList();
            var counterPickBidsForYear = bidsAndDrops.Bids.Where(x => x.CounterPick && x.LeagueYear.Year == year).ToList();
            var dropsForYear = bidsAndDrops.Drops.Where(x => x.LeagueYear.Year == year).ToList();

            var standardBidsByMasterGame = standardBidsForYear.GroupToDictionary(x => x.MasterGame);
            var counterPickBidsByMasterGame = counterPickBidsForYear.GroupToDictionary(x => x.MasterGame);
            var dropsByMasterGame = dropsForYear.GroupToDictionary(x => x.MasterGame);
            var allMasterGames = standardBidsByMasterGame.Keys.Concat(counterPickBidsByMasterGame.Keys).Concat(dropsByMasterGame.Keys).Distinct().ToList();

            foreach (var masterGame in allMasterGames)
            {
                var standardBidsForMasterGame = standardBidsByMasterGame.GetValueOrDefault(masterGame, new List<PickupBid>());
                var counterPickBidsForMasterGame = counterPickBidsByMasterGame.GetValueOrDefault(masterGame, new List<PickupBid>());
                var dropsForMasterGame = dropsByMasterGame.GetValueOrDefault(masterGame, new List<DropRequest>());
                var masterGameYear = masterGameYearDictionaryForYear[masterGame];

                var result = new TopBidsAndDropsGame
                {
                    ProcessDate = processDate,
                    MasterGameYear = masterGameYear,

                    TotalStandardBidCount = standardBidsForMasterGame.Count,
                    SuccessfulStandardBids = standardBidsForMasterGame.Count(x => x.Successful.HasValue && x.Successful.Value),
                    FailedStandardBids = standardBidsForMasterGame.Count(x => x.Successful.HasValue && !x.Successful.Value),
                    TotalStandardBidLeagues = standardBidsForMasterGame.Select(x => x.LeagueYear.Key).Distinct().Count(),
                    TotalStandardBidAmount = (int)standardBidsForMasterGame.Sum(x => x.BidAmount),

                    TotalCounterPickBidCount = counterPickBidsForMasterGame.Count,
                    SuccessfulCounterPickBids = counterPickBidsForMasterGame.Count(x => x.Successful.HasValue && x.Successful.Value),
                    FailedCounterPickBids = counterPickBidsForMasterGame.Count(x => x.Successful.HasValue && !x.Successful.Value),
                    TotalCounterPickBidLeagues = counterPickBidsForMasterGame.Select(x => x.LeagueYear.Key).Distinct().Count(),
                    TotalCounterPickBidAmount = (int)counterPickBidsForMasterGame.Sum(x => x.BidAmount),

                    TotalDropCount = dropsForMasterGame.Count,
                    SuccessfulDrops = dropsForMasterGame.Count(x => x.Successful.HasValue && x.Successful.Value),
                    FailedDrops = dropsForMasterGame.Count(x => x.Successful.HasValue && !x.Successful.Value),
                };

                results.Add(result);
            }
        }

        return results;
    }
}
