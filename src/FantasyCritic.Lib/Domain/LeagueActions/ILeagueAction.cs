namespace FantasyCritic.Lib.Domain.LeagueActions;

public interface ILeagueAction
{
    string PublisherNameOrManager { get; }
    Instant Timestamp { get; }
    string ActionType { get; }
    string Description { get; }
    bool ManagerAction { get; }
}
