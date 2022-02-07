using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class SucceededPickupBid
    {
        public SucceededPickupBid(PickupBid pickupBid, int slotNumber, string outcome)
        {
            PickupBid = pickupBid;
            SlotNumber = slotNumber;
            Outcome = outcome;
        }

        public PickupBid PickupBid { get; }
        public int SlotNumber { get; }
        public string Outcome { get; }
    }
}
