using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses;

public class PublisherSlotViewModel
{
    public PublisherSlotViewModel(PublisherSlot slot, LocalDate currentDate, LeagueYear leagueYear,
        SystemWideValues systemWideValues, IReadOnlySet<Guid> counterPickedPublisherGameIDs)
    {
        SlotNumber = slot.SlotNumber;
        OverallSlotNumber = slot.OverallSlotNumber;
        CounterPick = slot.CounterPick;
        SpecialSlot = slot.SpecialGameSlot.GetValueOrDefault(x => new SpecialGameSlotViewModel(x));
        PublisherGame = slot.PublisherGame.GetValueOrDefault(x => new PublisherGameViewModel(x, currentDate, counterPickedPublisherGameIDs.Contains(x.PublisherGameID), leagueYear.Options.CounterPicksBlockDrops));

        EligibilityErrors = slot.GetClaimErrorsForSlot(leagueYear).Select(x => x.Error).ToList();
        GameMeetsSlotCriteria = !EligibilityErrors.Any();

        var ineligiblePointsShouldCount = !SupportedYear.Year2022FeatureSupported(leagueYear.Year);
        ProjectedFantasyPoints = slot.GetProjectedOrRealFantasyPoints(GameMeetsSlotCriteria || ineligiblePointsShouldCount, leagueYear.PlayStatus.DraftFinished, leagueYear.Options.ScoringSystem, systemWideValues, currentDate);
    }

    public int SlotNumber { get; }
    public int OverallSlotNumber { get; }
    public bool CounterPick { get; }
    public SpecialGameSlotViewModel SpecialSlot { get; }
    public PublisherGameViewModel PublisherGame { get; }
    public IReadOnlyList<string> EligibilityErrors { get; }
    public bool GameMeetsSlotCriteria { get; }
    public decimal? ProjectedFantasyPoints { get; }
}
