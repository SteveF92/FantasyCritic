namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SetTimeFlagsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SetTimeFlags;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Hourly;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
