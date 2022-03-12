using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Web.Models.Responses;

public class QueueResultViewModel
{
    public QueueResultViewModel(ClaimResult domain)
    {
        Success = domain.Success;
        Errors = domain.Errors.Select(x => x.Error).ToList();
    }

    public bool Success { get; }
    public IReadOnlyList<string> Errors { get; }
}