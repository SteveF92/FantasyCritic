using FantasyCritic.Lib.Domain.ScoringSystems;

namespace FantasyCritic.Lib.Domain.Requests;

public class LeagueYearParameters
{
    public LeagueYearParameters(Guid leagueID, int year, string? leagueYearName, int standardGames, int counterPicks,
        int unrestrictedReleaseStatusDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames, bool dropOnlyDraftGames, bool grantSuperDrops,
        bool counterPicksBlockDrops, bool allowMoveIntoIneligible, int minimumBidAmount, bool enableBids, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots,
        DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem, TradingSystem tradingSystem, TiebreakSystem tiebreakSystem, ReleaseSystem releaseSystem,
        IneligibleGameSystem ineligibleGameSystem,
        AnnualDate counterPickDeadline, AnnualDate? mightReleaseDroppableDate, bool bidsOnlyBeforeNextScheduledDraft)
    {
        LeagueID = leagueID;
        Year = year;
        LeagueYearName = leagueYearName;
        StandardGames = standardGames;
        CounterPicks = counterPicks;
        UnrestrictedReleaseStatusDroppableGames = unrestrictedReleaseStatusDroppableGames;
        WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
        WillReleaseDroppableGames = willReleaseDroppableGames;
        DropOnlyDraftGames = dropOnlyDraftGames;
        GrantSuperDrops = grantSuperDrops;
        CounterPicksBlockDrops = counterPicksBlockDrops;
        AllowMoveIntoIneligible = allowMoveIntoIneligible;
        MinimumBidAmount = minimumBidAmount;
        EnableBids = enableBids;
        LeagueTags = leagueTags.ToList();
        SpecialGameSlots = specialGameSlots.ToList();
        DraftSystem = draftSystem;
        PickupSystem = pickupSystem;
        ScoringSystem = scoringSystem;
        TradingSystem = tradingSystem;
        TiebreakSystem = tiebreakSystem;
        ReleaseSystem = releaseSystem;
        IneligibleGameSystem = ineligibleGameSystem;
        CounterPickDeadline = counterPickDeadline;
        MightReleaseDroppableDate = mightReleaseDroppableDate;
        BidsOnlyBeforeNextScheduledDraft = bidsOnlyBeforeNextScheduledDraft;
    }

    public Guid LeagueID { get; }
    public int Year { get; }
    public string? LeagueYearName { get; }
    public int StandardGames { get; }
    public int CounterPicks { get; }
    public int UnrestrictedReleaseStatusDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool GrantSuperDrops { get; }
    public bool CounterPicksBlockDrops { get; }
    public bool AllowMoveIntoIneligible { get; }
    public int MinimumBidAmount { get; }
    public bool EnableBids { get; }
    public IReadOnlyList<LeagueTagStatus> LeagueTags { get; }
    public IReadOnlyList<SpecialGameSlot> SpecialGameSlots { get; }
    public DraftSystem DraftSystem { get; }
    public PickupSystem PickupSystem { get; }
    public ScoringSystem ScoringSystem { get; }
    public TradingSystem TradingSystem { get; }
    public TiebreakSystem TiebreakSystem { get; }
    public ReleaseSystem ReleaseSystem { get; }
    public IneligibleGameSystem IneligibleGameSystem { get; }
    public AnnualDate CounterPickDeadline { get; }
    public AnnualDate? MightReleaseDroppableDate { get; }
    public bool BidsOnlyBeforeNextScheduledDraft { get; }
}
