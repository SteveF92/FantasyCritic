using System.Data;
using DbUp.Engine;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL;
using NodaTime;
using Serilog;

namespace FantasyCritic.DatabaseUpdater.CodeMigrations;

/// <summary>
/// Backfills tbl_caching_topbidsanddrops for action processing weeks that do not yet have cached data.
/// </summary>
public class TopBidsAndDropsBackfillMigration : IScript
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly ILogger _logger;

    public TopBidsAndDropsBackfillMigration(RepositoryConfiguration repositoryConfiguration, ILogger logger)
    {
        _logger = logger;

        var longRunningConfig = repositoryConfiguration with
        {
            ConnectionString = ConnectionStringUtilities.WithDefaultCommandTimeout(
                repositoryConfiguration.ConnectionString,
                Duration.FromMinutes(10))
        };

        IFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(longRunningConfig);
        _masterGameRepo = new MySQLMasterGameRepo(longRunningConfig, userStore, longRunningConfig.Clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(longRunningConfig, userStore);
        _fantasyCriticRepo = new MySQLFantasyCriticRepo(longRunningConfig, userStore, _masterGameRepo, combinedDataRepo);
    }

    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        return ProvideScriptAsync().GetAwaiter().GetResult();
    }

    private async Task<string> ProvideScriptAsync()
    {
        _logger.Information("Starting TopBidsAndDropsBackfillMigration.");

        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();
        var weeks = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets);
        var existingProcessDates = (await _masterGameRepo.GetProcessingDatesForTopBidsAndDrops()).ToHashSet();
        var weeksToBackfill = weeks.Where(week => !existingProcessDates.Contains(week.ProcessDate)).ToList();

        _logger.Information(
            "Found {TotalWeekCount} action processing weeks; {ExistingWeekCount} already cached, {WeeksToBackfillCount} to backfill.",
            weeks.Count,
            existingProcessDates.Count,
            weeksToBackfill.Count);

        if (weeksToBackfill.Count == 0)
        {
            _logger.Information("All action processing weeks already have top bids and drops; nothing to do.");
            return string.Empty;
        }

        // GetPickupBidsAndDropsForProcessingSets loads league years per-publisher-year internally; that lookup
        // is expensive (many queries) and, across dozens/hundreds of weeks in this backfill, is repeatedly done
        // for the same years. Sharing a cache across the loop (opt-in via the optional parameter) avoids redoing
        // that work for years already seen in an earlier week, without affecting the normal single-week call path.
        Dictionary<int, IReadOnlyList<LeagueYear>> leagueYearCache = [];

        for (var weekIndex = 0; weekIndex < weeksToBackfill.Count; weekIndex++)
        {
            var week = weeksToBackfill[weekIndex];
            _logger.Information(
                "Backfilling week {WeekIndex}/{WeekCount}: {ProcessDate} ({ProcessingSetCount} processing sets)...",
                weekIndex + 1,
                weeksToBackfill.Count,
                week.ProcessDate,
                week.ProcessingSets.Count);

            var bidsAndDrops = await _fantasyCriticRepo.GetPickupBidsAndDropsForProcessingSets(week.ProcessingSets, leagueYearCache);
            var yearsInGroup = bidsAndDrops.Bids.Select(x => x.LeagueYear.Key.Year).Concat(bidsAndDrops.Drops.Select(x => x.LeagueYear.Key.Year)).Distinct().ToList();

            var allMasterGameYears = new List<MasterGameYear>();
            foreach (var year in yearsInGroup)
            {
                var masterGameYears = await _masterGameRepo.GetMasterGameYears(year);
                allMasterGameYears.AddRange(masterGameYears);
            }

            var topBidsAndDrops = TopBidsAndDropsFunctions.CalculateTopBidsAndDrops(week.ProcessDate, bidsAndDrops, yearsInGroup, allMasterGameYears);
            await _fantasyCriticRepo.InsertTopBidsAndDrops(topBidsAndDrops);
            existingProcessDates.Add(week.ProcessDate);

            _logger.Information(
                "Inserted {GameCount} top bids and drops rows for {ProcessDate} ({BidCount} bids, {DropCount} drops).",
                topBidsAndDrops.Count,
                week.ProcessDate,
                bidsAndDrops.Bids.Count,
                bidsAndDrops.Drops.Count);
        }

        _logger.Information("TopBidsAndDropsBackfillMigration completed successfully. Backfilled {WeekCount} weeks.", weeksToBackfill.Count);
        return string.Empty;
    }
}
