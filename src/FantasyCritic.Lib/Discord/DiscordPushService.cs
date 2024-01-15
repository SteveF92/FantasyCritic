using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordDotNetUtilities;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FantasyCritic.Lib.Discord;
public class DiscordPushService
{
    private static readonly ILogger Logger = Log.ForContext<DiscordPushService>();

    private const int MaxAttempts = 4;
    private const int MaxMessageLength = 2000;
    private readonly string _botToken;
    private readonly IClock _clock;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly DiscordSocketClient _client;
    private bool _botIsReady;
    private readonly bool _enabled;
    private readonly string _baseAddress;

    private readonly List<NewMasterGameMessage> _newMasterGameMessages = new();
    private readonly List<GameCriticScoreUpdateMessage> _gameCriticScoreUpdateMessages = new();
    private readonly List<MasterGameEditMessage> _masterGameEditMessages = new();

    public DiscordPushService(
        FantasyCriticDiscordConfiguration configuration,
        IClock clock,
        IServiceProvider serviceProvider,
        IDiscordFormatter discordFormatter)
    {
        _enabled = !string.IsNullOrEmpty(configuration.BotToken) && configuration.BotToken != "secret";
        _botToken = configuration.BotToken;
        _baseAddress = configuration.BaseAddress;
        _clock = clock;
        _serviceProvider = serviceProvider;
        _discordFormatter = discordFormatter;
        DiscordSocketConfig socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged,
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
        _client.Log += LogMessage;

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

        if (_botIsReady)
        {
            return true;
        }
        Log.Warning("Discord bot is not ready, cannot send message.");
        return false;

    }

    public void QueueNewMasterGameMessage(MasterGame masterGame)
    {
        _newMasterGameMessages.Add(new NewMasterGameMessage(masterGame));
    }

    public void QueueGameCriticScoreUpdateMessage(MasterGame game, decimal? oldCriticScore, decimal? newCriticScore)
    {
        _gameCriticScoreUpdateMessages.Add(new GameCriticScoreUpdateMessage(game, oldCriticScore, newCriticScore));
    }

    public void QueueMasterGameEditMessage(MasterGameYear existingGame, MasterGameYear editedGame, IReadOnlyList<string> changes)
    {
        _masterGameEditMessages.Add(new MasterGameEditMessage(existingGame, editedGame, changes));
    }

    public void ClearMasterGameEditQueue()
    {
        _masterGameEditMessages.Clear();
    }

    public async Task SendBatchedMasterGameUpdates()
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var fantasyCriticRepo = scope.ServiceProvider.GetRequiredService<IFantasyCriticRepo>();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        Logger.Information($"Sending master game updates. {_newMasterGameMessages.Count} New MasterGames | {_gameCriticScoreUpdateMessages.Count} Score Updates | {_masterGameEditMessages.Count} Edits");
        foreach (var scoreUpdate in _gameCriticScoreUpdateMessages)
        {
            Logger.Information($"Score Update: {scoreUpdate.Game.GameName} - {scoreUpdate.OldCriticScore} -> {scoreUpdate.NewCriticScore}");
        }

        var allChannels = await GetAllCombinedChannels(discordRepo, fantasyCriticRepo);
        var messageTasks = new List<Task>();

