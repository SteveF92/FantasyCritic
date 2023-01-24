using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetGameNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private const string NotableMissesQuestion =
        "Do you want to hear about \"notable misses\"? These are games that nobody in the league picked up, but are of a notably high score that would've made for a strong pick.";
    private const string IncludeLeagueGamesQuestion = "Do you want to hear about games in your league no matter what?";

    public SetGameNewsCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("set-game-news", "Sets what games this channel will get news announcements for.")]
    public async Task SetGameNews(
        [Summary("setting", "The game news setting for games other than games in your league.")]
        [Choice("Recommended (Default)","Recommended")]
        [Choice("All", "All")]
        [Choice("Might Release In Year", "MightReleaseInYear")]
        [Choice("Will Release In Year", "WillReleaseInYear")]
        [Choice("League Games Only", "LeagueGamesOnly")]
        [Choice("Off", "Off")]
        string setting = "Recommended"
        )
    {
        await DeferAsync();
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

        var requestedSettingEnum = RequestedGameNewsSetting.TryFromValue(setting);
        if (requestedSettingEnum is null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "No Change Made to Game News Configuration",
                $"An invalid option was selected.\nThe current Game News setting is still **{gameNewsChannel?.GameNewsSetting.Value ?? "Not Set"}**.",
                Context.User));
            return;
        }

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        var gameChannelIsLeagueChannel = leagueChannel != null && leagueChannel.ChannelID == Context.Channel.Id;

        if (!requestedSettingEnum.Equals(GameNewsSetting.All) && !requestedSettingEnum.Equals(GameNewsSetting.Off))
        {
            if (!gameChannelIsLeagueChannel && requestedSettingEnum.Equals(RequestedGameNewsSetting.LeagueGamesOnly))
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                    "No Change Made to Game News Configuration",
                    $"You must have a league associated with this channel in order to choose the League Games Only option.\nThe current Game News Setting is still **{gameNewsChannel?.GameNewsSetting.Value ?? "Not Set"}**.",
                    Context.User));
                return;
            }

            if (requestedSettingEnum.Equals(RequestedGameNewsSetting.Recommended))
            {
                await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id,
                        GameNewsSetting.MightReleaseInYear);

                var newsSettingsMessage = $"**Game News Setting:** {GameNewsSetting.MightReleaseInYear}";

                if (gameChannelIsLeagueChannel)
                {
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, leagueChannel.GuildID,
                        leagueChannel.ChannelID, true, true);
                    newsSettingsMessage += "\n**Include League Games**: Yes\n**Include Notable Misses:** Yes";
                }

                await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Recommended Game News Settings Applied",
                    newsSettingsMessage, Context.User));
                return;
            }

            if (gameChannelIsLeagueChannel)
            {
                if (requestedSettingEnum.Equals(RequestedGameNewsSetting.LeagueGamesOnly))
                {
                    await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSetting.Off);
                    await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                        "Game News Configuration Saved",
                        $"Game News Announcements now set to **{requestedSettingEnum.Value}**.",
                        Context.User));

                    var notableMissesMenuComponent = BuildYesOrNoSelectMenuComponent("notable-misses");

                    //Setting notable misses to false for now as they will have to respond to the dropdown menu that will send after this
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, leagueChannel.GuildID,
                        leagueChannel.ChannelID, true, false);
                    await FollowupAsync(NotableMissesQuestion, components: notableMissesMenuComponent);
                }
                else if (requestedSettingEnum.Equals(RequestedGameNewsSetting.MightReleaseInYear) || requestedSettingEnum.Equals(RequestedGameNewsSetting.WillReleaseInYear))
                {
                    var settingEnum = requestedSettingEnum.ToNormalSetting();
                    await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, settingEnum);
                    await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                        "Game News Configuration Saved",
                        $"Game News Announcements now set to **{requestedSettingEnum.Value}**.",
                        Context.User));

                    var includeLeagueGamesMenuComponent = BuildYesOrNoSelectMenuComponent("include-league-games");
                    var notableMissesMenuComponent = BuildYesOrNoSelectMenuComponent("notable-misses");
                    await FollowupAsync(IncludeLeagueGamesQuestion, components: includeLeagueGamesMenuComponent, ephemeral: true);
                    await FollowupAsync(NotableMissesQuestion, components: notableMissesMenuComponent, ephemeral: true);
                }
            }
        }
        else
        {
            if (gameChannelIsLeagueChannel && requestedSettingEnum.Equals(RequestedGameNewsSetting.All))
            {
                await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, leagueChannel!.GuildID,
                    leagueChannel!.ChannelID, true, true);
            }
            await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSetting.All);

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
                "Game News Configuration Saved",
                $"Game News Announcements now set to **{requestedSettingEnum.Value}**.",
                Context.User));
        }
    }

    private static MessageComponent BuildYesOrNoSelectMenuComponent(string customId)
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId(customId)
            .WithOptions(new List<SelectMenuOptionBuilder>
            {
                new("Yes", "yes"),
                new("No", "no"),
            });

        return new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .Build();
    }
}
