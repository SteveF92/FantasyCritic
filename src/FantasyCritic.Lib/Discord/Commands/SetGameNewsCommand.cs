using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.GameNewsSettings;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetGameNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;

    public SetGameNewsCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("set-game-news", "Sets what games this channel will get news announcements for.")]
    public async Task SetGameNews(
        [Summary("setting", "The game news setting. Valid settings are Off, League Games Only, Relevant, or All")]
        [Choice("Off", "Off")]
        [Choice("League Games Only", "LeagueGamesOnly")]
        [Choice("Relevant", "Relevant")]
        [Choice("All", "All")]
        string setting = ""
        )
    {
        await DeferAsync();
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var settingEnum = DiscordGameNewsSetting.TryFromName(setting);
        if (settingEnum.IsFailure)
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "No Change Made to Game News Configuration",
                $"No valid option was selected.\nThe current Game News Announcements setting is still **{leagueChannel.GameNewsSetting.Name}**.",
                Context.User));
            return;
        }

        await _discordRepo.SetIsGameNewsSetting(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, settingEnum.Value);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Game News Configuration Saved",
            $"Game News Announcements now set to **{settingEnum.Value}**.",
            Context.User));
    }
}
