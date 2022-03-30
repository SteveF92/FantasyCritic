namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record CreatePublisherForUserRequest(Guid LeagueID, int Year, Guid UserID, string PublisherName);
