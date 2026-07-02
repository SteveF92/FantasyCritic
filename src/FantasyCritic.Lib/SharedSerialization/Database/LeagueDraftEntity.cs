namespace FantasyCritic.Lib.SharedSerialization.Database;

public class LeagueDraftEntity
{
    public Guid DraftID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public int DraftNumber { get; set; }
    public string Name { get; set; } = null!;
    public LocalDate? ScheduledDate { get; set; }
    public int GamesToDraft { get; set; }
    public int CounterPicksToDraft { get; set; }
    public bool CounterPicksMustBeFromThisDraft { get; set; }
    public bool DraftOrderSet { get; set; }
    public string PlayStatus { get; set; } = null!;
    public Instant? DraftStartedTimestamp { get; set; }

    public LeagueDraft ToDomain(IEnumerable<PublisherDraftInfo> publisherDraftInfos)
    {
        return new LeagueDraft(DraftID, new LeagueYearKey(LeagueID, Year), DraftNumber, Name, ScheduledDate,
            GamesToDraft, CounterPicksToDraft, CounterPicksMustBeFromThisDraft, DraftOrderSet, Lib.Enums.PlayStatus.FromValue(PlayStatus),
            publisherDraftInfos, DraftStartedTimestamp);
    }
}
