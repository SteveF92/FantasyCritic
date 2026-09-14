using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendReleasingThisWeekUpdateJobHandler : IFantasyCriticCronJobHandler
{
    private const int MostHypedGamesToInclude = 10;

    private readonly InterLeagueService _interLeagueService;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;

    public SendReleasingThisWeekUpdateJobHandler(InterLeagueService interLeagueService, DiscordPushService discordPushService, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _discordPushService = discordPushService;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendReleasingThisWeekUpdate;
    public static FantasyCriticJobSchedule Schedule { get; } =
        FantasyCriticJobSchedule.Weekly(TimeExtensions.ReleasingThisWeekNewsDay, TimeExtensions.ReleasingThisWeekNewsTime);

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var today = _clock.GetToday();
        var endOfWeek = today.PlusWeeks(1);

        var allGames = await _interLeagueService.GetMasterGameYears(today.Year);
        var mostHypedGames = allGames
            .Where(x => x.MasterGame.ReleaseDate.HasValue && x.MasterGame.ReleaseDate.Value > today && x.MasterGame.ReleaseDate.Value <= endOfWeek)
            .OrderByDescending(x => x.HypeFactor)
            .Take(MostHypedGamesToInclude)
            .ToList();

        await _discordPushService.SendReleasingThisWeekUpdate(mostHypedGames, today.Year);
        return Result.Success();
    }
}
