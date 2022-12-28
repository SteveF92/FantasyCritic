using System.Globalization;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetPublisherCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IFantasyCriticUserStore _userStore;
    private readonly string _baseAddress;
    private const string NotFoundByUserErrorMessage = "No matches were found for that user. They may not have their Discord account linked to their Fantasy Critic account, or you might not be in the right channel for their league.";

    public GetPublisherCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        IClock clock,
        IDiscordFormatter discordFormatter,
        IFantasyCriticUserStore userStore,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _userStore = userStore;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("publisher", "Get publisher information. You can search with just a portion of the name.")]
    public async Task GetPublisherSlashCommand(
        [Summary("publisher_or_player_name",
            "The name of the publisher or the name of the player. You can input only a portion of the name.")]
        [MinLength(2)]
        string publisherOrPlayerName = "",
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")]
        int? year = null
    )
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                $"That year was not found for this league. Are you sure a league year is started for {year.Value}?",
                Context.User));
            return;
        }
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        if (publisherOrPlayerName != "" && publisherOrPlayerName.Trim() == "")
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "Please provide at least 3 characters to search with.",
                Context.User));
            return;
        }

        var publisherSearchResults = await FindPublishers(publisherOrPlayerName.ToLower().Trim(), Context.User, leagueChannel, Context.Channel.Id);

        if (!publisherSearchResults.HasAnyResults())
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "No Matches Found",
                "No matches were found.",
                Context.User));
            return;
        }

        var message = BuildMessageForMultiplePublishersFound(
            publisherSearchResults.FoundByPlayerName,
            publisherSearchResults.FoundByPublisherName);

        if (message != "")
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "Multiple Matches Found",
                message,
                Context.User));
            return;
        }

        Publisher? publisherFound;
        if (publisherOrPlayerName == "" && publisherSearchResults.PublisherFoundForDiscordUser != null)
        {
            publisherFound = publisherSearchResults.PublisherFoundForDiscordUser;
        }
        else
        {
            publisherFound = GetPublisherFromFoundLists(publisherSearchResults.FoundByPlayerName,
                publisherSearchResults.FoundByPublisherName);
        }

        if (publisherFound == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "Something went wrong.",
                Context.User));
            return;
        }

        var embedFieldBuilders = BuildPublisherEmbedFieldBuilders(publisherFound, leagueChannel);

        var publisherUrlBuilder = new PublisherUrlBuilder(_baseAddress, publisherFound.PublisherID);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"{publisherFound.GetPublisherAndUserDisplayName()} ({leagueChannel.LeagueYear.Year})",
            publisherUrlBuilder.BuildUrl("View Publisher"),
            Context.User,
            embedFieldBuilders));
    }

    [MessageCommand("FC Publisher")]
    public async Task GetPublisherMessageCommand(SocketMessage message)
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var publisherSearchResults = await FindPublishers("", message.Author, leagueChannel, Context.Channel.Id);

        if (publisherSearchResults.PublisherFoundForDiscordUser == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "No Matches Found",
                NotFoundByUserErrorMessage,
                Context.User));
            return;
        }

        var embedFieldBuilders = BuildPublisherEmbedFieldBuilders(publisherSearchResults.PublisherFoundForDiscordUser, leagueChannel);

        var publisherUrlBuilder = new PublisherUrlBuilder(_baseAddress, publisherSearchResults.PublisherFoundForDiscordUser.PublisherID);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"{publisherSearchResults.PublisherFoundForDiscordUser.GetPublisherAndUserDisplayName()} ({leagueChannel.LeagueYear.Year})",
            publisherUrlBuilder.BuildUrl("View Publisher"),
            Context.User,
            embedFieldBuilders));
    }

    [UserCommand("FC Publisher")]
    public async Task GetPublisherUserCommand(SocketUser user)
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var publisherSearchResults = await FindPublishers("", user, leagueChannel, Context.Channel.Id);

        if (publisherSearchResults.PublisherFoundForDiscordUser == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "No Matches Found",
                NotFoundByUserErrorMessage,
                Context.User));
            return;
        }

        var embedFieldBuilders = BuildPublisherEmbedFieldBuilders(publisherSearchResults.PublisherFoundForDiscordUser, leagueChannel);

        var publisherUrlBuilder = new PublisherUrlBuilder(_baseAddress, publisherSearchResults.PublisherFoundForDiscordUser.PublisherID);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"{publisherSearchResults.PublisherFoundForDiscordUser.GetPublisherAndUserDisplayName()} ({leagueChannel.LeagueYear.Year})",
            publisherUrlBuilder.BuildUrl("View Publisher"),
            Context.User,
            embedFieldBuilders));
    }

    private async Task<PublisherSearchResults> FindPublishers(string publisherOrPlayerName, SocketUser user,
        LeagueChannel leagueChannel, ulong channelId)
    {
        var searchResults = new PublisherSearchResults();

        if (string.IsNullOrEmpty(publisherOrPlayerName)) // find by discord user
        {
            var discordUserId = user.Id.ToString();
            var publisherForUser = await GetPublisherForDiscordUser(discordUserId, leagueChannel, channelId);
            if (publisherForUser != null)
            {
                searchResults.PublisherFoundForDiscordUser = publisherForUser;
            }
        }
        else // find by publisher search
        {
            var termToSearch = publisherOrPlayerName.ToLower().Trim();

            searchResults.FoundByPlayerName = leagueChannel.LeagueYear.Publishers.Where(p => p.User.UserName
                    .ToLower()
                    .Contains(termToSearch))
                .ToList();
            searchResults.FoundByPublisherName = leagueChannel.LeagueYear.Publishers.Where(p => p.PublisherName
                    .ToLower()
                    .Contains(termToSearch))
                .ToList();
        }

        return searchResults;
    }

    private static List<EmbedFieldBuilder> BuildPublisherEmbedFieldBuilders(Publisher publisherFound, LeagueChannel leagueChannel)
    {
        var pickedGames = GetSortedPublisherGames(publisherFound, false);
        var counterPickedGames = GetSortedPublisherGames(publisherFound, true);

        var remainingWillReleaseDrops =
            GetDropsRemainingText(leagueChannel.LeagueYear.Options.WillReleaseDroppableGames,
                publisherFound.WillReleaseGamesDropped);
        var remainingWillNotReleaseDrops = GetDropsRemainingText(
            leagueChannel.LeagueYear.Options.WillNotReleaseDroppableGames, publisherFound.WillNotReleaseGamesDropped);
        var remainingFreeDroppableGames = GetDropsRemainingText(leagueChannel.LeagueYear.Options.FreeDroppableGames,
            publisherFound.FreeGamesDropped);


        var pickedGamesMessage = string.Join("\n", pickedGames.Select(BuildGameMessage));
        var counterPickedGamesMessage = string.Join("\n", counterPickedGames.Select(BuildGameMessage));

        var embedFieldBuilders = BuildEmbedFieldBuilders(
            string.Join("\n", !string.IsNullOrEmpty(pickedGamesMessage) ? pickedGamesMessage : "No games on roster yet"),
            string.Join("\n", !string.IsNullOrEmpty(counterPickedGamesMessage) ? counterPickedGamesMessage : "No counter picks on roster yet"),
            publisherFound,
            leagueChannel,
            remainingWillReleaseDrops,
            leagueChannel.LeagueYear.Options.WillReleaseDroppableGames,
            remainingWillNotReleaseDrops,
            leagueChannel.LeagueYear.Options.WillNotReleaseDroppableGames,
            remainingFreeDroppableGames,
            leagueChannel.LeagueYear.Options.FreeDroppableGames);
        return embedFieldBuilders;
    }

    private static string GetDropsRemainingText(int numberOfDropsAllowed, int dropsDone)
    {
        return numberOfDropsAllowed == -1
            ? "♾️"
            : (numberOfDropsAllowed - dropsDone).ToString();
    }

    private static IEnumerable<PublisherGame> GetSortedPublisherGames(Publisher publisherFound, bool isCounterPick)
    {
        return publisherFound.PublisherGames.Where(g => g.CounterPick == isCounterPick).OrderBy(g => g.SlotNumber);
    }

    private static Publisher? GetPublisherFromFoundLists(IReadOnlyList<Publisher> foundByPlayerName,
        IReadOnlyList<Publisher> foundByPublisherName)
    {
        Publisher? publisherFound = null;

        if (foundByPlayerName.Count == 1)
        {
            publisherFound = foundByPlayerName[0];
        }
        else if (foundByPublisherName.Count == 1)
        {
            publisherFound = foundByPublisherName[0];
        }

        return publisherFound;
    }

    private static string BuildMessageForMultiplePublishersFound(IReadOnlyCollection<Publisher> foundByPlayerName,
        IReadOnlyCollection<Publisher> foundByPublisherName)
    {
        var message = "";

        if (foundByPlayerName.Count > 1 || foundByPublisherName.Count > 1)
        {
            if (foundByPlayerName.Any())
            {
                message +=
                    $"Match by player name: {string.Join(", ", foundByPlayerName.Select(p => p.User.UserName))}";
            }

            if (foundByPublisherName.Any())
            {
                message +=
                    $"Match by publisher name: {string.Join(", ", foundByPublisherName.Select(p => p.PublisherName))}";
            }
        }
        else if (foundByPlayerName.Any() && foundByPublisherName.Any())
        {
            var inBothLists = foundByPlayerName
                .Select(publisher => foundByPublisherName.FirstOrDefault(p => p.PublisherID == publisher.PublisherID))
                .Where(inOtherList => inOtherList != null)
                .ToList();

            if (inBothLists.Count != foundByPlayerName.Count)
            {
                message =
                    $"Match by player name: {string.Join(", ", foundByPlayerName.Select(p => p.User.UserName))}\n";
                message +=
                    $"Match by publisher name: {string.Join(", ", foundByPublisherName.Select(p => p.PublisherName))}\n";
            }
        }

        return message;
    }

    private static List<EmbedFieldBuilder> BuildEmbedFieldBuilders(string gamesMessage,
        string counterPickMessage,
        Publisher publisherFound,
        LeagueChannel leagueChannel,
        string remainingWillReleaseDrops,
        int leagueOptionWillReleaseDroppableGames,
        string remainingWillNotReleaseDrops,
        int leagueOptionWillNotReleaseDroppableGames,
        string remainingFreeDroppableGames,
        int leagueOptionsFreeDroppableGames)
    {
        return new List<EmbedFieldBuilder>
        {
            new()
            {
                Name = "Picks",
                Value = gamesMessage,
                IsInline = false
            },
            new()
            {
                Name = "Counter Picks",
                Value =  counterPickMessage,
                IsInline = false
            },
            new()
            {
                Name = "Current Score",
                Value = Math.Round(publisherFound.GetTotalFantasyPoints(leagueChannel.LeagueYear.SupportedYear, leagueChannel.LeagueYear.Options), 1),
                IsInline = false
            },
            new()
            {
                Name = "Remaining Budget",
                Value = $"{publisherFound.Budget:C}",
                IsInline = false
            },
            new()
            {
                Name = "'Will Release' Drops Remaining",
                Value = BuildDropDisplay(remainingWillReleaseDrops, leagueOptionWillReleaseDroppableGames)
            },
            new()
            {
                Name = "'Will Not Release' Drops Remaining",
                Value = BuildDropDisplay(remainingWillNotReleaseDrops, leagueOptionWillNotReleaseDroppableGames)
            },
            new()
            {
                Name = "'Unrestricted' Drops Remaining",
                Value = BuildDropDisplay(remainingFreeDroppableGames, leagueOptionsFreeDroppableGames)
            }
        };
    }

    private static string BuildDropDisplay(string remaining, int total)
    {
        return total switch
        {
            0 => "N/A",
            -1 => "♾️",
            _ => $"{remaining}/{total}"
        };
    }

    private static string BuildGameMessage(PublisherGame g)
    {
        var gameMessage = g.GameName;
        if (g.FantasyPoints != null)
        {
            // TODO: get manual critic score here if needed
            var criticScore = g.MasterGame?.MasterGame.CriticScore != null
                ? Math.Round(g.MasterGame.MasterGame.CriticScore.Value, 1).ToString(CultureInfo.InvariantCulture)
                : "N/A";

            gameMessage += $" - Score: {criticScore} - Points: {Math.Round(g.FantasyPoints.Value, 1)}";
        }
        else
        {
            if (g.MasterGame == null)
            {
                return gameMessage;
            }
            if (g.MasterGame.MasterGame.ReleaseDate != null)
            {
                gameMessage += $" - {g.MasterGame.MasterGame.ReleaseDate?.ToString()}";
            }
            else
            {
                gameMessage += $" - {g.MasterGame.MasterGame.EstimatedReleaseDate} (est)";
            }
        }

        return gameMessage;
    }

    private async Task<Publisher?> GetPublisherForDiscordUser(string discordUserId, LeagueChannel leagueChannel,
        ulong channelId)
    {
        Publisher? publisherFound = null;
        var fantasyCriticUser = await _userStore.FindByLoginAsync("Discord", discordUserId, CancellationToken.None);
        if (fantasyCriticUser != null)
        {
            publisherFound =
                leagueChannel.LeagueYear.Publishers.FirstOrDefault(p => p.User.Id == fantasyCriticUser.Id
                && leagueChannel.ChannelID == channelId);
        }

        return publisherFound;
    }

}
