using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;
public class TrendingCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly Uri _baseUri;
    private const string TrendingBidsValue = "Bids";
    private const string TrendingCounterPicksValue = "Counter Picks";
    private const string TrendingDropsValue = "Drops";
    private const int GameLimit = 10;

    public TrendingCommand(InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _baseUri = new Uri(fantasyCriticSettings.BaseAddress);
    }

    [UsedImplicitly]
    [SlashCommand("trending",
        "Get trending bids/counter picks/drops for the previous week.")]
    public async Task GetTrending(
        [Summary("trending_topic", "Whether you want trending bids, counter picks, or drops")]
        [Choice("Bids", TrendingBidsValue)]
        [Choice("Counter Picks", TrendingCounterPicksValue)]
        [Choice("Drops", TrendingDropsValue)]
        string trendingTopic)
    {
        await DeferAsync();

        var processingDatesWithData = await _interLeagueService.GetProcessingDatesForTopBidsAndDrops();
        if (!processingDatesWithData.Any())
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Loading Trends",
                "The trends could not be loaded right now. Please try again later.",
                Context.User));
        }

        var dateToUse = processingDatesWithData.Max();
        var allTopBidsAndDrops = await _interLeagueService.GetTopBidsAndDrops(dateToUse);

        var topBidsAndDropsByYear = allTopBidsAndDrops.GroupToDictionary(x => x.MasterGameYear.Year);
        var yearWithMostData = topBidsAndDropsByYear.MaxBy(x => x.Value.Count).Key;
        var topBidsAndDropsToUse = topBidsAndDropsByYear[yearWithMostData];

        var trends = topBidsAndDropsToUse
            .Where(g =>
            {
                return trendingTopic switch
                {
                    TrendingBidsValue => g.TotalStandardBidCount > 0,
                    TrendingCounterPicksValue => g.TotalCounterPickBidCount > 0,
                    TrendingDropsValue => g.TotalDropCount > 0,
                    _ => false
                };
            })
            .OrderByDescending(g =>
            {
                return trendingTopic switch
                {
                    TrendingBidsValue => g.TotalStandardBidCount,
                    TrendingCounterPicksValue => g.TotalCounterPickBidCount,
                    TrendingDropsValue => g.TotalDropCount,
                    _ => 0
                };
            })
            .Take(GameLimit);

        var formattedTrendingItems = trends.Select(g =>
        {
            return trendingTopic switch
            {
                TrendingBidsValue => $"**{g.MasterGameYear.MasterGame.GameName}** ({g.TotalStandardBidCount} bids)",
                TrendingCounterPicksValue => $"**{g.MasterGameYear.MasterGame.GameName}** ({g.TotalCounterPickBidCount} counter picks)",
                TrendingDropsValue => $"**{g.MasterGameYear.MasterGame.GameName}** ({g.TotalDropCount} drops)",
                _ => ""
            };
        });

        var messageToSend = string.Join("\n", formattedTrendingItems);

        var trendingPageUrl = new Uri(_baseUri, "topBidsAndDrops");

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            $"Trending {trendingTopic} for the Week Ending on {dateToUse}",
            messageToSend,
            Context.User, null, trendingPageUrl.ToString()));
    }
}
