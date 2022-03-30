namespace FantasyCritic.Web.Models.Requests.League;

public record CreatePublisherRequest(Guid LeagueID, int Year, string PublisherName);
