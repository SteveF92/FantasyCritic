using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class ConferenceCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ConferenceService _conferenceService;
    private readonly string _baseAddress;
    private const int PublisherPerPageLimit = 10;

    public ConferenceCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings,
        ConferenceService conferenceService)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _conferenceService = conferenceService;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("conference", "Get conference information.")]
    public async Task GetConference(
        [Summary("year", "The year for the conference (if not entered, defaults to the current year).")] int? year = null
        )
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                $"That year was not found for this conference. Are you sure a conference year is started for {year.Value}?",
                Context.User));
            return;
        }
        var conferenceChannel = await _discordRepo.GetConferenceChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (conferenceChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding Conference Configuration",
                "No conference configuration found for this channel. You may have to specify a year if your conference is for an upcoming year.",
                Context.User));
            return;
        }

        var conferencePublishersRanked = await GetRankedConferenceYearStandings(conferenceChannel);

        var buttonBuilder = new ComponentBuilder();

        if (conferencePublishersRanked.Count > PublisherPerPageLimit)
        {
            buttonBuilder.WithButton("Next", $"currentIndexNext:{0}_{conferenceChannel.ConferenceYear.Year}", emote: new Emoji("➡️"));
        }

        await FollowupAsync(embed: GetConferenceStandingsEmbed(conferenceChannel, conferencePublishersRanked, 0),
            components: buttonBuilder.Build());
    }

    private Embed GetConferenceStandingsEmbed(ConferenceChannel conferenceChannel,
        IList<string> conferencePublishersRanked, int index)
    {
        var conferenceUrl = new ConferenceUrlBuilder(_baseAddress, conferenceChannel.ConferenceYear.Conference.ConferenceID, conferenceChannel.ConferenceYear.Year)
            .BuildUrl();
        var startNumber = index * PublisherPerPageLimit;
        var endNumber = startNumber + PublisherPerPageLimit;
        return _discordFormatter.BuildRegularEmbedWithUserFooter(
            $"{conferenceChannel.ConferenceYear.Conference.ConferenceName} {conferenceChannel.ConferenceYear.Year}",
            string.Join("\n", conferencePublishersRanked.Take(new Range(startNumber, endNumber))),
            Context.User,
            url: conferenceUrl);
    }

    private async Task<IList<string>> GetRankedConferenceYearStandings(ConferenceChannel conferenceChannel)
    {
        var conferenceStandings = await _conferenceService.GetConferenceYearStandings(conferenceChannel.ConferenceYear);
        var conferencePublishersRanked = DiscordSharedMessageUtilities.RankConferencePublishers(conferenceStandings);
        return conferencePublishersRanked;
    }

    [ComponentInteraction("currentIndexNext:*_*")]
    public async Task NextButton(int currentIndex, int? year = null)
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                $"That year was not found for this conference. Are you sure a conference year is started for {year.Value}?",
                Context.User));
            return;
        }

        var conferenceChannel = await _discordRepo.GetConferenceChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (conferenceChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding Conference Configuration",
                "No conference configuration found for this channel. You may have to specify a year if your conference is for an upcoming year.",
                Context.User));
            return;
        }

        var conferencePublishersRanked = await GetRankedConferenceYearStandings(conferenceChannel);

        var newIndex = currentIndex + 1;

        var buttonBuilder = new ComponentBuilder();

        if (currentIndex >= 0)
        {
            buttonBuilder.WithButton("Previous", $"currentIndexPrev:{newIndex}_{conferenceChannel.ConferenceYear.Year}", emote: new Emoji("⬅️"));
        }

        var numberOfPages = conferencePublishersRanked.Count / PublisherPerPageLimit;

        if (newIndex < numberOfPages)
        {
            buttonBuilder.WithButton("Next", $"currentIndexNext:{newIndex}_{conferenceChannel.ConferenceYear.Year}", emote: new Emoji("➡️"));
        }

        await Context.Interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = GetConferenceStandingsEmbed(conferenceChannel, conferencePublishersRanked, newIndex);
            properties.Components = buttonBuilder.Build();
        });
    }

    [ComponentInteraction("currentIndexPrev:*_*")]
    public async Task PreviousButton(int currentIndex, int? year = null)
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                $"That year was not found for this conference. Are you sure a conference year is started for {year.Value}?",
                Context.User));
            return;
        }

        var conferenceChannel = await _discordRepo.GetConferenceChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (conferenceChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding Conference Configuration",
                "No conference configuration found for this channel. You may have to specify a year if your conference is for an upcoming year.",
                Context.User));
            return;
        }

        var conferencePublishersRanked = await GetRankedConferenceYearStandings(conferenceChannel);

        var buttonBuilder = new ComponentBuilder();

        var newIndex = currentIndex - 1;

        if (newIndex - 1 >= 0)
        {
            buttonBuilder.WithButton("Previous", $"currentIndexPrev:{newIndex}_{conferenceChannel.ConferenceYear.Year}", emote: new Emoji("⬅️"));
        }

        var numberOfPages = conferencePublishersRanked.Count / PublisherPerPageLimit;

        if (newIndex < numberOfPages)
        {
            buttonBuilder.WithButton("Next", $"currentIndexNext:{newIndex}_{conferenceChannel.ConferenceYear.Year}", emote: new Emoji("➡️"));
        }

        await Context.Interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = GetConferenceStandingsEmbed(conferenceChannel, conferencePublishersRanked, newIndex);
            properties.Components = buttonBuilder.Build();
        });
    }
}
