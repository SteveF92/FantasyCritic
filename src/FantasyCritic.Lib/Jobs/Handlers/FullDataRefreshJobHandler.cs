using FantasyCritic.Lib.Jobs.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class FullDataRefreshJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.FullDataRefresh;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTwoHours;

    private readonly FullDataRefresher _fullDataRefresher;
    private readonly ILogger<FullDataRefreshJobHandler> _logger;

    public FullDataRefreshJobHandler(FullDataRefresher fullDataRefresher, ILogger<FullDataRefreshJobHandler> logger)
    {
        _fullDataRefresher = fullDataRefresher;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _fullDataRefresher.FullDataRefresh();
        return Result.Success();
    }
}
