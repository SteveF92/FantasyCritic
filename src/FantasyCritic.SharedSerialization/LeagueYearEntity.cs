using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.SharedSerialization;

public class LeagueYearEntity
{
    public LeagueYearEntity()
    {

    }

    public LeagueYearEntity(League league, int year, LeagueOptions options, PlayStatus playStatus, bool draftOrderSet)
    {
        LeagueID = league.LeagueID;
        Year = year;

        StandardGames = options.StandardGames;
        GamesToDraft = options.GamesToDraft;
        CounterPicks = options.CounterPicks;
        CounterPicksToDraft = options.CounterPicksToDraft;
        FreeDroppableGames = options.FreeDroppableGames;
        WillNotReleaseDroppableGames = options.WillNotReleaseDroppableGames;
        WillReleaseDroppableGames = options.WillReleaseDroppableGames;
        DropOnlyDraftGames = options.DropOnlyDraftGames;
        GrantSuperDrops = options.GrantSuperDrops;
        CounterPicksBlockDrops = options.CounterPicksBlockDrops;
        MinimumBidAmount = options.MinimumBidAmount;
        CounterPickDeadlineMonth = options.CounterPickDeadline.Month;
        CounterPickDeadlineDay = options.CounterPickDeadline.Day;

        DraftSystem = options.DraftSystem.Value;
        PickupSystem = options.PickupSystem.Value;
        TiebreakSystem = options.TiebreakSystem.Value;
        ScoringSystem = options.ScoringSystem.Name;
        TradingSystem = options.TradingSystem.Value;
        PlayStatus = playStatus.Value;
        DraftOrderSet = draftOrderSet;
    }

    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public int StandardGames { get; set; }
    public int GamesToDraft { get; set; }
    public int CounterPicks { get; set; }
    public int CounterPicksToDraft { get; set; }
    public int FreeDroppableGames { get; set; }
    public int WillNotReleaseDroppableGames { get; set; }
    public int WillReleaseDroppableGames { get; set; }
    public bool DropOnlyDraftGames { get; set; }
    public bool GrantSuperDrops { get; set; }
    public bool CounterPicksBlockDrops { get; set; }
    public int MinimumBidAmount { get; set; }
    public string DraftSystem { get; set; } = null!;
    public string PickupSystem { get; set; } = null!;
    public string TiebreakSystem { get; set; } = null!;
    public string ScoringSystem { get; set; } = null!;
    public string TradingSystem { get; set; } = null!;
    public string PlayStatus { get; set; } = null!;
    public bool DraftOrderSet { get; set; }
    public int CounterPickDeadlineMonth { get; set; }
    public int CounterPickDeadlineDay { get; set; }
    public Instant? DraftStartedTimestamp { get; set; }
    public Guid? WinningUserID { get; set; }

    public LeagueYear ToDomain(League league, SupportedYear year, IEnumerable<EligibilityOverride> eligibilityOverrides,
        IEnumerable<TagOverride> tagOverrides, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots,
        FantasyCriticUser? winningUser, IEnumerable<Publisher> publishersInLeague)
    {
        DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
        PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
        TradingSystem tradingSystem = Lib.Enums.TradingSystem.FromValue(TradingSystem);
        TiebreakSystem tiebreakSystem = Lib.Enums.TiebreakSystem.FromValue(TiebreakSystem);
        ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

        AnnualDate counterPickDeadline = new AnnualDate(CounterPickDeadlineMonth, CounterPickDeadlineDay);

        LeagueOptions options = new LeagueOptions(StandardGames, GamesToDraft, CounterPicks, CounterPicksToDraft, FreeDroppableGames, WillNotReleaseDroppableGames, WillReleaseDroppableGames,
            DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops, MinimumBidAmount, leagueTags, specialGameSlots, draftSystem, pickupSystem, scoringSystem, tradingSystem, tiebreakSystem, counterPickDeadline);

        return new LeagueYear(league, year, options, Lib.Enums.PlayStatus.FromValue(PlayStatus), DraftOrderSet, eligibilityOverrides, tagOverrides, DraftStartedTimestamp, winningUser, publishersInLeague);
    }
}
