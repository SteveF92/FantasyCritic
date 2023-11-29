namespace FantasyCritic.Lib.Domain.LeagueActions;

public class LeagueManagerAction : ILeagueAction
{
    public LeagueManagerAction(LeagueYearKey leagueYearKey, Instant timestamp, string actionType, string description)
    {
        LeagueYearKey = leagueYearKey;
        Timestamp = timestamp;
        ActionType = actionType;
        Description = description;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueYearKey LeagueYearKey { get; }
    public Instant Timestamp { get; }
    public string ActionType { get; }
    public string Description { get; }
    public Guid ActionInternalID { get; }

    public string PublisherNameOrManager => "Manager";
    public bool ManagerAction => true;
}
