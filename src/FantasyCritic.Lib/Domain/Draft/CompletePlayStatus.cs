using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Draft;
public class CompletePlayStatus
{
    public CompletePlayStatus(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> activeUsers, bool isManager, bool conferenceDraftsNotEnabled)
    {
        PlayStatus = leagueYear.PlayStatus;
        DraftOrderSet = leagueYear.DraftOrderSet;
        ReadyToSetDraftOrder = DraftFunctions.LeagueIsReadyToSetDraftOrder(leagueYear.Publishers, activeUsers);
        StartDraftErrors = DraftFunctions.GetStartDraftResult(leagueYear, activeUsers, isManager, conferenceDraftsNotEnabled);
        DraftStatus = DraftFunctions.GetDraftStatus(leagueYear);
    }

    public PlayStatus PlayStatus { get; }
    public bool DraftOrderSet { get; }
    public bool ReadyToSetDraftOrder { get; }
    public IReadOnlyList<string> StartDraftErrors { get; }
    public DraftStatus? DraftStatus { get; }

    public bool ReadyToDraft => !StartDraftErrors.Any();
}
