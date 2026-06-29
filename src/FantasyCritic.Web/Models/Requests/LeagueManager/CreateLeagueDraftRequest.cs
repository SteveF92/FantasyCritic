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
    public Result IsValid()
    {
        if (AdditionalStandardGames < 0)
        {
            return Result.Failure("Additional standard games cannot be negative.");
        }

        if (NewSpecialGameSlots.Count > AdditionalStandardGames)
        {
            return Result.Failure("You must add at least as many 'Additional Standard Games' as new special slots. " +
                                  "Otherwise the new special slots would convert existing standard slots into special slots.");
        }

        return Result.Success();
    }

    public CreateLeagueDraftParameters ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        var specialGameSlots = NewSpecialGameSlots.Select(x => x.ToDomain(tagDictionary)).ToList();
        
        return new CreateLeagueDraftParameters(new LeagueYearKey(LeagueID, Year), Name, ScheduledDate,
            GamesToDraft, CounterPicksToDraft, AdditionalStandardGames, AdditionalCounterPicks, 
            specialGameSlots);
    }
}
