using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.MySQL.Entities;

internal class LeagueActionEntity
{
    public LeagueActionEntity()
    {

    }

    public LeagueActionEntity(LeagueAction action)
    {
        PublisherID = action.Publisher.PublisherID;
        Timestamp = action.Timestamp;
        ActionType = action.ActionType;
        Description = action.Description;
        ManagerAction = action.ManagerAction;
    }

    public Guid PublisherID { get; set; }
    public Instant Timestamp { get; set; }
    public string ActionType { get; set; }
    public string Description { get; set; }
    public bool ManagerAction { get; set; }

    public LeagueAction ToDomain(Publisher publisher)
    {
        LeagueAction action = new LeagueAction(publisher, Timestamp, ActionType, Description, ManagerAction);
        return action;
    }
}
