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
    List<SpecialGameSlotViewModel> NewSpecialGameSlots,
    bool CounterPicksMustBeFromThisDraft = true)
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
        var additionalSlots = ConvertToDomainSlots(AdditionalStandardGames, AdditionalCounterPicks, NewSpecialGameSlots, tagDictionary);

        return new CreateLeagueDraftParameters(new LeagueYearKey(LeagueID, Year), Name, ScheduledDate,
            GamesToDraft, CounterPicksToDraft, CounterPicksMustBeFromThisDraft, additionalSlots);
    }

    public static IReadOnlyList<RequestedNewDraftSlot> ConvertToDomainSlots(int standardGames, int counterPicks, IReadOnlyList<SpecialGameSlotViewModel> specialGameSlots, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        //Special slots are counted in standardGames, and are the last of the new standard slots.
        int regularSlotCount = standardGames - specialGameSlots.Count;
        List<RequestedNewDraftSlot> newSlots =
        [
            ..Enumerable.Range(0, regularSlotCount).Select(x => new RequestedNewDraftSlot(false, x, null, [])),
            ..specialGameSlots.OrderBy(x => x.SpecialSlotPosition)
                .Select((x, index) => new RequestedNewDraftSlot(false, regularSlotCount + index, null, x.GetRequiredTags(tagDictionary))),
            ..Enumerable.Range(0, counterPicks).Select(x => new RequestedNewDraftSlot(true, x, null, [])),
        ];

        return newSlots;
    }
}
