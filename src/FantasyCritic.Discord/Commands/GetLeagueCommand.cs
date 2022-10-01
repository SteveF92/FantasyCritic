using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Discord.Commands;
public class GetLeagueCommand : ICommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SlashCommandOptionBuilder[] Options { get; set; }

    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;

    public GetLeagueCommand(IDiscordRepo discordRepo, IFantasyCriticRepo fantasyCriticRepo, IClock clock)
    {
        Name = "league";
        Description = "Get league information.";
        Options = new SlashCommandOptionBuilder[]
        {
            new()
            {
                Name = "year",
                Description = "The year for the league",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = false
            }
        };
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        try
        {
            var currentDate = _clock.GetToday();

            var providedYear = command.Data.Options.FirstOrDefault();
            if (providedYear != null)
            {
                var yearValue = (long)providedYear.Value;
                var convertedYear = Convert.ToInt32(yearValue);
                currentDate = new LocalDate(convertedYear, 12, 31);
            }

            var systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
            var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), currentDate.Year);
            if (leagueChannel == null)
            {
                await command.RespondAsync("Error: No league configuration found for this channel.");
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
                                systemWideValues, currentDate);

                        return BuildPublisherLine(index + 1, publisher, totalPoints, projectedPoints, currentDate);
                    });

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{leagueChannel.LeagueYear.League.LeagueName} {leagueChannel.LeagueYear.Year}")
                .WithDescription(string.Join("\n", publisherLines))
                .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                .WithColor(16777215)
                .WithCurrentTimestamp();

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

        var publisherLine = $"**{rank}**";
        publisherLine += $"{(string.IsNullOrEmpty(publisher.PublisherIcon) ? $"{publisher.PublisherIcon} " : "")}**{publisher.PublisherName}** ";
        publisherLine += $"({publisher.User.UserName})\n";
        publisherLine += $"> **{Math.Round(totalPoints, 1)} points** ";
        publisherLine += $"*(Projected: {Math.Round(projectedPoints, 1)})*\n";
        publisherLine += $"> {gamesReleased}/{allWillRelease + gamesReleased} games released";

        return publisherLine;
    }
}
