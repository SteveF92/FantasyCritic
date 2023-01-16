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

        if (slot.SpecialGameSlot is not null)
        {
            SpecialSlot = new SpecialGameSlotViewModel(slot.SpecialGameSlot);
        }

        if (slot.PublisherGame is not null)
        {
            PublisherGame = new PublisherGameViewModel(slot.PublisherGame, currentDate, counterPickedPublisherGameIDs.Contains(slot.PublisherGame.PublisherGameID), leagueYear.Options.CounterPicksBlockDrops);
        }

        EligibilityErrors = slot.GetClaimErrorsForSlot(leagueYear).Select(x => x.Error).ToList();
        GameMeetsSlotCriteria = !EligibilityErrors.Any();

        ProjectedFantasyPoints = slot.GetProjectedFantasyPoints(leagueYear.SupportedYear, leagueYear.Options.ScoringSystem, systemWideValues,
            leagueYear.StandardGamesTaken, leagueYear.TotalNumberOfStandardGames);
    }

    public int SlotNumber { get; }
    public int OverallSlotNumber { get; }
    public bool CounterPick { get; }
    public SpecialGameSlotViewModel? SpecialSlot { get; }
    public PublisherGameViewModel? PublisherGame { get; }
    public IReadOnlyList<string> EligibilityErrors { get; }
    public bool GameMeetsSlotCriteria { get; }
    public decimal ProjectedFantasyPoints { get; }
}
