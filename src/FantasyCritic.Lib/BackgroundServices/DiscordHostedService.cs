using FantasyCritic.Lib.Discord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.BackgroundServices;
public class DiscordHostedService : BackgroundService
{
    private readonly ILogger<DiscordHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DiscordHostedService(ILogger<DiscordHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Beginning Discord request iteration");
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var requestService = scope.ServiceProvider.GetRequiredService<DiscordBotService>();
        await requestService.InitializeBotAsync();
        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogWarning("Stopped listening for Discord requests");
    }
}
