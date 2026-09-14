namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateRoyaleWinnersJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateRoyaleWinners;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
