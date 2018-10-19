using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class FailedAcquisitionBid
    {
        public FailedAcquisitionBid(AcquisitionBid acquisitionBid, string failureReason)
        {
            AcquisitionBid = acquisitionBid;
            FailureReason = failureReason;
        }

        public AcquisitionBid AcquisitionBid { get; }
        public string FailureReason { get; }
    }
}
