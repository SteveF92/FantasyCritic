namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshCriticScoresJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshCriticScores;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
