using Discord;
using Discord.WebSocket;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord;
public class DiscordPushService
{
    private const int MaxAttempts = 4;
    private readonly string _botToken;
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly DiscordSocketClient _client;
    private bool _botIsReady;

    public DiscordPushService(FantasyCriticDiscordConfiguration configuration, IDiscordRepo discordRepo, IDiscordFormatter discordFormatter)
    {
        _botToken = configuration.BotToken;
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
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

    public async Task SendLeagueYearScoreUpdateMessage(LeagueYearScoreChanges scoreChanges)
    {
        await StartBot();
        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return;
        }

        var changeList = scoreChanges.GetScoreChanges();
        if (!changeList.AnyChanges)
        {
            return;
        }

        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var leagueChannel = allChannels.FirstOrDefault(c => c.LeagueID == scoreChanges.LeagueYear.League.LeagueID);
        if (leagueChannel is null)
        {
            return;
        }

        var guild = _client.GetGuild(leagueChannel.GuildID);
        var channel = guild.GetTextChannel(leagueChannel.ChannelID);

        var embedFieldBuilders = new List<EmbedFieldBuilder>();

        foreach (var change in changeList.Changes.Where(c => c.RankChanged || c.ScoreChanged))
        {
            var changeText = "";
            if (change.ScoreChanged)
            {
                changeText = $"> Score has gone **{change.Direction}** from **{change.RoundedOldScore}** to **{change.RoundedNewScore}**";
            }
            if (change.RankChanged)
            {
                changeText += $"\n> Moved from **{change.FormattedOldRank}** place to **{change.FormattedNewRank}** place";
            }

            if (!string.IsNullOrEmpty(changeText))
            {
                embedFieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = $"{change.Publisher.PublisherName} (Player: {change.Publisher.User.UserName})",
                    Value = changeText,
                    IsInline = false
                });
            }
        }

        if (embedFieldBuilders.Any())
        {
            await channel.SendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Publisher Score Updates",
            "",
            null,
            embedFieldBuilders));
        }
    }

    public async Task SendPublisherNameUpdateMessage(Publisher publisher, string oldPublisherName, string newPublisherName)
    {
        await StartBot();
        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return;
        }

        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var leagueChannel = allChannels.FirstOrDefault(c => c.LeagueID == publisher.LeagueYearKey.LeagueID);
        if (leagueChannel is null)
        {
            return;
        }

        var guild = _client.GetGuild(leagueChannel.GuildID);
        var channel = guild.GetTextChannel(leagueChannel.ChannelID);

        var messageToSend = $"Publisher **{oldPublisherName}** is now known as **{newPublisherName}**";
        await channel.SendMessageAsync(messageToSend);
    }

    public async Task SendTradeUpdateMessage(Trade trade)
    {
        await StartBot();
        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return;
        }
        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var leagueChannel = allChannels.FirstOrDefault(c => c.LeagueID == trade.LeagueYear.League.LeagueID);
        if (leagueChannel is null)
        {
            return;
        }

        var guild = _client.GetGuild(leagueChannel.GuildID);
        var channel = guild.GetTextChannel(leagueChannel.ChannelID);

        var header = $"The following trade has been **{trade.Status.Value.ToUpper()}**";

        var embedFieldBuilder = new EmbedFieldBuilder
        {
            Name = "Trade Details",
            Value = BuildTradeMessage(trade, "", trade.Status.Equals(TradeStatus.Proposed)),
            IsInline = false
        };
        var embedFieldBuilders = new List<EmbedFieldBuilder>
        {
            embedFieldBuilder
        };

        await channel.SendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Trade Update",
            header,
            null,
            embedFieldBuilders));
    }

    private string BuildTradeMessage(Trade trade, string header, bool includeMessage)
    {
        var message = $"**{trade.Proposer.PublisherName}** will receive: ";

        var counterPartySendGames = BuildGameListText(trade.CounterPartyMasterGames);
        var hasCounterPartySendGames = !string.IsNullOrEmpty(counterPartySendGames);

        if (hasCounterPartySendGames)
        {
            message += counterPartySendGames;
        }

        if (trade.CounterPartyBudgetSendAmount != default)
        {
            if (hasCounterPartySendGames)
            {
                message += " and ";
            }
            message += $"**${trade.CounterPartyBudgetSendAmount} of budget**";
        }

        message += $"\n**{trade.CounterParty.PublisherName}** will receive: ";

        var proposerSendGames = BuildGameListText(trade.ProposerMasterGames);
        var hasProposerSendGames = !string.IsNullOrEmpty(proposerSendGames);
        if (hasProposerSendGames)
        {
            message += proposerSendGames;
        }

        if (trade.ProposerBudgetSendAmount != default)
        {
            if (hasProposerSendGames)
            {
                message += " and ";
            }

            message += $"**${trade.ProposerBudgetSendAmount}** of budget";
        }

        if (includeMessage)
        {
            message += $"\nMessage from ${trade.Proposer.PublisherName}: **{trade.Message}**";
        }

        return message;
    }

    private string BuildGameListText(IReadOnlyList<MasterGameYearWithCounterPick> games)
    {
        var gameNames = games.Select(g => g.MasterGameYear.MasterGame.GameName);
        var gameNameString = string.Join(" and ", gameNames);
        return gameNameString;
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
