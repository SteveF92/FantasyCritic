using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Scheduling;

public class ReleasingThisWeekNotificationTask : IScheduledTask
{
    private readonly ILogger<ReleasingThisWeekNotificationTask> _logger;
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "0 */1 * * *";

    public ReleasingThisWeekNotificationTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<ReleasingThisWeekNotificationTask>>();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var now = clock.GetCurrentInstant();
        var isTimeToNotify = IsTimeToNotify(now);
        var year = now.InZone(TimeExtensions.EasternTimeZone).Year;

        if (!isTimeToNotify)
        {
            return;
        }

        var upcomingGames = await GetUpcomingGames(scope.ServiceProvider, now);

        var discordPushService = scope.ServiceProvider.GetRequiredService<DiscordPushService>();

        await FireAndForgetUtilities.FireAndForgetAsync(_logger, async () => await discordPushService.SendReleasingThisWeekUpdate(upcomingGames, year));
    }

    private static bool IsTimeToNotify(Instant now)
    {
        var nycNow = now.InZone(TimeExtensions.EasternTimeZone);
        var dayOfWeek = nycNow.DayOfWeek;
        var timeOfDay = nycNow.TimeOfDay;
        var earliestTimeToSet = TimeExtensions.ReleasingThisWeekNewsTime.Minus(Period.FromMinutes(1));
        var latestTimeToSet = TimeExtensions.ReleasingThisWeekNewsTime.Plus(Period.FromMinutes(1));
        bool isTimeToNotify = dayOfWeek == TimeExtensions.ReleasingThisWeekNewsDay && timeOfDay > earliestTimeToSet && timeOfDay < latestTimeToSet;
        return isTimeToNotify;
    }

    private static async Task<IReadOnlyList<MasterGameYear>> GetUpcomingGames(
        IServiceProvider serviceProvider, Instant now)
    {
        var year = now.InZone(TimeExtensions.EasternTimeZone).Year;
        var interLeagueService = serviceProvider.GetRequiredService<InterLeagueService>();

        var allGames = await interLeagueService.GetMasterGameYears(year);
        var thisWeekGames = allGames.Where(g =>
            g.MasterGame.ReleaseDate.HasValue && g.MasterGame.ReleaseDate.Value < now.ToEasternDate().PlusWeeks(1));
        var mostHypedGames = thisWeekGames.OrderByDescending(g => g.HypeFactor).Take(10).ToList();

        return mostHypedGames;
    }
}
