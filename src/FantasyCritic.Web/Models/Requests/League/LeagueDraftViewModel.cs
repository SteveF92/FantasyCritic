using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueDraftViewModel
{
    public LeagueDraftViewModel(LeagueDraft domain, LeagueYear leagueYear, IEnumerable<FantasyCriticUser> activeUsers, bool isManager, bool conferenceDraftsNotEnabled)
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

        PlayStarted = domain.PlayStatus.PlayStarted;
        DraftIsActive = domain.PlayStatus.DraftIsActive;
        DraftIsPaused = domain.PlayStatus.DraftIsPaused;
        DraftFinished = domain.PlayStatus.DraftFinished;

        if (domain.PlayStatus.DraftIsActiveOrPaused)
        {
            var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
            DraftingCounterPicks = DraftPhase.CounterPicks.Equals(draftStatus?.DraftPhase);
        }

        if (!domain.PlayStatus.PlayStarted)
        {
            ReadyToSetDraftOrder = DraftFunctions.LeagueIsReadyToSetDraftOrder(leagueYear.Publishers, activeUsers);
            StartDraftErrors = DraftFunctions.GetStartDraftResult(leagueYear, domain, activeUsers, isManager, conferenceDraftsNotEnabled);
            ReadyToDraft = !StartDraftErrors.Any();
        }
        else
        {
            StartDraftErrors = [];
        }
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
    public bool PlayStarted { get; }
    public bool DraftIsActive { get; }
    public bool DraftIsPaused { get; }
    public bool DraftFinished { get; }
    public bool DraftingCounterPicks { get; }
    public bool ReadyToSetDraftOrder { get; }
    public IReadOnlyList<string> StartDraftErrors { get; }
    public bool ReadyToDraft { get; }
}
