using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Interfaces;
using System.Collections.Concurrent;
using DiscordDotNetUtilities.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;

public class GameNewsSettingsCommand : InteractionModuleBase<SocketInteractionContext>
{
    //Service Dependencies
    private readonly IDiscordRepo _discordRepo;

    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IDiscordFormatter _discordFormatter;

    //State
    private static IReadOnlyList<MasterGameTag>? _masterGameTags;

    /// <summary>
    /// First ulong - ChannelID, Second ulong - CommandMessageID
    /// </summary>
    private static readonly ConcurrentDictionary<ulong, ulong> _channelCommandDict = new();

    public GameNewsSettingsCommand(IDiscordRepo discordRepo, IMasterGameRepo masterGameRepo, IDiscordFormatter discordFormatter)
    {
        _discordRepo = discordRepo;
        _masterGameRepo = masterGameRepo;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("game-news-settings", "View and Change Game News Settings For This Channel.")]
    public async Task GameNewsSettings()
    {
        try
        {
            //If this is the first time interaction was called
            if (!Context.Interaction.HasResponded)
            {
                // Defer the interaction to extend the response window
                await DeferAsync();
            }

            //Update Master Game Tags Dictionary
            if (_masterGameTags == null)
            {
                _masterGameTags = await _masterGameRepo.GetMasterGameTags();
            }

            // Check to see if game news is enabled for this channel
            var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);
            var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);

            if (leagueChannel != null)
            {
                if (!leagueChannel.ShowPickedGameNews
                    && !leagueChannel.ShowEligibleGameNews
                    && !leagueChannel.ShowIneligibleGameNews
                    && gameNewsChannel == null)
                {
                    await SendDisabledGameNewsMessage();
                    return;
                }
            }
            else if (gameNewsChannel == null)
            {
                await SendDisabledGameNewsMessage();
                return;
            }

