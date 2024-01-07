using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
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

    [SlashCommand("set-league", "Sets the league to be associated with the current channel.")]
    public async Task SetLeague(
        [Summary("league_ID", "The ID for your league from the URL - https://www.fantasycritic.games/league/LEAGUE_ID_HERE/2022.")] string leagueIdParam
        )
    {
        await DeferAsync();
        Logger.Information("Attempting to set up channel {ChannelID} to track league {LeagueID}", Context.Channel.Id, leagueIdParam);

        var dateToCheck = _clock.GetGameEffectiveDate();

        var leagueId = leagueIdParam.ToLower().Trim();

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

            var usersInLeague = await _fantasyCriticRepo.GetActivePlayersForLeagueYear(league.LeagueID, dateToCheck.Year);
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
            var leagueYear = await _fantasyCriticRepo.GetLeagueYear(new Guid(leagueId), dateToCheck.Year);
            if (leagueYear is null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "Error Saving Channel Configuration",
                    $"This league has not been set up for the {dateToCheck.Year} year yet.",
                    Context.User));
                return;
            }

            await _discordRepo.SetLeagueChannel(new Guid(leagueId), Context.Guild.Id, Context.Channel.Id, leagueYear);
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

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Channel League Configuration Saved",
            $"Channel configured for League {leagueLinkWithName}.",
            Context.User,
            null,
            leagueUrlBuilder.GetOnlyUrl()));
    }
}
