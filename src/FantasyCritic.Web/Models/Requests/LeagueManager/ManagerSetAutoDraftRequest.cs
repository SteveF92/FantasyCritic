namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record ManagerSetAutoDraftRequest(Guid LeagueID, int Year, Dictionary<Guid, string> PublisherAutoDraft);
