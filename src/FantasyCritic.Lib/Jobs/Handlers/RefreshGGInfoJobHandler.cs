namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshGGInfoJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshGGInfo;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
