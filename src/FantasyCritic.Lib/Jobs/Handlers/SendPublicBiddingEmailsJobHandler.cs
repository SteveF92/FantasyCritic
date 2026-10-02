using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendPublicBiddingEmailsJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendPublicBiddingEmails;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.TimeCritical;

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly EmailSendingService _emailSendingService;
    private readonly ILogger<SendPublicBiddingEmailsJobHandler> _logger;

    public SendPublicBiddingEmailsJobHandler(IFantasyCriticRepo fantasyCriticRepo,
        GameAcquisitionService gameAcquisitionService, EmailSendingService emailSendingService, ILogger<SendPublicBiddingEmailsJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _gameAcquisitionService = gameAcquisitionService;
        _emailSendingService = emailSendingService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var publicBiddingSets = await PublicBiddingJobUtilities.GetPublicBiddingSets(_fantasyCriticRepo, _gameAcquisitionService);
        cancellationToken.ThrowIfCancellationRequested();
        await _emailSendingService.SendPublicBidEmails(publicBiddingSets);

        _logger.LogInformation("Sent public bidding emails for {LeagueCount} leagues.", publicBiddingSets.Count);
        await context.UpdateDetailedStatus(PublicBiddingJobUtilities.DescribeLeagues(publicBiddingSets));
        return Result.Success();
    }
}
