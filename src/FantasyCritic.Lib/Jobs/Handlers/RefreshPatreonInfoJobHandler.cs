namespace FantasyCritic.Lib.Jobs.Handlers;

internal class RefreshPatreonInfoJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.RefreshPatreonInfo;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
