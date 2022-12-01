using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetGameNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;

    public SetGameNewsCommand(IDiscordRepo discordRepo,
        IClock clock,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("set-game-news", "Sets what games this channel will get news announcements for.")]
    public async Task SetGameNews(
        [Summary("setting", "The game news setting. Valid settings are Off, Relevant, or All")]
        [Choice("Off", "Off")]
        [Choice("Relevant", "Relevant")]
        [Choice("All", "All")]
        string setting
        )
    {
        await DeferAsync();
        var settingEnum = DiscordGameNewsSetting.TryFromValue(setting);
        if (settingEnum is null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Invalid setting",
                "Valid settings are Off, Relevant, and All.",
                Context.User));
            return;
        }

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        await _discordRepo.SetIsGameNewsSetting(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, settingEnum);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Channel League Configuration Saved",
            $"Game News Announcements now set to **{settingEnum.Value}**.",
            Context.User));
    }
}
