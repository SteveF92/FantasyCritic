using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class ProcessedBidSet
    {
        public ProcessedBidSet()
        {
            SuccessBids = new List<PickupBid>();
            FailedBids = new List<FailedPickupBid>();
        }

        public ProcessedBidSet(IEnumerable<PickupBid> successBids, IEnumerable<FailedPickupBid> failedBids)
        {
            SuccessBids = successBids.ToList();
            FailedBids = failedBids.ToList();
        }

        public IReadOnlyList<PickupBid> SuccessBids { get; }
        public IReadOnlyList<FailedPickupBid> FailedBids { get; }

        public IEnumerable<PickupBid> ProcessedBids => FailedBids.Select(x => x.PickupBid).Concat(SuccessBids);

        public ProcessedBidSet AppendSet(ProcessedBidSet appendSet)
        {
            return new ProcessedBidSet(SuccessBids.Concat(appendSet.SuccessBids), FailedBids.Concat(appendSet.FailedBids));
        }
    }
}
