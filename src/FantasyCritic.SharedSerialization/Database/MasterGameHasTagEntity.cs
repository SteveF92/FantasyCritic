using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.SharedSerialization.Database;

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
    public Instant TimeAdded { get; set; }
}
