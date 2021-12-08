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
        public PublisherSlotAcquisitionResult(PublisherSlot publisherSlot)
        {
            PublisherSlot = publisherSlot;
            ClaimErrors = new List<ClaimError>();
        }

        public PublisherSlotAcquisitionResult(IEnumerable<ClaimError> claimErrors)
        {
            PublisherSlot = Maybe<PublisherSlot>.None;
            ClaimErrors = claimErrors.ToList();
        }

        public Maybe<PublisherSlot> PublisherSlot { get; }
        public IReadOnlyList<ClaimError> ClaimErrors { get; }
    }
}
