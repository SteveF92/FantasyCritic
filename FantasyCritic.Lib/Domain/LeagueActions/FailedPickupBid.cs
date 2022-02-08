using NodaTime;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class FailedPickupBid : IProcessedBid
    {
        public FailedPickupBid(PickupBid pickupBid, string failureReason, SystemWideValues systemWideValues, LocalDate currentDate)
        {
            PickupBid = pickupBid;
            FailureReason = failureReason;
            ProjectedPointsAtTimeOfBid = PickupBid.Publisher.GetProjectedFantasyPoints(systemWideValues, false, currentDate, false);
        }

        public PickupBid PickupBid { get; }
        public string FailureReason { get; }
        public decimal ProjectedPointsAtTimeOfBid { get; }

        public string Outcome => FailureReason;
    }
}
