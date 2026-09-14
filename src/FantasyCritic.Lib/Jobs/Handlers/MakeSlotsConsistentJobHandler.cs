namespace FantasyCritic.Lib.Jobs.Handlers;

internal class MakeSlotsConsistentJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.MakeSlotsConsistent;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
