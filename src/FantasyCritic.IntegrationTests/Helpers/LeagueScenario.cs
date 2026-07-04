using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Encapsulates all settings required to create a test league.
/// Pass an instance to <see cref="LeagueTestHelpers.CreateLeagueAsync"/> and use the
/// scenario's expected counts in test assertions.
/// </summary>
public sealed class LeagueScenario
{
    public required string Name { get; init; }

    // Player / publisher counts
    public int PlayerCount { get; init; } = 4;

    // Publisher slot counts
    public int StandardGames { get; init; } = 6;
    public int GamesToDraft { get; init; } = 6;
    public int CounterPicks { get; init; } = 1;
    public int CounterPicksToDraft { get; init; } = 1;

    // Droppable game limits — all zero for "one-shot" leagues
    public int UnrestrictedReleaseStatusDroppableGames { get; init; } = 0;
    public int WillNotReleaseDroppableGames { get; init; } = 0;
    public int WillReleaseDroppableGames { get; init; } = 0;

    // Drop / special rules
    public bool DropOnlyDraftGames { get; init; } = true;
    public bool GrantSuperDrops { get; init; } = false;
    public bool CounterPicksBlockDrops { get; init; } = true;
    public bool AllowMoveIntoIneligible { get; init; } = false;

    // Bid rules
    public int MinimumBidAmount { get; init; } = 0;
    public bool EnableBids { get; init; } = false;
    public bool BidsOnlyBeforeNextScheduledDraft { get; init; } = false;

    // System strings — must match the enum names used by the server
    public string DraftSystem { get; init; } = "Flexible";
    public string PickupSystem { get; init; } = "SemiPublicBiddingSecretCounterPicks";
    public string ScoringSystem { get; init; } = "LinearPositive";
    public string TradingSystem { get; init; } = "Standard";
    public string TiebreakSystem { get; init; } = "LowestProjectedPoints";
    public string ReleaseSystem { get; init; } = "MustBeReleased";
    public string IneligibleGameSystem { get; init; } = "DroppableAsWillNotRelease";

    // Special slots — none by default
    public bool HasSpecialSlots { get; init; } = false;
    public IReadOnlyList<SpecialGameSlotViewModel> SpecialGameSlots { get; init; } = [];

    /// <summary>
    /// Builds the <see cref="LeagueYearSettingsViewModel"/> request body for the given year.
    /// </summary>
    public LeagueYearSettingsViewModel BuildSettings(int year) =>
        new()
        {
            LeagueID = Guid.Empty,
            Year = year,
            LeagueYearName = null,
            StandardGames = StandardGames,
            CounterPicks = CounterPicks,
            UnrestrictedReleaseStatusDroppableGames = UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames = WillNotReleaseDroppableGames,
            WillReleaseDroppableGames = WillReleaseDroppableGames,
            UnlimitedUnrestrictedReleaseStatusDroppableGames = false,
            UnlimitedWillNotReleaseDroppableGames = false,
            UnlimitedWillReleaseDroppableGames = false,
            DropOnlyDraftGames = DropOnlyDraftGames,
            GrantSuperDrops = GrantSuperDrops,
            CounterPicksBlockDrops = CounterPicksBlockDrops,
            AllowMoveIntoIneligible = AllowMoveIntoIneligible,
            MinimumBidAmount = MinimumBidAmount,
            EnableBids = EnableBids,
            BidsOnlyBeforeNextScheduledDraft = BidsOnlyBeforeNextScheduledDraft,
            DraftSystem = DraftSystem,
            PickupSystem = PickupSystem,
            ScoringSystem = ScoringSystem,
            TradingSystem = TradingSystem,
            TiebreakSystem = TiebreakSystem,
            ReleaseSystem = ReleaseSystem,
            IneligibleGameSystem = IneligibleGameSystem,
            // December 1st of the draft year is a safe counter-pick deadline for test leagues
            CounterPickDeadline = new DateTimeOffset(year, 12, 1, 0, 0, 0, TimeSpan.Zero),
            MightReleaseDroppableDate = null,
            Tags = new LeagueTagOptionsViewModel { Banned = [], Required = [] },
            HasSpecialSlots = HasSpecialSlots,
            SpecialGameSlots = SpecialGameSlots.ToList(),
        };

    public IReadOnlyList<DraftSettingsRequest> BuildDraftSettings() =>
        new List<DraftSettingsRequest>
        {
            new()
            {
                Name = null,
                ScheduledDate = null,
                GamesToDraft = GamesToDraft,
                CounterPicksToDraft = CounterPicksToDraft,
            }
        };

    public override string ToString() => Name;
}

/// <summary>
/// Pre-defined scenarios used across test fixtures.
/// </summary>
public static class LeagueScenarios
{
    /// <summary>
    /// A simple 4-player league: 6 standard games + 1 counter-pick per publisher,
    /// Flexible draft, LinearPositive scoring, no drops or bids (one-shot).
    /// </summary>
    public static readonly LeagueScenario Standard = new()
    {
        Name = "Standard",
        PlayerCount = 4,
        StandardGames = 6,
        GamesToDraft = 6,
        CounterPicks = 1,
        CounterPicksToDraft = 1,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = false
    };

