namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PushGameReleaseMessagesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.PushGameReleaseMessages;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Cron("0 0 * * *");

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
