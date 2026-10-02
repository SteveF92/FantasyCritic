using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class TopBidsAndDropsUpdater
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly ILogger<TopBidsAndDropsUpdater> _logger;

    public TopBidsAndDropsUpdater(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, ILogger<TopBidsAndDropsUpdater> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _logger = logger;
    }

    public async Task UpdateTopBidsAndDropsForMostRecentWeek(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();
        var weeks = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets);
        if (weeks.Count == 0)
        {
            _logger.LogDebug("No processed weeks to update top bids and drops for.");
            await context.AppendDetailedStatus("Top bids and drops: no processed weeks.");
            return;
        }

        var mostRecentWeek = weeks.Last();
        await UpdateTopBidsAndDropsForWeek(mostRecentWeek, context, cancellationToken);
    }

    private async Task UpdateTopBidsAndDropsForWeek(ActionProcessingWeek week, FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var existingProcessDates = await _masterGameRepo.GetProcessingDatesForTopBidsAndDrops();
        if (existingProcessDates.Contains(week.ProcessDate))
        {
            _logger.LogDebug("Top bids and drops for {ProcessDate} already done.", week.ProcessDate);
            await context.AppendDetailedStatus($"Top bids and drops: {week.ProcessDate.ToISOString()} already done.");
            return;
        }

        var bidsAndDrops = await _fantasyCriticRepo.GetPickupBidsAndDropsForProcessingSets(week.ProcessingSets);
        var yearsInGroup = bidsAndDrops.Bids.Select(x => x.LeagueYear.Key.Year).Concat(bidsAndDrops.Drops.Select(x => x.LeagueYear.Key.Year)).Distinct().ToList();

        var allMasterGameYears = new List<MasterGameYear>();
        foreach (var year in yearsInGroup)
        {
            var masterGameYears = await _masterGameRepo.GetMasterGameYears(year);
            allMasterGameYears.AddRange(masterGameYears);
        }

        var topBidsAndDrops = TopBidsAndDropsFunctions.CalculateTopBidsAndDrops(week.ProcessDate, bidsAndDrops, yearsInGroup, allMasterGameYears);
        cancellationToken.ThrowIfCancellationRequested();
        await _fantasyCriticRepo.InsertTopBidsAndDrops(topBidsAndDrops);

        _logger.LogInformation("Updated top bids and drops for {ProcessDate}: {GameCount} games.", week.ProcessDate, topBidsAndDrops.Count);
        await context.AppendDetailedStatus($"Top bids and drops: {topBidsAndDrops.Count} games for {week.ProcessDate.ToISOString()}.");
    }
}
