namespace FantasyCritic.Lib.SharedSerialization.Database;

public class LeagueSlotEntity
{
    public LeagueSlotEntity()
    {

    }

    public LeagueSlotEntity(LeagueYearKey leagueYearKey, LeagueSlot domain)
    {
        LeagueSlotID = domain.LeagueSlotID;
        LeagueID = leagueYearKey.LeagueID;
        Year = leagueYearKey.Year;
        CounterPick = domain.CounterPick;
        SlotNumber = domain.SlotNumber;
        SlotName = domain.SlotName;
    }

    public Guid LeagueSlotID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public bool CounterPick { get; set; }
    public int SlotNumber { get; set; }
    public string? SlotName { get; set; }

    public LeagueSlot ToDomain(IEnumerable<MasterGameTag> requiredTags)
    {
        return new LeagueSlot(LeagueSlotID, CounterPick, SlotNumber, SlotName, requiredTags);
    }
}
