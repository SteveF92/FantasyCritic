using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetUpcomingGamesCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetUpcomingGamesCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("upcoming", "Get upcoming releases for publishers in the league.")]
    public async Task GetUpcomingGames()
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate();

        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, dateToCheck.Year);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Upcoming Games",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;

        var leagueYearPublisherPairs = leagueYear.Publishers.Select(publisher => new LeagueYearPublisherPair(leagueYear, publisher));

        var upcomingGamesData = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, false, dateToCheck);
        if (upcomingGamesData.Count == 0)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Upcoming Games",
                "No data found.",
                Context.User));
        }

        var message = "";

        foreach (var upcomingGameGrouping in upcomingGamesData)
        {
            var publisherGame = upcomingGameGrouping.FirstOrDefault(p => !p.CounterPick);

            if (publisherGame == null)
            {
                continue;
            }

            var publisher =
                leagueYear.Publishers.FirstOrDefault(p => p.PublisherID == publisherGame.PublisherID);
            if (publisher != null)
            {
                message += BuildGameMessage(publisher, upcomingGameGrouping.Key.MasterGame);
            }
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Upcoming Publisher Releases",
            message,
            Context.User));
    }

    private string BuildGameMessage(Publisher publisher, MasterGame masterGame)
    {
        var gameUrl = new GameUrlBuilder(_baseAddress, masterGame.MasterGameID).BuildUrl(masterGame.GameName);
        return $"**{masterGame.EstimatedReleaseDate}** - {gameUrl} - {publisher.GetPublisherAndUserDisplayName()}\n";
    }
}
