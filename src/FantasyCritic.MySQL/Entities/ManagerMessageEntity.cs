namespace FantasyCritic.MySQL.Entities;

internal class ManagerMessageEntity
{
    public ManagerMessageEntity()
    {

    }

    public ManagerMessageEntity(LeagueYear leagueYear, ManagerMessage domainMessage)
    {
        MessageID = domainMessage.MessageID;
        LeagueID = leagueYear.League.LeagueID;
        Year = leagueYear.Year;
        MessageText = domainMessage.MessageText;
        IsPublic = domainMessage.IsPublic;
        Timestamp = domainMessage.Timestamp;
    }

    public Guid MessageID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public string MessageText { get; set; }
    public bool IsPublic { get; set; }
    public Instant Timestamp { get; set; }

    public ManagerMessage ToDomain(IEnumerable<Guid> dismissedByUserIDs)
    {
        return new ManagerMessage(MessageID, MessageText, IsPublic, Timestamp, dismissedByUserIDs);
    }
}
