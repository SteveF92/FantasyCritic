using System.Data;
using DbUp.Engine;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL;

namespace FantasyCritic.DatabaseUpdater.CodeMigrations;

/// <summary>
/// Backfills tbl_caching_topbidsanddrops for action processing weeks that do not yet have cached data.
/// </summary>
public class TopBidsAndDropsBackfillMigration : IScript
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;

    public TopBidsAndDropsBackfillMigration(RepositoryConfiguration repositoryConfiguration)
    {
        IFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(repositoryConfiguration);
        _masterGameRepo = new MySQLMasterGameRepo(repositoryConfiguration, userStore, repositoryConfiguration.Clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(repositoryConfiguration, userStore);
        _fantasyCriticRepo = new MySQLFantasyCriticRepo(repositoryConfiguration, userStore, _masterGameRepo, combinedDataRepo);
    }

    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        return ProvideScriptAsync().GetAwaiter().GetResult();
    }

    private async Task<string> ProvideScriptAsync()
    {
        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();
        var weeks = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets);
        var existingProcessDates = (await _masterGameRepo.GetProcessingDatesForTopBidsAndDrops()).ToHashSet();
        var weeksToBackfill = weeks.Where(week => !existingProcessDates.Contains(week.ProcessDate)).ToList();

        if (weeksToBackfill.Count == 0)
        {
            return string.Empty;
        }

        foreach (var week in weeksToBackfill)
        {
            var bidsAndDrops = await _fantasyCriticRepo.GetPickupBidsAndDropsForProcessingSets(week.ProcessingSets);
            var yearsInGroup = bidsAndDrops.Bids.Select(x => x.LeagueYear.Key.Year).Concat(bidsAndDrops.Drops.Select(x => x.LeagueYear.Key.Year)).Distinct().ToList();

            var allMasterGameYears = new List<MasterGameYear>();
            foreach (var year in yearsInGroup)
            {
                var masterGameYears = await _masterGameRepo.GetMasterGameYears(year);
                allMasterGameYears.AddRange(masterGameYears);
            }

            var topBidsAndDrops = TopBidsAndDropsFunctions.CalculateTopBidsAndDrops(week.ProcessDate, bidsAndDrops, yearsInGroup, allMasterGameYears);
            await _fantasyCriticRepo.InsertTopBidsAndDrops(topBidsAndDrops);
        }

        return string.Empty;
    }
}
