using Discord;
using Discord.WebSocket;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Discord.Commands;
public class GetUpcomingGamesCommand : ICommand
{
    public string Name => "upcoming";
    public string Description => "Get upcoming releases for publishers in the league.";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[]
    {

    };

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly string _baseAddress;

    public GetUpcomingGamesCommand(IDiscordRepo discordRepo, IClock clock, string baseAddress)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _baseAddress = baseAddress;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        var dateToCheck = _clock.GetToday();

        var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), dateToCheck.Year);
        if (leagueChannel == null)
        {
            await command.RespondAsync($"Error: No league configuration found for this channel in {dateToCheck.Year}.");
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;

        var leagueYearPublisherPairs = leagueYear.Publishers.Select(publisher => new LeagueYearPublisherPair(leagueYear, publisher));

        var upcomingGamesData = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, false, dateToCheck);
        if (upcomingGamesData.Count == 0)
        {
            await command.RespondAsync("No data found!");
        }

        var message = "";

        foreach (var upcomingGame in upcomingGamesData)
        {
            var publisher =
                leagueYear.Publishers.First(p => p.PublisherGames.ContainsGame(upcomingGame.Key.MasterGame));
            message += $"**{upcomingGame.Key.MasterGame.EstimatedReleaseDate}** - {upcomingGame.Key.MasterGame.GameName} - {publisher.PublisherName} ({publisher.User.UserName})\n";
        }

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Upcoming Publisher Releases")
            .WithDescription(message)
            .WithFooter($"Requested by {command.User.Username}", command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
            .WithColor(16777215)
            .WithCurrentTimestamp();
        await command.RespondAsync(embed: embedBuilder.Build());
    }
}
