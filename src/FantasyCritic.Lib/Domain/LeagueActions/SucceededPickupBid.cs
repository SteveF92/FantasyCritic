namespace FantasyCritic.Lib.Domain.LeagueActions;

public class SucceededPickupBid : IProcessedBid
{
    public SucceededPickupBid(PickupBid pickupBid, LeagueSlot slot, string outcome, SystemWideValues systemWideValues)
    {
        PickupBid = pickupBid;
        Slot = slot;
        Outcome = outcome;
        ProjectedPointsAtTimeOfBid = PickupBid.Publisher.GetProjectedFantasyPoints(pickupBid.LeagueYear, systemWideValues);
    }

    public PickupBid PickupBid { get; }
    public LeagueSlot Slot { get; }
    public string Outcome { get; }
    public decimal ProjectedPointsAtTimeOfBid { get; }

    public override string ToString() => PickupBid + "|" + Outcome;

    public PickupBid ToFlatBid(Guid processSetID)
    {
        return new PickupBid(PickupBid.BidID, PickupBid.Publisher, PickupBid.LeagueYear, PickupBid.MasterGame,
            PickupBid.ConditionalDropPublisherGame, PickupBid.CounterPick,
            PickupBid.BidAmount, PickupBid.AllowIneligibleSlot, PickupBid.Priority, PickupBid.Timestamp, true, processSetID, Outcome,
            ProjectedPointsAtTimeOfBid);
    }
}
