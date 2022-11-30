using System.Globalization;
using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetPublisherCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetPublisherCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("publisher", "Get publisher information. You can search with just a portion of the name.")]
    public async Task GetPublisher(
        [Summary("publisher_or_player_name", "The name of the publisher or the name of the player. You can input only a portion of the name.")] string publisherOrPlayerName,
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
    {
        var dateToCheck = _clock.GetGameEffectiveDate(year);

        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, dateToCheck.Year);
        if (leagueChannel == null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var termToSearch = publisherOrPlayerName.ToLower().Trim();

        if (termToSearch.Length < 2)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "Please provide at least 3 characters to search with.",
                Context.User));
            return;
        }

        var foundByPlayerName =
            leagueChannel.LeagueYear.Publishers.Where(p => p.User.UserName.ToLower().Contains(termToSearch)).ToList();

        var foundByPublisherName =
            leagueChannel.LeagueYear.Publishers.Where(p => p.PublisherName.ToLower().Contains(termToSearch)).ToList();

        if (!foundByPlayerName.Any() && !foundByPublisherName.Any())
        {
            await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
                "No Matches Found",
                "No matches were found for your query.",
                Context.User));
            return;
        }

        var message = BuildMessageForMultiplePublishersFound(foundByPlayerName, foundByPublisherName);
        if (message != "")
        {
            await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
                "Multiple Matches Found",
                message,
                Context.User));
            return;
        }

        var publisherFound = GetPublisherFromFoundLists(foundByPlayerName, foundByPublisherName);

        if (publisherFound == null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Publisher",
                "Something went wrong.",
                Context.User));
            return;
        }

        var pickedGames = GetSortedPublisherGames(publisherFound, false);
        var counterPickedGames = GetSortedPublisherGames(publisherFound, true);

        var remainingWillReleaseDrops =
            GetDropsRemainingText(leagueChannel.LeagueYear.Options.WillReleaseDroppableGames,
                publisherFound.WillReleaseGamesDropped);
        var remainingWillNotReleaseDrops = GetDropsRemainingText(leagueChannel.LeagueYear.Options.WillNotReleaseDroppableGames, publisherFound.WillNotReleaseGamesDropped);
        var remainingFreeDroppableGames = GetDropsRemainingText(leagueChannel.LeagueYear.Options.FreeDroppableGames, publisherFound.FreeGamesDropped);

        var publisherUrlBuilder = new PublisherUrlBuilder(_baseAddress, publisherFound.PublisherID);

        var embedFieldBuilders = BuildEmbedFieldBuilders(
            string.Join("\n", pickedGames.Select(BuildGameMessage)),
            string.Join("\n", counterPickedGames.Select(BuildGameMessage)),
            publisherFound,
            leagueChannel,
            remainingWillReleaseDrops,
            leagueChannel.LeagueYear.Options.WillReleaseDroppableGames,
            remainingWillNotReleaseDrops,
            leagueChannel.LeagueYear.Options.WillNotReleaseDroppableGames,
            remainingFreeDroppableGames,
            leagueChannel.LeagueYear.Options.FreeDroppableGames);

        await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"{publisherFound.GetPublisherAndUserDisplayName()}",
            publisherUrlBuilder.BuildUrl("View Publisher"),
            Context.User,
            embedFieldBuilders));
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
            if (g.MasterGame != null)
            {
                if (g.MasterGame.MasterGame.ReleaseDate != null)
                {
                    gameMessage += $" - {g.MasterGame.MasterGame.ReleaseDate?.ToString()[10..]}"; // TODO: why was I using substring here?
                }
                else
                {
                    gameMessage += $" - {g.MasterGame.MasterGame.EstimatedReleaseDate} (est)";
                }
            }
        }

        return gameMessage;
    }
}
