namespace FantasyCritic.Lib.Domain.LeagueActions;
public interface ILeagueAction
{
    public string PublisherNameOrManager { get; }
    public Instant Timestamp { get; }
    public string ActionType { get; }
    public string Description { get; }
    public bool ManagerAction { get; }
}
