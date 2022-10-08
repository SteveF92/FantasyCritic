using Discord;
using Discord.WebSocket;
using FantasyCritic.Discord.Models;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Discord.Commands;
public class GetGameCommand : ICommand
{
    public string Name => "game";
    public string Description => "Get game information. You can search with just a portion of the name.";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[]
        {
            new()
            {
                Name = "game_name",
                Description = "The game name that you're searching for. You can input only a portion of the name.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true
            },
            new()
            {
                Name = "year",
                Description = "The year for the league (if not entered, defaults to the current year).",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = false
            }
        };

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IParameterParser _parameterParser;
    private readonly GameSearchingService _gameSearchingService;
    private readonly string _baseUrl;

    public GetGameCommand(IDiscordRepo discordRepo, IClock clock, IParameterParser parameterParser, GameSearchingService gameSearchingService, string baseUrl)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _parameterParser = parameterParser;
        _gameSearchingService = gameSearchingService;
        _baseUrl = baseUrl;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        var providedYear = command.Data.Options.FirstOrDefault(o => o.Name == "year");
        var dateToCheck = _parameterParser.GetDateFromProvidedYear(providedYear) ?? _clock.GetToday();

        var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), dateToCheck.Year);
        if (leagueChannel == null)
        {
            await command.RespondAsync($"Error: No league configuration found for this channel in {dateToCheck.Year}.");
            return;
        }
        
        var leagueYear = leagueChannel.LeagueYear;

        var termToSearch = command.Data.Options
            .First(o => o.Name == "game_name")
            .Value
            .ToString()!
            .ToLower()
            .Trim();

        if (termToSearch.Length < 2)
        {
            await command.RespondAsync($"Please provide at least 3 characters to search with.");
            return;
        }

        // TODO: remove accented characters from strings, Pokemon for example

        var matchingGames = await _gameSearchingService.SearchGamesWithLeaguePriority(termToSearch, leagueYear, 3);

        if (!matchingGames.Any())
        {
            await command.RespondAsync("No games found! Please check your search and try again.");
            return;
        }

        var gamesToDisplay = matchingGames
            .Select(game => new MatchedGameDisplay(game)
            {
                PublisherWhoPicked = FindPublisherWithGame(leagueYear, game, false),
                PublisherWhoCounterPicked = FindPublisherWithGame(leagueYear, game, true)
            }).ToList();

        var gameEmbeds = gamesToDisplay
            .Select(matchedGameDisplay =>
            {
                MasterGameYear masterGameYear = matchedGameDisplay.GameFound;
                return new EmbedFieldBuilder
                {
                    Name = masterGameYear.MasterGame.GameName,
                    Value = BuildGameDisplayText(matchedGameDisplay, leagueChannel.LeagueYear, dateToCheck),
                    IsInline = false
                };
            }).ToList();

        var embedBuilder = new EmbedBuilder()
            .WithTitle(gameEmbeds.Count == 0 ? "No Games Found" : "Games Found")
            .WithFields(gameEmbeds)
            .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
            .WithColor(16777215)
            .WithCurrentTimestamp();
        await command.RespondAsync(embed: embedBuilder.Build());
    }

    private string BuildGameTitleDisplayWithUrl(MasterGame masterGame)
    {
        return $"[View Game]({_baseUrl}/mastergame/{masterGame.MasterGameID})";
    }

    private string BuildGameDisplayText(MatchedGameDisplay matchedGameDisplay, LeagueYear leagueYear, LocalDate dateToCheck)
    {
        var gameFound = matchedGameDisplay.GameFound;

        var releaseDate = gameFound.MasterGame.ReleaseDate?.ToString() ??
                          $"{gameFound.MasterGame.EstimatedReleaseDate} (est)";
        var gameDisplayText =
            $"**Release Date:** {releaseDate}";

        var publisherWhoPicked = matchedGameDisplay.PublisherWhoPicked;
        if (publisherWhoPicked != null)
        {
            var score = gameFound.GetFantasyPoints(leagueYear.Options.ScoringSystem, false, dateToCheck);
            gameDisplayText +=
                $"\n**Picked:** {publisherWhoPicked.PublisherName} ({publisherWhoPicked.User.UserName})";

            if (!gameFound.WillRelease())
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
            var score = gameFound.GetFantasyPoints(leagueYear.Options.ScoringSystem, true, dateToCheck);
            gameDisplayText +=
                $"\n**Counter Picked:** {publisherWhoCounterPicked.PublisherName} ({publisherWhoCounterPicked.User.UserName})";

            if (score.HasValue)
            {
                gameDisplayText += $" ({score.Value})";
            }
        }

        gameDisplayText += $"\n{BuildGameTitleDisplayWithUrl(gameFound.MasterGame)}";

        return gameDisplayText;
    }

    private Publisher? FindPublisherWithGame(LeagueYear leagueYear, MasterGameYear game, bool lookingForCounterPick)
    {
        return leagueYear.Publishers.FirstOrDefault(p =>
            p.PublisherGames.Any(publisherGame =>
                publisherGame.MasterGame?.MasterGame.MasterGameID == game.MasterGame.MasterGameID
                && publisherGame.CounterPick == lookingForCounterPick));
    }
}
