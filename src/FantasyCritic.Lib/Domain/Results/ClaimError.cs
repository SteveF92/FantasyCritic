namespace FantasyCritic.Lib.Domain.Results;

public class ClaimError
{
    public ClaimError(string error, bool overridable, bool noSpaceError = false, bool noEligibleSpaceError = false)
    {
        Error = error;
        Overridable = overridable;
        NoSpaceError = noSpaceError;
        NoEligibleSpaceError = noEligibleSpaceError;
    }

    public string Error { get; }
    public bool Overridable { get; }
    public bool NoSpaceError { get; }
    public bool NoEligibleSpaceError { get; }
}
