using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class LinkToConferenceCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public LinkToConferenceCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("link-to-conference", "Get a link to the conference.")]
    public async Task GetConferenceLink(
        [Summary("year", "The year for the conference (if not entered, defaults to the current year).")] int? year = null
        )
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding Conference Configuration",
                $"That year was not found for this conference. Are you sure a conference year is started for {year.Value}?",
                Context.User));
            return;
        }
        var conferenceChannel = await _discordRepo.GetConferenceChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (conferenceChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Conference Link",
                "No conference configuration found for this channel.",
                Context.User));
            return;
        }

        var conferenceUrlBuilder = new ConferenceUrlBuilder(_baseAddress, conferenceChannel.ConferenceYear.Conference.ConferenceID, conferenceChannel.ConferenceYear.Year);
        var conferenceUrl = conferenceUrlBuilder.BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            $"Click here to visit the site for the conference {conferenceChannel.ConferenceYear.Conference.ConferenceName} ({conferenceChannel.ConferenceYear.Year})",
            "",
            Context.User,
            url: conferenceUrl));
    }
}
