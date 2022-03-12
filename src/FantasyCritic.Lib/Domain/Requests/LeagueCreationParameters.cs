using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Requests;

public class LeagueCreationParameters
{
    public LeagueCreationParameters(FantasyCriticUser manager, string leagueName, int standardGames, int gamesToDraft, int counterPicks, int counterPicksToDraft,
        int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames, bool dropOnlyDraftGames, bool counterPicksBlockDrops, int minimumBidAmount,
        int initialYear, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots,
        DraftSystem draftSystem, PickupSystem pickupSystem, TiebreakSystem tiebreakSystem, ScoringSystem scoringSystem, TradingSystem tradingSystem, bool publicLeague, bool testLeague)
    {
        Manager = manager;
        LeagueName = leagueName;
        StandardGames = standardGames;
        GamesToDraft = gamesToDraft;
        CounterPicks = counterPicks;
        CounterPicksToDraft = counterPicksToDraft;
        FreeDroppableGames = freeDroppableGames;
        WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
        WillReleaseDroppableGames = willReleaseDroppableGames;
        DropOnlyDraftGames = dropOnlyDraftGames;
        CounterPicksBlockDrops = counterPicksBlockDrops;
        MinimumBidAmount = minimumBidAmount;
        InitialYear = initialYear;
        LeagueTags = leagueTags.ToList();
        SpecialGameSlots = specialGameSlots.ToList();
        DraftSystem = draftSystem;
        PickupSystem = pickupSystem;
        TiebreakSystem = tiebreakSystem;
        ScoringSystem = scoringSystem;
        TradingSystem = tradingSystem;
        PublicLeague = publicLeague;
        TestLeague = testLeague;
    }

    public FantasyCriticUser Manager { get; }
    public string LeagueName { get; }
    public int StandardGames { get; }
    public int GamesToDraft { get; }
    public int CounterPicks { get; }
    public int CounterPicksToDraft { get; }
    public int FreeDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool CounterPicksBlockDrops { get; }
    public int MinimumBidAmount { get; }
    public int InitialYear { get; }
    public IReadOnlyList<LeagueTagStatus> LeagueTags { get; }
    public IReadOnlyList<SpecialGameSlot> SpecialGameSlots { get; }
    public DraftSystem DraftSystem { get; }
    public PickupSystem PickupSystem { get; }
    public ScoringSystem ScoringSystem { get; }
    public TradingSystem TradingSystem { get; }
    public TiebreakSystem TiebreakSystem { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
}
