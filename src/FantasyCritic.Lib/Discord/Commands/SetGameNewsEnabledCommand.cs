using Discord.Interactions;
using FantasyCritic.Lib.Discord.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetGameNewsEnabledCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly FantasyCriticSettings _fantasyCriticSettings;

    public SetGameNewsEnabledCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _fantasyCriticSettings = fantasyCriticSettings;
    }

    [SlashCommand("set-game-news", "Enables or disables the game news announcements for this channel.")]
    public async Task SetGameNewsEnabled(
        [Summary("enabled", "Enabled or disabled")] bool isEnabled
        )
    {
        var dateToCheck = _clock.GetToday();

        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, dateToCheck.Year);
        if (leagueChannel == null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var league = leagueChannel.LeagueYear.League;

        await _discordRepo.SetIsGameNewsEnabled(league.LeagueID, Context.Guild.Id, Context.Channel.Id, isEnabled);

        var leagueUrlBuilder = new LeagueUrlBuilder(_fantasyCriticSettings.BaseAddress, league.LeagueID, dateToCheck.Year);
        var leagueLink = leagueUrlBuilder.BuildUrl(league.LeagueName);

        await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Channel League Configuration Saved",
            $"Game News Announcements for this channel are now **{(isEnabled ? "ON" :"OFF")}**.",
            Context.User));
    }
}
