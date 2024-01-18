using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class PublicBidsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public PublicBidsCommand(IDiscordRepo discordRepo,
        IFantasyCriticRepo fantasyCriticRepo,
        InterLeagueService interLeagueService,
        GameAcquisitionService gameAcquisitionService,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _interLeagueService = interLeagueService;
        _gameAcquisitionService = gameAcquisitionService;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("public-bids", "View the current public bids for this week.")]
    public async Task GetPublicBids(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null)
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);
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

        if (leagueYear.Options.PickupSystem.Equals(PickupSystem.SecretBidding))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Bidding is Secret",
                "This league does not use public bidding.",
                Context.User));
            return;
        }

        if (!_gameAcquisitionService.IsInPublicBiddingWindow(leagueYear))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Bids Not Revealed Yet",
                "The league is not in the public bidding window yet.",
                Context.User));
            return;
        }

        var allPublicBiddingGames = await _gameAcquisitionService.GetPublicBiddingGames(dateToCheck.Year);

        var leagueYearPublicBids =
            allPublicBiddingGames.FirstOrDefault(g => g.LeagueYear.League.LeagueID == leagueYear.League.LeagueID)?.MasterGames;

        if (leagueYearPublicBids == null || !leagueYearPublicBids.Any())
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                "No Public Bids",
                "No public bids were found for this week.",
                Context.User));
            return;
        }

        var gameMessages = leagueYearPublicBids.Select(DiscordSharedMessageUtilities.BuildPublicBidGameMessage).ToList();
        var finalMessage = string.Join("\n", gameMessages);
        var lastSunday = DiscordSharedMessageUtilities.GetLastSunday();
        var header = $"Public Bids (Week of {lastSunday:MMMM dd, yyyy})";

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueYear.League.LeagueID,
            leagueYear.Year)
            .BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            header,
            finalMessage,
            Context.User,
            url: leagueUrl));
    }
}
