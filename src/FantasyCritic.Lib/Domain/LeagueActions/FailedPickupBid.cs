namespace FantasyCritic.Lib.Domain.LeagueActions;

public class FailedPickupBid : IProcessedBid
{
    public FailedPickupBid(PickupBid pickupBid, string failureReason, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        PickupBid = pickupBid;
        FailureReason = failureReason;
        ProjectedPointsAtTimeOfBid = PickupBid.Publisher.GetProjectedFantasyPoints(pickupBid.LeagueYear, systemWideValues, currentDate);
    }

    public PickupBid PickupBid { get; }
    public string FailureReason { get; }
    public decimal ProjectedPointsAtTimeOfBid { get; }

    public string Outcome => FailureReason;

    public override string ToString() => PickupBid.ToString() + "|" + Outcome;

    public PickupBid ToFlatBid(Guid processSetID)
    {
        return new PickupBid(PickupBid.BidID, PickupBid.Publisher, PickupBid.LeagueYear, PickupBid.MasterGame,
            PickupBid.ConditionalDropPublisherGame, PickupBid.CounterPick,
            PickupBid.BidAmount, PickupBid.AllowIneligibleSlot, PickupBid.Priority, PickupBid.Timestamp, false, processSetID, FailureReason,
            ProjectedPointsAtTimeOfBid);
    }
}
