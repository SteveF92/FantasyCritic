using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Discord.Commands;
public class BidResultsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly InterLeagueService _interLeagueService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public BidResultsCommand(IDiscordRepo discordRepo,
        FantasyCriticService fantasyCriticService,
        InterLeagueService interLeagueService,
        GameAcquisitionService gameAcquisitionService,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
        _gameAcquisitionService = gameAcquisitionService;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("bid-results", "View the most recent bid results.")]
    public async Task GetBidResults(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null)
    {
        await DeferAsync();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                $"That year was not found for this league. Are you sure a league year is started for {year.Value}?",
                Context.User));
            return;
        }
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                "No league configuration found for this channel. You may have to specify a year if your league is for an upcoming year.",
                Context.User));
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;

        var leagueActionSets = await _fantasyCriticService.GetLeagueActionProcessingSets(leagueYear);
        var normalActionSets = leagueActionSets.Where(x => !x.ProcessName.Contains("Special Auction")).ToList();
        if (!normalActionSets.Any())
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Actions To Report",
                "This league does not have any actions yet to report.",
                Context.User));
            return;
        }

        //For testing
        //var leagueActionSetToReport = normalActionSets.Single(x => x.ProcessTime.ToEasternDate().ToISOString() == "2023-11-11");

        var leagueActionSetToReport = normalActionSets.OrderByDescending(s => s.ProcessTime).First();
        var bidsByGame = leagueActionSetToReport.Bids.GroupToDictionary(x => x.MasterGame);

        var bidResultMessages = new List<string>();
        foreach (var bidGameAction in bidsByGame)
        {
            var messageToAdd = $"**{bidGameAction.Key}**\n";
            var orderedBids = bidGameAction.Value.OrderByDescending(x => x.Successful!.Value).ThenByDescending(x => x.BidAmount).ToList();
            foreach (var bid in orderedBids)
            {
                messageToAdd += $"{DiscordSharedMessageUtilities.BuildBidResultMessage(bid)}\n";
            }

            bidResultMessages.Add($"{messageToAdd}");
        }

        var dropResultMessages = leagueActionSetToReport.Drops.Select(DiscordSharedMessageUtilities.BuildDropResultMessage).ToList();

        var header = $"Bid/Drop Results (Week ending {leagueActionSetToReport.ProcessTime.ToEasternDate():MMMM dd, yyyy})";

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueYear.League.LeagueID,
            leagueYear.Year)
            .BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            header,
            "",
            Context.User,
            new List<EmbedFieldBuilder>
            {
              new()
              {
                  Name = "Bids",
                  Value = bidResultMessages.Any() ? string.Join("\n", bidResultMessages) : "No bids this week.",
                  IsInline = false,
              },
              new()
              {
              Name = "Drops",
              Value = dropResultMessages.Any() ? string.Join("\n", dropResultMessages) : "No drops this week.",
              IsInline = false,
            }
            },
            url: leagueUrl));
    }
}
