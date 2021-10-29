namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class FailedPickupBid
    {
        public FailedPickupBid(PickupBid pickupBid, string failureReason)
        {
            PickupBid = pickupBid;
            FailureReason = failureReason;
        }

        public PickupBid PickupBid { get; }
        public string FailureReason { get; }
    }
}
