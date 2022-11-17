using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetLeagueChannelCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly FantasyCriticSettings _fantasyCriticSettings;

    public SetLeagueChannelCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        IFantasyCriticRepo fantasyCriticRepo,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _fantasyCriticRepo = fantasyCriticRepo;
        _fantasyCriticSettings = fantasyCriticSettings;
    }

    [SlashCommand("set-league", "Sets the league to be associated with the current channel.")]
    public async Task SetLeague(
        [Summary("league_ID", "The ID for your league from the URL - https://www.fantasycritic.games/league/LEAGUE_ID_HERE/2022.")] string leagueIdParam
        )
    {
        var dateToCheck = _clock.GetGameEffectiveDate();

        var leagueId = leagueIdParam.ToLower().Trim();

        if (string.IsNullOrEmpty(leagueId))
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Setting League",
                "A league ID is required.",
                Context.User));
            return;
        }

        if (!Guid.TryParse(leagueId, out var leagueGuid))
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Setting League",
                $"`{leagueId}` is not a valid league ID.",
                Context.User));
            return;
        }

        var league = await _fantasyCriticRepo.GetLeague(leagueGuid);
        if (league == null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Setting League",
                $"No league was found for the league ID `{leagueId}`.",
                Context.User));
            return;
        }
        if (!league.PublicLeague)
        {
            // TODO: validate user for private leagues
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Setting League",
                $"You do not have access to this league.",
                Context.User));
            return;
        }

        await _discordRepo.SetLeagueChannel(new Guid(leagueId), Context.Guild.Id, Context.Channel.Id, dateToCheck.Year);

        var leagueUrlBuilder = new LeagueUrlBuilder(_fantasyCriticSettings.BaseAddress, league.LeagueID, dateToCheck.Year);
        var leagueLink = leagueUrlBuilder.BuildUrl(league.LeagueName);

        await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Channel League Configuration Saved",
            $"Channel configured for League {leagueLink}.",
            Context.User));
    }
}