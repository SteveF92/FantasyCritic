using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Domain
{
    public class GameActionSet
    {
        public GameActionSet(IEnumerable<PickupBid> bids, IEnumerable<DropRequest> drops)
        {
            Bids = bids;
            Drops = drops;
        }

        public IEnumerable<PickupBid> Bids { get; }
        public IEnumerable<DropRequest> Drops { get; }

        public bool Any() => Bids.Any() || Drops.Any();
    }
}
