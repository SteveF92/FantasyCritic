using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendPublicBiddingEmailsJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendPublicBiddingEmails;

    private readonly InterLeagueService _interLeagueService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly EmailSendingService _emailSendingService;

    public SendPublicBiddingEmailsJobHandler(InterLeagueService interLeagueService,
        GameAcquisitionService gameAcquisitionService, EmailSendingService emailSendingService)
    {
        _interLeagueService = interLeagueService;
        _gameAcquisitionService = gameAcquisitionService;
        _emailSendingService = emailSendingService;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var publicBiddingSets = await PublicBiddingJobUtilities.GetPublicBiddingSets(_interLeagueService, _gameAcquisitionService);
        await _emailSendingService.SendPublicBidEmails(publicBiddingSets);
        return Result.Success();
    }
}
