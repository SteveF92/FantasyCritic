using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class GameNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;
    private const string UpcomingValue = "Upcoming";
    private const string RecentValue = "Recent";

    public GameNewsCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("game-news", "Get upcoming or recent releases for publishers in the league (or for yourself in a DM).")]
    public async Task GetGameNews(
        [Summary("upcoming_or_recent", "Whether you want upcoming or recent releases")]
        [Choice("Upcoming Releases",UpcomingValue)]
        [Choice("Recent Releases", RecentValue)]
        string upcomingOrRecent = UpcomingValue)
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Game News",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;
        var leagueYearPublisherPairs = leagueYear.Publishers.Select(publisher => new LeagueYearPublisherPair(leagueYear, publisher));

        var isRecentReleases = upcomingOrRecent == RecentValue;
        var gameNewsData = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, isRecentReleases, dateToCheck);
        if (gameNewsData.Count == 0)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting Game News",
                "No data found.",
                Context.User));
        }

        var messages = new List<string>();
        foreach (var recentGameGrouping in gameNewsData)
        {
            var standardGame = recentGameGrouping.FirstOrDefault(p => !p.CounterPick);
            var counterPick = recentGameGrouping.FirstOrDefault(p => p.CounterPick);
            var standardPublisher = leagueYear.Publishers.FirstOrDefault(p => standardGame is not null && p.PublisherID == standardGame.PublisherID);
            var counterPickPublisher = leagueYear.Publishers.FirstOrDefault(p => counterPick is not null && p.PublisherID == counterPick.PublisherID);

            var gameMessage = DiscordSharedMessageUtilities.BuildGameMessage(standardPublisher, counterPickPublisher, recentGameGrouping.Key.MasterGame, _baseAddress);
            if (gameMessage is not null)
            {
                messages.Add(gameMessage);
            }
        }

        var message = string.Join("\n--------------------------------\n", messages);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"{upcomingOrRecent} Publisher Releases",
            message,
            Context.User));
    }
}
