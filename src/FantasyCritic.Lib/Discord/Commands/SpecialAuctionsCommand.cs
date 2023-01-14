using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class SpecialAuctionsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public SpecialAuctionsCommand(IDiscordRepo discordRepo,
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

    [SlashCommand("special-auctions", "View the special auctions that are currently active.")]
    public async Task GetSpecialAuctions(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null)
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);
        
        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                $"That year was not found for this league. Are you sure a league year is started for {year.Value}?",
                Context.User));
            return;
        }
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                "No league configuration found for this channel. You may have to specify a year if your league is for an upcoming year.",
                Context.User));
            return;
        }

        var leagueYear = leagueChannel.LeagueYear;

        var specialAuctions = await _gameAcquisitionService.GetActiveSpecialAuctionsForLeague(leagueYear);

        if (specialAuctions == null || !specialAuctions.Any())
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "No Special Auctions",
                "There are no special auctions running currently.",
                Context.User));
            return;
        }

        var specialAuctionMessages = new List<string>();
        foreach (var specialAuction in specialAuctions)
        {
            var specialAuctionMessage = $"**{specialAuction.MasterGameYear.MasterGame.GameName}**";
            var currentInstant = _clock.GetCurrentInstant();
            var duration = specialAuction.ScheduledEndTime - currentInstant;
            specialAuctionMessage += $"\n> Time Until Auction Ends: {DiscordSharedMessageUtilities.BuildRemainingTimeMessage(duration)}";
            specialAuctionMessages.Add(specialAuctionMessage);
        }
        
        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueYear.League.LeagueID,
            leagueYear.Year)
            .BuildUrl();

        var finalMessage = string.Join("\n", specialAuctionMessages);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Special Auction(s)",
            finalMessage,
            Context.User,
            url: leagueUrl));
    }
}
