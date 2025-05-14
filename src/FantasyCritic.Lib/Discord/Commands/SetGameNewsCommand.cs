using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Interfaces;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;
public class SetGameNewsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IDiscordFormatter _discordFormatter;
    private const string NotableMissesQuestion =
        "Do you want to hear about \"notable misses\"? These are games that nobody in the league picked up, but are of a notably high score that would've made for a strong pick.";
    private const string IncludeLeagueGamesQuestion = "Do you want to hear about games in your league no matter what?";

    public SetGameNewsCommand(IDiscordRepo discordRepo,
        IDiscordFormatter discordFormatter, IMasterGameRepo masterGameRepo)
    {
        _discordRepo = discordRepo;
        _discordFormatter = discordFormatter;
        _masterGameRepo = masterGameRepo;
    }

    [UsedImplicitly]
    [SlashCommand("set-game-news", "Sets what games this channel will get news announcements for.")]
    public async Task SetGameNews(
        [Summary("setting", "The game news setting for games other than games in your league.")]
        [Choice("Recommended (Default)","Recommended")]
        [Choice("All", "All")]
        [Choice("Might Release In Year", "MightReleaseInYear")]
        [Choice("Will Release In Year", "WillReleaseInYear")]
        [Choice("League Games Only", "LeagueGamesOnly")]
        [Choice("Off", "Off")]
        string setting = "Recommended",
        [Summary("skip_unannounced", "Whether or not to skip unannounced games in game news ")]
        bool skipUnannouncedGames = false
        )
    {
        await DeferAsync();

        if (!Context.Channel.HasPermissionToSendMessagesInChannel(Context.Client.CurrentUser.Id))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Change Made to Game News Configuration",
                "❌ The bot does not have permission to send messages in this channel and cannot be set up for Game News." +
                "\nPlease give the bot the **Send Messages** permission in this channel and try again.",
                Context.User));
            return;
        }

        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

        var requestedSettingEnum = RequestedGameNewsSetting.TryFromValue(setting);
        if (requestedSettingEnum is null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Change Made to Game News Configuration",
                $"An invalid option was selected.\nThe current Game News setting is still **{gameNewsChannel?.GameNewsSettingOld.Value ?? "Not Set"}**.",
                Context.User));
            return;
        }

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        var gameChannelIsLeagueChannel = leagueChannel != null && leagueChannel.ChannelID == Context.Channel.Id;

        var tagsToSkip = new List<MasterGameTag>();


        if (skipUnannouncedGames && !requestedSettingEnum.Equals(RequestedGameNewsSetting.Off))
        {
            var tagsDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
            var unannouncedTag = tagsDictionary["UnannouncedGame"];
            tagsToSkip.Add(unannouncedTag);
        }

        if (!requestedSettingEnum.Equals(GameNewsSettingOld.All) && !requestedSettingEnum.Equals(GameNewsSettingOld.Off))
        {
            if (!gameChannelIsLeagueChannel && requestedSettingEnum.Equals(RequestedGameNewsSetting.LeagueGamesOnly))
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                    "No Change Made to Game News Configuration",
                    $"You must have a league associated with this channel in order to choose the League Games Only option.\nThe current Game News Setting is still **{gameNewsChannel?.GameNewsSettingOld.Value ?? "Not Set"}**.",
                    Context.User));
                return;
            }

            if (requestedSettingEnum.Equals(RequestedGameNewsSetting.Recommended))
            {
                await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSettingOld.MightReleaseInYear);
                await _discordRepo.SetSkippedGameNewsTags(Context.Guild.Id, Context.Channel.Id, tagsToSkip);

                if (gameChannelIsLeagueChannel)
                {
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, leagueChannel.GuildID,
                        leagueChannel.ChannelID, true, true);
                }

                await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                    "Recommended Game News Settings Applied",
                    BuildGameNewsSettingsDisplayText(RequestedGameNewsSetting.MightReleaseInYear,
                        true,
                        true,
                        tagsToSkip),
                    Context.User));
                return;
            }

            if (gameChannelIsLeagueChannel)
            {
                if (requestedSettingEnum.Equals(RequestedGameNewsSetting.LeagueGamesOnly))
                {
                    await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSettingOld.Off);
                    await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                        "Game News Configuration Saved",
                        BuildGameNewsSettingsDisplayText(requestedSettingEnum, true, false, tagsToSkip),
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
                    await _discordRepo.SetSkippedGameNewsTags(Context.Guild.Id, Context.Channel.Id, tagsToSkip);
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, Context.Guild.Id, Context.Channel.Id, true, true);
                    await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                        "Game News Configuration Saved",
                        BuildGameNewsSettingsDisplayText(requestedSettingEnum, true, true, tagsToSkip),
                        Context.User));

                    var includeLeagueGamesMenuComponent = BuildYesOrNoSelectMenuComponent("include-league-games");
                    await FollowupAsync(IncludeLeagueGamesQuestion, components: includeLeagueGamesMenuComponent);
                }
            }
        }
        else
        {
            var includeLeagueGames = false;
            var notableMisses = false;
            switch (gameChannelIsLeagueChannel)
            {
                case true when requestedSettingEnum.Equals(RequestedGameNewsSetting.All):
                    includeLeagueGames = true;
                    notableMisses = true;
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, leagueChannel!.GuildID,
                        leagueChannel!.ChannelID, true, true);
                    break;
                case true when requestedSettingEnum.Equals(GameNewsSettingOld.Off):
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, leagueChannel.GuildID,
                        leagueChannel.ChannelID, false, false);
                    break;
            }

            var settingEnum = requestedSettingEnum.ToNormalSetting();
            await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, settingEnum);
            await _discordRepo.SetSkippedGameNewsTags(Context.Guild.Id, Context.Channel.Id, tagsToSkip);

            var resultText = BuildGameNewsSettingsDisplayText(requestedSettingEnum, includeLeagueGames, notableMisses, tagsToSkip);

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
                "Game News Configuration Saved",
                resultText,
                Context.User));
        }
    }

    private static string BuildGameNewsSettingsDisplayText(RequestedGameNewsSetting requestedSettingEnum,
        bool includeLeagueGames, bool includeNotableMisses,
        IReadOnlyCollection<MasterGameTag> skippedTags)
    {
        var settingText = $"Game News Announcements: **{requestedSettingEnum.Value}**";
        if (requestedSettingEnum.Equals(RequestedGameNewsSetting.Off))
        {
            return settingText;
        }
        settingText += $"\nInclude League Games: **{(includeLeagueGames ? "ON" : "OFF")}**";
        settingText += $"\nInclude Notable Misses: **{(includeNotableMisses ? "ON" : "OFF")}**";
        settingText += $"\nSkipped Tags: **{(skippedTags.Any() ? string.Join(", ", skippedTags.Select(t => t.ReadableName)) : "NONE")}**";
        return settingText;
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
