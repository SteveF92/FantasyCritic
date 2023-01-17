using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
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
        [Summary("setting", "The game news setting for games other than games in your league. Valid settings are " +
                            "Off, " +
                            "Might Release In Year (Sends updates for any game that might release this year), " +
                            "Will Release In Year (Sends updates only for games that WILL release this year), or" +
                            "All")]
        [Choice("Off", "Off")]
        [Choice("Might Release In Year", "MightReleaseInYear")]
        [Choice("Will Release In Year", "WillReleaseInYear")]
        [Choice("All", "All")]
        string setting = ""
        )
    {
        await DeferAsync();
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);
        if (gameNewsChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding Channel Configuration",
                "No game news configuration found for this channel.",
                Context.User));
            return;
        }

        var settingEnum = GameNewsSetting.TryFromValue(setting);
        if (settingEnum is null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "No Change Made to Game News Configuration",
                $"No valid option was selected.\nThe current Game News Announcements setting is still **{gameNewsChannel.GameNewsSetting.Value}**.",
                Context.User));
            return;
        }

        await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, settingEnum);

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Game News Configuration Saved",
            $"Game News Announcements now set to **{settingEnum.Value}**.",
            Context.User));
    }
}
