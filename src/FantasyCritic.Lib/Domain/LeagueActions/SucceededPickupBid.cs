namespace FantasyCritic.Lib.Domain.LeagueActions;

public class SucceededPickupBid : IProcessedBid
{
    public SucceededPickupBid(PickupBid pickupBid, int slotNumber, string outcome, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        PickupBid = pickupBid;
        SlotNumber = slotNumber;
        Outcome = outcome;
        ProjectedPointsAtTimeOfBid = PickupBid.Publisher.GetProjectedFantasyPoints(pickupBid.LeagueYear, systemWideValues, currentDate);
    }

    public PickupBid PickupBid { get; }
    public int SlotNumber { get; }
    public string Outcome { get; }
    public decimal ProjectedPointsAtTimeOfBid { get; }

    public override string ToString() => PickupBid.ToString() + "|" + Outcome;

    public PickupBid ToFlatBid(Guid processSetID)
    {
        return new PickupBid(PickupBid.BidID, PickupBid.Publisher, PickupBid.LeagueYear, PickupBid.MasterGame,
            PickupBid.ConditionalDropPublisherGame, PickupBid.CounterPick,
            PickupBid.BidAmount, PickupBid.AllowIneligibleSlot, PickupBid.Priority, PickupBid.Timestamp, true, processSetID, Outcome,
            ProjectedPointsAtTimeOfBid);
    }
}
