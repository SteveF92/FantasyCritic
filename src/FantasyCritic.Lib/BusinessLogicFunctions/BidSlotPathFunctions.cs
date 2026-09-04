using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Lib.BusinessLogicFunctions;

public static class BidSlotPathFunctions
{
    public const string NoSlotPathError =
        "You have no open roster spots. Place a drop request or add a conditional drop to this bid.";

    public const string DropRemovalBlockedError =
        "You can't cancel this drop while you have active bids that depend on it.";

    public static bool HasBidSlotAcquisitionPath(
        Publisher publisher,
        LeagueYear leagueYear,
        bool counterPick,
        PublisherGame? conditionalDropOnBid,
        IReadOnlyList<DropRequest> activeDropRequests)
    {
        if (HasOpenSlot(publisher, leagueYear, counterPick))
        {
            return true;
        }

        if (counterPick)
        {
            return false;
        }

        if (conditionalDropOnBid is not null)
        {
            return true;
        }

        return activeDropRequests.Count > 0;
    }

    public static bool WouldBlockDropRemoval(
        Publisher publisher,
        LeagueYear leagueYear,
        DropRequest dropToRemove,
        IReadOnlyList<DropRequest> activeDropRequests,
        IReadOnlyList<PickupBid> activeBids)
    {
        var remainingDrops = activeDropRequests
            .Where(x => x.DropRequestID != dropToRemove.DropRequestID)
            .ToList();

        if (HasOpenSlot(publisher, leagueYear, counterPick: false))
        {
            return false;
        }

        if (remainingDrops.Count > 0)
        {
            return false;
        }

        return activeBids.Any(x => !x.CounterPick && x.ConditionalDropPublisherGame is null);
    }

    private static bool HasOpenSlot(Publisher publisher, LeagueYear leagueYear, bool counterPick) =>
        publisher.GetPublisherSlots(leagueYear)
            .Any(x => x.CounterPick == counterPick && x.PublisherGame is null);
}
