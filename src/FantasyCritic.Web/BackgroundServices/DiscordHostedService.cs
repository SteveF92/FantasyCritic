using Microsoft.Extensions.Hosting;
using FantasyCritic.Lib.Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.BackgroundServices;
public class DiscordHostedService : BackgroundService
{
    private const int DelayMilliseconds = 5 * 1000;
    private readonly ILogger<DiscordHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DiscordHostedService(ILogger<DiscordHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Beginning Discord request iteration");
            var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var requestService = scope.ServiceProvider.GetRequiredService<DiscordRequestService>();
            await requestService.HandleRequests();
            await Task.Delay(DelayMilliseconds, stoppingToken);
        }

        _logger.LogWarning("Stopped listening for Discord requests");
    }
}
