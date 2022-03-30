namespace FantasyCritic.Web.Models.Requests.League;

public record QueueRankingRequest(Guid PublisherID, List<Guid> QueueRanks);
