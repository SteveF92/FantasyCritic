using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;

namespace FantasyCritic.SharedSerialization.Database;

public class DropRequestEntity
{
    public DropRequestEntity()
    {

    }

    public DropRequestEntity(DropRequest domain)
    {
        DropRequestID = domain.DropRequestID;
        PublisherID = domain.Publisher.PublisherID;
        MasterGameID = domain.MasterGame.MasterGameID;
        Timestamp = domain.Timestamp;
        Successful = domain.Successful;
        ProcessSetID = domain.ProcessSetID;
    }

    public DropRequestEntity(DropRequest domain, bool successful, Guid processSetID)
    {
        DropRequestID = domain.DropRequestID;
        PublisherID = domain.Publisher.PublisherID;
        MasterGameID = domain.MasterGame.MasterGameID;
        Timestamp = domain.Timestamp;
        Successful = successful;
        ProcessSetID = processSetID;
    }

    public Guid DropRequestID { get; set; }
    public Guid PublisherID { get; set; }
    public Guid MasterGameID { get; set; }
    public Instant Timestamp { get; set; }
    public bool? Successful { get; set; }
    public Guid? ProcessSetID { get; set; }

    public DropRequest ToDomain(Publisher publisher, MasterGame masterGame, LeagueYear leagueYear)
    {
        return new DropRequest(DropRequestID, publisher, leagueYear, masterGame, Timestamp, Successful, ProcessSetID);
    }
}
