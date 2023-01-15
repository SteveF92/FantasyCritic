namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record PlayerRemoveRequest(Guid UserID, Guid LeagueID, bool AllYears);
