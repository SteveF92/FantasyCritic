namespace FantasyCritic.Lib.Jobs.Handlers;

internal class FullDataRefreshJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.FullDataRefresh;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Cron("0 */2 * * *");

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