    /// <summary>
    /// Same as <see cref="Standard"/> but with automatic super drops enabled.
    /// </summary>
    public static readonly LeagueScenario StandardWithSuperDrops = new()
    {
        Name = "StandardWithSuperDrops",
        PlayerCount = Standard.PlayerCount,
        StandardGames = Standard.StandardGames,
        GamesToDraft = Standard.GamesToDraft,
        CounterPicks = Standard.CounterPicks,
        CounterPicksToDraft = Standard.CounterPicksToDraft,
        DraftSystem = Standard.DraftSystem,
        PickupSystem = Standard.PickupSystem,
        ScoringSystem = Standard.ScoringSystem,
        TradingSystem = Standard.TradingSystem,
        TiebreakSystem = Standard.TiebreakSystem,
        ReleaseSystem = Standard.ReleaseSystem,
        IneligibleGameSystem = Standard.IneligibleGameSystem,
        UnrestrictedReleaseStatusDroppableGames = Standard.UnrestrictedReleaseStatusDroppableGames,
        WillNotReleaseDroppableGames = Standard.WillNotReleaseDroppableGames,
        WillReleaseDroppableGames = Standard.WillReleaseDroppableGames,
        DropOnlyDraftGames = Standard.DropOnlyDraftGames,
        GrantSuperDrops = true,
        CounterPicksBlockDrops = Standard.CounterPicksBlockDrops,
        AllowMoveIntoIneligible = Standard.AllowMoveIntoIneligible,
        MinimumBidAmount = Standard.MinimumBidAmount,
        EnableBids = true
    };

    /// <summary>
    /// A minimal 3-player league for auto-skip view-model tests. Standard picks only (no counter-pick
    /// phase). Bids disabled so manager claims can pre-fill roster slots before the draft starts.
    /// </summary>
    public static readonly LeagueScenario ThreePlayerAutoSkip = new()
    {
        Name = "ThreePlayerAutoSkip",
        PlayerCount = 3,
        StandardGames = 2,
        GamesToDraft = 2,
        CounterPicks = 0,
        CounterPicksToDraft = 0,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "NoTrades",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = false,
    };

    /// <summary>
    /// A minimal 2-player league used by error-case and edge-case test fixtures.
    /// 2 standard games + 1 counter-pick per publisher. Fast to set up and exhaust.
    /// </summary>
    public static readonly LeagueScenario TwoPlayerSmall = new()
    {
        Name = "TwoPlayerSmall",
        PlayerCount = 2,
        StandardGames = 2,
        GamesToDraft = 2,
        CounterPicks = 1,
        CounterPicksToDraft = 1,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = true
    };

    /// <summary>
    /// A 4-player league configured for bid-processing tests: 6 standard slots (3 drafted, 3 open for bids)
    /// and 2 counter-pick slots (1 drafted, 1 open for counter-pick bids).
    /// </summary>
    public static readonly LeagueScenario FourPlayerBidding = new()
    {
        Name = "FourPlayerBidding",
        PlayerCount = 4,
        StandardGames = 6,
        GamesToDraft = 3,
        CounterPicks = 2,
        CounterPicksToDraft = 1,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = true
    };

    /// <summary>
    /// A 4-player league configured for drop and conditional-drop processing tests:
    /// full post-draft rosters (4 standard + 1 counter-pick per publisher) with droppable-game
    /// limits enabled so standalone drops, drop-then-bid, and conditional-drop bids can run.
    /// </summary>
    public static readonly LeagueScenario FourPlayerDrops = new()
    {
        Name = "FourPlayerDrops",
        PlayerCount = 4,
        StandardGames = 4,
        GamesToDraft = 4,
        CounterPicks = 1,
        CounterPicksToDraft = 1,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 3,
        WillReleaseDroppableGames = 1,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = true,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = true
    };

    /// <summary>
    /// A 4-player league configured for eligibility-change tests: 6 standard slots
    /// (3 drafted, 3 open for bids) and 2 counter-pick slots (1 drafted, 1 open for
    /// counter-pick bids). Each player gets 2 will-release drop allowances so that
    /// P3 can successfully complete both a standalone drop and a conditional-drop bid
    /// in the same processing run.
    /// CounterPicksBlockDrops is false so that all drafted standard games are
    /// droppable regardless of which games other players chose as counter-picks.
    /// </summary>
    public static readonly LeagueScenario FourPlayerEligibilityChange = new()
    {
        Name = "FourPlayerEligibilityChange",
        PlayerCount = 4,
        StandardGames = 6,
        GamesToDraft = 3,
        CounterPicks = 2,
        CounterPicksToDraft = 1,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "OnlyNeedsScore",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 2,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = false,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = true
    };
}
