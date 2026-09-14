namespace FantasyCritic.Lib.Jobs.Handlers;

internal class UpdateDailyPublisherStatisticsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.UpdateDailyPublisherStatistics;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Cron("0 22 * * *");

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
