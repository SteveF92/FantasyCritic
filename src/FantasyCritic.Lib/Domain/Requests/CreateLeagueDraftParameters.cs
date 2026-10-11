namespace FantasyCritic.Lib.Domain.Requests;

public class CreateLeagueDraftParameters
{
    public CreateLeagueDraftParameters(LeagueYearKey leagueYearKey, string name, LocalDate? scheduledDate,
        int gamesToDraft, int counterPicksToDraft, bool counterPicksMustBeFromThisDraft, IEnumerable<RequestedNewDraftSlot> additionalSlots)
    {
        LeagueYearKey = leagueYearKey;
        Name = name;
        ScheduledDate = scheduledDate;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        CounterPicksMustBeFromThisDraft = counterPicksMustBeFromThisDraft;
        AdditionalSlots = additionalSlots.ToList();
    }

    public LeagueYearKey LeagueYearKey { get; }
    public string Name { get; }
    public LocalDate? ScheduledDate { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public bool CounterPicksMustBeFromThisDraft { get; }
    public IReadOnlyList<RequestedNewDraftSlot> AdditionalSlots { get; }
}

public record RequestedNewDraftSlot(bool CounterPick, int SlotNumberWithinNewDraft, string? SlotName, IReadOnlyList<MasterGameTag> RequiredTags);
