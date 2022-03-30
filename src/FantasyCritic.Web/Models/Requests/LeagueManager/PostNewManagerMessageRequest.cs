namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record PostNewManagerMessageRequest(Guid LeagueID, int Year, string Message, bool IsPublic);
