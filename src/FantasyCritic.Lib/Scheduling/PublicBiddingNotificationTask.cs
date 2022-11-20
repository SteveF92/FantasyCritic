using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;

namespace FantasyCritic.Lib.Scheduling;

public class PublicBiddingNotificationTask : IScheduledTask
{
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "0 */1 * * *";

    public PublicBiddingNotificationTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine("Not sending emails - DEBUG version");
        return;
#endif
#pragma warning disable CS0162
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = serviceScopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var isTimeToNotify = IsTimeToNotify(scope.ServiceProvider);
        if (!isTimeToNotify)
        {
            return;
        }

        var publicBiddingSets = await GetPublicBiddingSets(scope.ServiceProvider);

        var emailSendingService = scope.ServiceProvider.GetRequiredService<EmailSendingService>();
        var discordPushService = scope.ServiceProvider.GetRequiredService<DiscordPushService>();
        
        await emailSendingService.SendPublicBidEmails(publicBiddingSets);
        await discordPushService.SendPublicBiddingSummary(publicBiddingSets);
#pragma warning restore CS0162
    }

    private static bool IsTimeToNotify(IServiceProvider serviceProvider)
    {
        var clock = serviceProvider.GetRequiredService<IClock>();
        var now = clock.GetCurrentInstant();
        var nycNow = now.InZone(TimeExtensions.EasternTimeZone);

        var dayOfWeek = nycNow.DayOfWeek;
        var timeOfDay = nycNow.TimeOfDay;
        var earliestTimeToSet = TimeExtensions.PublicBiddingRevealTime.Minus(Period.FromMinutes(1));
        var latestTimeToSet = TimeExtensions.PublicBiddingRevealTime.Plus(Period.FromMinutes(1));
        bool isTimeToNotify = dayOfWeek == TimeExtensions.PublicBiddingRevealDay && timeOfDay > earliestTimeToSet && timeOfDay < latestTimeToSet;
        return isTimeToNotify;
    }

    private static async Task<IReadOnlyList<LeagueYearPublicBiddingSet>> GetPublicBiddingSets(IServiceProvider serviceProvider)
    {
        var interLeagueService = serviceProvider.GetRequiredService<InterLeagueService>();
        var gameAcquisitionService = serviceProvider.GetRequiredService<GameAcquisitionService>();
        var supportedYears = await interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

        var publicBiddingSets = new List<LeagueYearPublicBiddingSet>();
        foreach (var year in activeYears)
        {
            var publicBiddingSetsForYear = await gameAcquisitionService.GetPublicBiddingGames(year.Year);
            publicBiddingSets.AddRange(publicBiddingSetsForYear);
        }

        return publicBiddingSets;
    }
}

