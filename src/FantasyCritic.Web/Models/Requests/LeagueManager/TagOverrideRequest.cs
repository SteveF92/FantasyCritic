namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record TagOverrideRequest(Guid LeagueID, int Year, Guid MasterGameID, List<string> Tags);
