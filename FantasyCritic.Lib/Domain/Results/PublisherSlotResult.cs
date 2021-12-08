using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain.Results
{
    public class PublisherSlotAcquisitionResult
    {
        public PublisherSlotAcquisitionResult(int slotNumber)
        {
            SlotNumber = slotNumber;
            ClaimErrors = new List<ClaimError>();
        }

        public PublisherSlotAcquisitionResult(IEnumerable<ClaimError> claimErrors)
        {
            SlotNumber = null;
            ClaimErrors = claimErrors.ToList();
        }

        public int? SlotNumber { get; }
        public IReadOnlyList<ClaimError> ClaimErrors { get; }
    }
}
