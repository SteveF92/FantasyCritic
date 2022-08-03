using FantasyCritic.Lib.DependencyInjection;

namespace FantasyCritic.Lib.Discord;
public abstract class BaseDiscordService
{
    private readonly string _botToken;

    protected BaseDiscordService(DiscordConfiguration configuration)
    {
        _botToken = configuration.BotToken;
    }
}
