using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
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
