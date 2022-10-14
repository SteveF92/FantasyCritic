using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord;
public class DiscordPushService
{
    private readonly string _botToken;
    private readonly IDiscordRepo _discordRepo;
    private readonly DiscordSocketClient _client;

    private KeyValuePair<MasterGame, IEnumerable<string>> _changeToReport;

    public DiscordPushService(FantasyCriticDiscordConfiguration configuration, IDiscordRepo discordRepo)
    {
        _botToken = configuration.BotToken;
        _discordRepo = discordRepo;
        DiscordSocketConfig socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true,
        };
        _client = new DiscordSocketClient(socketConfig);
    }

    public async Task SendMasterGameEditMessage(MasterGame game, IEnumerable<string> changes)
    {
        _client.Ready += Client_Ready;
        _client.Log += Log;

        _changeToReport = new KeyValuePair<MasterGame, IEnumerable<string>>(game, changes);

        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();
    }

    public async Task Client_Ready()
    {
        var allChannels = await _discordRepo.GetAllLeagueChannels();
        foreach (var leagueChannel in allChannels)
        {
            var channel = _client.GetGuild(leagueChannel.GuildID).GetChannel(leagueChannel.ChannelID) as SocketTextChannel;
            if (channel == null)
            {
                continue;
            }
            var changesMessage = string.Join("\n", _changeToReport.Value);
            await channel.SendMessageAsync($"**{_changeToReport.Key.GameName}**\n{changesMessage}");
        }
    }

    private static Task Log(LogMessage msg)
    {
        Serilog.Log.Information(msg.ToString());
        return Task.CompletedTask;
    }
}
