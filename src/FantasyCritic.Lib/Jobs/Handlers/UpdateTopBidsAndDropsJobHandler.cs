using FantasyCritic.Lib.Jobs.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateTopBidsAndDropsJobHandler : IFantasyCriticJobHandler
{
    private readonly TopBidsAndDropsUpdater _topBidsAndDropsUpdater;

    public UpdateTopBidsAndDropsJobHandler(TopBidsAndDropsUpdater topBidsAndDropsUpdater)
    {
        _topBidsAndDropsUpdater = topBidsAndDropsUpdater;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateTopBidsAndDrops;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _topBidsAndDropsUpdater.UpdateTopBidsAndDropsForMostRecentWeek();
        return Result.Success();
    }
}
