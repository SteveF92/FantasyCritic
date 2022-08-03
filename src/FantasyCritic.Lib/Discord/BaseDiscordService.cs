using Serilog;
using FantasyCritic.Lib.DependencyInjection;

namespace FantasyCritic.Lib.Discord;
public abstract class BaseDiscordService
{
    private static readonly ILogger _logger = Log.ForContext<BaseDiscordService>();

    private readonly string _botToken;

    protected BaseDiscordService(DiscordConfiguration configuration)
    {
        _botToken = configuration.BotToken;
    }

    protected Task SendMessage(string message)
    {
        _logger.Information("Message: {message}", message);
        return Task.CompletedTask;
    }
}
