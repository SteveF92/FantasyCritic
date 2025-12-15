using FantasyCritic.Lib.Royale;

namespace FantasyCritic.MySQL.Entities;
internal class RoyaleActionEntity
{
    public RoyaleActionEntity(RoyaleAction domain)
    {
        PublisherID = domain.Publisher.PublisherID;
        MasterGameID = domain.PublisherGame.MasterGame.MasterGame.MasterGameID;
        Timestamp = domain.Timestamp;
        ActionType = domain.ActionType;
        Description = domain.Description;
    }

    public Guid PublisherID { get; set; }
    public Guid MasterGameID { get; set; }
    public Instant Timestamp { get; set; }
    public string ActionType { get; set; } = null!;
    public string Description { get; set; } = null!;
}
