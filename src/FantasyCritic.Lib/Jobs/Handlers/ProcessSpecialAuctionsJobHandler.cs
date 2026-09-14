namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessSpecialAuctionsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessSpecialAuctions;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.EveryTenMinutes;

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
