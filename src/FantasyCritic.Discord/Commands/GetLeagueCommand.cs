using Discord;
using Discord.WebSocket;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Discord.Models;
using FantasyCritic.Discord.UrlBuilders;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Discord.Commands;
public class GetLeagueCommand : ICommand
{
    public string Name => "league";
    public string Description => "Get league information.";
    private const string YearParameterName = "year";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[]
    {
        new()
        {
            Name = YearParameterName,
            Description = "The year for the league (if not entered, defaults to the current year).",
            Type = ApplicationCommandOptionType.Integer,
            IsRequired = false
        }
    };

    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;
    private readonly IDiscordParameterParser _parameterParser;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly DiscordSettings _discordSettings;
    private readonly string _baseAddress;

    public GetLeagueCommand(IDiscordRepo discordRepo,
        IFantasyCriticRepo fantasyCriticRepo,
        IClock clock,
        IDiscordParameterParser parameterParser,
        IDiscordFormatter discordFormatter,
        DiscordSettings discordSettings,
        string baseAddress)
    {
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
        _parameterParser = parameterParser;
        _discordFormatter = discordFormatter;
        _discordSettings = discordSettings;
        _baseAddress = baseAddress;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        var providedYear = command.Data.Options.FirstOrDefault(o => o.Name == YearParameterName);
        var dateToCheck = _parameterParser.GetDateFromProvidedYear(providedYear) ?? _clock.GetToday();

        var systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
        var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), dateToCheck.Year);
        if (leagueChannel == null)
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding Game",
                "No league configuration found for this channel.",
                command.User));
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

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID,
            leagueChannel.LeagueYear.Year)
            .BuildUrl();

        await command.RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"{leagueChannel.LeagueYear.League.LeagueName} {leagueChannel.LeagueYear.Year}",
            string.Join("\n", publisherLines),
            command.User,
            url: leagueUrl));
    }

    private string BuildPublisherLine(int rank, Publisher publisher, decimal totalPoints, decimal projectedPoints, LocalDate currentDate)
    {
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
