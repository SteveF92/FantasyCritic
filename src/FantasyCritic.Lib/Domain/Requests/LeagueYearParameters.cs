using FantasyCritic.Lib.Domain.ScoringSystems;

namespace FantasyCritic.Lib.Domain.Requests;

public class LeagueYearParameters
{
    public LeagueYearParameters(Guid leagueID, int year, int standardGames, int gamesToDraft, int counterPicks, int counterPicksToDraft,
        int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames, bool dropOnlyDraftGames, bool grantSuperDrops,
        bool counterPicksBlockDrops, int minimumBidAmount, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots,
        DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem, TradingSystem tradingSystem, TiebreakSystem tiebreakSystem, ReleaseSystem releaseSystem,
        AnnualDate counterPickDeadline, AnnualDate? mightReleaseDroppableDate)
    {
        LeagueID = leagueID;
        Year = year;
        StandardGames = standardGames;
        GamesToDraft = gamesToDraft;
        CounterPicks = counterPicks;
        CounterPicksToDraft = counterPicksToDraft;
        FreeDroppableGames = freeDroppableGames;
        WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
        WillReleaseDroppableGames = willReleaseDroppableGames;
        DropOnlyDraftGames = dropOnlyDraftGames;
        GrantSuperDrops = grantSuperDrops;
        CounterPicksBlockDrops = counterPicksBlockDrops;
        MinimumBidAmount = minimumBidAmount;
        LeagueTags = leagueTags.ToList();
        SpecialGameSlots = specialGameSlots.ToList();
        DraftSystem = draftSystem;
        PickupSystem = pickupSystem;
        ScoringSystem = scoringSystem;
        TradingSystem = tradingSystem;
        TiebreakSystem = tiebreakSystem;
        ReleaseSystem = releaseSystem;
        CounterPickDeadline = counterPickDeadline;
        MightReleaseDroppableDate = mightReleaseDroppableDate;
    }

    public Guid LeagueID { get; }
    public int Year { get; }
    public int StandardGames { get; }
    public int GamesToDraft { get; }
    public int CounterPicks { get; }
    public int CounterPicksToDraft { get; }
    public int FreeDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool GrantSuperDrops { get; }
    public bool CounterPicksBlockDrops { get; }
    public int MinimumBidAmount { get; }
    public IReadOnlyList<LeagueTagStatus> LeagueTags { get; }
    public IReadOnlyList<SpecialGameSlot> SpecialGameSlots { get; }
    public DraftSystem DraftSystem { get; }
    public PickupSystem PickupSystem { get; }
    public ScoringSystem ScoringSystem { get; }
    public TradingSystem TradingSystem { get; }
    public TiebreakSystem TiebreakSystem { get; }
    public ReleaseSystem ReleaseSystem { get; }
    public AnnualDate CounterPickDeadline { get; }
    public AnnualDate? MightReleaseDroppableDate { get; }
}
