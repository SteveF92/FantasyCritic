using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueDraftViewModel
{
    public LeagueDraftViewModel(LeagueDraft domain)
    {
        DraftID = domain.DraftID;
        DraftNumber = domain.DraftNumber;
        Name = domain.Name;
        ScheduledDate = domain.ScheduledDate;
        GamesToDraft = domain.GamesToDraft;
        CounterPicksToDraft = domain.CounterPicksToDraft;
        PlayStatus = domain.PlayStatus.Value;
        DraftStartedTimestamp = domain.DraftStartedTimestamp;
        DraftOrderSet = domain.DraftOrderSet;
        PublisherDraftInfo = domain.PublisherDraftInfo.Select(x => new PublisherDraftInfoViewModel(x)).ToList();
    }

    public Guid DraftID { get; }
    public int DraftNumber { get; }
    public string Name { get; }
    public LocalDate? ScheduledDate { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public string PlayStatus { get; }
    public Instant? DraftStartedTimestamp { get; }
    public bool DraftOrderSet { get; }
    public List<PublisherDraftInfoViewModel> PublisherDraftInfo { get; }
}
