using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetLeagueLinkCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetLeagueLinkCommand(IDiscordRepo discordRepo,
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

    [SlashCommand("link", "Get a link to the league.")]
    public async Task GetLeagueLink(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Getting League Link",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var leagueUrlBuilder = new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID, leagueChannel.LeagueYear.Year);
        var leagueUrl = leagueUrlBuilder.BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"Click here to visit the site for the league {leagueChannel.LeagueYear.League.LeagueName} ({leagueChannel.LeagueYear.Year})",
            "",
            Context.User,
            url: leagueUrl));
    }
}
