using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Draft;
public class CompleteFirstDraftPlayStatus
{
    public CompleteFirstDraftPlayStatus(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> activeUsers, bool isManager, bool conferenceDraftsNotEnabled)
    {
        PlayStatus = leagueYear.FirstDraft.PlayStatus;
        DraftOrderSet = leagueYear.FirstDraft.DraftOrderSet;
        ReadyToSetDraftOrder = DraftFunctions.LeagueIsReadyToSetDraftOrder(leagueYear.Publishers, activeUsers);
        StartDraftErrors = DraftFunctions.GetStartDraftResult(leagueYear, leagueYear.FirstDraft, activeUsers, isManager, conferenceDraftsNotEnabled);
        DraftStatus = DraftFunctions.GetDraftStatus(leagueYear);
    }

    public PlayStatus PlayStatus { get; }
    public bool DraftOrderSet { get; }
    public bool ReadyToSetDraftOrder { get; }
    public IReadOnlyList<string> StartDraftErrors { get; }
    public DraftStatus? DraftStatus { get; }

    public bool ReadyToDraft => !StartDraftErrors.Any();
}
