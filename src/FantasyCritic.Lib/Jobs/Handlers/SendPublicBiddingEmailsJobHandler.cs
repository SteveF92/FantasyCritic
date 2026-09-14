using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendPublicBiddingEmailsJobHandler : IFantasyCriticCronJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly EmailSendingService _emailSendingService;

    public SendPublicBiddingEmailsJobHandler(InterLeagueService interLeagueService, GameAcquisitionService gameAcquisitionService, EmailSendingService emailSendingService)
    {
        _interLeagueService = interLeagueService;
        _gameAcquisitionService = gameAcquisitionService;
        _emailSendingService = emailSendingService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendPublicBiddingEmails;
    public static FantasyCriticJobSchedule Schedule { get; } =
        FantasyCriticJobSchedule.Weekly(TimeExtensions.PublicBiddingRevealDay, TimeExtensions.PublicBiddingRevealTime);

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var publicBiddingSets = await PublicBiddingSets.GetForActiveYears(_interLeagueService, _gameAcquisitionService);
        await _emailSendingService.SendPublicBidEmails(publicBiddingSets);
        return Result.Success();
    }
}
