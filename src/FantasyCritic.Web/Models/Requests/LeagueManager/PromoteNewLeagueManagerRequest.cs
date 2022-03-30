namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record PromoteNewLeagueManagerRequest(Guid LeagueID, Guid NewManagerUserID);