        var today = _clock.GetToday();
        foreach (var combinedChannel in allChannels)
        {
            var guild = _client.GetGuild(combinedChannel.GuildID);
            var channel = guild?.GetChannel(combinedChannel.ChannelID);
            if (channel is not SocketTextChannel textChannel)
            {
                continue;
            }

            var newMasterGamesToSend = _newMasterGameMessages
                .Where(x => combinedChannel.CombinedSetting.NewGameIsRelevant(x.MasterGame, combinedChannel.ActiveLeagueYears, combinedChannel.ChannelKey, today))
                .ToList();
            var scoreUpdatesToSend = _gameCriticScoreUpdateMessages
                .Where(x => combinedChannel.CombinedSetting.ScoredGameIsRelevant(x.Game, combinedChannel.ActiveLeagueYears, x.NewCriticScore, today))
                .ToList();
            var editsToSend = _masterGameEditMessages
                .Where(x => combinedChannel.CombinedSetting.ExistingGameIsRelevant(x.ExistingGame.MasterGame,
                    x.ExistingGame.GetWillReleaseStatus() != x.EditedGame.GetWillReleaseStatus(), combinedChannel.ActiveLeagueYears, combinedChannel.ChannelKey, today))
                .ToList();

            if (!newMasterGamesToSend.Any() && !scoreUpdatesToSend.Any() && !editsToSend.Any())
            {
                continue;
            }

            var gameUpdateMessages = new Dictionary<MasterGame, List<string>>();

            //master games
            foreach (var newMasterGameMessage in newMasterGamesToSend)
            {
                if (!gameUpdateMessages.ContainsKey(newMasterGameMessage.MasterGame))
                {
                    gameUpdateMessages.Add(newMasterGameMessage.MasterGame, new List<string>());
                }
                gameUpdateMessages[newMasterGameMessage.MasterGame].Add("Just added!");
                var tagNames = newMasterGameMessage.MasterGame.Tags.Select(t => t.ReadableName);
                gameUpdateMessages[newMasterGameMessage.MasterGame].Add($"Tags: {string.Join(", ", tagNames)}");
                gameUpdateMessages[newMasterGameMessage.MasterGame].Add($"Release Date: {newMasterGameMessage.MasterGame.GetReleaseDateString()}");
                if (!string.IsNullOrEmpty(newMasterGameMessage.MasterGame.Notes))
                {
                    gameUpdateMessages[newMasterGameMessage.MasterGame].Add($"Note: {newMasterGameMessage.MasterGame.Notes}");
                }
            }

            //score updates
            var scoreUpdateLookup = scoreUpdatesToSend.ToLookup(x => x.Game);
            var editsLookup = editsToSend.ToLookup(x => x.EditedGame.MasterGame);
            var existingGames = scoreUpdatesToSend.Select(x => x.Game).Concat(editsToSend.Select(x => x.EditedGame.MasterGame)).Distinct().ToList();
            foreach (var existingGame in existingGames)
            {
                var changeMessages = new List<string>();
                var scoreUpdate = scoreUpdateLookup[existingGame].LastOrDefault();
                if (scoreUpdate is not null && (scoreUpdate.NewCriticScore is not null || scoreUpdate.OldCriticScore is not null))
                {
                    var newCriticScoreRounded = scoreUpdate.NewCriticScore != null ? (decimal?)Math.Round(scoreUpdate.NewCriticScore.Value, 1) : null;
                    var oldCriticScoreRounded = scoreUpdate.OldCriticScore != null ? (decimal?)Math.Round(scoreUpdate.OldCriticScore.Value, 1) : null;

                    if (scoreUpdate.NewCriticScore == null)
                    {
                        changeMessages.Add("Score has been removed.");
                    }
                    else if (scoreUpdate.OldCriticScore == null)
                    {
                        changeMessages.Add($"Now has a score of **{newCriticScoreRounded}**");
                    }
                    else
                    {
                        var scoreDiff = scoreUpdate.NewCriticScore.Value - scoreUpdate.OldCriticScore.Value;
                        if (scoreDiff != 0 && Math.Abs(scoreDiff) >= 1)
                        {
                            var direction = scoreDiff > 0 ? "UP" : "DOWN";
                            changeMessages.Add($"Score has gone **{direction}** from **{oldCriticScoreRounded}** to **{newCriticScoreRounded}**");
                        }
                    }
                }

                var gameEdits = editsLookup[existingGame].ToList();
                if (gameEdits.Any())
                {
                    changeMessages.AddRange(gameEdits.SelectMany(x => x.Changes));
                }

                if (changeMessages.Any())
                {
                    if (!gameUpdateMessages.ContainsKey(existingGame))
                    {
                        gameUpdateMessages.Add(existingGame, new List<string>());
                    }
                    foreach (var changeMessage in changeMessages)
                    {
                        gameUpdateMessages[existingGame].Add(changeMessage);
                    }
                }
            }

            var allPublisherInActiveYears = new List<Publisher>();
            if (combinedChannel.ActiveLeagueYears is not null)
            {
                allPublisherInActiveYears = combinedChannel.ActiveLeagueYears.SelectMany(x => x.Publishers).ToList();
            }

            var messagesToSend = gameUpdateMessages
                .Select(gameUpdateMessage =>
                {
                    string publisherOfGameNoteFinalString = "";
                    if (combinedChannel.ActiveLeagueYears is not null)
                    {
                        var publisherOfGameNote = GetPublisherOfGameNote(allPublisherInActiveYears, gameUpdateMessage.Key, today);
                        publisherOfGameNoteFinalString = $" [{publisherOfGameNote}]";
                    }

                    var gameAndPublisherMessage = $"**{gameUpdateMessage.Key.GameName}**{publisherOfGameNoteFinalString}";

                    return $"{gameAndPublisherMessage}\n{string.Join("\n", gameUpdateMessage.Value.Select(c => $"> {c}"))}";
                }).ToList();

            if (!messagesToSend.Any())
            {
                continue;
            }

            Logger.Information("Building a master game update with {gameUpdatesPerChannel} messages.", messagesToSend.Count);
            var messagesToActuallySend = new MessageListBuilder(messagesToSend, MaxMessageLength)
                .WithDivider("\n")
                .Build();

            messageTasks.AddRange(messagesToActuallySend.Select(message => textChannel.TrySendMessageAsync(message, flags: MessageFlags.SuppressEmbeds)));
        }

