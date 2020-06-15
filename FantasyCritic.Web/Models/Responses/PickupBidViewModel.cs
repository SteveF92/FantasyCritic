using System;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PickupBidViewModel
    {
        public PickupBidViewModel(PickupBid pickupBid, IClock clock)
        {
            BidID = pickupBid.BidID;
            BidAmount = pickupBid.BidAmount;
            Priority = pickupBid.Priority;
            Timestamp = pickupBid.Timestamp.ToDateTimeUtc();
            Successful = pickupBid.Successful;
            MasterGame = new MasterGameViewModel(pickupBid.MasterGame, clock);
        }

        public Guid BidID { get; }
        public uint BidAmount { get; }
        public int Priority { get; }
        public DateTime Timestamp { get; }
        public bool? Successful { get; }
        public MasterGameViewModel MasterGame { get; }
    }
}
