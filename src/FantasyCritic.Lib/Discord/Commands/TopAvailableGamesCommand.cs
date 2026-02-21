using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;

public class TopAvailableGamesCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly FantasyCriticSettings _fantasyCriticSettings;
    private readonly GameSearchingService _gameSearchingService;
    private readonly IReadOnlyFantasyCriticUserStore _userStore;
    private readonly IDiscordRepo _discordRepo;
    private readonly IClock _clock;
    private readonly Uri _baseUri;
    private const int GameLimit = 5;

    public TopAvailableGamesCommand(InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings,
        GameSearchingService gameSearchingService,
        IReadOnlyFantasyCriticUserStore userStore,
        IDiscordRepo discordRepo,
        IClock clock)
    {
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _fantasyCriticSettings = fantasyCriticSettings;
        _gameSearchingService = gameSearchingService;
        _userStore = userStore;
        _discordRepo = discordRepo;
        _clock = clock;
        _baseUri = new Uri(fantasyCriticSettings.BaseAddress);
    }

    [UsedImplicitly]
    [SlashCommand("top-available-games",
        "Get top available games for your publisher (must be linked to Discord).")]
    public async Task GetTopAvailableGames()
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var fantasyCriticUser = await _userStore.GetFantasyCriticUserForDiscordUser(Context.User.Id);
        if (fantasyCriticUser == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic User Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var publisherFound = leagueChannel.LeagueYear.Publishers.FirstOrDefault(p =>
            p.User.Id == fantasyCriticUser.Id && leagueChannel.ChannelID == Context.Channel.Id);

        if (publisherFound == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic Publisher Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var topPossibleMasterGameYears = await TopPossibleMasterGameYears(leagueChannel, publisherFound);

        if (topPossibleMasterGameYears.Count == 0)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Top Available Games Found",
                "Unable to find any games.",
                Context.User));
            return;
        }

        var buttonBuilder = new ComponentBuilder();

        if (topPossibleMasterGameYears.Count > GameLimit)
        {
            buttonBuilder.WithButton("Next", $"nextButton:{0}_{leagueChannel.LeagueYear.League.LeagueID}", emote: new Emoji("▶️"));
        }

        await FollowupAsync(
            embed: BuildTopAvailableGamesEmbed(topPossibleMasterGameYears, leagueChannel, 0),
            components: buttonBuilder.Build(),
            ephemeral: true);
    }

    private Embed BuildTopAvailableGamesEmbed(List<PossibleMasterGameYear> topPossibleMasterGameYears,
        LeagueChannel leagueChannel, int newIndex)
    {
        var leagueUrl = new LeagueUrlBuilder(_fantasyCriticSettings.BaseAddress,
            leagueChannel.LeagueYear.League.LeagueID,
            leagueChannel.LeagueYear.Year).BuildUrl();
        var topAvailableGames = topPossibleMasterGameYears.Skip(newIndex * GameLimit).Take(GameLimit).ToList();
        var topAvailableGamesEmbedFields = BuildTopAvailableGamesEmbedFields(topAvailableGames);
        return _discordFormatter.BuildRegularEmbedWithUserFooter(
            "Top Available Games",
            "",
            Context.User, topAvailableGamesEmbedFields, leagueUrl);
    }

    private async Task<List<PossibleMasterGameYear>> TopPossibleMasterGameYears(LeagueChannel leagueChannel, Publisher publisherFound)
    {
        var topAvailableGames =
            await _gameSearchingService.GetTopAvailableGames(leagueChannel.LeagueYear, publisherFound);

        var topPossibleMasterGameYears = topAvailableGames.Select(x => new PossibleMasterGameYear(x.MasterGame,
                x.Taken,
                x.AlreadyOwned,
                x.IsEligible,
                x.IsEligibleInOpenSlot,
                x.IsReleased,
                x.WillReleaseStatus,
                x.HasScore))
            .ToList();
        return topPossibleMasterGameYears;
    }

    private List<EmbedFieldBuilder> BuildTopAvailableGamesEmbedFields(List<PossibleMasterGameYear> topPossibleMasterGameYears)
    {
        var currentDate = _clock.GetCurrentInstant().ToEasternDate();
        var topAvailableGamesEmbedFields = topPossibleMasterGameYears.Select(g => new EmbedFieldBuilder
        {
            Name = g.MasterGame.MasterGame.GameName,
            Value = $"Release Date: {g.MasterGame.MasterGame.ReleaseDate?.ToString() ?? "TBA"}" +
                    $"\nHype Factor: {g.MasterGame.HypeFactor}" +
                    $"\nStatus: {g.GetStatus(currentDate)}" +
                    $"\n[View Game]({new GameUrlBuilder(_fantasyCriticSettings.BaseAddress, g.MasterGame.MasterGame.MasterGameID).GetOnlyUrl()})",
            IsInline = false,
        }).ToList();
        return topAvailableGamesEmbedFields;
    }

    [UsedImplicitly]
    [ComponentInteraction("nextButton:*_*")]
    public async Task NextButton(int currentIndex, string leagueID)
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var fantasyCriticUser = await _userStore.GetFantasyCriticUserForDiscordUser(Context.User.Id);
        if (fantasyCriticUser == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic User Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var publisherFound = leagueChannel.LeagueYear.Publishers.FirstOrDefault(p =>
            p.User.Id == fantasyCriticUser.Id && leagueChannel.ChannelID == Context.Channel.Id);

        if (publisherFound == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic Publisher Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var topPossibleMasterGameYears = await TopPossibleMasterGameYears(leagueChannel, publisherFound);

        var topAvailableGamesEmbedFields = BuildTopAvailableGamesEmbedFields(topPossibleMasterGameYears);

        if (topAvailableGamesEmbedFields.Count == 0)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Top Available Games Found",
                "Unable to find any games.",
                Context.User));
            return;
        }

        var newIndex = currentIndex + 1;

        var buttonBuilder = new ComponentBuilder();

        if (currentIndex >= 0)
        {
            buttonBuilder.WithButton("Previous", $"prevButton:{newIndex}_{leagueID}", emote: new Emoji("◀️"));
            buttonBuilder.WithButton("First", $"firstButton:{newIndex}_{leagueID}", emote: new Emoji("⏮️"));
        }

        var numberOfPages = topAvailableGamesEmbedFields.Count / GameLimit;

        if (newIndex < numberOfPages)
        {
            buttonBuilder.WithButton("Next", $"nextButton:{newIndex}_{leagueID}", emote: new Emoji("▶️"));
        }

        await Context.Interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = BuildTopAvailableGamesEmbed(topPossibleMasterGameYears, leagueChannel, newIndex);
            properties.Components = buttonBuilder.Build();
        });
    }

    [UsedImplicitly]
    [ComponentInteraction("firstButton:*_*")]
    public async Task FirstButton(int currentIndex, string leagueID)
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var fantasyCriticUser = await _userStore.GetFantasyCriticUserForDiscordUser(Context.User.Id);
        if (fantasyCriticUser == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic User Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var publisherFound = leagueChannel.LeagueYear.Publishers.FirstOrDefault(p =>
            p.User.Id == fantasyCriticUser.Id && leagueChannel.ChannelID == Context.Channel.Id);

        if (publisherFound == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic Publisher Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var topPossibleMasterGameYears = await TopPossibleMasterGameYears(leagueChannel, publisherFound);

        var topAvailableGamesEmbedFields = BuildTopAvailableGamesEmbedFields(topPossibleMasterGameYears);

        if (topAvailableGamesEmbedFields.Count == 0)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Top Available Games Found",
                "Unable to find any games.",
                Context.User));
            return;
        }

        var buttonBuilder = new ComponentBuilder();

        var numberOfPages = topAvailableGamesEmbedFields.Count / GameLimit;

        if (numberOfPages > 0)
        {
            buttonBuilder.WithButton("Next", $"nextButton:{0}_{leagueID}", emote: new Emoji("▶️"));
        }

        await Context.Interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = BuildTopAvailableGamesEmbed(topPossibleMasterGameYears, leagueChannel, 0);
            properties.Components = buttonBuilder.Build();
        });
    }


    [UsedImplicitly]
    [ComponentInteraction("prevButton:*_*")]
    public async Task PreviousButton(int currentIndex, string leagueID)
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                "No league configuration found for this channel.",
                Context.User));
            return;
        }

        var fantasyCriticUser = await _userStore.GetFantasyCriticUserForDiscordUser(Context.User.Id);
        if (fantasyCriticUser == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic User Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var publisherFound = leagueChannel.LeagueYear.Publishers.FirstOrDefault(p =>
            p.User.Id == fantasyCriticUser.Id && leagueChannel.ChannelID == Context.Channel.Id);

        if (publisherFound == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Getting Top Available Games",
                $"No Fantasy Critic Publisher Found. You must link your Discord account to Fantasy Critic to use this command. " +
                $"You can do so [here]({new Uri(_baseUri, "/Account/Manage")}).",
                Context.User));
            return;
        }

        var topPossibleMasterGameYears = await TopPossibleMasterGameYears(leagueChannel, publisherFound);

        var topAvailableGamesEmbedFields = BuildTopAvailableGamesEmbedFields(topPossibleMasterGameYears);

        if (topAvailableGamesEmbedFields.Count == 0)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "No Top Available Games Found",
                "Unable to find any games.",
                Context.User));
            return;
        }

        var newIndex = currentIndex - 1;

        var buttonBuilder = new ComponentBuilder();

        if (newIndex - 1 >= 0)
        {
            buttonBuilder.WithButton("Previous", $"prevButton:{newIndex}_{leagueID}", emote: new Emoji("◀"));
            buttonBuilder.WithButton("First", $"firstButton:{newIndex}_{leagueID}", emote: new Emoji("⏮️"));
        }

        var numberOfPages = topAvailableGamesEmbedFields.Count / GameLimit;

        if (newIndex < numberOfPages)
        {
            buttonBuilder.WithButton("Next", $"nextButton:{newIndex}_{leagueID}", emote: new Emoji("▶"));
        }

        await Context.Interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = BuildTopAvailableGamesEmbed(topPossibleMasterGameYears, leagueChannel, newIndex);
            properties.Components = buttonBuilder.Build();
        });
    }
}
