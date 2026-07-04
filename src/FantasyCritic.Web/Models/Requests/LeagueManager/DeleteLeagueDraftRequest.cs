namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record DeleteLeagueDraftRequest(Guid DraftID, Guid LeagueID, int Year);
