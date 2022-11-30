using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetRecentGamesCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetRecentGamesCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("recent", "Get recent releases for publishers in the league.")]
    public async Task GetRecentGames()
    {
        var dateToCheck = _clock.GetGameEffectiveDate();

        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, dateToCheck.Year);
        if (leagueChannel == null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Recent Games",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;

        var leagueYearPublisherPairs = leagueYear.Publishers.Select(publisher => new LeagueYearPublisherPair(leagueYear, publisher));

        var recentGamesData = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, true, dateToCheck);
        if (recentGamesData.Count == 0)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Recent Games",
                "No data found.",
                Context.User));
        }

        var message = "";

        foreach (var recentGame in recentGamesData)
        {
            var publisher =
                leagueYear.Publishers.First(p => p.PublisherGames.ContainsGame(recentGame.Key.MasterGame));
            message += BuildGameMessage(publisher, recentGame.Key.MasterGame);
        }

        await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Recent Publisher Releases",
            message,
            Context.User));
    }

    private string BuildGameMessage(Publisher publisher, MasterGame masterGame)
    {
        var gameUrl = new GameUrlBuilder(_baseAddress, masterGame.MasterGameID).BuildUrl(masterGame.GameName);
        return $"**{masterGame.EstimatedReleaseDate}** - {gameUrl} - {publisher.GetPublisherAndUserDisplayName()}\n";
    }
}
