using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using JetBrains.Annotations;
using Serilog;

namespace FantasyCritic.Lib.Discord.Commands;

public class SetConferenceNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly ILogger Logger = Log.ForContext<SetConferenceNewsCommand>();

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly InterLeagueService _interLeagueService;
    private readonly FantasyCriticSettings _fantasyCriticSettings;

    public SetConferenceNewsCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        InterLeagueService interLeagueService,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _interLeagueService = interLeagueService;
        _fantasyCriticSettings = fantasyCriticSettings;
    }

    [UsedImplicitly]
    [SlashCommand("set-conference-news", "Enable or disable league news updates for the conference associated with this channel.")]
    public async Task SetConferenceNews(
    [Summary("send_league_news", "Whether or not to send league news updates for all conference leagues in this channel")] bool sendLeagueNews)
    {
        await DeferAsync();
        Logger.Information("Attempting to update SendLeagueNews setting for channel {ChannelID} to {SendLeagueNews}", Context.Channel.Id, sendLeagueNews);

        var dateToCheck = _clock.GetGameEffectiveDate();
        var supportedYears = await _interLeagueService.GetSupportedYears();

        var conferenceChannel = await _discordRepo.GetConferenceChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);

        if (conferenceChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Updating Conference News Setting",
                "This channel is not configured for any conference. Please use `/set-conference` first to associate this channel with a conference.",
                Context.User));
            return;
        }

        try
        {
            await _discordRepo.SetConferenceLeagueNewsSetting(Context.Guild.Id, Context.Channel.Id, sendLeagueNews);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error updating SendLeagueNews setting for channel {ChannelID}", Context.Channel.Id);
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Updating Conference News Setting",
                "There was an error updating the conference news setting.",
                Context.User));
            return;
        }

        var conferenceUrlBuilder = new ConferenceUrlBuilder(
            _fantasyCriticSettings.BaseAddress,
            conferenceChannel.ConferenceYear.Conference.ConferenceID,
            dateToCheck.Year);
        var conferenceLinkWithName = conferenceUrlBuilder.BuildUrl(conferenceChannel.ConferenceYear.Conference.ConferenceName);

        var statusText = sendLeagueNews
            ? "✅ **Enabled**: This channel will now receive league news updates for all leagues in the conference."
            : "❌ **Disabled**: This channel will no longer receive league news updates for conference leagues.";

        var messageText = $"Conference news setting updated for {conferenceLinkWithName}.\n\n{statusText}";

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Conference News Setting Updated",
            messageText,
            Context.User,
            null,
            conferenceUrlBuilder.GetOnlyUrl()));
    }
}
