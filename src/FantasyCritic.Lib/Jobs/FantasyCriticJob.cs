using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJob
{
    public FantasyCriticJob(Guid jobID, FantasyCriticJobType type, FantasyCriticJobRunType runType, IMinimalFantasyCriticUser? createdByUser, FantasyCriticJobStatus status,
        string? detailedStatus, string? errorMessage, Instant? scheduledFor, Instant createdAt, Instant? startedAt, Instant? finishedAt)
    {
        JobID = jobID;
        Type = type;
        RunType = runType;
        CreatedByUser = createdByUser;
        Status = status;
        DetailedStatus = detailedStatus;
        ErrorMessage = errorMessage;
        ScheduledFor = scheduledFor;
        CreatedAt = createdAt;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
    }

    public Guid JobID { get; }
    public FantasyCriticJobType Type { get; }
    public FantasyCriticJobRunType RunType { get; }
    public IMinimalFantasyCriticUser? CreatedByUser { get; }
    public FantasyCriticJobStatus Status { get; }
    public string? DetailedStatus { get; }
    public string? ErrorMessage { get; }
    public Instant? ScheduledFor { get; }
    public Instant CreatedAt { get; }
    public Instant? StartedAt { get; }
    public Instant? FinishedAt { get; }

    //The scheduler always sets ScheduledFor and manual runs never do, so it identifies cron runs regardless of who enqueued them.
    public bool IsCronRun => ScheduledFor is not null;

    public bool AllowedByRunType => IsCronRun ? RunType.AllowsCron : RunType.AllowsManual;

    public override string ToString()
    {
        return $"{JobID} - {Type.Value} - {Status} - {CreatedAt}";
    }
}
