using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendReleasingThisWeekUpdateJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendReleasingThisWeekUpdate;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(TimeExtensions.ReleasingThisWeekNewsDay, TimeExtensions.ReleasingThisWeekNewsTime);

    private readonly InterLeagueService _interLeagueService;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<SendReleasingThisWeekUpdateJobHandler> _logger;

    public SendReleasingThisWeekUpdateJobHandler(InterLeagueService interLeagueService, DiscordPushService discordPushService,
        IClock clock, ILogger<SendReleasingThisWeekUpdateJobHandler> logger)
    {
        _interLeagueService = interLeagueService;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var today = _clock.GetToday();
        var upcomingGames = await GetUpcomingGames(today);
        var year = today.Year;
        await _discordPushService.SendReleasingThisWeekUpdate(upcomingGames, year);
        return Result.Success();
    }

    private async Task<IReadOnlyList<MasterGameYear>> GetUpcomingGames(LocalDate today)
    {
        var year = today.Year;

        var allGames = await _interLeagueService.GetMasterGameYears(year);
        var thisWeekGames = allGames.Where(g =>
            g.MasterGame.ReleaseDate.HasValue &&
            g.MasterGame.ReleaseDate.Value > today &&
            g.MasterGame.ReleaseDate.Value <= today.PlusWeeks(1));
        var mostHypedGames = thisWeekGames.OrderByDescending(g => g.HypeFactor).Take(10).ToList();

        return mostHypedGames;
    }
}