            await SendGameNewsCommandMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GameNewsSettingCommand: {ex.Message}");
            await FollowupAsync("An error occurred while processing your request.", ephemeral: true);
            throw;
        }
    }

    #region Command Messages

    private async Task SendGameNewsCommandMessage()
    {
        var isLeagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id) != null;

        var commandMessageComponents = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithSelectMenu(GetPresetSettingSelection(isLeagueChannel)))
            .AddRow(new ActionRowBuilder().WithButton(GetSetCustomFiltersButton()))
            .AddRow(new ActionRowBuilder().WithButton(GetDisableGameNewsButtion()))
            .Build();

        var message = await FollowupAsync(embed: await CreateCommandMessageEmbed(), components: commandMessageComponents);
        _channelCommandDict[Context.Channel.Id] = message.Id;
    }

    private async Task UpdateCommandMessage()
    {
        _channelCommandDict.TryGetValue(Context.Channel.Id, out ulong commandMessageID);
        if (commandMessageID == default)
        {
            Serilog.Log.Error("Could not find the GameNews Command message for given channel {ChannelId}", Context.Channel.Id);
            return;
        }

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);
        bool isLeagueChannel = leagueChannel != null;

        var components = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithSelectMenu(GetPresetSettingSelection(isLeagueChannel)))
            .AddRow(new ActionRowBuilder().WithButton(GetSetCustomFiltersButton()))
            .AddRow(new ActionRowBuilder().WithButton(GetDisableGameNewsButtion()))
            .Build();

        var msgEmbed = await CreateCommandMessageEmbed();

        await Context.Channel.ModifyMessageAsync(commandMessageID, msg =>
        {
            msg.Embed = msgEmbed;
            msg.Components = components;
        });
    }

    public async Task<Embed> CreateCommandMessageEmbed()
    {
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

        bool enableGameNews = true;  //<-- Just a filler bool to show game news is on, even though it would be on at this point.
        bool showPickedGameNews = leagueChannel?.ShowPickedGameNews ?? false;
        bool showEligibleGameNews = leagueChannel?.ShowEligibleGameNews ?? false;
        bool showIneligibleGameNews = leagueChannel?.ShowIneligibleGameNews ?? false;
        NotableMissSetting notableMissSetting = leagueChannel?.NotableMissSetting ?? NotableMissSetting.None;
        bool showAlreadyReleasedGameNews = gameNewsChannel?.GameNewsSetting.ShowAlreadyReleasedNews ?? false;
        bool showNewGameAnnouncements = gameNewsChannel?.GameNewsSetting.ShowNewGameAnnouncements ?? false;
        bool showWillReleaseInYearNews = gameNewsChannel?.GameNewsSetting.ShowWillReleaseInYearNews ?? false;
        bool showMightReleaseInYearNews = gameNewsChannel?.GameNewsSetting.ShowMightReleaseInYearNews ?? false;
        bool showWillNotReleaseInYearNews = gameNewsChannel?.GameNewsSetting.ShowWillNotReleaseInYearNews ?? false;
        bool showJustReleasedAnnouncements = gameNewsChannel?.GameNewsSetting.ShowJustReleasedAnnouncements ?? false;
        bool showScoreGameNews = gameNewsChannel?.GameNewsSetting.ShowScoreGameNews ?? false;
        bool showEditedGameNews = gameNewsChannel?.GameNewsSetting.ShowEditedGameNews ?? false;
        var skippedTags = gameNewsChannel?.SkippedTags ?? new List<MasterGameTag>();

        string GetEmoji(bool? setting) => setting switch
        {
            true => "✅",
            false => "❌",
            _ => string.Empty
        };

        var embedFieldBuilders = new List<EmbedFieldBuilder>
        {
            new()
            {
                Name = "General News Settings",
                Value = $"> ℹ️ Game News Enabled: {(enableGameNews == false ? "**No**" : "**Yes**")}\n" +
                        $"> ℹ️ Is League Channel: {(leagueChannel != null ? "**Yes**" : "**No**")}\n" +
                        $"> ℹ️  State: {GetSettingState(leagueChannel, gameNewsChannel?.GameNewsSetting)}",
                IsInline = false
            }
        };

        if (leagueChannel != null)
        {
            embedFieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = "League News Settings",
                Value = $"> {GetEmoji(showPickedGameNews)} Enable Picked Game News Override\n" +
                        $"> {GetEmoji(showEligibleGameNews)} Show Eligible Game News\n" +
                        $"> {GetEmoji(showIneligibleGameNews)} Show Ineligible Game News\n" +
                        (notableMissSetting != null
                            ? $"> ℹ️ Notable Miss Setting: **{notableMissSetting.ReadableName}**"
                            : ""),
                IsInline = false
            });

            if (CheckIsPickedOnly(leagueChannel))
            {
                embedFieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = "Tips",
                    Value = "> ℹ️ To change other filters, you must turn off \"Picked Game News Override\", or enable \"Eligible/Ineligible Game News\". Right now you are only showing league picked news, so there is nothing else to filter.",
                    IsInline = false
                });

                return GetGameNewsSettingsEmbed(embedFieldBuilders);
            }
        }

        string announcementsSectionHeader = "Game Announcement Filters";
        if (leagueChannel != null && leagueChannel.ShowPickedGameNews)
        {
            announcementsSectionHeader = "Non-Picked Game Announcement Filters";
        }

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = announcementsSectionHeader,
            Value = $"> {GetEmoji(showNewGameAnnouncements)} Show New Game Announcements\n" +
                    $"> {GetEmoji(showJustReleasedAnnouncements)} Show Just Released Announcements",
            IsInline = false
        });

        string gameStatusSettingsSectionHeader = "Game Status Filters";
        if (leagueChannel != null && leagueChannel.ShowPickedGameNews)
        {
            gameStatusSettingsSectionHeader = "Non-Picked Game Status Filters";
        }

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = gameStatusSettingsSectionHeader,
            Value = $"> {GetEmoji(showAlreadyReleasedGameNews)} Show Already Released Game News\n" +
                    $"> {GetEmoji(showWillReleaseInYearNews)} Show Will Release In Year News\n" +
                    $"> {GetEmoji(showMightReleaseInYearNews)} Show Might Release In Year News\n" +
                    $"> {GetEmoji(showWillNotReleaseInYearNews)} Show Will Not Release In Year News\n",

            IsInline = false
        });

        string gameUpdateSettingsSectionHeader = "Game Update Filters";
        if (leagueChannel != null && leagueChannel.ShowPickedGameNews)
        {
            gameUpdateSettingsSectionHeader = "Non-Picked Game Update Filters";
        }

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = gameUpdateSettingsSectionHeader,
            Value = $"> {GetEmoji(showScoreGameNews)} Show Score Game News\n" +
                    $"> {GetEmoji(showEditedGameNews)} Show Edited Game News",
            IsInline = false
        });

        string skippedTagsSectionHeader = "Skipped Tags Filters";
        if (leagueChannel != null && leagueChannel.ShowPickedGameNews)
        {
            skippedTagsSectionHeader = "Non-Picked Skipped Tags Filters";
        }

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = skippedTagsSectionHeader,
            Value = skippedTags.Any() ? string.Join("\n", skippedTags.Select(tag => $" - {tag.ReadableName}")) : "None",
            IsInline = false,
        });

        return GetGameNewsSettingsEmbed(embedFieldBuilders);
    }

    private Embed GetGameNewsSettingsEmbed(List<EmbedFieldBuilder> builders)
    {
        return _discordFormatter.BuildRegularEmbedWithUserFooter(
                    "Current Game News Settings",
                    "> **Need Help?**\n" +
                    "> Documentation [[Link]](https://www.fantasycritic.games/discord-bot#game-news-settings)\n" +
                    "> Discord [[Link]](https://discord.com/invite/dNa7DD3)",
                    Context.User,
                    builders);
    }

    private string GetSettingState(MinimalLeagueChannel? leagueChannel, GameNewsSetting? gameNewsSettings)
    {
        if (gameNewsSettings?.IsAllOn() ?? false
            && (leagueChannel?.ShowEligibleGameNews ?? true)
            && (leagueChannel?.ShowPickedGameNews ?? true)
            && leagueChannel?.NotableMissSetting == NotableMissSetting.ScoreUpdates)
        {
            return "All";
        }

        if (IsRecommendedSettings(leagueChannel, gameNewsSettings))
        {
            return "Recommended";
        }

        if ((leagueChannel?.ShowPickedGameNews ?? false)
            && (!leagueChannel?.ShowEligibleGameNews ?? false)
            && (!leagueChannel?.ShowIneligibleGameNews ?? false)
            && (leagueChannel?.NotableMissSetting == NotableMissSetting.None))
        {
            return "LeagueOnly";
        }

        return "Custom";
    }

    private static bool IsRecommendedSettings(MinimalLeagueChannel? leagueChannel, GameNewsSetting? gameNewsSetting)
    {
        if (gameNewsSetting == null) return false;

        bool leagueRecommended = leagueChannel is null or { ShowPickedGameNews: true, ShowEligibleGameNews: true };

        bool gameNewsRecommended = gameNewsSetting.IsRecommended();

        bool result = leagueRecommended && gameNewsRecommended;

        return result;
    }

    #endregion Command Messages

    #region Setting Category Messages

    private async Task SendDisabledGameNewsMessage()
    {
        var enableGameNewsMessage = new ComponentBuilder()
            .WithButton(GetEnableGameNewsButton())
            .Build();

        await FollowupAsync("Game News is currently off for this channel, Do you want to turn it on?:", components: enableGameNewsMessage, ephemeral: true);
    }

    private async Task SendCustomSettingsMessage()
    {
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);

        var customSettingsBuilder = new ComponentBuilder();
        if (leagueChannel != null)
        {
            customSettingsBuilder.AddRow(new ActionRowBuilder().WithButton(GetChangeLeagueNewsSettingsButton()));
        }

        var customSettingsMessage = customSettingsBuilder
            .AddRow(new ActionRowBuilder().WithButton(GetChangeGameAnnouncementsSettingsButton()))
            .AddRow(new ActionRowBuilder().WithButton(GetChangeGameStatusSettingsButton()))
            .AddRow(new ActionRowBuilder().WithButton(GetChangeGameUpdateSettingsButton()))
            .AddRow(new ActionRowBuilder().WithButton(GetChangeSkippedTagsSettingsButton()))
            .Build();

        await FollowupAsync("**Custom Game News Settings** \n", components: customSettingsMessage, ephemeral: true);
    }

    private async Task SendGameAnnouncementsSettingsMessage(GameNewsSetting settings)
    {
        var gameAnnouncementsSettingsMessage = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithButton(GetNewGameAnnouncementButton(settings.ShowNewGameAnnouncements)))
            .AddRow(new ActionRowBuilder().WithButton(GetJustReleasedAnnouncementButton(settings.ShowJustReleasedAnnouncements)))
            .Build();
        await FollowupAsync("**Set Game Announcements Settings** \n", components: gameAnnouncementsSettingsMessage, ephemeral: true);
    }

    private async Task SendGameNewsReleaseSettingsMessage(GameNewsSetting settings)
    {
        //Discord only allows 5 rows! If we want to add more, we have to rethink this.
        var gameStatusSettingsMessage = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithButton(GetAlreadyReleasedGameNewsButton(settings.ShowAlreadyReleasedNews)))
            .AddRow(new ActionRowBuilder().WithButton(GetWillReleaseInYearButton(settings.ShowWillReleaseInYearNews)))
            .AddRow(new ActionRowBuilder().WithButton(GetMightReleaseInYearButton(settings.ShowMightReleaseInYearNews)))
            .AddRow(new ActionRowBuilder().WithButton(GetWillNotReleaseInYearButton(settings.ShowWillNotReleaseInYearNews)))
            .Build();

        await FollowupAsync("**Set Game News Status Settings** \n", components: gameStatusSettingsMessage, ephemeral: true);
    }

    private async Task SendGameNewsUpdateSettingsMessage(GameNewsSetting settings)
    {
        var gameNewsUpdateSettingsMessage = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithButton(GetScoreGameNewsButton(settings.ShowScoreGameNews)))
            .AddRow(new ActionRowBuilder().WithButton(GetEditedGameNewsButton(settings.ShowEditedGameNews)))
            .Build();

        await FollowupAsync("**Set Game News Update Settings** \n", components: gameNewsUpdateSettingsMessage, ephemeral: true);
    }

    private async Task SendUnableToChangeSettingsMessage()
    {
        await FollowupAsync("ℹ️ Unable to change these settings unless Picked Game News Override is off, or Eligible/Ineligible Game News is enabled", ephemeral: true);
    }

    private async Task SendLeagueGameNewsSettingsMessage(MinimalLeagueChannel settings)
    {
        var leagueGameNewsSettingsMessage = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithButton(GetEnablePickedGameNewsButton(settings.ShowPickedGameNews)))
            .AddRow(new ActionRowBuilder().WithButton(GetEnableEligibleLeagueGameNewsOnlyButton(settings.ShowEligibleGameNews)))
            .AddRow(new ActionRowBuilder().WithButton(GetEnableIneligibleLeagueGameNewsOnlyButton(settings.ShowIneligibleGameNews)))
            .AddRow(new ActionRowBuilder().WithSelectMenu(GetNotableMissSettingSelection(settings.NotableMissSetting)))
            .Build();
        await FollowupAsync("**Set League Game News Settings** \n", components: leagueGameNewsSettingsMessage, ephemeral: true);
    }

    private async Task SendGameNewsSkipTagsSettingsMessage(GameNewsChannel settings)
    {
        var gameNewsSkipTagsSettingsMessage = new ComponentBuilder()
            .AddRow(new ActionRowBuilder().WithSelectMenu(GetSkippedTagsSelection(settings.SkippedTags)))
            .Build();
        await FollowupAsync("**Set Game News Skip Tags Settings** \n", components: gameNewsSkipTagsSettingsMessage, ephemeral: true);
    }

    #endregion Setting Category Messages

    #region Handle Interactions

    [ComponentInteraction("button_*")]
    public async Task HandleButtonPresses(string button)
    {
        // Defer the interaction response to extend the response window
        await DeferAsync();

        // Cast the interaction to SocketMessageComponent
        var interaction = Context.Interaction as SocketMessageComponent;

        if (interaction == null)
        {
            await FollowupAsync("Failed to process the button interaction.", ephemeral: true);
            return;
        }

        var guildID = Context.Guild.Id;
        var channelID = Context.Channel.Id;

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

        GameNewsSetting settings = gameNewsChannel?.GameNewsSetting ?? GameNewsSetting.GetOffSetting();

        if (_masterGameTags == null)
        {
            _masterGameTags = await _masterGameRepo.GetMasterGameTags();
        }

        //Special condition for enable game news, as this may be the first time the channel had news set up
        // so we will make a new settings class, and set it default.
        if (button == "enable_game_news")
        {
            await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSetting.GetRecommendedSetting());
            if (leagueChannel != null)
            {
                await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, guildID, channelID, true, true, false, NotableMissSetting.ScoreUpdates);
            }
            await SendGameNewsCommandMessage();
            return;
        }

        //Get Command message ID for any buttons that will update Command
        _channelCommandDict.TryGetValue(Context.Channel.Id, out var CommandMessageID);

        // Toggle the specified setting
        switch (button)
        {
            case "disable_game_news":
                try
                {
                    await Context.Channel.DeleteMessageAsync(CommandMessageID);
                }
                catch (Exception ex)
                {
                    await FollowupAsync($"There was an error trying to delete the original Command message Error:{ex.Message}", ephemeral: true);
                    break;
                }
                await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSetting.GetOffSetting());
                if (leagueChannel != null)
                {
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, false, false, false, NotableMissSetting.None);
                }
                await UpdateCommandMessage();
                break;

            case "set_custom_filters":
                await SendCustomSettingsMessage();
                break;

            case "change_league_news_settings":
                await interaction.DeleteOriginalResponseAsync();

                if (leagueChannel == null)
                {
                    await FollowupAsync("This channel is not a league channel, if you are certain a league has been assigned to this channel contact support", ephemeral: true);
                    return;
                }
                await SendLeagueGameNewsSettingsMessage(leagueChannel);
                break;

            case "change_game_announcements_settings":
                if (leagueChannel != null && CheckIsPickedOnly(leagueChannel))
                {
                    await SendUnableToChangeSettingsMessage();
                    break;
                }
                await SendGameAnnouncementsSettingsMessage(settings);
                break;

            case "change_game_status_settings":
                if (leagueChannel != null && CheckIsPickedOnly(leagueChannel))
                {
                    await SendUnableToChangeSettingsMessage();
                    break;
                }
                await SendGameNewsReleaseSettingsMessage(settings);
                break;

            case "change_game_update_settings":
                if (leagueChannel != null && CheckIsPickedOnly(leagueChannel))
                {
                    await SendUnableToChangeSettingsMessage();
                    break;
                }
                await SendGameNewsUpdateSettingsMessage(settings);
                break;

            case "change_skipped_tags_settings":
                await interaction.DeleteOriginalResponseAsync();
                await SendGameNewsSkipTagsSettingsMessage(gameNewsChannel!);
                break;

            case "picked_game_news":
                await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, guildID, channelID, !leagueChannel.ShowPickedGameNews, leagueChannel.ShowEligibleGameNews, leagueChannel.ShowIneligibleGameNews,
                    leagueChannel.NotableMissSetting);
                await UpdateButtonState("picked_game_news", !leagueChannel.ShowPickedGameNews);
                await UpdateCommandMessage();
                break;

            case "eligible_game_news":
                await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, guildID, channelID, leagueChannel.ShowPickedGameNews, !leagueChannel.ShowEligibleGameNews, leagueChannel.ShowIneligibleGameNews,
                    leagueChannel.NotableMissSetting);
                await UpdateButtonState("eligible_game_news", !leagueChannel.ShowEligibleGameNews);
                await UpdateCommandMessage();
                break;

            case "ineligible_game_news":
                await _discordRepo.SetLeagueGameNewsSetting(leagueChannel!.LeagueID, guildID, channelID, leagueChannel.ShowPickedGameNews, leagueChannel.ShowEligibleGameNews, !leagueChannel.ShowIneligibleGameNews,
                    leagueChannel.NotableMissSetting);
                await UpdateButtonState("ineligible_game_news", !leagueChannel.ShowIneligibleGameNews);
                await UpdateCommandMessage();

                break;

            case "score_game_news":
                var scoreGameNewsChanged = settings with { ShowScoreGameNews = !settings.ShowScoreGameNews };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, scoreGameNewsChanged);
                await UpdateButtonState("score_game_news", scoreGameNewsChanged.ShowScoreGameNews);
                await UpdateCommandMessage();
                break;

            case "just_released_announcements":
                var justReleasedGameNewsChanged = settings with { ShowJustReleasedAnnouncements = !settings.ShowJustReleasedAnnouncements };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, justReleasedGameNewsChanged);
                await UpdateButtonState("just_released_announcements", justReleasedGameNewsChanged.ShowJustReleasedAnnouncements);
                await UpdateCommandMessage();
                break;

            case "new_game_announcements":
                var newGameAnnouncementsChanged = settings with { ShowNewGameAnnouncements = !settings.ShowNewGameAnnouncements };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, newGameAnnouncementsChanged);
                await UpdateButtonState("new_game_announcements", newGameAnnouncementsChanged.ShowNewGameAnnouncements);
                await UpdateCommandMessage();
                break;

            case "already_released_game_news":
                var alreadyReleasedGameNewsChanged = settings with { ShowAlreadyReleasedNews = !settings.ShowAlreadyReleasedNews };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, alreadyReleasedGameNewsChanged);
                await UpdateButtonState("already_released_game_news", alreadyReleasedGameNewsChanged.ShowAlreadyReleasedNews);
                await UpdateCommandMessage();
                break;

            case "will_release_in_year":
                var willReleaseInYearGameNewsChanged = settings with { ShowWillReleaseInYearNews = !settings.ShowWillReleaseInYearNews };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, willReleaseInYearGameNewsChanged);
                await UpdateButtonState("will_release_in_year", willReleaseInYearGameNewsChanged.ShowWillReleaseInYearNews);
                await UpdateCommandMessage();
                break;

            case "might_release_in_year":
                var mightReleaseInYearGameNewsChanged = settings with { ShowMightReleaseInYearNews = !settings.ShowMightReleaseInYearNews };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, mightReleaseInYearGameNewsChanged);
                await UpdateButtonState("might_release_in_year", mightReleaseInYearGameNewsChanged.ShowMightReleaseInYearNews);
                await UpdateCommandMessage();
                break;

            case "will_not_release_in_year":
                var willNotReleaseInYearGameNewsChanged = settings with { ShowWillNotReleaseInYearNews = !settings.ShowWillNotReleaseInYearNews };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, willNotReleaseInYearGameNewsChanged);
                await UpdateButtonState("will_not_release_in_year", willNotReleaseInYearGameNewsChanged.ShowWillNotReleaseInYearNews);
                await UpdateCommandMessage();
                break;

            case "edited_game_news":
                var editedGameNewsChanged = settings with { ShowEditedGameNews = !settings.ShowEditedGameNews };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, editedGameNewsChanged);
                await UpdateButtonState("edited_game_news", editedGameNewsChanged.ShowEditedGameNews);
                await UpdateCommandMessage();
                break;
        }
    }

    [ComponentInteraction("selection_*")]
    public async Task HandleSelectMenu(string selection)
    {
        // Defer the interaction response to extend the response window
        await DeferAsync();

        // Cast the interaction to SocketMessageComponent
        var component = Context.Interaction as SocketMessageComponent;

        if (component == null)
        {
            await FollowupAsync("Failed to process the select menu interaction.", ephemeral: true);
            return;
        }

        //Get Command message ID for any buttons that will update Command
        _channelCommandDict.TryGetValue(Context.Channel.Id, out var CommandMessageID);

        if (_masterGameTags == null)
        {
            _masterGameTags = await _masterGameRepo.GetMasterGameTags();
        }

        // Retrieve the selected values
        var selectedValues = component.Data.Values;

        if (selectedValues == null || !selectedValues.Any())
        {
            return;
        }

        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);

        switch (selection)
        {
            case "notable_miss":
                await _discordRepo.SetLeagueGameNewsSetting(
                    leagueChannel!.LeagueID,
                    Context.Guild.Id,
                    Context.Channel.Id,
                    leagueChannel.ShowPickedGameNews,
                    leagueChannel.ShowEligibleGameNews,
                    leagueChannel.ShowIneligibleGameNews,
                    NotableMissSetting.FromValue(selectedValues.First()));

                await UpdateCommandMessage();
                break;

            case "skipped_tags":
                var selectedTags = _masterGameTags?
                    .Where(tag => selectedValues.Contains(tag.Name))
                    .ToList();

                await _discordRepo.SetSkippedGameNewsTags(Context.Guild.Id, Context.Channel.Id, selectedTags ?? new List<MasterGameTag>());
                await UpdateCommandMessage();
                break;

            case "preset_settings":

                var preset = selectedValues.First();
                switch (preset)
                {
                    case "All":
                        await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSetting.GetAllOnSetting());
                        if (leagueChannel != null)
                        {
                            await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, true, true, true, NotableMissSetting.ScoreUpdates);
                        }
                        await UpdateCommandMessage();
                        break;

                    case "Recommended":
                        var recommendedSettings = GameNewsSetting.GetRecommendedSetting();
                        await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, recommendedSettings);
                        await _discordRepo.SetSkippedGameNewsTags(Context.Guild.Id, Context.Channel.Id, new List<MasterGameTag>());
                        if (leagueChannel != null)
                        {
                            await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, true, true, false, NotableMissSetting.ScoreUpdates);
                        }
                        await UpdateCommandMessage();
                        break;

                    case "LeagueOnly":
                        if (leagueChannel == null)
                        {
                            await FollowupAsync("This channel is not a league channel, if you are certain a league has been assigned to this channel contact support", ephemeral: true);
                            return;
                        }
                        await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, Context.Guild.Id, Context.Channel.Id, true, false, false, NotableMissSetting.None);
                        await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, GameNewsSetting.GetOffSetting());
                        await UpdateCommandMessage();
                        break;
                }
                break;

            default:
                await FollowupAsync($"Couldn't handle Selection menu: {selection} ", ephemeral: true);
                Serilog.Log.Error($"Couldn't handle Selection Menu: {selection}");
                return;
        }
    }

    private async Task UpdateButtonState(string buttonId, bool newState)
    {
        // Retrieve the original response message
        var originalMessage = await Context.Interaction.GetOriginalResponseAsync();

        // Create a new ComponentBuilder to rebuild the components
        var updatedComponentBuilder = new ComponentBuilder();

        // Iterate through the existing components
        foreach (var actionRow in originalMessage.Components)
        {
            var actionRowBuilder = new ActionRowBuilder();

            foreach (var component in actionRow.Components)
            {
                if (component is ButtonComponent button && button.CustomId == "button_" + buttonId)
                {
                    // Replace the target button with the updated button
                    var updatedButton = new ButtonBuilder()
                        .WithCustomId(button.CustomId)
                        .WithLabel(button.Label)
                        .WithEmote(new Emoji(newState ? "✅" : "❌"))
                        .WithStyle(button.Style);

                    actionRowBuilder.WithButton(updatedButton);
                }
                else
                {
                    // Add the existing button or other components as-is
                    actionRowBuilder.AddComponent(component);
                }
            }

            // Add the updated action row to the component builder
            updatedComponentBuilder.AddRow(actionRowBuilder);
        }

        // Modify the original response with the updated components
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Components = updatedComponentBuilder.Build();
        });
    }

    #endregion Handle Interactions

    #region Button Builders

    private static ButtonBuilder GetChangeLeagueNewsSettingsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_change_league_news_settings")
            .WithLabel("Change League News Settings")
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetChangeGameAnnouncementsSettingsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_change_game_announcements_settings")
            .WithLabel("Change Game Announcement Settings")
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetChangeGameStatusSettingsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_change_game_status_settings")
            .WithLabel("Change Game Status Settings")
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetChangeGameUpdateSettingsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_change_game_update_settings")
            .WithLabel("Change Game Update Settings")
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetChangeSkippedTagsSettingsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_change_skipped_tags_settings")
            .WithLabel("Change Skipped Tags Settings")
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetEnableGameNewsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_enable_game_news")
            .WithLabel("Enable Game News")
            .WithStyle(ButtonStyle.Success);
    }

    private static ButtonBuilder GetDisableGameNewsButtion()
    {
        return new ButtonBuilder()
            .WithCustomId("button_disable_game_news")
            .WithLabel("Disable Game News")
            .WithStyle(ButtonStyle.Danger);
    }

    private static ButtonBuilder GetSetCustomFiltersButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_set_custom_filters")
            .WithLabel("Set Custom Filters [Advanced]")
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetEnablePickedGameNewsButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_picked_game_news")
            .WithLabel("Enable Picked Game News")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetEnableEligibleLeagueGameNewsOnlyButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_eligible_game_news")
            .WithLabel("Enable Eligible League Game News")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetEnableIneligibleLeagueGameNewsOnlyButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_ineligible_game_news")
            .WithLabel("Enable Ineligible League Game News")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetAlreadyReleasedGameNewsButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_already_released_game_news")
            .WithLabel("Enable Already Released Game News")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetWillReleaseInYearButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_will_release_in_year")
            .WithLabel("Enable Will Release in Year")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetMightReleaseInYearButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_might_release_in_year")
            .WithLabel("Enable Might Release in Year")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetWillNotReleaseInYearButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_will_not_release_in_year")
            .WithLabel("Enable Will Not Release in Year")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetNewGameAnnouncementButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_new_game_announcements")
            .WithLabel("Enable New Game Announcements")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetEditedGameNewsButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_edited_game_news")
            .WithLabel("Enable Edited Game News")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetJustReleasedAnnouncementButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_just_released_announcements")
            .WithLabel("Enable Just Released Announcements")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    private static ButtonBuilder GetScoreGameNewsButton(bool initialSetting)
    {
        return new ButtonBuilder()
            .WithCustomId("button_score_game_news")
            .WithLabel("Enable Score Game News")
            .WithEmote(new Emoji(initialSetting ? "✅" : "❌"))
            .WithStyle(ButtonStyle.Primary);
    }

    #endregion Button Builders

    #region Select Menu Builders

    private static SelectMenuBuilder GetNotableMissSettingSelection(NotableMissSetting defaultSetting)
    {
        return new SelectMenuBuilder()
            .WithCustomId("selection_notable_miss")
            .WithPlaceholder("Notable Miss Options")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Initial Score", "InitialScore", description: NotableMissSetting.InitialScore.Description, isDefault: defaultSetting == NotableMissSetting.InitialScore)
            .AddOption("Initial Score and Updates", "ScoreUpdates", description: NotableMissSetting.ScoreUpdates.Description, isDefault: defaultSetting == NotableMissSetting.ScoreUpdates)
            .AddOption("None", "None", description: NotableMissSetting.None.Description, isDefault: defaultSetting == NotableMissSetting.None);
    }

    private static SelectMenuBuilder GetPresetSettingSelection(bool isLeagueChannel)
    {
        var selectMenu = new SelectMenuBuilder()
            .WithCustomId("selection_preset_settings")
            .WithPlaceholder("Select Preset Settings")
            .WithMinValues(0)
            .WithMaxValues(1)
            .AddOption("Show All Game News", "All", description: "Sets the channel to receive all game news")
            .AddOption("Show Recommended Game News", "Recommended", description: "Sets the recommended game news settings for this channel.");

        if (isLeagueChannel)
        {
            selectMenu.AddOption("Show League News Only", "LeagueOnly", description: "Only news about games picked by your league, nothing else!");
        }

        return selectMenu;
    }

    private static SelectMenuBuilder GetSkippedTagsSelection(IReadOnlyList<MasterGameTag>? skippedTags)
    {
        if (_masterGameTags == null)
        {
            Serilog.Log.Error("Master Game Tags are null when trying to build Skipped Tags Select Menu.");
            throw new Exception("Master Game Tags are null when trying to build Skipped Tags Select Menu.");
        }

        var selectMenu = new SelectMenuBuilder()
            .WithCustomId("selection_skipped_tags")
            .WithPlaceholder("Select Skipped Tags")
            .WithMinValues(0)
            .WithMaxValues(_masterGameTags.Count);

        foreach (var tag in _masterGameTags)
        {
            var truncatedDescription = tag.Description.Length > 95
                ? tag.Description.Substring(0, 95) + "..."
                : tag.Description;

            selectMenu.AddOption(tag.ReadableName, tag.Name, description: truncatedDescription, isDefault: skippedTags?.Contains(tag));
        }
        return selectMenu;
    }

    #endregion Select Menu Builders

    private bool CheckIsPickedOnly(MinimalLeagueChannel leagueChannel)
    {
        return leagueChannel.ShowPickedGameNews == true
            && leagueChannel.ShowEligibleGameNews == false
            && leagueChannel.ShowIneligibleGameNews == false;
    }
}
