namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record RemovePlayerFromLeagueRequest(Guid LeagueID, Guid UserID);
