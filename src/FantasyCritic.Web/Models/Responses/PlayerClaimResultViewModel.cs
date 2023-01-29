using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Web.Models.Responses;

public class PlayerClaimResultViewModel
{
    public PlayerClaimResultViewModel(ClaimResult domain)
    {
        Success = domain.Success;
        Errors = domain.Errors.Select(x => x.Error).ToList();
        Overridable = domain.Overridable;
        ShowAsWarning = domain.ShowAsWarning;
        NoEligibleSpaceError = domain.NoEligibleSpaceError;
    }

    public bool Success { get; }
    public IReadOnlyList<string> Errors { get; }
    public bool Overridable { get; }
    public bool ShowAsWarning { get; }
    public bool NoEligibleSpaceError { get; }
}
