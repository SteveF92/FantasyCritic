using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
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

    [SlashCommand("set-game-news", "Sets what games this channel will get news announcements for.")]
    public async Task SetGameNews(
        [Summary("setting", "Off, Relevant, or All")] string setting
        )
    {
        var dateToCheck = _clock.GetGameEffectiveDate();

        var settingEnum = DiscordGameNewsSetting.TryFromValue(setting);
        if (settingEnum is null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Invalid setting",
                "Valid settings are Off, Relevant, and All..",
                Context.User));
            return;
        }

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

        await _discordRepo.SetIsGameNewsSetting(league.LeagueID, Context.Guild.Id, Context.Channel.Id, settingEnum);

        await RespondAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Channel League Configuration Saved",
            $"Game News Announcements now set to **{settingEnum.Value}**.",
            Context.User));
    }
}
