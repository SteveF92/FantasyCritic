namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record DeleteManagerMessageRequest(Guid LeagueID, int Year, Guid MessageID);
