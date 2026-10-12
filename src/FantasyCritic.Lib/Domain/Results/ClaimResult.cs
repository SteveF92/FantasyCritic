namespace FantasyCritic.Lib.Domain.Results;

public class ClaimResult
{
    public ClaimResult(LeagueSlot bestSlot)
        : this(new List<ClaimError>(), bestSlot)
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

    public ClaimResult(IEnumerable<ClaimError> errors, LeagueSlot? bestSlot)
    {
        Success = !errors.Any() && bestSlot is not null;
        BestSlot = bestSlot;
        Errors = errors.ToList();
        Overridable = errors.All(x => x.Overridable);
    }

    public bool Success { get; }
    public LeagueSlot? BestSlot { get; }
    public IReadOnlyList<ClaimError> Errors { get; }
    public bool Overridable { get; }

    public bool NoSpaceError => Errors.Any(x => x.NoSpaceError);
    public bool NoEligibleSpaceError => Errors.Any(x => x.NoEligibleSpaceError);
    public bool ShowAsWarning => Overridable || NoEligibleSpaceError;
}
