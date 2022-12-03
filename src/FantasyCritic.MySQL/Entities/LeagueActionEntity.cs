using System.Diagnostics.CodeAnalysis;
using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.MySQL.Entities;

internal class LeagueActionEntity
{
    public LeagueActionEntity()
    {

    }

    [SetsRequiredMembers]
    public LeagueActionEntity(LeagueAction action)
    {
        PublisherID = action.Publisher.PublisherID;
        Timestamp = action.Timestamp;
        ActionType = action.ActionType;
        Description = action.Description;
        ManagerAction = action.ManagerAction;
    }

    public required Guid PublisherID { get; init; }
    public required Instant Timestamp { get; init; }
    public required string ActionType { get; init; }
    public required string Description { get; init; }
    public required bool ManagerAction { get; init; }

    public LeagueAction ToDomain(Publisher publisher)
    {
        LeagueAction action = new LeagueAction(publisher, Timestamp, ActionType, Description, ManagerAction);
        return action;
    }
}
