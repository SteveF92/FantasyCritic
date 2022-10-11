using Discord;
using Discord.WebSocket;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Discord.UrlBuilders;
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
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[] { };

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetUpcomingGamesCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        string baseAddress)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = baseAddress;
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        var dateToCheck = _clock.GetToday();

        var leagueChannel = await _discordRepo.GetLeagueChannel(command.Channel.Id.ToString(), dateToCheck.Year);
        if (leagueChannel == null)
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Upcoming Games",
                "No league configuration found for this channel.",
                command.User));
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;

        var leagueYearPublisherPairs = leagueYear.Publishers.Select(publisher => new LeagueYearPublisherPair(leagueYear, publisher));

        var upcomingGamesData = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, false, dateToCheck);
        if (upcomingGamesData.Count == 0)
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Upcoming Games",
                "No data found.",
                command.User));
        }

        var message = "";

        foreach (var upcomingGame in upcomingGamesData)
        {
            var publisher =
                leagueYear.Publishers.First(p => p.PublisherGames.ContainsGame(upcomingGame.Key.MasterGame));
            var upcomingGameMessage = BuildUpcomingGameMessage(publisher, upcomingGame.Key.MasterGame);
            message += upcomingGameMessage;
        }

        await command.RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Upcoming Publisher Releases",
            message,
            command.User));
    }

    private string BuildUpcomingGameMessage(Publisher publisher, MasterGame masterGame)
    {
        var gameUrl = new GameUrlBuilder(_baseAddress, masterGame.MasterGameID).BuildUrl(masterGame.GameName);
        return $"**{masterGame.EstimatedReleaseDate}** - {gameUrl} - {publisher.PublisherName} ({publisher.User.UserName})\n";
    }
}
