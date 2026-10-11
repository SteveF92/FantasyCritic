namespace FantasyCritic.Lib;

public class LeagueSlot
{
    public LeagueSlot(Guid leagueSlotID, bool counterPick, int slotNumber, string? slotName,
        IEnumerable<MasterGameTag> requiredTags)
    {
        LeagueSlotID = leagueSlotID;
        CounterPick = counterPick;
        SlotNumber = slotNumber;
        SlotName = slotName;
        RequiredTags = requiredTags.ToList();
    }

    public Guid LeagueSlotID { get; }
    public bool CounterPick { get; }
    public int SlotNumber { get; }
    public string? SlotName { get; }
    public IReadOnlyList<MasterGameTag> RequiredTags { get; }
}
