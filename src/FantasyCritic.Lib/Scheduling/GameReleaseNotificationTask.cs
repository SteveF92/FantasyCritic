using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FantasyCritic.Lib.Scheduling;

public class GameReleaseNotificationTask : IScheduledTask
{
    private static readonly ILogger _logger = Log.ForContext<GameReleaseNotificationTask>();
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "0 */1 * * *";

    public GameReleaseNotificationTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.Information("About to run Master Game Release Push");
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var now = clock.GetCurrentInstant();
        var isTimeToNotify = IsTimeToNotify(now);
        if (!isTimeToNotify)
        {
            _logger.Information("Not time to run Master Game Release Push");
            return;
        }

        _logger.Information("Time to run Master Game Release Push");
        var interLeagueService = scope.ServiceProvider.GetRequiredService<InterLeagueService>();
        var discordPushService = scope.ServiceProvider.GetRequiredService<DiscordPushService>();

        var easternDate = now.ToEasternDate();
        var allMasterGames = await interLeagueService.GetMasterGameYears(easternDate.Year);
        var masterGamesReleasingToday = allMasterGames.Where(x => x.MasterGame.ReleaseDate.HasValue && x.MasterGame.ReleaseDate.Value == easternDate).ToList();
        if (!masterGamesReleasingToday.Any())
        {
            _logger.Information("No games for Master Game Release Push");
            return;
        }

        _logger.Information("{masterGamesReleasingTodayCount} games for Master Game Release Push", masterGamesReleasingToday.Count);
        await discordPushService.SendGameReleaseUpdates(masterGamesReleasingToday);
    }

    private static bool IsTimeToNotify(Instant now)
    {
        var nycNow = now.InZone(TimeExtensions.EasternTimeZone);

        var timeOfDay = nycNow.TimeOfDay;
        var earliestTimeToSend = LocalTime.Midnight.Minus(Period.FromMinutes(1));
        var latestTimeToSend = LocalTime.Midnight.Plus(Period.FromMinutes(1));
        bool isTimeToNotify = timeOfDay > earliestTimeToSend && timeOfDay < latestTimeToSend;
        return isTimeToNotify;
    }
}

