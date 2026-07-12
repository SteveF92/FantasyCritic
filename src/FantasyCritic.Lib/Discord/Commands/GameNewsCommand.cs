using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;

public class GameNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly PublisherService _publisherService;
    private readonly IReadOnlyFantasyCriticUserStore _fantasyCriticUserStore;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;
    private const string UpcomingValue = "Upcoming";
    private const string RecentValue = "Recent";
    private const string TimeRangeAll = "All";
    private const string TimeRangeToday = "Today";
    private const string TimeRangeThisWeek = "ThisWeek";
    private const string TimeRangeThisMonth = "ThisMonth";

    public GameNewsCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        PublisherService publisherService,
        IReadOnlyFantasyCriticUserStore fantasyCriticUserStore,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _publisherService = publisherService;
        _fantasyCriticUserStore = fantasyCriticUserStore;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [UsedImplicitly]
    [SlashCommand("game-news",
        "Get upcoming or recent releases for publishers in the league (or for yourself in a DM).")]
    public async Task GetGameNews(
        [Summary("upcoming_or_recent", "Whether you want upcoming or recent releases")]
        [Choice("Upcoming Releases", UpcomingValue)]
        [Choice("Recent Releases", RecentValue)]
        string upcomingOrRecent = UpcomingValue,
        [Summary("time_range", "Filter to a specific time range")]
        [Choice("All", TimeRangeAll)]
        [Choice("Today", TimeRangeToday)]
        [Choice("This Week", TimeRangeThisWeek)]
        [Choice("This Month", TimeRangeThisMonth)]
        string timeRange = TimeRangeAll)
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var isRecentReleases = upcomingOrRecent == RecentValue;
        var cutoffDate = GetCutoffDate(dateToCheck, timeRange, isRecentReleases);
        var hasTimeFilter = timeRange != TimeRangeAll;

        if (Context.Channel is IDMChannel)
        {
            var user = await _fantasyCriticUserStore.GetFantasyCriticUserForDiscordUser(Context.User.Id);
            if (user == null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "No User Found",
                    "You must link your Discord account to your Fantasy Critic account on the Fantasy Critic website to use this bot via DM.",
                    Context.User));
                return;
            }

            var myGameNews = await _publisherService.GetMyGameNews(user);
            var myGameNewsSet = MyGameNewsSet.BuildMyGameNews(myGameNews, dateToCheck, 10);
            var gameNewsToUse = isRecentReleases ? myGameNewsSet.RecentGames : myGameNewsSet.UpcomingGames;

            if (hasTimeFilter)
            {
                gameNewsToUse = gameNewsToUse
                    .Where(x => x.MasterGameYear.MasterGame.ReleaseDate.HasValue
                        && (isRecentReleases
                            ? x.MasterGameYear.MasterGame.ReleaseDate.Value >= cutoffDate
                            : x.MasterGameYear.MasterGame.ReleaseDate.Value <= cutoffDate))
                    .ToList();
            }

            if (gameNewsToUse.Count == 0)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "No Releases Found",
                    "No data found.",
                    Context.User));
                return;
            }

            var formattedMessage = DiscordSharedMessageUtilities.BuildDateGroupedGameMessagesForUser(gameNewsToUse, _baseAddress);

            var dmTitle = hasTimeFilter
                ? $"Your Releases {GetTimeRangeLabel(timeRange)} ({user.UserName})"
                : $"Your {upcomingOrRecent} Releases ({user.UserName})";

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                dmTitle,
                formattedMessage,
                Context.User));
        }
        else
        {
            var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
            if (leagueChannel == null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Getting Game News",
                    "No league configuration found for this channel.",
                    Context.User));
                return;
            }

            var leagueYear = leagueChannel.LeagueYear;
            var leagueYearPublisherPairs =
                leagueYear.Publishers.Select(publisher => new LeagueYearPublisherPair(leagueYear, publisher));

            if (leagueYear.Publishers.Count == 0 || leagueYear.Publishers.Sum(p => p.PublisherGames.Count) == 0)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Getting Game News",
                    "No publisher games found, have you done your draft yet?",
                    Context.User));
                return;
            }

            var gameNewsData = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, dateToCheck, isRecentReleases, 10);
            if (gameNewsData.Count == 0)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Getting Game News",
                    "No data found.",
                    Context.User));
                return;
            }

            var matchedGames = DiscordSharedMessageUtilities.ConvertToMatchedGameDisplays(gameNewsData, leagueYear);

            if (hasTimeFilter)
            {
                matchedGames = matchedGames
                    .Where(x => x.GameFound.MasterGame.ReleaseDate.HasValue
                        && (isRecentReleases
                            ? x.GameFound.MasterGame.ReleaseDate.Value >= cutoffDate
                            : x.GameFound.MasterGame.ReleaseDate.Value <= cutoffDate))
                    .ToList();
            }

            if (matchedGames.Count == 0)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Getting Game News",
                    "No data found.",
                    Context.User));
                return;
            }

            var formattedMessage = DiscordSharedMessageUtilities.BuildDateGroupedGameMessages(matchedGames, _baseAddress);

            if (formattedMessage.Length > 4096)
            {
                formattedMessage = "There are too many games to list in a Discord Message.";
            }

            var leagueTitle = hasTimeFilter
                ? $"Publisher Releases {GetTimeRangeLabel(timeRange)}"
                : $"{upcomingOrRecent} Publisher Releases";

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                leagueTitle,
                formattedMessage,
                Context.User));
        }
    }

    private static LocalDate GetCutoffDate(LocalDate dateToCheck, string timeRange, bool isRecent) => timeRange switch
    {
        TimeRangeToday => isRecent ? dateToCheck.PlusDays(-1) : dateToCheck.PlusDays(1),
        TimeRangeThisWeek => isRecent ? dateToCheck.PlusWeeks(-1) : dateToCheck.PlusWeeks(1),
        TimeRangeThisMonth => isRecent ? dateToCheck.PlusMonths(-1) : dateToCheck.PlusMonths(1),
        _ => isRecent ? LocalDate.MinIsoValue : LocalDate.MaxIsoValue
    };

    private static string GetTimeRangeLabel(string timeRange) => timeRange switch
    {
        TimeRangeToday => "Today",
        TimeRangeThisWeek => "This Week",
        TimeRangeThisMonth => "This Month",
        _ => ""
    };
}
