using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class BidTimesViewModel
    {
        public BidTimesViewModel(Instant nextPublicBiddingTime, Instant nextBidLockTime, bool actionProcessingMode)
        {
            NextPublicBiddingTime = nextPublicBiddingTime;
            NextBidLockTime = nextBidLockTime;
            ActionProcessingMode = actionProcessingMode;
        }

        public Instant NextPublicBiddingTime { get; }
        public Instant NextBidLockTime { get; }
        public bool ActionProcessingMode { get; }
    }
}
