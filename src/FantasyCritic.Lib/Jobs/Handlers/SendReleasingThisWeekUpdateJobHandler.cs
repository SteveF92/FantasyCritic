using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendReleasingThisWeekUpdateJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendReleasingThisWeekUpdate;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(TimeExtensions.ReleasingThisWeekNewsDay, TimeExtensions.ReleasingThisWeekNewsTime);

    private readonly IMasterGameRepo _masterGameRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<SendReleasingThisWeekUpdateJobHandler> _logger;

    public SendReleasingThisWeekUpdateJobHandler(IMasterGameRepo masterGameRepo, DiscordPushService discordPushService,
        IClock clock, ILogger<SendReleasingThisWeekUpdateJobHandler> logger)
    {
        _masterGameRepo = masterGameRepo;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var today = _clock.GetToday();
        var upcomingGames = await GetUpcomingGames(today);
        var year = today.Year;
        cancellationToken.ThrowIfCancellationRequested();
        await _discordPushService.SendReleasingThisWeekUpdate(upcomingGames, year);

        var throughDate = today.PlusWeeks(1).ToISOString();
        _logger.LogInformation("Pushed the releasing this week update: the {GameCount} most hyped games releasing through {ThroughDate}.", upcomingGames.Count, throughDate);
        await context.UpdateDetailedStatus($"The {upcomingGames.Count} most hyped games releasing through {throughDate}.");
        return Result.Success();
    }

    private async Task<IReadOnlyList<MasterGameYear>> GetUpcomingGames(LocalDate today)
    {
        var year = today.Year;

        var allGames = await _masterGameRepo.GetMasterGameYears(year);
        var thisWeekGames = allGames.Where(g =>
            g.MasterGame.ReleaseDate.HasValue &&
            g.MasterGame.ReleaseDate.Value > today &&
            g.MasterGame.ReleaseDate.Value <= today.PlusWeeks(1));
        var mostHypedGames = thisWeekGames.OrderByDescending(g => g.HypeFactor).Take(10).ToList();

        return mostHypedGames;
    }
}
