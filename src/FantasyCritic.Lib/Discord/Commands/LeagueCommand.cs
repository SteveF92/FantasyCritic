using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;
public class LeagueCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public LeagueCommand(IDiscordRepo discordRepo,
        IFantasyCriticRepo fantasyCriticRepo,
        InterLeagueService interLeagueService,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [UsedImplicitly]
    [SlashCommand("league", "Get league information.")]
    public async Task GetLeague(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);

        var systemWideValues = await _fantasyCriticRepo.GetSystemWideValues();
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

        var previousYearWinner = await _fantasyCriticRepo.GetLeagueYearWinner(leagueYear.League.LeagueID, leagueYear.Year - 1);
        var publisherLines = DiscordSharedMessageUtilities.RankLeaguePublishers(leagueYear, previousYearWinner, systemWideValues, dateToCheck);

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueYear.League.LeagueID,
            leagueYear.Year)
            .BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            $"{leagueYear.League.LeagueName} ({leagueYear.Year})",
            string.Join("\n", publisherLines),
            Context.User,
            url: leagueUrl));
    }
}
