using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Web.Models.Responses;

public class DropGameResultViewModel
{
    public DropGameResultViewModel(DropResult domain)
    {
        Success = domain.Result.IsSuccess;
        if (domain.Result.IsFailure)
        {
            Errors = new List<string>() { domain.Result.Error };
        }
    }

    public bool Success { get; }
    public IReadOnlyList<string>? Errors { get; }
}
