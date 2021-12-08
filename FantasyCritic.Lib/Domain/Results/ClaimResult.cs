using System.Collections.Generic;
using System.Linq;

namespace FantasyCritic.Lib.Domain.Results
{
    public class ClaimResult
    {
        public ClaimResult(int idealSlotNumber)
        : this(new List<ClaimError>())
        {
            IdealSlotNumber = idealSlotNumber;
        }

        public ClaimResult(string claimError)
        : this(new ClaimError(claimError, false))
        {

        }

        public ClaimResult(ClaimError error)
        : this (new List<ClaimError>(){error})
        {

        }

        public ClaimResult(IEnumerable<ClaimError> errors)
        {
            Success = !errors.Any();
            Errors = errors.ToList();
            Overridable = errors.All(x => x.Overridable);
        }

        public bool Success { get; }
        public int? IdealSlotNumber { get; }
        public IReadOnlyList<ClaimError> Errors { get; }
        public bool Overridable { get; }
    }
}
