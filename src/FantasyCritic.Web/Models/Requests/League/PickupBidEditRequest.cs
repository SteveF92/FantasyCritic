namespace FantasyCritic.Web.Models.Requests.League;

public record PickupBidEditRequest(Guid PublisherID, Guid BidID, Guid? ConditionalDropPublisherGameID, uint BidAmount);
