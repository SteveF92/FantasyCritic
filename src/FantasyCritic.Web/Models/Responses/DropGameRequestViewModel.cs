using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;

public class DropGameRequestViewModel
{
    public DropGameRequestViewModel(DropRequest dropRequest, LocalDate currentDate)
    {
        DropRequestID = dropRequest.DropRequestID;
        PublisherID = dropRequest.Publisher.PublisherID;
        PublisherName = dropRequest.Publisher.PublisherName;
        Timestamp = dropRequest.Timestamp;
        Successful = dropRequest.Successful;
        MasterGame = new MasterGameViewModel(dropRequest.MasterGame, currentDate);
    }

    public Guid DropRequestID { get; }
    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public Instant Timestamp { get; }
    public bool? Successful { get; }
    public MasterGameViewModel MasterGame { get; }
}
