using System.Globalization;
using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Discord.Commands;
public class GetPublisherCommand : ICommand
{
    public string Name => "publisher";
    public string Description => "Get publisher information. You can search with just a portion of the name.";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[]
        {
            new()
            {
                Name = "publisher_or_player_name",
                Description = "The name of the publisher or the name of the player. You can input only a portion of the name.",
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
    private readonly string _baseAddress;

    public GetPublisherCommand(IDiscordRepo discordRepo, IClock clock, IParameterParser parameterParser, string baseAddress)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _parameterParser = parameterParser;
        _baseAddress = baseAddress;
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

        var termToSearch = command.Data.Options
            .First(o => o.Name == "publisher_or_player_name")
            .Value
            .ToString()!
            .ToLower()
            .Trim();

        if (termToSearch.Length < 2)
        {
            await command.RespondAsync($"Please provide at least 3 characters to search with.");
            return;
        }

        var foundByPlayerName =
            leagueChannel.LeagueYear.Publishers.Where(p => p.User.UserName.ToLower().Contains(termToSearch)).ToList();

        var foundByPublisherName =
            leagueChannel.LeagueYear.Publishers.Where(p => p.PublisherName.ToLower().Contains(termToSearch)).ToList();

        if (!foundByPlayerName.Any() && !foundByPublisherName.Any())
        {
            var noMatchesEmbedBuilder = new EmbedBuilder()
                .WithTitle("No Matches Found")
                .WithDescription("No matches were found for your query.")
                .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                .WithColor(16777215)
                .WithCurrentTimestamp();
            await command.RespondAsync(embed: noMatchesEmbedBuilder.Build());
            return;
        }

        if (foundByPlayerName.Count > 1 || foundByPublisherName.Count > 1)
        {
            var message = "";

            if (foundByPlayerName.Any())
            {
                message +=
                    $"Match by player name: ${string.Join(", ", foundByPlayerName.Select(p => p.User.UserName))}";
            }
            if (foundByPublisherName.Any())
            {
                message +=
                    $"Match by publisher name: ${string.Join(", ", foundByPublisherName.Select(p => p.PublisherName))}";
            }

            var multipleMatchesEmbedBuilder = new EmbedBuilder()
                .WithTitle("Multiple Matches Found")
                .WithDescription(message)
                .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                .WithColor(16777215)
                .WithCurrentTimestamp();
            await command.RespondAsync(embed: multipleMatchesEmbedBuilder.Build());
            return;
        }
        else if (foundByPlayerName.Any() && foundByPublisherName.Any())
        {
            var inBothLists = foundByPlayerName
                .Select(publisher => foundByPublisherName.FirstOrDefault(p => p.PublisherID == publisher.PublisherID))
                .Where(inOtherList => inOtherList != null)
                .ToList();

            if (inBothLists.Count != foundByPlayerName.Count)
            {
                var message =
                    $"Match by player name: {string.Join(", ", foundByPlayerName.Select(p => p.User.UserName))}\n";
                message += $"Match by publisher name: {string.Join(", ", foundByPublisherName.Select(p => p.PublisherName))}\n";
                var multipleMatchesEmbedBuilder = new EmbedBuilder()
                    .WithTitle("Multiple Matches Found")
                    .WithDescription(message)
                    .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                    .WithColor(16777215)
                    .WithCurrentTimestamp();
                await command.RespondAsync(embed: multipleMatchesEmbedBuilder.Build());
                return;
            }
        }

        //after all that, we should have just one publisher found

        Publisher? publisherFound = null;

        if (foundByPlayerName.Count == 1)
        {
            publisherFound = foundByPlayerName[0];
        }
        else if (foundByPublisherName.Count == 1)
        {
            publisherFound = foundByPublisherName[0];
        }

        if (publisherFound == null)
        {
            await command.RespondAsync("Something went wrong.");
            return;
        }

        var pickedGames = publisherFound.PublisherGames.Where(g => !g.CounterPick).OrderBy(g => g.SlotNumber);
        var counterPickedGames = publisherFound.PublisherGames.Where(g => g.CounterPick).OrderBy(g => g.SlotNumber);

        var gamesMessage = string.Join("\n", pickedGames.Select(MakeGameMessage));
        var counterPickMessage = string.Join("\n", counterPickedGames.Select(MakeGameMessage));

        var leagueOptionWillReleaseDroppableGames = leagueChannel.LeagueYear.Options.WillReleaseDroppableGames;
        var leagueOptionWillNotReleaseDroppableGames = leagueChannel.LeagueYear.Options.WillNotReleaseDroppableGames;
        var leagueOptionsFreeDroppableGames = leagueChannel.LeagueYear.Options.FreeDroppableGames;

        var remainingWillReleaseDrops = leagueOptionWillReleaseDroppableGames == -1
            ? "♾️"
            : (leagueOptionWillReleaseDroppableGames - publisherFound.WillReleaseGamesDropped).ToString();

        var remainingWillNotReleaseDrops = leagueOptionWillNotReleaseDroppableGames == -1
            ? "♾️"
            : (leagueOptionWillNotReleaseDroppableGames - publisherFound.WillNotReleaseGamesDropped).ToString();

        var remainingFreeDroppableGames = leagueOptionsFreeDroppableGames == -1
            ? "️♾️"
            : (leagueOptionsFreeDroppableGames - publisherFound.FreeGamesDropped).ToString();

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"{publisherFound.PublisherName} (Player: {publisherFound.User.UserName})")
            .WithFields(new List<EmbedFieldBuilder>
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
                    Value = publisherFound.Budget,
                    IsInline = false
                },
                new()
                {
                    Name = "'Will Release' Drops Remaining",
                    Value = MakeDropDisplay(remainingWillReleaseDrops, leagueOptionWillReleaseDroppableGames)
                },
                new()
                {
                    Name = "'Will Not Release' Drops Remaining",
                    Value = MakeDropDisplay(remainingWillNotReleaseDrops, leagueOptionWillNotReleaseDroppableGames)
                },
                new()
                {
                    Name = "'Unrestricted' Drops Remaining",
                    Value = MakeDropDisplay(remainingFreeDroppableGames, leagueOptionsFreeDroppableGames)
                }
            })
            .WithDescription($"[Visit Publisher Page]({_baseAddress}/publisher/{publisherFound.PublisherID}/)")
            .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
            .WithColor(16777215)
            .WithCurrentTimestamp();
        await command.RespondAsync(embed: embedBuilder.Build());
    }

    private string MakeDropDisplay(string remaining, int total)
    {
        if (total == 0)
        {
            return "N/A";
        }
        if (total == -1)
        {
            return "♾️";
        }
        return $"{remaining}/{total}";
    }
    private string MakeGameMessage(PublisherGame g)
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
