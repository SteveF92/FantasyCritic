using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Lib.Domain.Combinations;
public record BidsAndDropsSet(IReadOnlyList<PickupBid> Bids, IReadOnlyList<DropRequest> Drops);
