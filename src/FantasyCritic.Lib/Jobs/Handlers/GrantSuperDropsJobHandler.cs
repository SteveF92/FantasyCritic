namespace FantasyCritic.Lib.Jobs.Handlers;

internal class GrantSuperDropsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.GrantSuperDrops;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
