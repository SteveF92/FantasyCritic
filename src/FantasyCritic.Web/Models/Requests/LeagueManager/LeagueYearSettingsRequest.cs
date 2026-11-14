using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class LeagueYearSettingsRequest
{
    public int Year { get; init; }
    public string? LeagueYearName { get; init; }

    [Range(1, 50)]
    public int StandardGames { get; init; }
    [Range(0, 50)]
    public int CounterPicks { get; init; }

    [Range(0, 100)]
    public int UnrestrictedReleaseStatusDroppableGames { get; init; }
    [Range(0, 100)]
    public int WillNotReleaseDroppableGames { get; init; }
    [Range(0, 100)]
    public int WillReleaseDroppableGames { get; init; }
    public bool UnlimitedUnrestrictedReleaseStatusDroppableGames { get; init; }
    public bool UnlimitedWillNotReleaseDroppableGames { get; init; }
    public bool UnlimitedWillReleaseDroppableGames { get; init; }
    public bool DropOnlyDraftGames { get; init; }
    public bool GrantSuperDrops { get; init; }
    public bool CounterPicksBlockDrops { get; init; }
    public bool AllowMoveIntoIneligible { get; init; }

    public int MinimumBidAmount { get; init; }
    public bool EnableBids { get; init; }
    public bool BidsOnlyBeforeNextScheduledDraft { get; init; }
    public required string PickupSystem { get; init; }
    public required string ScoringSystem { get; init; }
    public required string TradingSystem { get; init; }
    public required string TiebreakSystem { get; init; }
    public required string ReleaseSystem { get; init; }
    public required string IneligibleGameSystem { get; init; }
    public LocalDate CounterPickDeadline { get; init; }
    public LocalDate? MightReleaseDroppableDate { get; init; }

    public required LeagueTagOptionsViewModel Tags { get; init; }
    public required List<SpecialGameSlotViewModel> SpecialGameSlots { get; init; }

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

        if (SpecialGameSlots.Count > StandardGames)
        {
            return Result.Failure("You cannot have more special game slots than the number of games per player.");
        }

        if (SpecialGameSlots.Any(x => !x.RequiredTags.Any()))
        {
            return Result.Failure("All of your special slots must list at least one tag.");
        }

        return Result.Success();
    }

    public LeagueYearParameters ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
        TradingSystem tradingSystem = Lib.Enums.TradingSystem.FromValue(TradingSystem);
        TiebreakSystem tiebreakSystem = Lib.Enums.TiebreakSystem.FromValue(TiebreakSystem);
        ReleaseSystem releaseSystem = Lib.Enums.ReleaseSystem.FromValue(ReleaseSystem);
        IneligibleGameSystem ineligibleGameSystem = Lib.Enums.IneligibleGameSystem.FromValue(IneligibleGameSystem);
        ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

        int unrestrictedReleaseStatusDroppableGames = UnrestrictedReleaseStatusDroppableGames;
        if (UnlimitedUnrestrictedReleaseStatusDroppableGames)
        {
            unrestrictedReleaseStatusDroppableGames = -1;
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
        var leagueSlots = ConvertToDomainSlots(StandardGames, CounterPicks, SpecialGameSlots, tagDictionary);

        LeagueYearParameters parameters = new LeagueYearParameters(Year, LeagueYearName,
            unrestrictedReleaseStatusDroppableGames, willNotReleaseDroppableGames, willReleaseDroppableGames, DropOnlyDraftGames, GrantSuperDrops, CounterPicksBlockDrops, AllowMoveIntoIneligible, MinimumBidAmount, EnableBids,
            leagueTags, leagueSlots, pickupSystem, scoringSystem, tradingSystem, tiebreakSystem, releaseSystem, ineligibleGameSystem, counterPickDeadline, mightReleaseDroppableDate, BidsOnlyBeforeNextScheduledDraft);
        return parameters;
    }

    public static IReadOnlyList<LeagueSlot> ConvertToDomainSlots(int standardGames, int counterPicks, IReadOnlyList<SpecialGameSlotViewModel> specialGameSlots, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        //Special slots are counted in standardGames, and are the last of the standard slots.
        int regularSlotCount = standardGames - specialGameSlots.Count;
        List<LeagueSlot> slots =
        [
            ..Enumerable.Range(0, regularSlotCount).Select(x => new LeagueSlot(Guid.NewGuid(), false, x, null, [])),
            ..specialGameSlots.OrderBy(x => x.SpecialSlotPosition)
                .Select((x, index) => new LeagueSlot(Guid.NewGuid(), false, regularSlotCount + index, null, x.GetRequiredTags(tagDictionary))),
            ..Enumerable.Range(0, counterPicks).Select(x => new LeagueSlot(Guid.NewGuid(), true, x, null, [])),
        ];

        return slots;
    }
}
