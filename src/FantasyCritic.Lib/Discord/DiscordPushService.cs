using Discord;
using Discord.WebSocket;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord;
public class DiscordPushService
{
    private const int MaxAttempts = 4;
    private const int MaxMessageLength = 2000;
    private readonly string _botToken;
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly DiscordSocketClient _client;
    private bool _botIsReady;
    private readonly bool _enabled;

    public DiscordPushService(FantasyCriticDiscordConfiguration configuration, IDiscordRepo discordRepo, IDiscordFormatter discordFormatter)
    {
        _enabled = !string.IsNullOrEmpty(configuration.BotToken);
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

    private async Task<bool> StartBot()
    {
        if (!_enabled)
        {
            return false;
        }

        if (_botIsReady)
        {
            return true;
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

        if (!_botIsReady)
        {
            Serilog.Log.Warning("Discord bot is not ready, cannot send message.");
            return false;
        }

        return true;
    }

    public async Task SendMasterGameEditMessage(MasterGame game, IReadOnlyList<string> changes)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
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
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
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
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
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
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
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

    public async Task SendPublicBiddingSummary(IEnumerable<LeagueYearPublicBiddingSet> publicBiddingSets)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var channelLookup = allChannels.ToLookup(c => c.LeagueID);

        foreach (var publicBiddingSet in publicBiddingSets)
        {
            var leagueChannels = channelLookup[publicBiddingSet.LeagueYear.League.LeagueID].ToList();
            if (!leagueChannels.Any())
            {
                continue;
            }
        }
    }

    public async Task SendActionProcessingSummary(IEnumerable<LeagueActionProcessingSet> leagueActionSets)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var allChannels = await _discordRepo.GetAllLeagueChannels();
        var channelLookup = allChannels.ToLookup(c => c.LeagueID);

        foreach (var leagueAction in leagueActionSets)
        {
            var leagueChannels = channelLookup[leagueAction.LeagueYear.League.LeagueID].ToList();
            if (!leagueChannels.Any())
            {
                continue;
            }

            var actionMessages = new List<string>();
            foreach (var drop in leagueAction.Drops)
            {
                if (!drop.Successful.HasValue)
                {
                    throw new Exception($"Drop {drop.DropRequestID} has a null value in the Successful property.");
                }

                var nameToUse = $"{drop.Publisher.PublisherName} ({drop.Publisher.User.UserName})";

                var statusMessage = drop.Successful.Value
                    ? "DROPPED"
                    : "FAILED TO DROP";
                var messageToAdd = $"**{nameToUse}** {statusMessage} {drop.MasterGame.GameName}";
                actionMessages.Add(messageToAdd);
            }

            foreach (var bid in leagueAction.Bids)
            {
                if (!bid.Successful.HasValue)
                {
                    throw new Exception($"Bid {bid.BidID} has a null value in the Successful property.");
                }

                var nameToUse = $"{bid.Publisher.PublisherName} ({bid.Publisher.User.UserName})";

                var statusMessage = bid.Successful.Value
                    ? "ACQUIRED"
                    : "FAILED TO ACQUIRE";

                var counterPickMessage = bid.CounterPick ? " (as a Counter Pick) " : "";

                var outcomeMessage = !string.IsNullOrEmpty(bid.Outcome) ? $"- {bid.Outcome}" : "";

                var messageToAdd =
                    $"**{nameToUse}** {statusMessage} {bid.MasterGame.GameName}{counterPickMessage}with a bid of ${bid.BidAmount} {outcomeMessage}";
                actionMessages.Add(messageToAdd);
            }

            var messageListToSend = BuildMessageListFromStringList(actionMessages, MaxMessageLength, "League Action Updates");

            foreach (var leagueChannel in leagueChannels)
            {
                foreach (var messageToSend in messageListToSend)
                {
                    var guild = _client.GetGuild(leagueChannel.GuildID);
                    var channel = guild.GetTextChannel(leagueChannel.ChannelID);
                    await channel.SendMessageAsync(messageToSend);
                }
            }
        }
    }

    private IReadOnlyList<string> BuildMessageListFromStringList(IReadOnlyList<string> messagesToCombine, int maxMessageLength,
        string firstMessageTitle = "")
    {
        var messageList = new List<string>();
        if (!string.IsNullOrEmpty(firstMessageTitle))
        {
            messageList.Add($"**{firstMessageTitle}**\n");
        }

        var listIndex = 0;
        foreach (var updateMessage in messagesToCombine)
        {
            var messageToAdd = $"{updateMessage}\n";
            var concatenatedMessage = messageList[listIndex] + messageToAdd;
            if (concatenatedMessage.Length >= maxMessageLength)
            {
                messageList.Add(messageToAdd);
                listIndex++;
            }
            else
            {
                messageList[listIndex] += messageToAdd;
            }
        }
        return messageList;
    }

    public async Task SendTradeUpdateMessage(Trade trade)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
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
            Value = BuildTradeMessage(trade, trade.Status.Equals(TradeStatus.Proposed)),
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

    private string BuildTradeMessage(Trade trade, bool includeMessage)
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
            message += $"\nMessage from {trade.Proposer.PublisherName}: **{trade.Message}**";
        }

        return message;
    }

    private string BuildGameListText(IReadOnlyList<MasterGameYearWithCounterPick> games)
    {
        var gameNames = games.Select(g => g.MasterGameYear.MasterGame.GameName);
        var gameNameString = string.Join(" and ", gameNames);
        return gameNameString;
    }

    private Task Client_Ready()
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
