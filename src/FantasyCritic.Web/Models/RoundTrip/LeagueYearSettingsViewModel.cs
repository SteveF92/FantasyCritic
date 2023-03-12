using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using Newtonsoft.Json;

namespace FantasyCritic.Web.Models.RoundTrip;

public class LeagueYearSettingsViewModel
{
    [JsonConstructor]
    public LeagueYearSettingsViewModel(Guid leagueID, int year, int standardGames, int gamesToDraft, int counterPicks,
        int counterPicksToDraft, int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames, bool unlimitedFreeDroppableGames,
        bool unlimitedWillNotReleaseDroppableGames, bool unlimitedWillReleaseDroppableGames, bool dropOnlyDraftGames, bool grantSuperDrops, bool counterPicksBlockDrops,
        int minimumBidAmount, string draftSystem, string pickupSystem, string scoringSystem, string tradingSystem, string tiebreakSystem, string releaseSystem,
        LocalDate counterPickDeadline, LocalDate? mightReleaseDroppableDate, LeagueTagOptionsViewModel tags, List<SpecialGameSlotViewModel> specialGameSlots)
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
        UnlimitedFreeDroppableGames = unlimitedFreeDroppableGames;
        UnlimitedWillNotReleaseDroppableGames = unlimitedWillNotReleaseDroppableGames;
        UnlimitedWillReleaseDroppableGames = unlimitedWillReleaseDroppableGames;
        DropOnlyDraftGames = dropOnlyDraftGames;
        GrantSuperDrops = grantSuperDrops;
        CounterPicksBlockDrops = counterPicksBlockDrops;
        MinimumBidAmount = minimumBidAmount;
        DraftSystem = draftSystem;
        PickupSystem = pickupSystem;
        ScoringSystem = scoringSystem;
        TradingSystem = tradingSystem;
        TiebreakSystem = tiebreakSystem;
        ReleaseSystem = releaseSystem;
        CounterPickDeadline = counterPickDeadline;
        MightReleaseDroppableDate = mightReleaseDroppableDate;

        Tags = tags;
        SpecialGameSlots = specialGameSlots;
    }

    public LeagueYearSettingsViewModel(LeagueYear leagueYear)
    {
        LeagueID = leagueYear.League.LeagueID;
        Year = leagueYear.Year;
        StandardGames = leagueYear.Options.StandardGames;
        GamesToDraft = leagueYear.Options.GamesToDraft;
        CounterPicks = leagueYear.Options.CounterPicks;
        CounterPicksToDraft = leagueYear.Options.CounterPicksToDraft;

        FreeDroppableGames = leagueYear.Options.FreeDroppableGames;
        if (leagueYear.Options.FreeDroppableGames == -1)
        {
            FreeDroppableGames = 0;
            UnlimitedFreeDroppableGames = true;
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
        MinimumBidAmount = leagueYear.Options.MinimumBidAmount;

        DraftSystem = leagueYear.Options.DraftSystem.Value;
        PickupSystem = leagueYear.Options.PickupSystem.Value;
        TiebreakSystem = leagueYear.Options.TiebreakSystem.Value;
        ScoringSystem = leagueYear.Options.ScoringSystem.Name;
        TradingSystem = leagueYear.Options.TradingSystem.Value;
        ReleaseSystem = leagueYear.Options.ReleaseSystem.Value;
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
        SpecialGameSlots = leagueYear.Options.SpecialGameSlots.Select(x => new SpecialGameSlotViewModel(x)).ToList();
        HasSpecialSlots = leagueYear.Options.HasSpecialSlots;
    }

    public Guid LeagueID { get; }
    public int Year { get; }
    [Range(1, 50)]
    public int StandardGames { get; }
    [Range(1, 50)]
    public int GamesToDraft { get; }
    [Range(0, 50)]
    public int CounterPicks { get; }
    [Range(0, 50)]
    public int CounterPicksToDraft { get; }

    [Range(0, 100)]
    public int FreeDroppableGames { get; }
    [Range(0, 100)]
    public int WillNotReleaseDroppableGames { get; }
    [Range(0, 100)]
    public int WillReleaseDroppableGames { get; }
    public bool UnlimitedFreeDroppableGames { get; }
    public bool UnlimitedWillNotReleaseDroppableGames { get; }
    public bool UnlimitedWillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool GrantSuperDrops { get; }
    public bool CounterPicksBlockDrops { get; }

    public int MinimumBidAmount { get; }
    public string DraftSystem { get; }
    public string PickupSystem { get; }
    public string ScoringSystem { get; }
    public string TradingSystem { get; }
    public string TiebreakSystem { get; }
    public string ReleaseSystem { get; }
    public LocalDate CounterPickDeadline { get; }
    public LocalDate? MightReleaseDroppableDate { get; }

    public LeagueTagOptionsViewModel Tags { get; }
    public List<SpecialGameSlotViewModel> SpecialGameSlots { get; }
    public bool HasSpecialSlots { get; }

    public Result IsValid()
    {
        if (Tags.Required.Any())
        {
            return Result.Failure("Impressive API usage, but required tags are not ready for prime time yet.");
        }

        if (CounterPickDeadline.Year != Year)
        {
            return Result.Failure($"The counter pick deadline must be in {Year}.");
        }

        if (MightReleaseDroppableDate.HasValue && MightReleaseDroppableDate.Value.Year != Year)
        {
            return Result.Failure($"The 'might release droppable date' must be in {Year}.");
        }

        return Result.Success();
    }

    public LeagueYearParameters ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
        PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
        TradingSystem tradingSystem = Lib.Enums.TradingSystem.FromValue(TradingSystem);
        TiebreakSystem tiebreakSystem = Lib.Enums.TiebreakSystem.FromValue(TiebreakSystem);
        ReleaseSystem releaseSystem = Lib.Enums.ReleaseSystem.FromValue(ReleaseSystem);
        ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

        int freeDroppableGames = FreeDroppableGames;
        if (UnlimitedFreeDroppableGames)
        {
            freeDroppableGames = -1;
        }
        int willNotReleaseDroppableGames = WillNotReleaseDroppableGames;
        if (UnlimitedWillNotReleaseDroppableGames)
        {
            willNotReleaseDroppableGames = -1;
        }
        int willReleaseDroppableGames = WillReleaseDroppableGames;
        if (UnlimitedWillReleaseDroppableGames)
        {
            willReleaseDroppableGames = -1;
        }

        var counterPickDeadline = new AnnualDate(CounterPickDeadline.Month, CounterPickDeadline.Day);
        AnnualDate? mightReleaseDroppableDate = null;
        if (MightReleaseDroppableDate.HasValue)
        {
            mightReleaseDroppableDate = new AnnualDate(MightReleaseDroppableDate.Value.Month, MightReleaseDroppableDate.Value.Day);
        }

        var leagueTags = Tags.ToDomain(tagDictionary);
        var specialGameSlots = SpecialGameSlots.Select(x => x.ToDomain(tagDictionary));

        LeagueYearParameters parameters = new LeagueYearParameters(LeagueID, Year, StandardGames, GamesToDraft, CounterPicks, CounterPicksToDraft,
            freeDroppableGames, willNotReleaseDroppableGames, willReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops, MinimumBidAmount,
            leagueTags, specialGameSlots, draftSystem, pickupSystem, scoringSystem, tradingSystem, tiebreakSystem, releaseSystem, counterPickDeadline, mightReleaseDroppableDate);
        return parameters;
    }
}
