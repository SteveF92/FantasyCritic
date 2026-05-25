using NodaTime;

namespace FantasyCritic.Lib.SharedSerialization.Database;

public class LeagueDraftEntity
{
    public Guid DraftID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public int DraftNumber { get; set; }
    public string Name { get; set; } = null!;
    public DateTime? ScheduledDate { get; set; }
    public int GamesToDraft { get; set; }
    public int CounterPicksToDraft { get; set; }
    public bool DraftOrderSet { get; set; }
    public string PlayStatus { get; set; } = null!;
    public Instant? DraftStartedTimestamp { get; set; }

    public LeagueDraft ToDomain(IEnumerable<PublisherDraftInfo> publisherDraftInfos)
    {
        LocalDate? scheduledDate = ScheduledDate.HasValue
            ? LocalDate.FromDateTime(ScheduledDate.Value)
            : null;

        return new LeagueDraft(DraftID, new LeagueYearKey(LeagueID, Year), DraftNumber, Name, scheduledDate,
            GamesToDraft, CounterPicksToDraft, DraftOrderSet, Lib.Enums.PlayStatus.FromValue(PlayStatus),
            publisherDraftInfos, DraftStartedTimestamp);
    }
}
