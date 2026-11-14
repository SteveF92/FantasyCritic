using System.Linq;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

public static class LeagueYearSettingsResponseExtensions
{
    /// <summary>
    /// The request that saves these settings unchanged, for tests that read a league's options, change some, and send
    /// them back. The lists are copied, so changing the request's lists leaves the response model alone.
    /// </summary>
    public static LeagueYearSettingsRequest ToRequest(this LeagueYearSettingsResponse settings) =>
        new()
        {
            Year = settings.Year,
            LeagueYearName = settings.LeagueYearName,
            StandardGames = settings.StandardGames,
            CounterPicks = settings.CounterPicks,
            UnrestrictedReleaseStatusDroppableGames = settings.UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames = settings.WillNotReleaseDroppableGames,
            WillReleaseDroppableGames = settings.WillReleaseDroppableGames,
            UnlimitedUnrestrictedReleaseStatusDroppableGames = settings.UnlimitedUnrestrictedReleaseStatusDroppableGames,
            UnlimitedWillNotReleaseDroppableGames = settings.UnlimitedWillNotReleaseDroppableGames,
            UnlimitedWillReleaseDroppableGames = settings.UnlimitedWillReleaseDroppableGames,
            DropOnlyDraftGames = settings.DropOnlyDraftGames,
            GrantSuperDrops = settings.GrantSuperDrops,
            CounterPicksBlockDrops = settings.CounterPicksBlockDrops,
            AllowMoveIntoIneligible = settings.AllowMoveIntoIneligible,
            MinimumBidAmount = settings.MinimumBidAmount,
            EnableBids = settings.EnableBids,
            BidsOnlyBeforeNextScheduledDraft = settings.BidsOnlyBeforeNextScheduledDraft,
            PickupSystem = settings.PickupSystem,
            ScoringSystem = settings.ScoringSystem,
            TradingSystem = settings.TradingSystem,
            TiebreakSystem = settings.TiebreakSystem,
            ReleaseSystem = settings.ReleaseSystem,
            IneligibleGameSystem = settings.IneligibleGameSystem,
            CounterPickDeadline = settings.CounterPickDeadline,
            MightReleaseDroppableDate = settings.MightReleaseDroppableDate,
            Tags = new LeagueTagOptionsViewModel { Banned = settings.Tags.Banned.ToList(), Required = settings.Tags.Required.ToList() },
            SpecialGameSlots = settings.SpecialGameSlots
                .Select(slot => new SpecialGameSlotViewModel { SpecialSlotPosition = slot.SpecialSlotPosition, RequiredTags = slot.RequiredTags.ToList() })
                .ToList(),
        };
}
