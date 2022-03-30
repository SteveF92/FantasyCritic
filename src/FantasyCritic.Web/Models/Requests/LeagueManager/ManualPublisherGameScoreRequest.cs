namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record ManualPublisherGameScoreRequest(Guid PublisherID, Guid PublisherGameID, decimal ManualCriticScore);
