using FantasyCritic.Lib.Jobs.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateFantasyPointsJobHandler : IFantasyCriticJobHandler
{
    private readonly FantasyPointsUpdater _fantasyPointsUpdater;

    public UpdateFantasyPointsJobHandler(FantasyPointsUpdater fantasyPointsUpdater)
    {
        _fantasyPointsUpdater = fantasyPointsUpdater;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateFantasyPoints;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _fantasyPointsUpdater.UpdateFantasyPoints(context, cancellationToken);
        return Result.Success();
    }
}
