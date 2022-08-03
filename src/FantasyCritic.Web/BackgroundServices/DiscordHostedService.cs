using Microsoft.Extensions.Hosting;
using FantasyCritic.Lib.Discord;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.BackgroundServices;
public class DiscordHostedService : BackgroundService
{
    private const int DelayMilliseconds = 1 * 1000;
    private readonly ILogger<DiscordHostedService> _logger;
    private readonly DiscordRequestService _requestService;

    public DiscordHostedService(ILogger<DiscordHostedService> logger, DiscordRequestService requestService)
    {
        _logger = logger;
        _requestService = requestService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Beginning Discord request iteration");
            await _requestService.HandleRequests();
            await Task.Delay(DelayMilliseconds, stoppingToken);
        }
        _logger.LogWarning("Stopped listening for Discord requests");
    }
}
