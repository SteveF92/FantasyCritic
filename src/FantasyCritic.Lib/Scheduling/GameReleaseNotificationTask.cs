using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyCritic.Lib.Scheduling;

public class GameReleaseNotificationTask : IScheduledTask
{
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "0 */1 * * *";

    public GameReleaseNotificationTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var now = clock.GetCurrentInstant();
        var isTimeToNotify = IsTimeToNotify(now);
        if (!isTimeToNotify)
        {
            return;
        }

        var interLeagueService = scope.ServiceProvider.GetRequiredService<InterLeagueService>();
        var discordPushService = scope.ServiceProvider.GetRequiredService<DiscordPushService>();

        var easternDate = now.ToEasternDate();
        var allMasterGames = await interLeagueService.GetMasterGameYears(easternDate.Year);
        var masterGamesReleasingToday = allMasterGames.Where(x => x.MasterGame.ReleaseDate == easternDate).ToList();
        if (!masterGamesReleasingToday.Any())
        {
            return;
        }

        await discordPushService.SendGameReleaseUpdates(masterGamesReleasingToday, easternDate.Year);
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

