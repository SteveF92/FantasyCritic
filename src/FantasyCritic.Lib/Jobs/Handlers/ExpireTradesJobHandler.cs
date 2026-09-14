namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ExpireTradesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ExpireTrades;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
