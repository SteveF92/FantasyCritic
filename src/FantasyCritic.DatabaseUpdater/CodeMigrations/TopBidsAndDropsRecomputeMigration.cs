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
/// Recomputes tbl_caching_topbidsanddrops for every action processing week in the site's history.
///
/// This exists because <see cref="TopBidsAndDropsBackfillMigration"/> (and the live weekly job it mirrors,
/// <c>AdminService.UpdateTopBidsAndDropsForWeek</c>) didn't exclude test/custom-rules leagues from the
/// calculation, unlike the old ad-hoc FantasyCritic.DBUtility tool did for its original 2022-2024 backfill.
/// Now that <see cref="TopBidsAndDropsFunctions.CalculateTopBidsAndDrops"/> filters on <c>League.AffectsStats</c>,
/// this migration re-derives every week from scratch so the whole table is consistent.
///
/// Each week's old rows are deleted and its new rows inserted together in one transaction, rather than wiping
/// the whole table up front, so a partial failure only affects whichever week was mid-flight, not every week.
///
/// GetPickupBidsAndDropsForProcessingSets loads league years per-publisher-year internally; that lookup is
/// expensive (many queries) and, across the many weeks in a year, would otherwise redo that work for every
/// week. A cache shared across the loop (opt-in via the optional parameter) avoids redoing that work for years
/// already seen in an earlier week, without affecting the normal single-week call path used elsewhere.
/// </summary>
public class TopBidsAndDropsRecomputeMigration : IScript
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly ILogger _logger;

    public TopBidsAndDropsRecomputeMigration(RepositoryConfiguration repositoryConfiguration, ILogger logger)
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
        _logger.Information("Starting TopBidsAndDropsRecomputeMigration.");

        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();
        var weeks = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets);
        _logger.Information("Found {WeekCount} action processing weeks to recompute.", weeks.Count);

        if (weeks.Count == 0)
        {
            _logger.Information("No action processing weeks found; nothing to do.");
            return string.Empty;
        }

        Dictionary<int, IReadOnlyList<LeagueYear>> leagueYearCache = [];

        for (var weekIndex = 0; weekIndex < weeks.Count; weekIndex++)
        {
            var week = weeks[weekIndex];
            _logger.Information(
                "Recomputing week {WeekIndex}/{WeekCount}: {ProcessDate} ({ProcessingSetCount} processing sets)...",
                weekIndex + 1,
                weeks.Count,
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
            await _fantasyCriticRepo.ReplaceTopBidsAndDropsForProcessDate(week.ProcessDate, topBidsAndDrops);

            _logger.Information(
                "Replaced {GameCount} top bids and drops rows for {ProcessDate} ({BidCount} bids, {DropCount} drops).",
                topBidsAndDrops.Count,
                week.ProcessDate,
                bidsAndDrops.Bids.Count,
                bidsAndDrops.Drops.Count);
        }

        _logger.Information("TopBidsAndDropsRecomputeMigration completed successfully. Recomputed {WeekCount} weeks.", weeks.Count);
        return string.Empty;
    }
}
