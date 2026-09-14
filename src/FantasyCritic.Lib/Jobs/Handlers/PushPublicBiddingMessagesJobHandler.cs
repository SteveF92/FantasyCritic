using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PushPublicBiddingMessagesJobHandler : IFantasyCriticCronJobHandler
{
    private readonly InterLeagueService _interLeagueService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly DiscordPushService _discordPushService;

    public PushPublicBiddingMessagesJobHandler(InterLeagueService interLeagueService, GameAcquisitionService gameAcquisitionService, DiscordPushService discordPushService)
    {
        _interLeagueService = interLeagueService;
        _gameAcquisitionService = gameAcquisitionService;
        _discordPushService = discordPushService;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.PushPublicBiddingMessages;
    public static FantasyCriticJobSchedule Schedule { get; } =
        FantasyCriticJobSchedule.Weekly(TimeExtensions.PublicBiddingRevealDay, TimeExtensions.PublicBiddingRevealTime);

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var publicBiddingSets = await PublicBiddingSets.GetForActiveYears(_interLeagueService, _gameAcquisitionService);
        await _discordPushService.SendPublicBiddingSummary(publicBiddingSets);
        return Result.Success();
    }
}
