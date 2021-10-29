using System.Collections.Generic;
using System.Linq;

namespace FantasyCritic.Lib.Domain.LeagueActions
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
