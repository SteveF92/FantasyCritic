namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PrepareForActionProcessingJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.PrepareForActionProcessing;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
