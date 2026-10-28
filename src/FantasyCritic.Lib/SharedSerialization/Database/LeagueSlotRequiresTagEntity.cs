namespace FantasyCritic.Lib.SharedSerialization.Database;

public class LeagueSlotRequiresTagEntity
{
    public LeagueSlotRequiresTagEntity()
    {

    }

    public LeagueSlotRequiresTagEntity(LeagueSlot leagueSlot, MasterGameTag tag)
    {
        LeagueSlotID = leagueSlot.LeagueSlotID;
        Tag = tag.Name;
    }

    public Guid LeagueSlotID { get; set; }
    public string Tag { get; set; } = null!;
}
