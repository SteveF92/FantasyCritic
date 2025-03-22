namespace FantasyCritic.Lib.SharedSerialization.Database;

public class MasterGameHasTagEntity
{
    public MasterGameHasTagEntity()
    {

    }

    public MasterGameHasTagEntity(MasterGame masterGame, MasterGameTag tag)
    {
        MasterGameID = masterGame.MasterGameID;
        TagName = tag.Name;
    }

    public Guid MasterGameID { get; set; }
    public string TagName { get; set; } = null!;
}
