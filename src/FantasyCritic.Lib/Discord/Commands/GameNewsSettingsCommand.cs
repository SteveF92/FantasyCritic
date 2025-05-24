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

            // Initialize settings for this interaction
            var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
            var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

            bool isLeagueChannel = leagueChannel != null;

            if (gameNewsChannel == null && !isLeagueChannel)
            {
                await SendDisabledGameNewsMessage();
                return;
            }

            if (isLeagueChannel)
            {
                await SendLeagueGameNewsCommand();
            }
            else
            {
                await SendGameNewsOnlyCommand();
            }
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
        var guildId = Context.Guild.Id;
        var channelId = Context.Channel.Id;

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(guildId, channelId);
        if (leagueChannel != null)
        {
            await SendLeagueGameNewsCommand();
        }
        else
        {
            await SendGameNewsOnlyCommand();
        }
    }

    private async Task SendLeagueGameNewsCommand()
    {
        var message = await FollowupAsync(embed: await CreateCommandMessageEmbed(), components: GetLeagueCommandComponent());
        _channelCommandDict[Context.Channel.Id] = message.Id;
    }

    private async Task SendGameNewsOnlyCommand()
    {
        var message = await FollowupAsync(embed: await CreateCommandMessageEmbed(), components: GetGameNewsOnlyCommandComponent());
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

        if (gameNewsChannel == null)
        {
            Serilog.Log.Error("Could not find the GameNews channel for given channel {ChannelId}", Context.Channel.Id);
            return;
        }

        var updatedComponents = leagueChannel == null ? GetGameNewsOnlyCommandComponent() : GetLeagueCommandComponent();

        var msgEmbed = await CreateCommandMessageEmbed();

        await Context.Channel.ModifyMessageAsync(commandMessageID, msg =>
        {
            msg.Embed = msgEmbed;
            msg.Components = updatedComponents;
        });
    }

    public async Task<Embed> CreateCommandMessageEmbed()
    {
        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

        bool enableGameNews = true;  //<-- Just a filler bool to show game news is on, even though it would be on at this point.
        bool? showPickedGameNews = leagueChannel?.ShowPickedGameNews;
        bool? showEligibleGameNews = leagueChannel?.ShowEligibleGameNews;
        bool? showIneligibleGameNews = leagueChannel?.ShowIneligibleGameNews;
        NotableMissSetting? notableMissSetting = leagueChannel?.NotableMissSetting;
        bool? showAlreadyReleasedGameNews = gameNewsChannel?.GameNewsSetting.ShowAlreadyReleasedGameNews;
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
                        $"> ℹ️  State: {(IsRecommendedSettings(leagueChannel, gameNewsChannel) == true ? "**Recommended**" : "**Custom**")}",
                IsInline = false
            }
        };

        if (leagueChannel != null)
        {
            embedFieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = "League Settings",
                Value = $"> {GetEmoji(showPickedGameNews)} Enable Picked Game News Override\n" +
                        $"> {GetEmoji(showEligibleGameNews)} Show Eligible Game News\n" +
                        $"> {GetEmoji(showIneligibleGameNews)} Show Ineligible Game News\n" +
                        (notableMissSetting != null
                            ? $"> ℹ️ Notable Miss Setting: **{notableMissSetting.ReadableName}**"
                            : ""),
                IsInline = false
            });
        }

        string announcementsSectionHeader = "Game Announcements";
        if (leagueChannel != null && leagueChannel.ShowPickedGameNews)
        {
            announcementsSectionHeader = "Non-Picked Game Announcements";
        }

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = announcementsSectionHeader,
            Value = $"> {GetEmoji(showNewGameAnnouncements)} Show New Game News\n" +
                    $"> {GetEmoji(showJustReleasedAnnouncements)} Show Released Game News",
            IsInline = false
        });

        string gameStatusSettingsSectionHeader = "Game Status Settings";
        if (leagueChannel != null && leagueChannel.ShowPickedGameNews)
        {
            gameStatusSettingsSectionHeader = "Non-Picked Game Status Settings";
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

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = "Game Update Settings",
            Value = $"> {GetEmoji(showScoreGameNews)} Show Score Game News\n" +
                    $"> {GetEmoji(showEditedGameNews)} Show Edited Game News",
            IsInline = false
        });

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = "Skipped Tags",
            Value = skippedTags.Any() ? string.Join("\n", skippedTags.Select(tag => $" - {tag.ReadableName}")) : "None",
            IsInline = false
        });

        var embed = _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Current Game News Settings",
            string.Empty,
            Context.User,
            embedFieldBuilders);

        return embed;
    }

    private static bool IsRecommendedSettings(MinimalLeagueChannel? leagueChannel, GameNewsChannel? gameNewsChannel)
    {
        bool leagueRecommended = leagueChannel is null or { ShowPickedGameNews: true, ShowEligibleGameNews: true };

        bool gameNewsRecommended = gameNewsChannel?.GameNewsSetting.IsRecommended() ?? true;

        bool result = leagueRecommended && gameNewsRecommended;

        return result;
    }

    #endregion Command Messages

    #region Command Components

    private static MessageComponent GetLeagueCommandComponent()
    {
        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton(GetDisableGameNewsButton())
                .WithButton(GetSetRecommendedSettingsButton()))
            .AddRow(new ActionRowBuilder().WithButton(GetChangeLeagueNewsSettingsButton()))
            .AddRow(new ActionRowBuilder()
                .WithButton(GetChangeGameAnnouncementsSettingsButton()))
                .WithButton(GetChangeGameStatusSettingsButton())
                .WithButton(GetChangeGameUpdateSettingsButton())
            .AddRow(new ActionRowBuilder().WithButton(GetChangeSkippedTagsSettingsButton()))
            .Build();
    }

    private static MessageComponent GetGameNewsOnlyCommandComponent()
    {
        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton(GetDisableGameNewsButton())
                .WithButton(GetSetRecommendedSettingsButton()))
            .AddRow(new ActionRowBuilder()
                .WithButton(GetChangeGameAnnouncementsSettingsButton()))
                .WithButton(GetChangeGameStatusSettingsButton())
                .WithButton(GetChangeGameUpdateSettingsButton())
            .AddRow(new ActionRowBuilder().WithButton(GetChangeSkippedTagsSettingsButton()))
            .Build();
    }

    #endregion Command Components

    #region Setting Category Messages

    private async Task SendDisabledGameNewsMessage()
    {
        var enableGameNewsMessage = new ComponentBuilder()
            .WithButton(GetEnableGameNewsButton())
            .Build();

        await FollowupAsync("Game News is currently off for this channel, Do you want to turn it on?:", components: enableGameNewsMessage, ephemeral: true);
    }

    private async Task SendGameAnnoucementsSettingsMessage(GameNewsSetting settings)
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
            .AddRow(new ActionRowBuilder().WithButton(GetAlreadyReleasedGameNewsButton(settings.ShowAlreadyReleasedGameNews)))
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

        var guildID = Context.Guild.Id;
        var channelID = Context.Channel.Id;

        var leagueChannel = await _discordRepo.GetMinimalLeagueChannel(Context.Guild.Id, Context.Channel.Id);
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

        GameNewsSetting settings = gameNewsChannel?.GameNewsSetting ?? GameNewsSetting.GetRecommendedSetting();

        if (_masterGameTags == null)
        {
            _masterGameTags = await _masterGameRepo.GetMasterGameTags();
        }

        //Special condition for enable game news, as this may be the first time the channel had news set up
        // so we will make a new settings class, and set it default.
        if (button == "enable_game_news")
        {
            await _discordRepo.SetGameNewsSetting(Context.Guild.Id, Context.Channel.Id, settings);
            await SendGameNewsCommandMessage();
            if (leagueChannel != null)
            {
                await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, guildID, channelID, true, true, false, NotableMissSetting.ScoreUpdates);
            }
            return;
        }

        //Get Command message ID for any buttons that will update Command
        _channelCommandDict.TryGetValue(Context.Channel.Id, out var CommandMessageID);

        // Toggle the specified setting
        switch (button)
        {
            case "change_league_news_settings":
                if (leagueChannel == null)
                {
                    await FollowupAsync("This channel is not a league channel, if you are certain a league has been assigned to this channel contact support", ephemeral: true);
                    return;
                }
                await SendLeagueGameNewsSettingsMessage(leagueChannel);
                break;

            case "change_game_announcements_settings":
                await SendGameAnnoucementsSettingsMessage(settings);
                break;

            case "change_game_status_settings":
                await SendGameNewsReleaseSettingsMessage(settings);
                break;

            case "change_game_update_settings":
                await SendGameNewsUpdateSettingsMessage(settings);
                break;

            case "change_skipped_tags_settings":
                if (gameNewsChannel == null)
                {
                    await FollowupAsync("This channel is not a game news channel, try resending the game news command, if this persists contact support", ephemeral: true);
                    return;
                }
                await SendGameNewsSkipTagsSettingsMessage(gameNewsChannel);
                break;

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

                await _discordRepo.SetGameNewsSetting(guildID, channelID, GameNewsSetting.GetOffSetting());
                await FollowupAsync("Game News has been disabled", ephemeral: true);
                break;

            case "set_recommended_settings":
                settings = GameNewsSetting.GetRecommendedSetting();
                await _discordRepo.SetGameNewsSetting(guildID, channelID, settings);
                await _discordRepo.SetSkippedGameNewsTags(guildID, channelID, new List<MasterGameTag>());
                if (leagueChannel != null)
                {
                    await _discordRepo.SetLeagueGameNewsSetting(leagueChannel.LeagueID, guildID, channelID, true, true, false, NotableMissSetting.ScoreUpdates);
                }
                await UpdateCommandMessage();
                await FollowupAsync("Recommended settings have been set", ephemeral: true);
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

            case "released_game_news":
                var releasedGameNewsChanged = settings with { ShowJustReleasedAnnouncements = !settings.ShowJustReleasedAnnouncements };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, releasedGameNewsChanged);
                await UpdateButtonState("released_game_news", releasedGameNewsChanged.ShowJustReleasedAnnouncements);
                await UpdateCommandMessage();
                break;

            case "new_game_news":
                var newGameNewsChanged = settings with { ShowNewGameAnnouncements = !settings.ShowNewGameAnnouncements };
                await _discordRepo.SetGameNewsSetting(guildID, channelID, newGameNewsChanged);
                await UpdateButtonState("new_game_news", newGameNewsChanged.ShowNewGameAnnouncements);
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

        if (gameNewsChannel == null)
        {
            await FollowupAsync("Something went wrong, no settings could be found for this channel", ephemeral: true);
            return;
        }

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
            .WithCustomId("button_change_game_announcement_settings")
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

    private static ButtonBuilder GetDisableGameNewsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_disable_game_news")
            .WithLabel("Disable Game News")
            .WithStyle(ButtonStyle.Danger);
    }

    private static ButtonBuilder GetSetRecommendedSettingsButton()
    {
        return new ButtonBuilder()
            .WithCustomId("button_set_recommended_settings")
            .WithLabel("Set Recommended Settings")
            .WithStyle(ButtonStyle.Success);
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
            .WithCustomId("button_new_game_news")
            .WithLabel("Enable New Game News")
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
            .WithCustomId("button_released_game_news")
            .WithLabel("Enable Released Game News")
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
}
