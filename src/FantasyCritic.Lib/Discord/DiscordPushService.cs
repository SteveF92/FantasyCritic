using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord;
public class DiscordPushService
{
    private const int MaxAttempts = 4;
    private readonly string _botToken;
    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordSocketClient _client;
    private bool _botIsReady;

    public DiscordPushService(FantasyCriticDiscordConfiguration configuration, IDiscordRepo discordRepo, IFantasyCriticRepo fantasyCriticRepo)
    {
        _botToken = configuration.BotToken;
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        DiscordSocketConfig socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true,
        };
        _client = new DiscordSocketClient(socketConfig);
        _botIsReady = false;
    }

    public async Task StartBot()
    {
        if (_botIsReady)
        {
            return;
        }

        _client.Ready += Client_Ready;
        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();

        var attempts = 0;
        while (!_botIsReady)
        {
            if (attempts > MaxAttempts)
            {
                break;
            }

            await Task.Delay(1000);
            attempts++;
        }
    }

    public async Task SendMasterGameEditMessage(MasterGame game, IReadOnlyList<string> changes)
    {
        await StartBot();
        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return;
        }

        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var newsEnabledChannels = allChannels.Where(x => x.IsGameNewsEnabled).ToList();
        foreach (var leagueChannel in newsEnabledChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            var channel = guild.GetChannel(leagueChannel.ChannelID);
            if (channel is not SocketTextChannel textChannel)
            {
                continue;
            }
            var changesMessage = string.Join("\n", changes);
            await textChannel.SendMessageAsync($"**{game.GameName}**\n{changesMessage}");
        }
    }

    public async Task SendLeagueActionMessage(LeagueAction action)
    {
        await StartBot();
        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return;
        }

        var leagueId = action.Publisher.LeagueYearKey.LeagueID;
        var leagueChannels = await _discordRepo.GetLeagueChannels(leagueId);

        if (leagueChannels is null)
        {
            return;
        }

        foreach (var leagueChannel in leagueChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            SocketTextChannel? channel = guild.GetTextChannel(leagueChannel.ChannelID);
            if (channel is null)
            {
                continue;
            }

            await channel.SendMessageAsync($"**{action.Publisher.PublisherName}** {action.Description} (at {action.Timestamp.ToEasternDate()}");
        }
    }

    public async Task SendPublisherScoreUpdateMessage(LeagueYear leagueYear, Publisher publisher, decimal oldScore, decimal newScore)
    {
        await StartBot();
        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return;
        }

        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var leagueChannel = allChannels.FirstOrDefault(c => c.LeagueID == leagueYear.League.LeagueID);
        if (leagueChannel is null)
        {
            return;
        }


        var guild = _client.GetGuild(leagueChannel.GuildID);
        var channel = guild.GetTextChannel(leagueChannel.ChannelID);

        //var rankedPublishers = leagueYear.Publishers.OrderBy(p
        //    => p.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options));

        // TODO: determine publisher ranking changes

        var nameToShow = $"{publisher.PublisherName} (Player: {publisher.User.UserName})";

        var roundedOldScore = Math.Round(oldScore, 1);
        var roundedNewScore = Math.Round(newScore, 1);
        var scoreDiff = roundedOldScore - roundedNewScore;
        if (scoreDiff != 0 && Math.Abs(scoreDiff) >= 1)
        {
            var direction = scoreDiff < 0 ? "UP" : "DOWN";
            var messageToSend = $"**{nameToShow}**'s score has gone **{direction}** from **{roundedOldScore}** to **{roundedNewScore}**";
            await channel.SendMessageAsync(messageToSend);
        }
    }

    public async Task SendPublisherNameUpdateMessage(LeagueYear leagueYear, string oldPublisherName, string newPublisherName)
    {
        await StartBot();
        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return;
        }

        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var leagueChannel = allChannels.FirstOrDefault(c => c.LeagueID == leagueYear.League.LeagueID);
        if (leagueChannel is null)
        {
            return;
        }

        var guild = _client.GetGuild(leagueChannel.GuildID);
        var channel = guild.GetTextChannel(leagueChannel.ChannelID);

        var messageToSend = $"Publisher **{oldPublisherName}** is now known as **{newPublisherName}**";
        await channel.SendMessageAsync(messageToSend);
    }

    public Task Client_Ready()
    {
        _botIsReady = true;
        return Task.CompletedTask;
    }

    private static Task Log(LogMessage msg)
    {
        Serilog.Log.Information(msg.ToString());
        return Task.CompletedTask;
    }
}
