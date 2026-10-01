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

    public Task UpdateTopBidsAndDropsForMostRecentWeek()
    {
        return Task.CompletedTask;
    }

    private Task UpdateTopBidsAndDropsForWeek(ActionProcessingWeek week)
    {
        return Task.CompletedTask;
    }
}
