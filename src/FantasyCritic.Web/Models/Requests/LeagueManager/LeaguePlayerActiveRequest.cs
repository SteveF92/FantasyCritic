namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record LeaguePlayerActiveRequest(Guid LeagueID, int Year, Dictionary<Guid, bool> ActiveStatus);
