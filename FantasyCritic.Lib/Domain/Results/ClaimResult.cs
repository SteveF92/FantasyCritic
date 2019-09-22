using System.Collections.Generic;
using System.Linq;

namespace FantasyCritic.Lib.Domain.Results
{
    public class ClaimResult
    {
        public ClaimResult()
        : this(new List<ClaimError>())
        {

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
        public IReadOnlyList<ClaimError> Errors { get; }
        public bool Overridable { get; }
    }
}
