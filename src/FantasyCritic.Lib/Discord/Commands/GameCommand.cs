using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;

public class GameCommand : InteractionModuleBase<SocketInteractionContext>
{
    private const int MaxNumberOfConferenceSearchButtons = 3;
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly GameSearchingService _gameSearchingService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly ConferenceService _conferenceService;
    private readonly string _baseAddress;

    public GameCommand(IDiscordRepo discordRepo,
        IClock clock,
        GameSearchingService gameSearchingService,
        InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings,
        IMasterGameRepo masterGameRepo,
        ConferenceService conferenceService
    )
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _gameSearchingService = gameSearchingService;
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _masterGameRepo = masterGameRepo;
        _conferenceService = conferenceService;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [UsedImplicitly]
    [SlashCommand("game", "Get game information. You can search with just a portion of the name.")]
    public async Task GetGame(
            [Summary("game_name",
                "The game name that you're searching for. You only need to enter a portion of the name.")]
            string gameName,
            [Summary("year", "The year for the league (if not entered, defaults to the current year).")]
            int? year = null)
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);

        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                $"That year was not found for this league. Are you sure a league year is started for {year.Value}?",
                Context.User));
            return;
        }

        LeagueChannel? leagueChannel =
            await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        var termToSearch = gameName.ToLower().Trim();

        if (termToSearch.Length < 2)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding Game",
                "Please provide at least 3 characters to search with.",
                Context.User));
            return;
        }

        var matchingGames = await _gameSearchingService.SearchGamesWithLeaguePriority(termToSearch, dateToCheck.Year, 3,
            leagueChannel?.LeagueYear);
        if (!matchingGames.Any())
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding Game",
                "No games found! Please check your search and try again.",
                Context.User));
            return;
        }

        var gamesToDisplay = matchingGames
            .Select(game => new MatchedGameDisplay(game)
            {
                PublisherWhoPicked = leagueChannel != null
                    ? FindPublisherWithGame(leagueChannel.LeagueYear, game, false)
                    : null,
                PublisherWhoCounterPicked = leagueChannel != null
                    ? FindPublisherWithGame(leagueChannel.LeagueYear, game, true)
                    : null
            }).ToList();

        var gameEmbeds = gamesToDisplay
            .Select(matchedGameDisplay =>
            {
                var masterGameYear = matchedGameDisplay.GameFound;
                return new EmbedFieldBuilder
                {
                    Name = masterGameYear.MasterGame.GameName,
                    Value = BuildGameDisplayText(matchedGameDisplay, leagueChannel?.LeagueYear, dateToCheck),
                    IsInline = false
                };
            }).ToList();

        var buttonBuilder = new ComponentBuilder();

        if (matchingGames.Count > 0)
        {
            Guid? conferenceId = null;
            if (leagueChannel != null)
            {
                conferenceId = leagueChannel.LeagueYear.League.ConferenceID;
            }
            else
            {
                var conferenceChannel =
                    await _discordRepo.GetConferenceChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
                if (conferenceChannel != null)
                {
                    conferenceId = conferenceChannel.ConferenceYear.Conference.ConferenceID;
                }
            }

            if (conferenceId != null)
            {
                var numberOfButtons = matchingGames.Count < MaxNumberOfConferenceSearchButtons
                    ? matchingGames.Count
                    : MaxNumberOfConferenceSearchButtons;
                for (var i = 0; i < numberOfButtons; i++)
                {
                    var gameToFind = matchingGames[i];
                    buttonBuilder.WithButton($"Find {gameToFind.MasterGame.GameName} in Conference",
                        $"conferenceGameSearch:{gameToFind.MasterGame.MasterGameID}_{conferenceId}_{gameToFind.Year}",
                        emote: new Emoji("🔎"));
                }
            }
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            gameEmbeds.Count == 0
                ? "No Games Found"
                : $"{gameEmbeds.Count} Game{(gameEmbeds.Count > 1 ? "(s)" : "")} Found",
            "",
            Context.User,
            gameEmbeds), components: buttonBuilder.Build()
            );
    }

    [UsedImplicitly]
    [ComponentInteraction("conferenceGameSearch:*_*_*")]
    public async Task SearchGameInConference(string masterGameId, string conferenceId, int year)
    {
        await DeferAsync();

        var dateToCheck = _clock.GetGameEffectiveDate(year);

        var gameToSearch = await _masterGameRepo.GetMasterGameYear(new Guid(masterGameId), year);
        var conferenceYear = await _conferenceService.GetConferenceYear(new Guid(conferenceId), year);
        if (gameToSearch != null && conferenceYear != null)
        {
            var matchingGames = await _gameSearchingService.SearchGameInConferenceYear(gameToSearch, conferenceYear);
            if (matchingGames.Count > 0)
            {

                var gamesToDisplay = matchingGames
                    .Select(matchedGameAndLeagueYear => new MatchedGameDisplay(gameToSearch)
                    {
                        PublisherWhoPicked = FindPublisherWithGame(matchedGameAndLeagueYear.LeagueYear,
                            matchedGameAndLeagueYear.MasterGameYear, false),
                        PublisherWhoCounterPicked = FindPublisherWithGame(matchedGameAndLeagueYear.LeagueYear,
                            matchedGameAndLeagueYear.MasterGameYear, true),
                        LeagueYear = matchedGameAndLeagueYear.LeagueYear
                    }).ToList();

                var gameEmbeds = gamesToDisplay
                    .Select(matchedGameDisplay => new EmbedFieldBuilder
                    {
                        Name = matchedGameDisplay.LeagueYear!.League.LeagueName,
                        Value = BuildConferenceGameDisplayText(matchedGameDisplay, dateToCheck),
                        IsInline = false
                    }).ToList();


                var title = $"Conference Leagues with {gameToSearch.MasterGame.GameName}";
                await Context.Interaction.FollowupAsync(
                    embed: _discordFormatter.BuildRegularEmbedWithUserFooter(title, "", Context.User, gameEmbeds));
            }
            else
            {
                await Context.Interaction.FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "No Game(s) Found", "Could not find that game in the conference.",
                    Context.User));
            }
        }
        else
        {
            if (gameToSearch == null)
            {
                await Context.Interaction.FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "No Game found", $"Could not find that game in the year {year}.",
                    Context.User));
            }
            else if (conferenceYear == null)
            {
                await Context.Interaction.FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "No Conference found", $"Could not find that conference in the year {year}.",
                    Context.User));
            }
        }
    }

    private string BuildConferenceGameDisplayText(MatchedGameDisplay matchedGameDisplay, LocalDate dateToCheck)
    {
        var gameFound = matchedGameDisplay.GameFound;

        var releaseDateDisplayText = GetReleaseDateText(gameFound, dateToCheck);

        var gameDisplayText = "";

        var publisherWhoPicked = matchedGameDisplay.PublisherWhoPicked;
        if (publisherWhoPicked != null)
        {
            gameDisplayText +=
                $"\n**Picked by:** {publisherWhoPicked.GetPublisherAndUserDisplayName()}";
        }

        var publisherWhoCounterPicked = matchedGameDisplay.PublisherWhoCounterPicked;
        if (publisherWhoCounterPicked != null)
        {
            gameDisplayText +=
                $"\n**Counter Picked by:** {publisherWhoCounterPicked.GetPublisherAndUserDisplayName()}\n";
        }

        gameDisplayText += $"\n**Release Date:** {releaseDateDisplayText}";
        gameDisplayText += $"\n**Release Status (for {dateToCheck.Year}):** {(gameFound.MasterGame.IsReleased(dateToCheck) ? "Released" : gameFound.GetWillReleaseStatus().ReadableName)}";

        gameDisplayText += $"\n**Hype Factor:** {Math.Round(gameFound.HypeFactor, 1)}";
        gameDisplayText += $"\n**% Drafted:** {(gameFound.PercentStandardGame == 0 ? "N/A" : $"{Math.Round(gameFound.PercentStandardGame * 100, 0)}%")}";
        gameDisplayText += $"\n**% Counter Picked:** {(gameFound.PercentCounterPick == 0 ? "N/A" : $"{Math.Round(gameFound.PercentCounterPick * 100, 0)}%")}";

        var gameUrlBuilder = new GameUrlBuilder(_baseAddress, gameFound.MasterGame.MasterGameID);
        gameDisplayText += $"\n{gameUrlBuilder.BuildUrl("View Game")}";

        return gameDisplayText;
    }

    private string BuildGameDisplayText(MatchedGameDisplay matchedGameDisplay, LeagueYear? leagueYear, LocalDate dateToCheck)
    {
        var gameFound = matchedGameDisplay.GameFound;

        var releaseDateDisplayText = GetReleaseDateText(gameFound, dateToCheck);

        var gameDisplayText = "";

        if (leagueYear != null)
        {
            var publisherWhoPicked = matchedGameDisplay.PublisherWhoPicked;
            if (publisherWhoPicked != null)
            {
                var score = gameFound.GetFantasyPoints(leagueYear.Options.ReleaseSystem, leagueYear.Options.ScoringSystem, false, dateToCheck);
                gameDisplayText +=
                    $"\n**Picked by:** {publisherWhoPicked.GetPublisherAndUserDisplayName()}";

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
                    $"\n**Counter Picked by:** {publisherWhoCounterPicked.GetPublisherAndUserDisplayName()}";

                if (score.HasValue)
                {
                    gameDisplayText += $" ({score.Value})";
                }
            }
        }

        gameDisplayText += $"\n**Release Date:** {releaseDateDisplayText}";
        gameDisplayText += $"\n**Release Status (for {dateToCheck.Year}):** {(gameFound.MasterGame.IsReleased(dateToCheck) ? "Released" : gameFound.GetWillReleaseStatus().ReadableName)}";

        if (gameFound.MasterGame.CriticScore.HasValue)
        {
            gameDisplayText += $"\n**Score:** {Math.Round(gameFound.MasterGame.CriticScore.Value, 1)}";
        }
        else
        {
            var projectedScore = gameFound.GetProjectedFantasyPoints(leagueYear?.Options.ScoringSystem ?? ScoringSystem.GetDefaultScoringSystem(dateToCheck.Year), false);
            gameDisplayText += $"\n**Projected Score:** {Math.Round(projectedScore, 1)}";
        }

        gameDisplayText += $"\n**Hype Factor:** {Math.Round(gameFound.HypeFactor, 1)}";
        gameDisplayText += $"\n**% Drafted:** {(gameFound.PercentStandardGame == 0 ? "N/A" : $"{Math.Round(gameFound.PercentStandardGame * 100, 0)}%")}";
        gameDisplayText += $"\n**% Counter Picked:** {(gameFound.PercentCounterPick == 0 ? "N/A" : $"{Math.Round(gameFound.PercentCounterPick * 100, 0)}%")}";

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
        }
        ;

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
