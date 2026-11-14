using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueYearSettingsResponse
{
    public LeagueYearSettingsResponse(LeagueYear leagueYear)
    {
        LeagueID = leagueYear.League.LeagueID;
        Year = leagueYear.Year;
        LeagueYearName = leagueYear.LeagueYearName;

        StandardGames = leagueYear.Options.StandardGames;
        CounterPicks = leagueYear.Options.CounterPicks;

        UnrestrictedReleaseStatusDroppableGames = leagueYear.Options.UnrestrictedReleaseStatusDroppableGames;
        if (leagueYear.Options.UnrestrictedReleaseStatusDroppableGames == -1)
        {
            UnrestrictedReleaseStatusDroppableGames = 0;
            UnlimitedUnrestrictedReleaseStatusDroppableGames = true;
        }
        WillNotReleaseDroppableGames = leagueYear.Options.WillNotReleaseDroppableGames;
        if (leagueYear.Options.WillNotReleaseDroppableGames == -1)
        {
            WillNotReleaseDroppableGames = 0;
            UnlimitedWillNotReleaseDroppableGames = true;
        }
        WillReleaseDroppableGames = leagueYear.Options.WillReleaseDroppableGames;
        if (leagueYear.Options.WillReleaseDroppableGames == -1)
        {
            WillReleaseDroppableGames = 0;
            UnlimitedWillReleaseDroppableGames = true;
        }
        DropOnlyDraftGames = leagueYear.Options.DropOnlyDraftGames;
        GrantSuperDrops = leagueYear.Options.GrantSuperDrops;
        CounterPicksBlockDrops = leagueYear.Options.CounterPicksBlockDrops;
        AllowMoveIntoIneligible = leagueYear.Options.AllowMoveIntoIneligible;
        MinimumBidAmount = leagueYear.Options.MinimumBidAmount;
        EnableBids = leagueYear.Options.EnableBids;
        BidsOnlyBeforeNextScheduledDraft = leagueYear.Options.BidsOnlyBeforeNextScheduledDraft;

        PickupSystem = leagueYear.Options.PickupSystem.Value;
        TiebreakSystem = leagueYear.Options.TiebreakSystem.Value;
        ScoringSystem = leagueYear.Options.ScoringSystem.Name;
        TradingSystem = leagueYear.Options.TradingSystem.Value;
        ReleaseSystem = leagueYear.Options.ReleaseSystem.Value;
        IneligibleGameSystem = leagueYear.Options.IneligibleGameSystem.Value;
        CounterPickDeadline = leagueYear.CounterPickDeadline;
        MightReleaseDroppableDate = leagueYear.MightReleaseDroppableDate;

        var bannedTags = leagueYear.Options.LeagueTags
            .Where(x => x.Status == TagStatus.Banned)
            .Select(x => x.Tag.Name)
            .ToList();

        var requiredTags = leagueYear.Options.LeagueTags
            .Where(x => x.Status == TagStatus.Required)
            .Select(x => x.Tag.Name)
            .ToList();

        Tags = new LeagueTagOptionsViewModel(bannedTags, requiredTags);
        SpecialGameSlots = leagueYear.Options.SpecialGameSlots.Select(x => new SpecialGameSlotViewModel(x, leagueYear.Options)).ToList();
        HasSpecialSlots = leagueYear.Options.HasSpecialSlots;
        OneShotMode = leagueYear.OneShotMode;
    }

    public Guid LeagueID { get; }
    public int Year { get; }
    public string? LeagueYearName { get; }

    public int StandardGames { get; }
    public int CounterPicks { get; }

    public int UnrestrictedReleaseStatusDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
    public bool UnlimitedUnrestrictedReleaseStatusDroppableGames { get; }
    public bool UnlimitedWillNotReleaseDroppableGames { get; }
    public bool UnlimitedWillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool GrantSuperDrops { get; }
    public bool CounterPicksBlockDrops { get; }
    public bool AllowMoveIntoIneligible { get; }

    public int MinimumBidAmount { get; }
    public bool EnableBids { get; }
    public bool BidsOnlyBeforeNextScheduledDraft { get; }
    public string PickupSystem { get; }
    public string ScoringSystem { get; }
    public string TradingSystem { get; }
    public string TiebreakSystem { get; }
    public string ReleaseSystem { get; }
    public string IneligibleGameSystem { get; }
    public LocalDate CounterPickDeadline { get; }
    public LocalDate? MightReleaseDroppableDate { get; }

    public LeagueTagOptionsViewModel Tags { get; }
    public List<SpecialGameSlotViewModel> SpecialGameSlots { get; }
    public bool HasSpecialSlots { get; }
    public bool OneShotMode { get; }
}
