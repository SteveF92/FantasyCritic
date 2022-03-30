namespace FantasyCritic.Web.Models.Requests.League;

public record BidPriorityOrderRequest(Guid PublisherID, List<Guid> BidPriorities);
