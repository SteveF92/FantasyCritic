using System;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class AcquisitionBidViewModel
    {
        public AcquisitionBidViewModel(AcquisitionBid acquisitionBid)
        {
            BidID = acquisitionBid.BidID;
            BidAmount = acquisitionBid.BidAmount;
            Priority = acquisitionBid.Priority;
            Timestamp = acquisitionBid.Timestamp.ToDateTimeUtc();
            Successful = acquisitionBid.Successful;
            MasterGame = new MasterGameViewModel(acquisitionBid.MasterGame);
        }

        public Guid BidID { get; }
        public uint BidAmount { get; }
        public int Priority { get; }
        public DateTime Timestamp { get; }
        public bool? Successful { get; }
        public MasterGameViewModel MasterGame { get; }
    }
}
