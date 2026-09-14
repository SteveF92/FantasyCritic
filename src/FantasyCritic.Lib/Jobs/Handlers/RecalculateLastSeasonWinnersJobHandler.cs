namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RecalculateLastSeasonWinnersJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RecalculateLastSeasonWinners;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
