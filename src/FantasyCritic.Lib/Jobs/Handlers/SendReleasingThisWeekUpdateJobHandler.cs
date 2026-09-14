using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendReleasingThisWeekUpdateJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendReleasingThisWeekUpdate;
    public static FantasyCriticJobSchedule Schedule { get; } =
        FantasyCriticJobSchedule.Weekly(TimeExtensions.ReleasingThisWeekNewsDay, TimeExtensions.ReleasingThisWeekNewsTime);

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