        Logger.Information("Pushing out {gameUpdateChannels} game updates to channels.", messageTasks.Count);
        await Task.WhenAll(messageTasks);

        _newMasterGameMessages.Clear();
        _gameCriticScoreUpdateMessages.Clear();
        _masterGameEditMessages.Clear();
    }

    private static string GetPublisherOfGameNote(IReadOnlyList<Publisher> allPublisherInActiveYears, MasterGame masterGame, LocalDate currentDate)
    {
        var standardPickPublisher = allPublisherInActiveYears.FirstOrDefault(x =>
            x.PublisherGames.Any(y => !y.CounterPick && y.MasterGame is not null &&
                                      y.MasterGame.MasterGame.Equals(masterGame)));

        var counterPickPublisher = allPublisherInActiveYears.FirstOrDefault(x =>
            x.PublisherGames.Any(y => y.CounterPick && y.MasterGame is not null &&
                                      y.MasterGame.MasterGame.Equals(masterGame)));

        if (standardPickPublisher is null && counterPickPublisher is null)
        {
            if (!masterGame.IsReleased(currentDate) && !masterGame.CriticScore.HasValue)
            {
                return "Available";
            }

            return "Unpublished";
        }

        if (standardPickPublisher is not null && counterPickPublisher is not null)
        {
            return $"Picked by **{standardPickPublisher.GetPublisherAndUserDisplayName()}**, Counter Picked by **{counterPickPublisher.GetPublisherAndUserDisplayName()}**";
        }

        if (standardPickPublisher is not null)
        {
            return $"Picked by **{standardPickPublisher.GetPublisherAndUserDisplayName()}**";
        }

        //Must be just counter pick
        return $"**Counter Picked** by **{counterPickPublisher!.GetPublisherAndUserDisplayName()}**";
    }

    public async Task SendGameReleaseUpdates(IReadOnlyList<MasterGameYear> masterGamesReleasingToday)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var fantasyCriticRepo = scope.ServiceProvider.GetRequiredService<IFantasyCriticRepo>();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var allChannels = await GetAllCombinedChannels(discordRepo, fantasyCriticRepo);
        var messageTasks = new List<Task>();
        foreach (var combinedChannel in allChannels)
        {
            var guild = _client.GetGuild(combinedChannel.GuildID);
            var channel = guild?.GetChannel(combinedChannel.ChannelID);
            if (channel is not SocketTextChannel textChannel)
            {
                continue;
            }

            IReadOnlyList<MasterGameYear> relevantGamesForLeague = masterGamesReleasingToday
                .Where(x => combinedChannel.CombinedSetting.ReleasedGameIsRelevant(x.MasterGame, combinedChannel.ActiveLeagueYears))
                .ToList();
            if (!relevantGamesForLeague.Any())
            {
                continue;
            }

            var releaseMessages = relevantGamesForLeague.Select(x => $"**{x.MasterGame.GameName}** has released!");
            var releaseMessage = string.Join("\n", releaseMessages);
            messageTasks.Add(textChannel.TrySendMessageAsync(releaseMessage));
        }

        await Task.WhenAll(messageTasks);
    }

    public async Task SendLeagueActionMessage(LeagueAction action)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var leagueId = action.Publisher.LeagueYearKey.LeagueID;

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var leagueChannels = await discordRepo.GetLeagueChannels(leagueId);

        var messageTasks = new List<Task>();
        foreach (var leagueChannel in leagueChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            SocketTextChannel? channel = guild?.GetTextChannel(leagueChannel.ChannelID);
            if (channel is null)
            {
                continue;
            }

            messageTasks.Add(channel.TrySendMessageAsync($"**{action.Publisher.GetPublisherAndUserDisplayName()}** {action.Description} (at {action.Timestamp.ToEasternDate()})"));
        }

        await Task.WhenAll(messageTasks);
    }

    public async Task SendLeagueActionMessage(LeagueManagerAction action)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var leagueId = action.LeagueYearKey.LeagueID;

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var leagueChannels = await discordRepo.GetLeagueChannels(leagueId);

        var messageTasks = new List<Task>();
        foreach (var leagueChannel in leagueChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            SocketTextChannel? channel = guild?.GetTextChannel(leagueChannel.ChannelID);
            if (channel is null)
            {
                continue;
            }

            messageTasks.Add(channel.TrySendMessageAsync($"**{action.ActionType}** {action.Description} (at {action.Timestamp.ToEasternDate()})"));
        }

        await Task.WhenAll(messageTasks);
    }

    public async Task SendLeagueYearScoreUpdateMessage(LeagueYearScoreChanges scoreChanges)
    {
        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var leagueChannels = await discordRepo.GetLeagueChannels(scoreChanges.LeagueYear.League.LeagueID);
        await SendLeagueYearScoreUpdateMessage(scoreChanges, leagueChannels);
    }

    public async Task SendLeagueYearScoreUpdateMessage(LeagueYearScoreChanges scoreChanges, IEnumerable<MinimalLeagueChannel> leagueChannels)
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

        var channels = GetSocketTextChannels(leagueChannels);
        if (!channels.Any())
        {
            return;
        }

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
                    Name = change.Publisher.GetPublisherAndUserDisplayName(),
                    Value = changeText,
                    IsInline = false
                });
            }
        }

        if (embedFieldBuilders.Any())
        {
            var tasks = channels.Select(channel => channel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
                "Publisher Score Updates",
                "",
                null,
                embedFieldBuilders)));
            await Task.WhenAll(tasks);
        }
    }

    public async Task SendPublisherNameUpdateMessage(Publisher publisher, string oldPublisherName, string newPublisherName)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var channels = await GetChannelsForLeague(publisher.LeagueYearKey.LeagueID, discordRepo);
        if (!channels.Any())
        {
            return;
        }

        var messageToSend = $"Publisher **{oldPublisherName}** ({publisher.User.UserName}) is now known as **{newPublisherName}**";


        var messageTasks = channels.Select(channel => channel.TrySendMessageAsync(messageToSend));
        await Task.WhenAll(messageTasks);
    }

    public async Task SendPublisherEditMessage(EditPublisherRequest editPublisherRequest)
    {
        var messageToSend = $"Publisher **{editPublisherRequest.Publisher.GetPublisherAndUserDisplayName()}** edited by League Manager: {editPublisherRequest.GetActionString()}";
        var leagueID = editPublisherRequest.Publisher.LeagueYearKey.LeagueID;

        await SendLeagueManagerActionMessage(leagueID, messageToSend);
    }

    private async Task SendLeagueManagerActionMessage(Guid leagueID, string message)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var channels = await GetChannelsForLeague(leagueID, discordRepo);
        if (!channels.Any())
        {
            return;
        }

        var leagueManagerActionMessage = $"**League Manager Action**: {message}";

        var messageTasks = channels.Select(channel => channel.TrySendMessageAsync(leagueManagerActionMessage));
        await Task.WhenAll(messageTasks);
    }

    public async Task SendLeagueManagerAddPublisherGameMessage(Publisher publisher, string gameName)
    {
        var messageToSend = $"Publisher **{publisher.GetPublisherAndUserDisplayName()}** has had the game **{gameName}** manually **ADDED** by the League Manager.";
        await SendLeagueManagerActionMessage(publisher.LeagueYearKey.LeagueID, messageToSend);
    }

    public async Task SendLeagueManagerRemovePublisherGameMessage(Publisher publisher, string gameName)
    {
        var leagueID = publisher.LeagueYearKey.LeagueID;
        var messageToSend = $"Publisher **{publisher.GetPublisherAndUserDisplayName()}** has had the game **{gameName}** manually **REMOVED** by the League Manager.";
        await SendLeagueManagerActionMessage(leagueID, messageToSend);
    }

    public async Task SendNewPublisherMessage(Publisher publisher)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var channels = await GetChannelsForLeague(publisher.LeagueYearKey.LeagueID, discordRepo);
        if (!channels.Any())
        {
            return;
        }

        var messageToSend = $"Publisher **{publisher.GetPublisherAndUserDisplayName()}** has joined the league!";

        var messageTasks = channels.Select(channel => channel.TrySendMessageAsync(messageToSend));
        await Task.WhenAll(messageTasks);
    }

    public async Task SendPublicBiddingSummary(IEnumerable<LeagueYearPublicBiddingSet> publicBiddingSets)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var allChannels = await discordRepo.GetAllLeagueChannels();
        var channelLookup = allChannels.ToLookup(c => c.LeagueID);

        var messageTasks = new List<Task>();
        foreach (var publicBiddingSet in publicBiddingSets)
        {
            var leagueChannels = channelLookup[publicBiddingSet.LeagueYear.League.LeagueID].ToList();
            if (!leagueChannels.Any())
            {
                continue;
            }

            var leagueLink = new LeagueUrlBuilder(_baseAddress, publicBiddingSet.LeagueYear.League.LeagueID, publicBiddingSet.LeagueYear.Year).BuildUrl();
            var lastSunday = DiscordSharedMessageUtilities.GetLastSunday();
            var header = $"Public Bids (Week of {lastSunday:MMMM dd, yyyy})";

            string finalMessage;

            if (publicBiddingSet.MasterGames.Any())
            {
                var gameMessages = publicBiddingSet.MasterGames.Select(DiscordSharedMessageUtilities.BuildPublicBidGameMessage).ToList();
                finalMessage = string.Join("\n", gameMessages);
            }
            else
            {
                finalMessage = "There were no bids this week.";
            }

            foreach (var leagueChannel in leagueChannels)
            {
                var guild = _client.GetGuild(leagueChannel.GuildID);
                var channel = guild?.GetTextChannel(leagueChannel.ChannelID);
                if (channel is null)
                {
                    continue;
                }

                SocketRole? roleToMention = null;
                if (leagueChannel.BidAlertRoleID != null)
                {
                    roleToMention =
                        channel.Guild.Roles.FirstOrDefault(r => r.Id == leagueChannel.BidAlertRoleID);
                }

                messageTasks.Add(channel.TrySendMessageAsync(roleToMention?.Mention ?? "", embed: _discordFormatter.BuildRegularEmbed(
                    header,
                    finalMessage,
                    url: leagueLink)));
            }
        }

        await Task.WhenAll(messageTasks);
    }

    public async Task SendActionProcessingSummary(IReadOnlyList<LeagueActionProcessingSet> leagueActionSets)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var allChannels = await discordRepo.GetAllLeagueChannels();
        var channelLookup = allChannels.ToLookup(c => c.LeagueID);

        var dropTasks = new List<Task>();
        foreach (var leagueAction in leagueActionSets)
        {
            var leagueChannels = channelLookup[leagueAction.LeagueYear.League.LeagueID].ToList();
            if (!leagueChannels.Any())
            {
                continue;
            }

            dropTasks.Add(SendAllDropMessages(leagueAction, leagueChannels));
        }

        await Task.WhenAll(dropTasks);

        var bidTasks = new List<Task>();
        foreach (var leagueAction in leagueActionSets)
        {
            var leagueChannels = channelLookup[leagueAction.LeagueYear.League.LeagueID].ToList();
            if (!leagueChannels.Any())
            {
                continue;
            }

            bidTasks.Add(SendAllBidMessages(leagueAction, leagueChannels));
        }

        await Task.WhenAll(bidTasks);
    }

    private async Task SendAllBidMessages(LeagueActionProcessingSet leagueAction, IEnumerable<MinimalLeagueChannel> leagueChannels)
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
            var messageToAdd = $"**{bidGameAction.Key}**\n";
            var orderedBids = bidGameAction.Value.OrderByDescending(x => x.Successful!.Value).ThenByDescending(x => x.BidAmount).ToList();
            foreach (var bid in orderedBids)
            {
                var counterPickMessage = bid.CounterPick ? "(🎯 Counter Pick)" : "";
                if (bid.Successful!.Value)
                {
                    messageToAdd += $"- Won by {bid.Publisher.GetPublisherAndUserDisplayName()} with a bid of ${bid.BidAmount} {counterPickMessage}\n";
                }
                else
                {
                    messageToAdd += $"- {bid.Publisher.GetPublisherAndUserDisplayName()}'s bid of ${bid.BidAmount} did not succeed: {bid.Outcome} {counterPickMessage}\n";
                }
            }

            bidMessages.Add($"{messageToAdd}");
        }

        if (bidMessages.Any())
        {
            var messageListToSend = new MessageListBuilder(bidMessages,
                MaxMessageLength)
            .WithTitle("Bids", new[] { TextStyleOption.Bold, TextStyleOption.Underline }, "\n", 1)
            .WithDivider("\n")
            .Build();
            await SendAllMessagesToAllLeagueChannels(leagueChannels, messageListToSend, true);
        }
    }

    private async Task SendAllDropMessages(LeagueActionProcessingSet leagueAction, IEnumerable<MinimalLeagueChannel> leagueChannels)
    {
        var dropMessages = new List<string>();
        foreach (var drop in leagueAction.Drops)
        {
            if (!drop.Successful.HasValue)
            {
                throw new Exception($"Drop {drop.DropRequestID} Successful property is null");
            }

            var statusMessage = drop.Successful.Value ? "Successful" : "Failed";
            var messageToAdd = $"**{drop.Publisher.GetPublisherAndUserDisplayName()}**: {drop.MasterGame.GameName} (Drop {statusMessage})";
            dropMessages.Add(messageToAdd);
        }

        if (dropMessages.Any())
        {
            var dropMessageListToSend = new MessageListBuilder(dropMessages,
                MaxMessageLength)
            .WithTitle("Drops", new[] { TextStyleOption.Bold, TextStyleOption.Underline }, "\n", 1)
            .WithDivider("\n")
            .Build();
            await SendAllMessagesToAllLeagueChannels(leagueChannels, dropMessageListToSend, false);
        }
    }

    private async Task SendAllMessagesToAllLeagueChannels(IEnumerable<MinimalLeagueChannel> leagueChannels, IReadOnlyList<string> messageListToSend, bool mentionRole = false)
    {
        var messageTasks = new List<Task>();
        foreach (var leagueChannel in leagueChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            var channel = guild?.GetTextChannel(leagueChannel.ChannelID);
            if (channel is null)
            {
                continue;
            }

            var roleToMention = mentionRole && leagueChannel.BidAlertRoleID != null
                ? channel.Guild.Roles.FirstOrDefault(r => r.Id == leagueChannel.BidAlertRoleID)
                : null;

            foreach (var messageToSend in messageListToSend)
            {
                messageTasks.Add(channel.TrySendMessageAsync($"{roleToMention?.Mention ?? ""}\n{messageToSend}"));
            }
        }

        await Task.WhenAll(messageTasks);
    }

    public async Task SendTradeUpdateMessage(Trade trade)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var channels = await GetChannelsForLeague(trade.LeagueYear.League.LeagueID, discordRepo);
        if (!channels.Any())
        {
            return;
        }

        var header = $"The following trade has been **{trade.Status.ReadableName.ToUpper()}**";

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

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, trade.LeagueYear.League.LeagueID,
                trade.LeagueYear.Year)
            .BuildUrl();

        var messageTasks = channels.Select(channel => channel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Trade Update",
            header,
            null,
            embedFieldBuilders,
            leagueUrl)));

        await Task.WhenAll(messageTasks);
    }

    public async Task SendLeagueManagerAnnouncementMessage(LeagueYear leagueYear, string message)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();
        var userStore = scope.ServiceProvider.GetRequiredService<IFantasyCriticUserStore>();

        var channels = await GetChannelsForLeague(leagueYear.League.LeagueID, discordRepo);
        if (!channels.Any())
        {
            return;
        }

        SocketUser? user = null;
        var leagueManagerDiscordUser = await GetDiscordUserIdForFantasyCriticUser(leagueYear.League.LeagueManager, userStore);
        if (leagueManagerDiscordUser != null)
        {
            user = await _client.GetUserAsync(leagueManagerDiscordUser.Value) as SocketUser;
        }

        IEnumerable<Task<RestUserMessage?>> messageTasks;
        if (user != null)
        {
            messageTasks = channels.Select(channel => channel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                "New Message from the League Manager",
                message,
                user)));
        }
        else
        {
            messageTasks = channels.Select(channel => channel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
                "New Message from the League Manager",
                message,
                new EmbedFooterBuilder
                {
                    Text = "Error finding Discord user for League Manager."
                })));
        }
        await Task.WhenAll(messageTasks);
    }

    public async Task SendFinalYearStandings(IReadOnlyList<LeagueYear> leagueYears, LocalDate dateToCheck)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var fantasyCriticRepo = scope.ServiceProvider.GetRequiredService<IFantasyCriticRepo>();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var systemWideValues = await fantasyCriticRepo.GetSystemWideValues();

        var messageTasks = new List<Task>();
        foreach (var leagueYear in leagueYears)
        {
            var previousYearWinner = await fantasyCriticRepo.GetLeagueYearWinner(leagueYear.League.LeagueID, leagueYear.Year - 1);
            var leagueChannels = await discordRepo.GetLeagueChannels(leagueYear.League.LeagueID);
            var publisherLines = DiscordSharedMessageUtilities.RankLeaguePublishers(leagueYear, previousYearWinner, systemWideValues, dateToCheck, true);
            if (publisherLines.Count == 0)
            {
                continue;
            }

            var publisherStrings = string.Join("\n", publisherLines);

            var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueYear.League.LeagueID,
                    leagueYear.Year)
                .BuildUrl();

            foreach (var minimalLeagueChannel in leagueChannels)
            {
                var guild = _client.GetGuild(minimalLeagueChannel.GuildID);
                var channel = guild?.GetChannel(minimalLeagueChannel.ChannelID);
                if (channel is not SocketTextChannel textChannel)
                {
                    continue;
                }

                messageTasks.Add(textChannel.TrySendMessageAsync(embed: _discordFormatter.BuildRegularEmbed(
                    $"Final Standings for {leagueYear.League.LeagueName} ({leagueYear.Year})",
                    publisherStrings,
                    url: leagueUrl)));
            }
        }

        await Task.WhenAll(messageTasks);
    }

    private static async Task<ulong?> GetDiscordUserIdForFantasyCriticUser(FantasyCriticUser fantasyCriticUser, IFantasyCriticUserStore userStore)
    {
        var externalLogins = await userStore.GetLoginsAsync(fantasyCriticUser, CancellationToken.None);
        var discordProviderKey = externalLogins.SingleOrDefault(x => x.LoginProvider == "discord")?.ProviderKey;
        if (discordProviderKey is not null)
        {
            return ulong.TryParse(discordProviderKey, out var discordUserId)
                ? discordUserId
                : null;
        }
        return null;
    }

    private static string BuildTradeMessage(Trade trade, bool includeMessage)
    {
        var message = $"**{trade.Proposer.GetPublisherAndUserDisplayName()}** will receive: ";

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
            message += $"**${trade.CounterPartyBudgetSendAmount}** of budget";
        }

        message += $"\n**{trade.CounterParty.GetPublisherAndUserDisplayName()}** will receive: ";

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
            message += $"\nMessage from **{trade.Proposer.GetPublisherAndUserDisplayName()}**: **{trade.Message}**";
        }

        return message;
    }

    private async Task<IReadOnlyList<SocketTextChannel>> GetChannelsForLeague(Guid leagueID, IDiscordRepo discordRepo)
    {
        var leagueChannels = await discordRepo.GetLeagueChannels(leagueID);
        return GetSocketTextChannels(leagueChannels);
    }

    private IReadOnlyList<SocketTextChannel> GetSocketTextChannels(IEnumerable<MinimalLeagueChannel> leagueChannels)
    {
        var channels = new List<SocketTextChannel>();
        foreach (var leagueChannel in leagueChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            var channel = guild?.GetTextChannel(leagueChannel.ChannelID);
            if (channel is not null)
            {
                channels.Add(channel);
            }
        }

        return channels;
    }

    private static string BuildGameListText(IEnumerable<MasterGameYearWithCounterPick> games)
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

    private static Task LogMessage(LogMessage msg)
    {
        Logger.Information(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task SendSpecialAuctionMessage(SpecialAuction specialAuction, string specialAuctionAction)
    {
        bool shouldRun = await StartBot();
        if (!shouldRun)
        {
            return;
        }

        var serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var discordRepo = scope.ServiceProvider.GetRequiredService<IDiscordRepo>();

        var leagueChannels = await discordRepo.GetLeagueChannels(specialAuction.LeagueYearKey.LeagueID);
        if (!leagueChannels.Any())
        {
            return;
        }

        var messageTasks = new List<Task>();

        var leagueLink = new LeagueUrlBuilder(_baseAddress, specialAuction.LeagueYearKey.LeagueID, specialAuction.LeagueYearKey.Year).BuildUrl();

        foreach (var leagueChannel in leagueChannels)
        {
            var guild = _client.GetGuild(leagueChannel.GuildID);
            var channel = guild?.GetChannel(leagueChannel.ChannelID);
            if (channel is not SocketTextChannel textChannel)
            {
                continue;
            }

            SocketRole? roleToMention = null;
            if (leagueChannel.BidAlertRoleID != null)
            {
                roleToMention =
                    channel.Guild.Roles.FirstOrDefault(r => r.Id == leagueChannel.BidAlertRoleID);
            }

            var currentInstant = _clock.GetCurrentInstant();
            var duration = specialAuction.ScheduledEndTime - currentInstant;

            var title = "";

            var embedFieldBuilders = new List<EmbedFieldBuilder>();

            switch (specialAuctionAction)
            {
                case "create":
                    title = "Special Auction Created";
                    embedFieldBuilders = new List<EmbedFieldBuilder>
                    {
                        new()
                        {
                            Name = "Game To Bid On",
                            Value = specialAuction.MasterGameYear.MasterGame.GameName,
                            IsInline = false
                        },
                        new()
                        {
                            Name = "Time Until Auction Ends",
                            Value = DiscordSharedMessageUtilities.BuildRemainingTimeMessage(duration),
                            IsInline = false
                        }
                    };
                    break;
                case "cancel":
                    title = "Special Auction Cancelled";
                    embedFieldBuilders = new List<EmbedFieldBuilder>
                    {
                        new()
                        {
                            Name = "Game That Was Being Bid On",
                            Value = specialAuction.MasterGameYear.MasterGame.GameName,
                            IsInline = false
                        }
                    };
                    break;
            }

            if (!embedFieldBuilders.Any())
            {
                continue;
            }

            messageTasks.Add(textChannel.TrySendMessageAsync(roleToMention?.Mention ?? "", embed: _discordFormatter.BuildRegularEmbed(
            title,
            "",
            null,
            embedFieldBuilders,
            leagueLink)));
        }

        await Task.WhenAll(messageTasks);
    }

    private static async Task<IReadOnlyList<CombinedChannel>> GetAllCombinedChannels(IDiscordRepo discordRepo, IFantasyCriticRepo fantasyCriticRepo)
    {
        var leagueChannelsTask = discordRepo.GetAllLeagueChannels();
        var gameNewsChannelsTask = discordRepo.GetAllGameNewsChannels();
        await Task.WhenAll(leagueChannelsTask, gameNewsChannelsTask);

        var leagueChannels = await leagueChannelsTask;
        var gameNewsChannels = await gameNewsChannelsTask;

        var leagueIDs = leagueChannels.Select(x => x.LeagueID).Distinct().ToList();
        var leagueYears = await fantasyCriticRepo.GetActiveLeagueYears(leagueIDs);
        var leagueYearLookup = leagueYears.ToLookup(x => x.League.LeagueID);

        var leagueChannelDictionary = leagueChannels.ToDictionary(x => x.ChannelKey);
        var gameNewsChannelDictionary = gameNewsChannels.ToDictionary(x => x.ChannelKey);

        var channelKeys = leagueChannels.Select(x => x.ChannelKey)
            .Concat(gameNewsChannels.Select(x => x.ChannelKey)).Distinct().ToList();

        List<CombinedChannel> combinedChannels = new List<CombinedChannel>();
        foreach (var channelKey in channelKeys)
        {
            var leagueChannel = leagueChannelDictionary.GetValueOrDefault(channelKey);
            var gameNewsChannel = gameNewsChannelDictionary.GetValueOrDefault(channelKey);
            MultiYearLeagueChannel? multiYearLeagueChannel = null;
            if (leagueChannel is not null)
            {
                var activeLeagueYears = leagueYearLookup[leagueChannel.LeagueID].ToList();
                multiYearLeagueChannel = leagueChannel.ToMultiYearLeagueChannel(activeLeagueYears);
            }

            combinedChannels.Add(new CombinedChannel(multiYearLeagueChannel, gameNewsChannel));
        }

        return combinedChannels;
    }
}
