using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Web.Models.Responses;

public class FantasyCriticJobViewModel
{
    public FantasyCriticJobViewModel(FantasyCriticJob domain)
    {

    }

    public Guid JobID { get; }
    public string Type { get; }
    public string RunType { get; }
    public string Severity { get; }
    public string? CreatedByUserDisplayName { get; }
    public string Status { get; }
    public string? DetailedStatus { get; }
    public string? ErrorMessage { get; }
    public Instant? ScheduledFor { get; }
    public Instant CreatedAt { get; }
    public Instant? StartedAt { get; }
    public Instant? FinishedAt { get; }
}
