namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateTopBidsAndDropsJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateTopBidsAndDrops;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
