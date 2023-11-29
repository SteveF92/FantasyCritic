using System.Diagnostics.CodeAnalysis;
using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.MySQL.Entities;

internal class LeagueManagerActionEntity
{
    public LeagueManagerActionEntity()
    {

    }

    [SetsRequiredMembers]
    public LeagueManagerActionEntity(LeagueManagerAction action)
    {
        LeagueID = action.LeagueYearKey.LeagueID;
        Year = action.LeagueYearKey.Year;
        Timestamp = action.Timestamp;
        ActionType = action.ActionType;
        Description = action.Description;
    }

    public required Guid LeagueID { get; init; }
    public required int Year { get; init; }
    public required Instant Timestamp { get; init; }
    public required string ActionType { get; init; }
    public required string Description { get; init; }

    public LeagueManagerAction ToDomain()
    {
        LeagueManagerAction action = new LeagueManagerAction(new LeagueYearKey(LeagueID, Year), Timestamp, ActionType, Description);
        return action;
    }
}
