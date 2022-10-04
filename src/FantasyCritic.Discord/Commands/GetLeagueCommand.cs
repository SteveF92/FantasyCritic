using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Discord.Commands;
public class GetLeagueCommand : ICommand
{
    public string Name => "league";
    public string Description => "Get league information.";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[]
    {
        new()
        {
            Name = "year",
            Description = "The year for the league (if not entered, defaults to the current year).",
            Type = ApplicationCommandOptionType.Integer,
            IsRequired = false
        }
    };

    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;
    private readonly IParameterParser _parameterParser;
    private readonly string _baseUrl;

    public GetLeagueCommand(IDiscordRepo discordRepo, IFantasyCriticRepo fantasyCriticRepo, IClock clock,
        IParameterParser parameterParser, string baseUrl)
    {
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
        _parameterParser = parameterParser;
        _baseUrl = baseUrl;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        try
        {
            var providedYear = command.Data.Options.FirstOrDefault(o => o.Name == "year");
            var dateToCheck = _parameterParser.GetDateFromProvidedYear(providedYear) ?? _clock.GetToday();

            var systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
            var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), dateToCheck.Year);
            if (leagueChannel == null)
            {
                await command.RespondAsync($"Error: No league configuration found for this channel in {dateToCheck.Year}.");
                return;
            }

            var rankedPublishers = leagueChannel.LeagueYear.Publishers.OrderBy(p
                => p.GetTotalFantasyPoints(leagueChannel.LeagueYear.SupportedYear, leagueChannel.LeagueYear.Options));

            var publisherLines =
                rankedPublishers
                    .Select((publisher, index) =>
                    {
                        var totalPoints = publisher
                            .GetTotalFantasyPoints(leagueChannel.LeagueYear.SupportedYear, leagueChannel.LeagueYear.Options);

                        var projectedPoints = publisher
                            .GetProjectedFantasyPoints(leagueChannel.LeagueYear,
                                systemWideValues, dateToCheck);

                        return BuildPublisherLine(index + 1, publisher, totalPoints, projectedPoints, dateToCheck);
                    });

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{leagueChannel.LeagueYear.League.LeagueName} {leagueChannel.LeagueYear.Year}")
                .WithDescription(string.Join("\n", publisherLines))
                .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                .WithColor(16777215)
                .WithCurrentTimestamp()
                .WithUrl($"{_baseUrl}league/{leagueChannel.LeagueYear.League.LeagueID}/{leagueChannel.LeagueYear.Year}");

            await command.RespondAsync(embed: embedBuilder.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving LeagueChannel {ex.Message}");
            await command.RespondAsync("There was an error executing this command. Please try again.");
        }
    }

    private string BuildPublisherLine(int rank, Publisher publisher, decimal totalPoints, decimal projectedPoints, LocalDate currentDate)
    {
        //TODO: is there a way to reuse MinimalPublisherViewModel code for this part?
        var allWillRelease = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.WillRelease());

        var gamesReleased = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.MasterGame!.MasterGame.IsReleased(currentDate));

        var publisherLine = $"**{rank}.**";
        publisherLine += $"{(string.IsNullOrEmpty(publisher.PublisherIcon) ? $"{publisher.PublisherIcon} " : "")}**{publisher.PublisherName}** ";
        publisherLine += $"({publisher.User.UserName})\n";
        publisherLine += $"> **{Math.Round(totalPoints, 1)} points** ";
        publisherLine += $"*(Projected: {Math.Round(projectedPoints, 1)})*\n";
        publisherLine += $"> {gamesReleased}/{allWillRelease + gamesReleased} games released";

        return publisherLine;
    }
}
