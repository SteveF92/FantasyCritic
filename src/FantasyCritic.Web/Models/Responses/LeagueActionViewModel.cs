using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueActionViewModel
{
    public LeagueActionViewModel(LeagueAction leagueAction, IClock clock)
    {
        LeagueName = leagueAction.Publisher.LeagueYear.League.LeagueName;
        PublisherName = leagueAction.Publisher.PublisherName;
        Timestamp = leagueAction.Timestamp.ToDateTimeUtc();
        ActionType = leagueAction.ActionType;
        Description = leagueAction.Description;
        ManagerAction = leagueAction.ManagerAction;
    }

    public string LeagueName { get; }
    public string PublisherName { get; }
    public DateTime Timestamp { get; }
    public string ActionType { get; }
    public string Description { get; }
    public bool ManagerAction { get; }
}