using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendPublicBiddingDiscordMessagesJobHandler : IFantasyCriticJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendPublicBiddingDiscordMessages;

    private readonly InterLeagueService _interLeagueService;
    private readonly DiscordPushService _discordPushService;
    private readonly GameAcquisitionService _gameAcquisitionService;

    public SendPublicBiddingDiscordMessagesJobHandler(InterLeagueService interLeagueService,
        DiscordPushService discordPushService, GameAcquisitionService gameAcquisitionService)
    {
        _interLeagueService = interLeagueService;
        _discordPushService = discordPushService;
        _gameAcquisitionService = gameAcquisitionService;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var publicBiddingSets = await PublicBiddingJobUtilities.GetPublicBiddingSets(_interLeagueService, _gameAcquisitionService);
        await _discordPushService.SendPublicBiddingSummary(publicBiddingSets);
        return Result.Success();
    }
}
