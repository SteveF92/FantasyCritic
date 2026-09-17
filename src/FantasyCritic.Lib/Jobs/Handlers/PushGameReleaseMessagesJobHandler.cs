using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PushGameReleaseMessagesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.PushGameReleaseMessages;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.AtOnePastMidnightEastern;

    private readonly InterLeagueService _interLeagueService;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<PushGameReleaseMessagesJobHandler> _logger;

    public PushGameReleaseMessagesJobHandler(InterLeagueService interLeagueService, DiscordPushService discordPushService,
        IClock clock, ILogger<PushGameReleaseMessagesJobHandler> logger)
    {
        _interLeagueService = interLeagueService;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("About to run Master Game Release Push");

        var today = _clock.GetToday();
        var allMasterGames = await _interLeagueService.GetMasterGameYears(today.Year);
        var masterGamesReleasingToday = allMasterGames.Where(x => x.MasterGame.ReleaseDate.HasValue && x.MasterGame.ReleaseDate.Value == today).ToList();
        if (!masterGamesReleasingToday.Any())
        {
            _logger.LogInformation("No games for Master Game Release Push");
            return Result.Success();
        }

        _logger.LogInformation("{masterGamesReleasingTodayCount} games for Master Game Release Push", masterGamesReleasingToday.Count);
        await _discordPushService.SendGameReleaseUpdates(masterGamesReleasingToday);
        return Result.Success();
    }
}
