using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs.Utilities;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendPublicBiddingDiscordMessagesJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendPublicBiddingDiscordMessages;

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly ILogger<SendPublicBiddingDiscordMessagesJobHandler> _logger;

    public SendPublicBiddingDiscordMessagesJobHandler(IFantasyCriticRepo fantasyCriticRepo,
        DiscordPushService discordPushService, GameAcquisitionService gameAcquisitionService, ILogger<SendPublicBiddingDiscordMessagesJobHandler> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _gameAcquisitionService = gameAcquisitionService;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var publicBiddingSets = await PublicBiddingJobUtilities.GetPublicBiddingSets(_fantasyCriticRepo, _gameAcquisitionService);
        cancellationToken.ThrowIfCancellationRequested();
        await _discordPushService.SendPublicBiddingSummary(publicBiddingSets);

        _logger.LogInformation("Pushed public bidding Discord messages for {LeagueCount} leagues.", publicBiddingSets.Count);
        await context.UpdateDetailedStatus(PublicBiddingJobUtilities.DescribeLeagues(publicBiddingSets));
        return Result.Success();
    }
}
