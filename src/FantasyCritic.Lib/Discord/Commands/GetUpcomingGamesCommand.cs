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

        List<string> messages = new List<string>();
        foreach (var recentGameGrouping in upcomingGamesData)
        {
            var standardGame = recentGameGrouping.FirstOrDefault(p => !p.CounterPick);
            var counterPick = recentGameGrouping.FirstOrDefault(p => p.CounterPick);
            var standardPublisher = leagueYear.Publishers.FirstOrDefault(p => standardGame is not null && p.PublisherID == standardGame.PublisherID);
            var counterPickPublisher = leagueYear.Publishers.FirstOrDefault(p => counterPick is not null && p.PublisherID == counterPick.PublisherID);

            var gameMessage = DiscordUtilities.BuildGameMessage(standardPublisher, counterPickPublisher, recentGameGrouping.Key.MasterGame, _baseAddress);
            if (gameMessage is not null)
            {
                messages.Add(gameMessage);
            }
        }

        var message = string.Join("\n--------------------------------\n", messages);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Upcoming Publisher Releases",
            message,
            Context.User));
    }
}
