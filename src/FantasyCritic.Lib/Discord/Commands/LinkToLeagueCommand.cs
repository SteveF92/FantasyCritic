using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class LinkToLeagueCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public LinkToLeagueCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("link-to-league", "Get a link to the league.")]
    public async Task GetLeagueLink(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
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
                "Error Getting League Link",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var leagueUrlBuilder = new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID, leagueChannel.LeagueYear.Year);
        var leagueUrl = leagueUrlBuilder.BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            $"Click here to visit the site for the league {leagueChannel.LeagueYear.League.LeagueName} ({leagueChannel.LeagueYear.Year})",
            "",
            Context.User,
            url: leagueUrl));
    }
}
