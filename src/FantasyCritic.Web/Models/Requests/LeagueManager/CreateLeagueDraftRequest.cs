using FantasyCritic.Lib.Domain.Requests;
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
    List<SpecialGameSlotViewModel> NewSpecialGameSlots)
{
    public CreateLeagueDraftParameters ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        var specialGameSlots = NewSpecialGameSlots.Select(x => x.ToDomain(tagDictionary)).ToList();
        
        return new CreateLeagueDraftParameters(new LeagueYearKey(LeagueID, Year), Name, ScheduledDate,
            GamesToDraft, CounterPicksToDraft, AdditionalStandardGames, AdditionalCounterPicks, 
            specialGameSlots);
    }
}
