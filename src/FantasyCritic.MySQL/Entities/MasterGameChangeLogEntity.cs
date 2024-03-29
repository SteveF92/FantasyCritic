using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;
internal class MasterGameChangeLogEntity
{
    public MasterGameChangeLogEntity()
    {

    }

    public MasterGameChangeLogEntity(MasterGameChangeLogEntry domain)
    {
        MasterGameChangeID = domain.MasterGameChangeID;
        MasterGameID = domain.MasterGame.MasterGameID;
        ChangedByUserID = domain.ChangedByUser.Id;
        Timestamp = domain.Timestamp;
        Description = domain.Description;
    }

    public Guid MasterGameChangeID { get; set; }
    public Guid MasterGameID { get; set; }
    public Guid ChangedByUserID { get; set; }
    public Instant Timestamp { get; set; }
    public string Description { get; set; } = null!;

    public MasterGameChangeLogEntry ToDomain(MasterGame masterGame, FantasyCriticUser changedByUser)
    {
        return new MasterGameChangeLogEntry(MasterGameChangeID, masterGame, changedByUser, Timestamp, Description);
    }
}
