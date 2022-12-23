using Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetGameCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly GameSearchingService _gameSearchingService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetGameCommand(IDiscordRepo discordRepo,
        IClock clock,
        GameSearchingService gameSearchingService,
        InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings
        )
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _gameSearchingService = gameSearchingService;
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("game", "Get game information. You can search with just a portion of the name.")]
    public async Task GetGame(
        [Summary("game_name", "The game name that you're searching for. You can input only a portion of the name.")] string gameName,
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null)
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);

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
                "Error Finding League Configuration",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var termToSearch = gameName.ToLower().Trim();

        if (termToSearch.Length < 2)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding Game",
                "Please provide at least 3 characters to search with.",
                Context.User));
            return;
        }

        var matchingGames = await _gameSearchingService.SearchGamesWithLeaguePriority(termToSearch,
            leagueChannel.LeagueYear, 3);
        if (!matchingGames.Any())
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding Game",
                "No games found! Please check your search and try again.",
                Context.User));
            return;
        }

        var gamesToDisplay = matchingGames
            .Select(game => new MatchedGameDisplay(game)
            {
                PublisherWhoPicked = FindPublisherWithGame(leagueChannel.LeagueYear, game, false),
                PublisherWhoCounterPicked = FindPublisherWithGame(leagueChannel.LeagueYear, game, true)
            }).ToList();

        var gameEmbeds = gamesToDisplay
            .Select(matchedGameDisplay =>
            {
                var masterGameYear = matchedGameDisplay.GameFound;
                return new EmbedFieldBuilder
                {
                    Name = masterGameYear.MasterGame.GameName,
                    Value = BuildGameDisplayText(matchedGameDisplay, leagueChannel.LeagueYear, dateToCheck),
                    IsInline = false
                };
            }).ToList();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            gameEmbeds.Count == 0
                ? "No Games Found"
                : $"{gameEmbeds.Count} Game{(gameEmbeds.Count > 1 ? "(s)" : "")} Found",
            "",
            Context.User,
            gameEmbeds));
    }

    private string BuildGameDisplayText(MatchedGameDisplay matchedGameDisplay, LeagueYear leagueYear, LocalDate dateToCheck)
    {
        var gameFound = matchedGameDisplay.GameFound;

        var releaseDateDisplayText = GetReleaseDateText(gameFound, dateToCheck);

        var gameDisplayText = $"**Release Date:** {releaseDateDisplayText}";

        var projectedScore = gameFound.GetProjectedFantasyPoints(leagueYear.Options.ScoringSystem, false);

        gameDisplayText += $"\n**Projected Score:** {Math.Round(projectedScore, 1)}";
        gameDisplayText += $"\n**Hype Factor:** {Math.Round(gameFound.HypeFactor, 1)}";
        gameDisplayText += $"\n**% Drafted:** {(gameFound.PercentStandardGame == 0 ? "N/A" : $"{Math.Round(gameFound.PercentStandardGame * 100, 0)}%")}";
        gameDisplayText += $"\n**% Counter Picked:** {(gameFound.PercentCounterPick == 0 ? "N/A" : $"{Math.Round(gameFound.PercentCounterPick * 100, 0)}%")}";

        var publisherWhoPicked = matchedGameDisplay.PublisherWhoPicked;
        if (publisherWhoPicked != null)
        {
            var score = gameFound.GetFantasyPoints(leagueYear.Options.ReleaseSystem, leagueYear.Options.ScoringSystem, false, dateToCheck);
            gameDisplayText +=
                $"\n**Picked:** {publisherWhoPicked.GetPublisherAndUserDisplayName()}";

            if (!gameFound.CouldRelease())
            {
                gameDisplayText += " (Will Not Release This Year)";
            }
            else if (score.HasValue)
            {
                gameDisplayText += $" ({score.Value})";
            }
        }

        var publisherWhoCounterPicked = matchedGameDisplay.PublisherWhoCounterPicked;
        if (publisherWhoCounterPicked != null)
        {
            var score = gameFound.GetFantasyPoints(leagueYear.Options.ReleaseSystem, leagueYear.Options.ScoringSystem, true, dateToCheck);
            gameDisplayText +=
                $"\n**Counter Picked:** {publisherWhoCounterPicked.GetPublisherAndUserDisplayName()}";

            if (score.HasValue)
            {
                gameDisplayText += $" ({score.Value})";
            }
        }

        var gameUrlBuilder = new GameUrlBuilder(_baseAddress, gameFound.MasterGame.MasterGameID);
        gameDisplayText += $"\n{gameUrlBuilder.BuildUrl("View Game")}";

        return gameDisplayText;
    }

    private string GetReleaseDateText(MasterGameYear gameFound, LocalDate dateToCheck)
    {
        var releaseDateDisplayText = $"{gameFound.MasterGame.EstimatedReleaseDate} (est)";

        if (gameFound.MasterGame.ReleaseDate == null)
        {
            return releaseDateDisplayText;
        }
        releaseDateDisplayText = $"{gameFound.MasterGame.ReleaseDate?.ToString()}";
        if (gameFound.MasterGame.IsReleased(dateToCheck) || _clock.GetToday().Year != dateToCheck.Year)
        {
            return releaseDateDisplayText;
        }
        var daysUntilRelease = GetDaysUntilRelease(gameFound.MasterGame.ReleaseDate, dateToCheck);
        if (daysUntilRelease > 0)
        {
            releaseDateDisplayText += $" ({daysUntilRelease} days until release)";
        };

        return releaseDateDisplayText;
    }

    private static int GetDaysUntilRelease(LocalDate? releaseDate, LocalDate dateToCheck)
    {
        return releaseDate == null ? -1 : Period.DaysBetween(dateToCheck, releaseDate.Value);
    }

    private static Publisher? FindPublisherWithGame(LeagueYear leagueYear, MasterGameYear game, bool lookingForCounterPick)
    {
        return leagueYear.Publishers.FirstOrDefault(p =>
            p.PublisherGames.Any(publisherGame =>
                publisherGame.MasterGame?.MasterGame.MasterGameID == game.MasterGame.MasterGameID
                && publisherGame.CounterPick == lookingForCounterPick));
    }
}
