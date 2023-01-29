namespace FantasyCritic.Lib.Domain.Results;

public class ClaimResult
{
    public ClaimResult(int bestSlotNumber)
        : this(new List<ClaimError>(), bestSlotNumber)
    {
    }

    public ClaimResult(string claimError)
        : this(new ClaimError(claimError, false))
    {

    }

    public ClaimResult(ClaimError error)
        : this(new List<ClaimError>() { error }, null)
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

    public bool NoSpaceError => Errors.Any(x => x.NoSpaceError);
    public bool NoEligibleSpaceError => Errors.Any(x => x.NoEligibleSpaceError);
    public bool ShowAsWarning => Overridable || NoEligibleSpaceError;
}
