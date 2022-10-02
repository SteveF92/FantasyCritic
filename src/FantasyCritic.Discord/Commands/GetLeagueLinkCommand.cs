using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Discord.Commands;
public class GetLeagueLinkCommand : ICommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SlashCommandOptionBuilder[] Options { get; set; }

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;

    public GetLeagueLinkCommand(IDiscordRepo discordRepo, IClock clock)
    {
        Name = "link";
        Description = "Get a link to the league.";
        Options = new SlashCommandOptionBuilder[]
        {
            new()
            {
                Name = "year",
                Description = "The year for the league.",
                Type = ApplicationCommandOptionType.Integer,
                IsRequired = false
            }
        };
        _discordRepo = discordRepo;
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

            var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), currentDate.Year);
            if (leagueChannel == null)
            {
                await command.RespondAsync("Error: No league configuration found for this channel.");
                return;
            }

            var leagueUrl =
                $"https://www.fantasycritic.games/league/{leagueChannel.LeagueYear.League.LeagueID}/{leagueChannel.LeagueYear.Year}";

            var embedBuilder = new EmbedBuilder()
                .WithTitle(
                    $"Click here to visit the site for the league {leagueChannel.LeagueYear.League.LeagueName} ({leagueChannel.LeagueYear.Year})")
                .WithUrl(leagueUrl)
                .WithFooter($"Requested by {command.User.Username}",
                    command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
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
