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
            //new()
            //{
            //    Name = "user",
            //    Description = "The users whose roles you want to be listed",
            //    Type = ApplicationCommandOptionType.User,
            //    IsRequired = true
            //}
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

            var systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
            var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), currentDate.Year);
            //var guildUser = (SocketGuildUser)command.Data.Options.First().Value;
            //var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));
            //var embedBuilder = new EmbedBuilder()
            //    .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            //    .WithTitle("Roles")
            //    .WithDescription(roleList)
            //    .WithColor(Color.Green)
            //    .WithCurrentTimestamp();

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
                .WithFooter($"Requested by {command.User.Username}")
                .WithColor(16777215)
                .WithCurrentTimestamp();

            await command.RespondAsync(embed: embedBuilder.Build());

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving LeagueChannel {ex.Message}");
        }
    }

    private string BuildPublisherLine(int rank, Publisher publisher, decimal totalPoints, decimal projectedPoints, LocalDate currentDate)
    {
        var dateToCheck = currentDate;

        //TODO: do I need to do this?
        //if (leagueYear.SupportedYear.Finished)
        //{
        //    dateToCheck = new LocalDate(Year, 12, 31);
        //}

        //TODO: is there a way to reuse MinimalPublisherViewModel code for this part?
        var allWillRelease = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.WillRelease());

        var gamesReleased = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.MasterGame!.MasterGame.IsReleased(dateToCheck));

        var publisherLine = $"**{rank}**";
        publisherLine += $"{(string.IsNullOrEmpty(publisher.PublisherIcon) ? $"{publisher.PublisherIcon} " : "")}**{publisher.PublisherName}** ";
        publisherLine += $"({publisher.User.UserName})\n";
        publisherLine += $"> **{Math.Round(totalPoints, 1)} points** ";
        publisherLine += $"*(Projected: {Math.Round(projectedPoints, 1)})*\n";
        publisherLine += $"> {gamesReleased}/{allWillRelease + gamesReleased} games released";

        return publisherLine;
    }
}
