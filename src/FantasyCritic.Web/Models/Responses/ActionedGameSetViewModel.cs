namespace FantasyCritic.Web.Models.Responses;

public class ActionedGameSetViewModel
{
    public ActionedGameSetViewModel(IEnumerable<MasterGameViewModel> pickupActions, IEnumerable<MasterGameViewModel> dropActions,
        IEnumerable<LeagueActionViewModel> leagueActions, IEnumerable<LeagueActionProcessingSetViewModel> leagueActionSets)
    {
        PickupActions = pickupActions.ToList();
        DropActions = dropActions.ToList();
        LeagueActions = leagueActions.ToList();
        LeagueActionSets = leagueActionSets.ToList();
    }

    public IReadOnlyList<MasterGameViewModel> PickupActions { get; }
    public IReadOnlyList<MasterGameViewModel> DropActions { get; }
    public IReadOnlyList<LeagueActionViewModel> LeagueActions { get; }

    public IReadOnlyList<LeagueActionProcessingSetViewModel> LeagueActionSets { get; }
}
