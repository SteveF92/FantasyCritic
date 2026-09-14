namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessActionsJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessActions;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
