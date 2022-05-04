namespace FantasyCritic.Lib.Domain.LeagueActions;
public record SpecialAuctionWithBids(SpecialAuction SpecialAuction, IReadOnlyList<PickupBid> Bids);
