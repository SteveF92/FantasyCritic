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
public class GetRecentGamesCommand : ICommand
{
    public string Name => "recent";
    public string Description => "Get recent releases for publishers in the league.";
    public SlashCommandOptionBuilder[] Options => new SlashCommandOptionBuilder[] { };

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetRecentGamesCommand(IDiscordRepo discordRepo,
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
                "Error Getting Recent Games",
                "No league configuration found for this channel.",
                command.User));
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;

        var leagueYearPublisherPairs = leagueYear.Publishers.Select(publisher => new LeagueYearPublisherPair(leagueYear, publisher));

        var recentGamesData = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, true, dateToCheck);
        if (recentGamesData.Count == 0)
        {
            await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Recent Games",
                "No data found.",
                command.User));
        }

        var message = "";

        foreach (var recentGame in recentGamesData)
        {
            var publisher =
                leagueYear.Publishers.First(p => p.PublisherGames.ContainsGame(recentGame.Key.MasterGame));
            message += BuildGameMessage(publisher, recentGame.Key.MasterGame);
        }

        await command.RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Recent Publisher Releases",
            message,
            command.User));
    }

    private string BuildGameMessage(Publisher publisher, MasterGame masterGame)
    {
        var gameUrl = new GameUrlBuilder(_baseAddress, masterGame.MasterGameID).BuildUrl(masterGame.GameName);
        return $"**{masterGame.EstimatedReleaseDate}** - {gameUrl} - {publisher.PublisherName} ({publisher.User.UserName})\n";
    }
}
