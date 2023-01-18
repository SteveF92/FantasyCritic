using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetLeagueGameNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;

    public SetLeagueGameNewsCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("set-league-game-news", "Sets whether this channel should receive game news for the games in your league.")]
    public async Task SetLeagueGameNews(
        [Summary("setting", "The game news setting for games other than games in your league. Valid settings are Off and On")]
        [Choice("Off", "Off")]
        [Choice("On", "On")]
        string setting = ""
        )
    {
        await DeferAsync();
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding Channel Configuration",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        bool settingBool;
        switch (setting.ToUpper())
        {
            case "ON":
                settingBool = true;
                break;
            case "OFF":
                settingBool = false;
                break;
            default:
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                    "No Change Made to Game News Configuration",
                    $"No valid option was selected.\nThe current Game News Announcements setting is still **{leagueChannel.SendLeagueMasterGameUpdates.ToOnOffString()}**.",
                    Context.User));
                return;
        }

        await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, settingBool);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Game News Configuration Saved",
            $"Game News Announcements now set to **{settingBool.ToOnOffString()}**.",
            Context.User));
    }
}
