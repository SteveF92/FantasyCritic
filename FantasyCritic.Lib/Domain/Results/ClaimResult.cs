using System.Collections.Generic;
using System.Linq;

namespace FantasyCritic.Lib.Domain.Results
{
    public class ClaimResult
    {
        public ClaimResult(int bestSlotNumber)
        : this(new List<ClaimError>(), bestSlotNumber)
        {
        }

        public ClaimResult(string claimError, int? bestSlotNumber)
        : this(new ClaimError(claimError, false), bestSlotNumber)
        {

        }

        public ClaimResult(ClaimError error, int? bestSlotNumber)
        : this (new List<ClaimError>(){error}, bestSlotNumber)
        {

        }

        public ClaimResult(IEnumerable<ClaimError> errors, int? bestSlotNumber)
        {
            Success = !errors.Any() && bestSlotNumber.HasValue;
            BestSlotNumber = bestSlotNumber;
            Errors = errors.ToList();
            Overridable = errors.All(x => x.Overridable);
        }

        public bool Success { get; }
        public int? BestSlotNumber { get; }
        public IReadOnlyList<ClaimError> Errors { get; }
        public bool Overridable { get; }

        public bool NoSpaceError => Errors.Any(x => NoSpaceError);

    }
}
