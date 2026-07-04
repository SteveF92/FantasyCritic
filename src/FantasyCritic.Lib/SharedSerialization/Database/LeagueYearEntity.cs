using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.SharedSerialization.Database;

public class LeagueYearEntity
{
    public LeagueYearEntity()
    {

    }

    public LeagueYearEntity(League league, int year, LeagueOptions options, bool? conferenceLocked, bool underReview, string? leagueYearName)
    {
        LeagueID = league.LeagueID;
        Year = year;

        StandardGames = options.StandardGames;
        CounterPicks = options.CounterPicks;
        UnrestrictedReleaseStatusDroppableGames = options.UnrestrictedReleaseStatusDroppableGames;
        WillNotReleaseDroppableGames = options.WillNotReleaseDroppableGames;
        WillReleaseDroppableGames = options.WillReleaseDroppableGames;
        DropOnlyDraftGames = options.DropOnlyDraftGames;
        GrantSuperDrops = options.GrantSuperDrops;
        CounterPicksBlockDrops = options.CounterPicksBlockDrops;
        AllowMoveIntoIneligible = options.AllowMoveIntoIneligible;
        MinimumBidAmount = options.MinimumBidAmount;
        EnableBids = options.EnableBids;
        BidsOnlyBeforeNextScheduledDraft = options.BidsOnlyBeforeNextScheduledDraft;
        CounterPickDeadlineMonth = options.CounterPickDeadline.Month;
        CounterPickDeadlineDay = options.CounterPickDeadline.Day;
        MightReleaseDroppableMonth = options.MightReleaseDroppableDate?.Month;
        MightReleaseDroppableDay = options.MightReleaseDroppableDate?.Day;

        DraftSystem = options.DraftSystem.Value;
        PickupSystem = options.PickupSystem.Value;
        TiebreakSystem = options.TiebreakSystem.Value;
        ScoringSystem = options.ScoringSystem.Name;
        TradingSystem = options.TradingSystem.Value;
        ReleaseSystem = options.ReleaseSystem.Value;
        IneligibleGameSystem = options.IneligibleGameSystem.Value;
        ConferenceLocked = conferenceLocked;

        UnderReview = underReview;
        LeagueYearName = leagueYearName;
    }

    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public string? LeagueYearName { get; set; }
    public int StandardGames { get; set; }
    public int CounterPicks { get; set; }
    public int UnrestrictedReleaseStatusDroppableGames { get; set; }
    public int WillNotReleaseDroppableGames { get; set; }
    public int WillReleaseDroppableGames { get; set; }
    public bool DropOnlyDraftGames { get; set; }
    public bool GrantSuperDrops { get; set; }
    public bool CounterPicksBlockDrops { get; set; }
    public bool AllowMoveIntoIneligible { get; set; }
    public int MinimumBidAmount { get; set; }
    public bool EnableBids { get; set; }
    public bool BidsOnlyBeforeNextScheduledDraft { get; set; }
    public string DraftSystem { get; set; } = null!;
    public string PickupSystem { get; set; } = null!;
    public string TiebreakSystem { get; set; } = null!;
    public string ScoringSystem { get; set; } = null!;
    public string TradingSystem { get; set; } = null!;
    public string ReleaseSystem { get; set; } = null!;
    public string IneligibleGameSystem { get; set; } = null!;
    public int CounterPickDeadlineMonth { get; set; }
    public int CounterPickDeadlineDay { get; set; }
    public int? MightReleaseDroppableMonth { get; set; }
    public int? MightReleaseDroppableDay { get; set; }
    public Guid? WinningUserID { get; set; }
    public bool? ConferenceLocked { get; set; }
    public bool UnderReview { get; set; }

    public LeagueYear ToDomain(League league, SupportedYear year, IEnumerable<EligibilityOverride> eligibilityOverrides,
        IEnumerable<TagOverride> tagOverrides, IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<SpecialGameSlot> specialGameSlots,
        FantasyCriticUser? winningUser, IEnumerable<Publisher> publishersInLeague, IEnumerable<LeagueDraft> leagueDrafts)
    {
        DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
        PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
        TradingSystem tradingSystem = Lib.Enums.TradingSystem.FromValue(TradingSystem);
        TiebreakSystem tiebreakSystem = Lib.Enums.TiebreakSystem.FromValue(TiebreakSystem);
        ReleaseSystem releaseSystem = Lib.Enums.ReleaseSystem.FromValue(ReleaseSystem);
        IneligibleGameSystem ineligibleGameSystem = Lib.Enums.IneligibleGameSystem.FromValue(IneligibleGameSystem);
        ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

        AnnualDate counterPickDeadline = new AnnualDate(CounterPickDeadlineMonth, CounterPickDeadlineDay);

        AnnualDate? mightReleaseDroppableDate = null;
        if (MightReleaseDroppableMonth.HasValue && MightReleaseDroppableDay.HasValue)
        {
            mightReleaseDroppableDate = new AnnualDate(MightReleaseDroppableMonth.Value, MightReleaseDroppableDay.Value);
        }
        
        LeagueOptions options = new LeagueOptions(StandardGames, CounterPicks, UnrestrictedReleaseStatusDroppableGames, WillNotReleaseDroppableGames, WillReleaseDroppableGames,
            DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops, AllowMoveIntoIneligible, MinimumBidAmount,
            EnableBids, leagueTags, specialGameSlots, draftSystem, pickupSystem, scoringSystem, tradingSystem, tiebreakSystem, releaseSystem, ineligibleGameSystem,
            counterPickDeadline, mightReleaseDroppableDate, BidsOnlyBeforeNextScheduledDraft);

        return new LeagueYear(league, year, options, leagueDrafts, eligibilityOverrides, tagOverrides,
            winningUser, publishersInLeague, ConferenceLocked, UnderReview, LeagueYearName);
    }
}
