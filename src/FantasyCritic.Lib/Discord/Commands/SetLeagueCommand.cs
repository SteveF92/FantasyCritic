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

public class SetLeagueCommand : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly ILogger Logger = Log.ForContext<SetLeagueCommand>();

    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IReadOnlyFantasyCriticUserStore _userStore;
    private readonly FantasyCriticSettings _fantasyCriticSettings;

    public SetLeagueCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        IFantasyCriticRepo fantasyCriticRepo,
        IReadOnlyFantasyCriticUserStore userStore,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _fantasyCriticRepo = fantasyCriticRepo;
        _userStore = userStore;
        _fantasyCriticSettings = fantasyCriticSettings;
    }

    [UsedImplicitly]
    [SlashCommand("set-league", "Sets the league to be associated with the current channel.")]
    public async Task SetLeague(
        [Summary("league_ID", "The ID for your league from the URL - https://www.fantasycritic.games/league/LEAGUE_ID_HERE/2022.")] string leagueIdParam,
        [Summary("year", "The year of the league to track. If not specified, the current year will be used.")] int? year = null
        )
    {
        await DeferAsync();
        Logger.Information("Attempting to set up channel {ChannelID} to track league {LeagueID}", Context.Channel.Id, leagueIdParam);

        var dateToCheck = _clock.GetGameEffectiveDate();

        string leagueId;

        // Regex pattern to match the league URL and capture GUID and year
        const string pattern = @"(?:https?://)?(?:www\.)?fantasycritic\.games/league/([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})/(\d{4})";

        var match = Regex.Match(leagueIdParam, pattern);

        if (match.Success)
        {
            leagueId = match.Groups[1].Value.ToLower().Trim();
            var yearValue = match.Groups[2].Value.ToLower().Trim();
            if (year == null)
            {
                int.TryParse(yearValue, out var yearInt);
                year = yearInt;
            }
        }
        else
        {
            leagueId = leagueIdParam.ToLower().Trim();
        }

        if (string.IsNullOrEmpty(leagueId))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Setting League",
                "A league ID is required.",
                Context.User));
            return;
        }

        if (!Guid.TryParse(leagueId, out var leagueGuid))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Setting League",
                $"`{leagueId}` is not a valid league ID.",
                Context.User));
            return;
        }

        var league = await _fantasyCriticRepo.GetLeague(leagueGuid);
        if (league == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Setting League",
                $"No league was found for the league ID `{leagueId}`.",
                Context.User));
            return;
        }

        if (!league.PublicLeague)
        {
            const string responseMessage = "You do not have access to this league. To link a private league, you must be a member of the league, and you must link your Fantasy Critic and Discord accounts.";
            if (Context.User is null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                    "Error Setting League",
                    responseMessage));
                return;
            }

            var discordUser = Context.User!;
            var fantasyCriticUser = await _userStore.FindByLoginAsync("Discord", discordUser.Id.ToString(), CancellationToken.None);
            if (fantasyCriticUser is null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Setting League",
                    responseMessage,
                    Context.User));
                return;
            }

            var usersInLeague = await _fantasyCriticRepo.GetUsersInLeague(league.LeagueID);
            if (!usersInLeague.Contains(fantasyCriticUser))
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Setting League",
                    responseMessage,
                    Context.User));
                return;
            }
        }

        try
        {
            await _discordRepo.SetLeagueChannel(new Guid(leagueId), Context.Guild.Id, Context.Channel.Id);
        }
        catch (Exception ex)
        {
            if (ex.Message.ToLower().Contains("duplicate"))
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Saving Channel Configuration",
                    "This channel is already registered to this league.",
                    Context.User));
                return;
            }
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Saving Channel Configuration",
                "There was an error saving this league configuration.",
                Context.User));
            return;
        }
        var leagueUrlBuilder =
            new LeagueUrlBuilder(_fantasyCriticSettings.BaseAddress, league.LeagueID, dateToCheck.Year);
        var leagueLinkWithName = leagueUrlBuilder.BuildUrl(league.LeagueName);

        var hasPermissionToSendMessages = Context.Channel.HasPermissionToSendMessagesInChannel(Context.Client.CurrentUser.Id);
        var permissionToSendMessagesText = hasPermissionToSendMessages
            ? "✅ The bot permissions are set up correctly to send league updates in this channel."
            : "❌ **WARNING:** The bot does NOT have permissions to send messages in this channel and you will not be able to receive league updates." +
              "\nPlease give the bot the **Send Messages** permission in this channel to receive league updates (Note: you won't need to run this command again).";

        var messageText = $"Channel configured for League {leagueLinkWithName}.\n{permissionToSendMessagesText}";

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Channel League Configuration Saved",
            messageText,
            Context.User,
            null,
            leagueUrlBuilder.GetOnlyUrl()));
    }
}
