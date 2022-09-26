using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;

public class DropGameRequestViewModel
{
    public DropGameRequestViewModel(DropRequest dropRequest, LocalDate currentDate, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearLookup)
    {
        DropRequestID = dropRequest.DropRequestID;
        PublisherID = dropRequest.Publisher.PublisherID;
        PublisherName = dropRequest.Publisher.PublisherName;
        Timestamp = dropRequest.Timestamp;
        Successful = dropRequest.Successful;

        var masterGameYear = masterGameYearLookup[dropRequest.MasterGame.MasterGameID];
        MasterGame = new MasterGameYearViewModel(masterGameYear, currentDate);
    }

    public Guid DropRequestID { get; }
    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public Instant Timestamp { get; }
    public bool? Successful { get; }
    public MasterGameYearViewModel MasterGame { get; }
}
