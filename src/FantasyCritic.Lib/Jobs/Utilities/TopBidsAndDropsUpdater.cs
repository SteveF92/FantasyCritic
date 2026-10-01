using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal class TopBidsAndDropsUpdater
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;

    public TopBidsAndDropsUpdater(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
    }

    public async Task UpdateTopBidsAndDropsForMostRecentWeek(CancellationToken cancellationToken)
    {
        var actionProcessingSets = await _fantasyCriticRepo.GetActionProcessingSets();
        var weeks = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets);
        if (weeks.Count == 0)
        {
            return;
        }

        var mostRecentWeek = weeks.Last();
        await UpdateTopBidsAndDropsForWeek(mostRecentWeek, cancellationToken);
    }

    private async Task UpdateTopBidsAndDropsForWeek(ActionProcessingWeek week, CancellationToken cancellationToken)
    {
        var existingProcessDates = await _masterGameRepo.GetProcessingDatesForTopBidsAndDrops();
        if (existingProcessDates.Contains(week.ProcessDate))
        {
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
    }
}
