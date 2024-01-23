using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Scheduling.Lib;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyCritic.Lib.Scheduling;

public class RefreshDataTask : IScheduledTask
{
    private readonly IServiceProvider _serviceProvider;
    public string Schedule => "0 */2 * * *";

    public RefreshDataTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = serviceScopeFactory.CreateScope();
        var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();
        await adminService.FullDataRefresh();

        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var now = clock.GetCurrentInstant();
        var isTimeToGetStatistics = IsTimeToGetStatistics(now);
        if (isTimeToGetStatistics)
        {
            await adminService.UpdateDailyPublisherStatistics();
        }
    }

    private static bool IsTimeToGetStatistics(Instant now)
    {
        var nycNow = now.InZone(TimeExtensions.EasternTimeZone);
        var timeOfDay = nycNow.TimeOfDay;
        var earliestTimeToCache = new LocalTime(22, 59, 00);
        var latestTimeToCache = new LocalTime(23, 20, 00);
        bool isTimeToNotify = timeOfDay > earliestTimeToCache && timeOfDay < latestTimeToCache;
        return isTimeToNotify;
    }
}
