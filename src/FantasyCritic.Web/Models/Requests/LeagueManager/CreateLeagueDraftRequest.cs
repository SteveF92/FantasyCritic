using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record CreateLeagueDraftRequest(
    Guid LeagueID,
    int Year,
    string Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft,
    int AdditionalStandardGames,
    int AdditionalCounterPicks,
    List<SpecialGameSlotViewModel> NewSpecialSlots);
