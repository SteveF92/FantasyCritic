namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshCachesJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshCaches;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
