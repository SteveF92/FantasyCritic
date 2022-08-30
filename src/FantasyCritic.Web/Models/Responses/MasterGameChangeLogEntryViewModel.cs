namespace FantasyCritic.Web.Models.Responses;

public class MasterGameChangeLogEntryViewModel
{
    public MasterGameChangeLogEntryViewModel(MasterGameChangeLogEntry domain)
    {
        MasterGameChangeID = domain.MasterGameChangeID;
        ChangedByUser = new FantasyCriticUserViewModel(domain.ChangedByUser);
        Timestamp = domain.Timestamp;
        Change = domain.Change;
    }

    public Guid MasterGameChangeID { get; }
    public FantasyCriticUserViewModel ChangedByUser { get; }
    public Instant Timestamp { get; }
    public string Change { get; }
}
