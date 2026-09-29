using FantasyCritic.Lib.Jobs.Utilities;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshCachesJobHandler : IFantasyCriticJobHandler
{
    private readonly CacheRefresher _cacheRefresher;

    public RefreshCachesJobHandler(CacheRefresher cacheRefresher)
    {
        _cacheRefresher = cacheRefresher;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshCaches;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        await _cacheRefresher.RefreshCaches(context, cancellationToken);
        return Result.Success();
    }
}
