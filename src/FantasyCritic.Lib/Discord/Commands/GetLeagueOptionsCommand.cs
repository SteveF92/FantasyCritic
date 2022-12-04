using System.Globalization;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Discord.Commands;
public class GetLeagueOptionsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public GetLeagueOptionsCommand(IDiscordRepo discordRepo,
        IFantasyCriticRepo fantasyCriticRepo,
        IClock clock,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("league-options", "Get league options.")]
    public async Task GetLeagueOptions(
        [Summary("year", "The year for the league (if not entered, defaults to the current year).")] int? year = null
        )
    {
        await DeferAsync();
        var dateToCheck = _clock.GetGameEffectiveDate(year);

        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, dateToCheck.Year);
        if (leagueChannel == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed(
                "Error Finding League Configuration",
                "No league configuration found for this channel. You may have to specify a year if your league is for an upcoming year.",
                Context.User));
            return;
        }

        var leagueYearOptions = leagueChannel.LeagueYear.Options;

        var message = $"**Total Standard Games:** {leagueYearOptions.StandardGames}\n";
        message += $"**Games to Draft:** {leagueYearOptions.GamesToDraft}\n";
        message += $"**Pickup Games:** {leagueYearOptions.StandardGames - leagueYearOptions.GamesToDraft}\n";
        message += $"**Total Counter Picks:** {leagueYearOptions.CounterPicks}\n";
        message += $"**Counter Picks to Draft:** {leagueYearOptions.CounterPicksToDraft}\n";

        if (!leagueYearOptions.CounterPickDeadline.Equals(new AnnualDate(12, 31)))
        {
            message += $"**Counter Pick Deadline:** {leagueYearOptions.CounterPickDeadline.ToString("MMMM d", new DateTimeFormatInfo())}\n";
        }

        message += $"**Minimum Bid Amount:** {leagueYearOptions.MinimumBidAmount}\n";
        message += $"**\"Any Unreleased\" Droppable Games:**: {(leagueYearOptions.FreeDroppableGames == -1 ? "Unlimited" : leagueYearOptions.FreeDroppableGames)}\n";
        message += $"**\"Will Not Release\" Droppable Games:** {(leagueYearOptions.WillNotReleaseDroppableGames == -1 ? "Unlimited" : leagueYearOptions.WillNotReleaseDroppableGames)}\n";
        message += $"**\"Will Release\" Droppable Games:** {(leagueYearOptions.WillReleaseDroppableGames == -1 ? "Unlimited" : leagueYearOptions.WillReleaseDroppableGames)}\n";
        message += $"**Drop Only Draft Games:** {YesOrNo(leagueYearOptions.DropOnlyDraftGames)}\n";
        message += $"**Automatic Super Drops:** {(YesOrNo(leagueYearOptions.GrantSuperDrops))}\n";
        message += $"**Counter Picks Block Drops:** {(YesOrNo(leagueYearOptions.GrantSuperDrops))}\n";
        message += $"**Bidding System:** {leagueYearOptions.PickupSystem.ReadableName}\n";
        message += $"**Tiebreak System:** {leagueYearOptions.TiebreakSystem}\n";
        message += $"**Trading System:** {leagueYearOptions.TradingSystem.ReadableName}\n";
        message += $"**Banned Tags:** {BuildBannedTagsList(leagueYearOptions)}\n";
        message += $"**Special Game Slots:** {(leagueYearOptions.HasSpecialSlots && leagueYearOptions.SpecialGameSlots.Count > 0 ? leagueYearOptions.SpecialGameSlots.Count : "None")}\n";
        message += $"**Public League:** {(YesOrNo(leagueChannel.LeagueYear.League.PublicLeague))}\n";
        message += $"**Test League:** {YesOrNo(leagueChannel.LeagueYear.League.TestLeague)}\n";

        var leagueUrl = new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID,
            leagueChannel.LeagueYear.Year)
            .BuildUrl();

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            $"League Options for {leagueChannel.LeagueYear.Year}",
            message,
            Context.User,
            url: leagueUrl));
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
