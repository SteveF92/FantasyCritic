using System.Globalization;
using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.Commands;
public class LeagueOptionsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public LeagueOptionsCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [UsedImplicitly]
    [SlashCommand("league-options", "Get league options.")]
    public async Task GetLeagueOptions(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        if (year != null && supportedYears.All(y => y.Year != year.Value))
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                $"That year was not found for this league. Are you sure a league year is started for {year.Value}?",
                Context.User));
            return;
        }
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears, year);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter(
                "Error Finding League Configuration",
                "No league configuration found for this channel. You may have to specify a year if your league is for an upcoming year.",
                Context.User));
            return;
        }

        var leagueYearOptions = leagueChannel.LeagueYear.Options;

        var picksCounterPicksMessage = $"Total Standard Games: **{leagueYearOptions.StandardGames}**\n";
        picksCounterPicksMessage += $"Games to Draft: **{leagueYearOptions.GamesToDraft}**\n";
        picksCounterPicksMessage += $"Pickup Games: **{leagueYearOptions.StandardGames - leagueYearOptions.GamesToDraft}**\n";
        picksCounterPicksMessage += $"Total Counter Picks: **{leagueYearOptions.CounterPicks}**\n";
        picksCounterPicksMessage += $"Counter Picks to Draft: **{leagueYearOptions.CounterPicksToDraft}**\n";
        if (!leagueYearOptions.CounterPickDeadline.Equals(new AnnualDate(12, 31)))
        {
            picksCounterPicksMessage += $"Counter Pick Deadline: **{leagueYearOptions.CounterPickDeadline.ToString("MMMM d", new DateTimeFormatInfo())}**\n";
        }
        picksCounterPicksMessage += $"Special Game Slots: **{(leagueYearOptions.HasSpecialSlots && leagueYearOptions.SpecialGameSlots.Count > 0 ? leagueYearOptions.SpecialGameSlots.Count : "None")}**\n";

        var bidsAndDropsMessage = $"Minimum Bid Amount: **${leagueYearOptions.MinimumBidAmount}**\n";
        bidsAndDropsMessage += $"\"Any Unreleased\" Droppable Games: **{(leagueYearOptions.FreeDroppableGames == -1 ? "Unlimited" : leagueYearOptions.FreeDroppableGames)}**\n";
        bidsAndDropsMessage += $"\"Will Not Release\" Droppable Games: **{(leagueYearOptions.WillNotReleaseDroppableGames == -1 ? "Unlimited" : leagueYearOptions.WillNotReleaseDroppableGames)}**\n";
        bidsAndDropsMessage += $"\"Will Release\" Droppable Games: **{(leagueYearOptions.WillReleaseDroppableGames == -1 ? "Unlimited" : leagueYearOptions.WillReleaseDroppableGames)}**\n";
        bidsAndDropsMessage += $"Drop Only Draft Games: **{YesOrNo(leagueYearOptions.DropOnlyDraftGames)}**\n";
        bidsAndDropsMessage += $"Automatic Super Drops: **{YesOrNo(leagueYearOptions.GrantSuperDrops)}**\n";
        bidsAndDropsMessage += $"Counter Picks Block Drops: **{YesOrNo(leagueYearOptions.CounterPicksBlockDrops)}**\n";

        var systemBasedOptionsMessage = $"Bidding System: **{leagueYearOptions.PickupSystem.ReadableName}**\n";
        systemBasedOptionsMessage += $"Tiebreak System: **{leagueYearOptions.TiebreakSystem.ReadableName}**\n";
        systemBasedOptionsMessage += $"Trading System: **{leagueYearOptions.TradingSystem.ReadableName}**\n";
        systemBasedOptionsMessage += $"Game Release Rule: **{leagueYearOptions.ReleaseSystem.ReadableName}**\n";
        systemBasedOptionsMessage += $"Scoring Rule: **{leagueYearOptions.ScoringSystem.GetReadableString()}**\n";

        var leagueVisibilityMessage = $"Public League: **{YesOrNo(leagueChannel.LeagueYear.League.PublicLeague)}**\n";
        leagueVisibilityMessage += $"Test League: **{YesOrNo(leagueChannel.LeagueYear.League.TestLeague)}**\n";
        leagueVisibilityMessage += $"Custom Rules League: **{YesOrNo(leagueChannel.LeagueYear.League.CustomRulesLeague)}**\n";

        var embedFieldBuilders = new List<EmbedFieldBuilder>
        {
            BuildEmbedFieldBuilder("Picks & Counter Picks", picksCounterPicksMessage),
            BuildEmbedFieldBuilder("Bids & Drops", bidsAndDropsMessage),
            BuildEmbedFieldBuilder("Banned Tags", BuildBannedTagsList(leagueYearOptions)),
            BuildEmbedFieldBuilder("System-Based Options", systemBasedOptionsMessage),
            BuildEmbedFieldBuilder("League Visibility", leagueVisibilityMessage)
        };

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID,
            leagueChannel.LeagueYear.Year)
            .BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter(
            $"League Options for {leagueChannel.LeagueYear.Year}",
            "",
            Context.User,
            embedFieldBuilders,
            leagueUrl));
    }

    private static EmbedFieldBuilder BuildEmbedFieldBuilder(string title, string message)
    {
        return new EmbedFieldBuilder
        {
            Name = title,
            Value = message,
            IsInline = false
        };
    }

    private static string YesOrNo(bool flagToCheck)
    {
        return flagToCheck ? "Yes" : "No";
    }

    private static string BuildBannedTagsList(LeagueOptions leagueYearOptions)
    {
        var bannedTags = leagueYearOptions.LeagueTags.Where(t => t.Status.Equals(TagStatus.Banned)).ToList();
        return bannedTags.Any()
            ? string.Join(", ", bannedTags.Select(t => t.Tag.ReadableName))
            : string.Empty;
    }
}
