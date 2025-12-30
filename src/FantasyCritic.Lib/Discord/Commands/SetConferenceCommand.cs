using System.Text.RegularExpressions;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using JetBrains.Annotations;
using Serilog;

namespace FantasyCritic.Lib.Discord.Commands;

public class SetConferenceCommand : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly ILogger Logger = Log.ForContext<SetConferenceCommand>();

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IConferenceRepo _conferenceRepo;
    private readonly FantasyCriticSettings _fantasyCriticSettings;

    public SetConferenceCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        IConferenceRepo conferenceRepo,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _conferenceRepo = conferenceRepo;
        _fantasyCriticSettings = fantasyCriticSettings;
    }

    [UsedImplicitly]
    [SlashCommand("set-conference", "Sets the conference to be associated with the current channel.")]
    public async Task SetConference(
        [Summary("conference_url_id", $"The Conference ID or the URL for your conference.")] string conferenceIdParam
        )
    {
        await DeferAsync();
        Logger.Information("Attempting to set up channel {ChannelID} to track conference {ConferenceID}", Context.Channel.Id, conferenceIdParam);

        var dateToCheck = _clock.GetGameEffectiveDate();

        const string pattern = @"(?:https?://)?(?:www\.)?(fantasycritic\.games|localhost:\d+)/conference/([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})/(\d{4})";

        var match = Regex.Match(conferenceIdParam, pattern);

        var conferenceId = match.Success ? match.Groups[2].Value.ToLower().Trim() : conferenceIdParam.ToLower().Trim();

        if (string.IsNullOrEmpty(conferenceId))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Setting Conference",
                "A conference ID is required.",
                Context.User));
            return;
        }

        if (!Guid.TryParse(conferenceId, out var conferenceGuid))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Setting Conference",
                $"`{conferenceId}` is not a valid conference ID.",
                Context.User));
            return;
        }

        var conference = await _conferenceRepo.GetConference(conferenceGuid);
        if (conference == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Setting Conference",
                $"No conference was found for the conference ID `{conferenceId}`.",
                Context.User));
            return;
        }

        try
        {
            await _discordRepo.SetConferenceChannel(new Guid(conferenceId), Context.Guild.Id, Context.Channel.Id);
        }
        catch (Exception ex)
        {
            if (ex.Message.ToLower().Contains("duplicate"))
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Saving Channel Configuration",
                    "This channel is already registered to this conference.",
                    Context.User));
                return;
            }
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Saving Channel Configuration",
                "There was an error saving this conference configuration.",
                Context.User));
            return;
        }

        var conferenceUrlBuilder =
            new ConferenceUrlBuilder(_fantasyCriticSettings.BaseAddress, conference.ConferenceID, dateToCheck.Year);
        var conferenceLinkWithName = conferenceUrlBuilder.BuildUrl(conference.ConferenceName);

        var hasPermissionToSendMessages = Context.Channel.HasPermissionToSendMessagesInChannel(Context.Client.CurrentUser.Id);
        var permissionToSendMessagesText = hasPermissionToSendMessages
            ? "✅ The bot permissions are set up correctly to send conference updates in this channel."
            : "❌ **WARNING:** The bot does NOT have permissions to send messages in this channel and you will not be able to receive conference updates." +
              "\nPlease give the bot the **Send Messages** permission in this channel to receive conference updates (Note: you won't need to run this command again).";

        var messageText = $"Channel configured for Conference {conferenceLinkWithName}.\n{permissionToSendMessagesText}";

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Channel Conference Configuration Saved",
            messageText,
            Context.User,
            null,
            conferenceUrlBuilder.GetOnlyUrl()));
    }
}
