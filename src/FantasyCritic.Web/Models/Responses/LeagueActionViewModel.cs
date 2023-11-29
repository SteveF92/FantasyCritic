using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueActionViewModel
{
    public LeagueActionViewModel(LeagueYear leagueYear, ILeagueAction leagueAction)
    {
        LeagueName = leagueYear.League.LeagueName;
        PublisherName = leagueAction.PublisherNameOrManager;
        Timestamp = leagueAction.Timestamp;
        ActionType = leagueAction.ActionType;
        Description = leagueAction.Description;
        ManagerAction = leagueAction.ManagerAction;
    }

    public string LeagueName { get; }
    public string PublisherName { get; }
    public Instant Timestamp { get; }
    public string ActionType { get; }
    public string Description { get; }
    public bool ManagerAction { get; }
}
