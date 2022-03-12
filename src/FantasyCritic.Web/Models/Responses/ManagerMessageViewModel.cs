namespace FantasyCritic.Web.Models.Responses;

public class ManagerMessageViewModel
{

    public ManagerMessageViewModel(ManagerMessage domain, bool isDismissed)
    {
        MessageID = domain.MessageID;
        MessageText = domain.MessageText;
        IsPublic = domain.IsPublic;
        Timestamp = domain.Timestamp;
        IsDismissed = isDismissed;
    }

    public Guid MessageID { get; }
    public string MessageText { get; }
    public bool IsPublic { get; }
    public Instant Timestamp { get; }
    public bool IsDismissed { get; }
}
