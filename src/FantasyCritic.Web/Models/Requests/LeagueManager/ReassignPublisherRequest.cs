namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record ReassignPublisherRequest(Guid LeagueID, int Year, Guid PublisherID, string InviteDisplayName, int InviteDisplayNumber);
