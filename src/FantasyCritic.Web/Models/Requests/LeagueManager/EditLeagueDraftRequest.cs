namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record EditLeagueDraftRequest(
    Guid DraftID,
    Guid LeagueID,
    int Year,
    string Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft);
