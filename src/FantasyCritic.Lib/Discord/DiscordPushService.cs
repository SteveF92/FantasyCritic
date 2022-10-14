using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FantasyCritic.Lib.Discord;
public class DiscordPushService
{
    private readonly string _botToken;
    private readonly IDiscordRepo _discordRepo;
    private bool _started;

    public DiscordPushService(IConfigurationRoot configuration, IDiscordRepo discordRepo)
    {
        _botToken = configuration["BotToken"];
        _discordRepo = discordRepo;
    }

    public async Task SendMasterGameEditMessage(MasterGame game, IEnumerable<string> changes)
    {
        var allChannels = await _discordRepo.GetAllLeagueChannels();
        using DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig());

        await client.LoginAsync(TokenType.Bot, _botToken);
        await client.StartAsync();

        
    }
}
