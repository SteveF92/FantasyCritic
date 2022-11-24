using Discord;
using Discord.WebSocket;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord.UrlBuilders;
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
    private readonly string _baseAddress;

    public DiscordPushService(
        FantasyCriticDiscordConfiguration configuration,
        IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter)
    {
        _enabled = !string.IsNullOrEmpty(configuration.BotToken) && configuration.BotToken != "secret";
        _botToken = configuration.BotToken;
        _baseAddress = configuration.BaseAddress;
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
        var newsEnabledChannels = allChannels.Where(x => x.GameNewsSetting != DiscordGameNewsSetting.Off).ToList();
        foreach (var leagueChannel in newsEnabledChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            var channel = guild.GetChannel(leagueChannel.ChannelID);
            if (channel is not SocketTextChannel textChannel)
            {
                continue;
            }
            var changesMessage = string.Join("\n", changes);
            await textChannel.TrySendMessageAsync($"**{game.GameName}**\n{changesMessage}");
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

            await channel.TrySendMessageAsync($"**{action.Publisher.PublisherName}** {action.Description} (at {action.Timestamp.ToEasternDate()})");
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
            await channel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
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
        await channel.TrySendMessageAsync(messageToSend);
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

            var gameMessages = new List<string>();
            foreach (var publicBid in publicBiddingSet.MasterGames)
            {
                var gameMessage = "";
                var releaseDate = publicBid.MasterGameYear.MasterGame.EstimatedReleaseDate;
                gameMessage += $"**{publicBid.MasterGameYear.MasterGame.GameName}**";

                if (publicBid.CounterPick)
                {
                    gameMessage += " (🎯 Counter Pick Bid)";
                }
                
                gameMessage += $"\n> Release Date: {releaseDate}";

                var roundedHypeFactor = Math.Round(publicBid.MasterGameYear.HypeFactor, 1);
                gameMessage += $"\n> Hype Factor: {roundedHypeFactor}\n";
                gameMessages.Add(gameMessage);
            }

            var leagueLink = new LeagueUrlBuilder(_baseAddress, publicBiddingSet.LeagueYear.League.LeagueID, publicBiddingSet.LeagueYear.Year).BuildUrl();
            var finalMessage = string.Join("\n", gameMessages);
            var lastSunday = GetLastSunday();
            var header = $"Public Bids (Week of {lastSunday:MMMM dd, yyyy})";

            foreach (var leagueChannel in leagueChannels)
            {
                var guild = _client.GetGuild(leagueChannel.GuildID);
                var channel = guild.GetTextChannel(leagueChannel.ChannelID);

                await channel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
                    header,
                    finalMessage,
                    url: leagueLink));
            }
        }
    }

    private static DateTime GetLastSunday()
    {
        var currentDate = DateTime.Now;
        var currentDayOfWeek = (int)currentDate.DayOfWeek;
        var lastSundayDate = currentDate.AddDays(-currentDayOfWeek);
        return lastSundayDate;
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

            await SendAllDropMessages(leagueAction, leagueChannels);
            await SendAllBidMessages(leagueAction, leagueChannels);
        }
    }

    private async Task SendAllBidMessages(LeagueActionProcessingSet leagueAction, List<MinimalLeagueChannel> leagueChannels)
    {
        var bidMessages = new List<string>();
        var leagueActionDictionaryByGame = new Dictionary<string, List<PickupBid>>();

        foreach (var leagueActionBid in leagueAction.Bids)
        {
            if (leagueActionDictionaryByGame.ContainsKey(leagueActionBid.MasterGame.GameName))
            {
                leagueActionDictionaryByGame[leagueActionBid.MasterGame.GameName].Add(leagueActionBid);
            }
            else
            {
                leagueActionDictionaryByGame.Add(leagueActionBid.MasterGame.GameName,
                    new List<PickupBid> { leagueActionBid });
            }
        }

        foreach (var bidGameAction in leagueActionDictionaryByGame)
        {
            var winningBidAmount = bidGameAction.Value.OrderByDescending(b => b.BidAmount).First().BidAmount;

            var messageToAdd = $"**{bidGameAction.Key}**\n";
            foreach (var bid in bidGameAction.Value)
            {
                if (!bid.Successful.HasValue)
                {
                    throw new Exception($"Bid {bid.BidID} Successful property is null");
                }

                var nameDisplay = $"**{bid.Publisher.PublisherName} ({bid.Publisher.User.UserName})**";

                if (bid.Successful.Value)
                {
                    var counterPickMessage = bid.CounterPick ? "(🎯 Counter Pick)" : "";
                    messageToAdd += $"- Won by {nameDisplay} with a bid of ${bid.BidAmount} {counterPickMessage}\n";
                }
                else
                {
                    var lossReason = bid.BidAmount == winningBidAmount
                        ? "lost on tiebreakers"
                        : "was outbid";
                    messageToAdd += $"- {nameDisplay}'s bid of ${bid.BidAmount} {lossReason}\n";
                }
            }

            bidMessages.Add($"{messageToAdd}");
        }

        var messageListToSend = BuildMessageListFromStringList(bidMessages, MaxMessageLength, "Bids", 2);
        await SendAllMessagesToAllLeagueChannels(leagueChannels, messageListToSend);
    }

    private async Task SendAllDropMessages(LeagueActionProcessingSet leagueAction, List<MinimalLeagueChannel> leagueChannels)
    {
        var dropMessages = new List<string>();
        foreach (var drop in leagueAction.Drops)
        {
            if (!drop.Successful.HasValue)
            {
                throw new Exception($"Drop {drop.DropRequestID} Successful property is null");
            }

            var nameToUse = $"{drop.Publisher.PublisherName} ({drop.Publisher.User.UserName})";

            var statusMessage = drop.Successful.Value ? "Successful" : "Failed";
            var messageToAdd = $"**{nameToUse}**: {drop.MasterGame.GameName} (Drop {statusMessage})";
            dropMessages.Add(messageToAdd);
        }

        var dropMessageListToSend = BuildMessageListFromStringList(dropMessages, MaxMessageLength, "Drops", 2);
        await SendAllMessagesToAllLeagueChannels(leagueChannels, dropMessageListToSend);
    }

    private async Task SendAllMessagesToAllLeagueChannels(List<MinimalLeagueChannel> leagueChannels, IReadOnlyList<string> messageListToSend)
    {
        foreach (var leagueChannel in leagueChannels)
        {
            foreach (var messageToSend in messageListToSend)
            {
                var guild = _client.GetGuild(leagueChannel.GuildID);
                var channel = guild.GetTextChannel(leagueChannel.ChannelID);
                await channel.TrySendMessageAsync(messageToSend);
            }
        }
    }

    private IReadOnlyList<string> BuildMessageListFromStringList(IReadOnlyList<string> messagesToCombine, int maxMessageLength,
        string firstMessageTitle = "", int newLinesAfterFirstMessage = 0)
    {
        var messageList = new List<string>();
        if (!string.IsNullOrEmpty(firstMessageTitle))
        {
            var title = $"**{firstMessageTitle}**";
            for (var i = 0; i < newLinesAfterFirstMessage; i++)
            {
                title += "\n";
            }
            messageList.Add(title);
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

        await channel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
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
