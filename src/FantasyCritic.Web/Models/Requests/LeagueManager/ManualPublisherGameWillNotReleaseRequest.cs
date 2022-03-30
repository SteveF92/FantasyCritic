namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record ManualPublisherGameWillNotReleaseRequest(Guid PublisherID, Guid PublisherGameID, bool WillNotRelease);
