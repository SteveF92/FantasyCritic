using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.MySQL.Entities.Conferences;
internal class ConferenceManagerMessageEntity
{
    public ConferenceManagerMessageEntity()
    {

    }

    public ConferenceManagerMessageEntity(ConferenceYear conferenceYear, ManagerMessage domainMessage)
    {
        MessageID = domainMessage.MessageID;
        ConferenceID = conferenceYear.Conference.ConferenceID;
        Year = conferenceYear.Year;
        MessageText = domainMessage.MessageText;
        IsPublic = domainMessage.IsPublic;
        Timestamp = domainMessage.Timestamp;
    }

    public Guid MessageID { get; set; }
    public Guid ConferenceID { get; set; }
    public int Year { get; set; }
    public string MessageText { get; set; } = null!;
    public bool IsPublic { get; set; }
    public Instant Timestamp { get; set; }

    public ManagerMessage ToDomain(IEnumerable<Guid> dismissedByUserIDs)
    {
        return new ManagerMessage(MessageID, MessageText, IsPublic, Timestamp, dismissedByUserIDs);
    }
}
