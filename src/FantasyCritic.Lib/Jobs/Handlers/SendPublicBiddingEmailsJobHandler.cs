using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendPublicBiddingEmailsJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendPublicBiddingEmails;
    public static FantasyCriticJobSchedule Schedule { get; } =
        FantasyCriticJobSchedule.Weekly(TimeExtensions.PublicBiddingRevealDay, TimeExtensions.PublicBiddingRevealTime);

    public Task Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
