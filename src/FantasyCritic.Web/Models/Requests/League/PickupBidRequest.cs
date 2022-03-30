namespace FantasyCritic.Web.Models.Requests.League;

public record PickupBidRequest(Guid PublisherID, Guid MasterGameID, Guid? ConditionalDropPublisherGameID, bool CounterPick, uint BidAmount);
