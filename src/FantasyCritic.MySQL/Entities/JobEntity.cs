using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.MySQL.Entities;

internal class JobEntity
{
    public Guid JobID { get; set; }
    public string JobType { get; set; } = null!;
    public Guid? CreatedByUserID { get; set; }
    public string Status { get; set; } = null!;
    public string? DetailedStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public Instant? ScheduledFor { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? StartedAt { get; set; }
    public Instant? FinishedAt { get; set; }
    public Instant? CancelledAt { get; set; }
    public Guid? CancelledByUserID { get; set; }

    //From tbl_job_type, so it is always the type's current RunType/Severity rather than the ones in force when the job was enqueued.
    public string RunType { get; set; } = null!;
    public string Severity { get; set; } = null!;

    public string? CreatedByUserDisplayName { get; set; }
    public string? CreatedByUserEmailAddress { get; set; }
    public string? CancelledByUserDisplayName { get; set; }
    public string? CancelledByUserEmailAddress { get; set; }

    public FantasyCriticJob ToDomain()
    {
        MinimalFantasyCriticUser? createdByUser = null;
        if (CreatedByUserID.HasValue)
        {
            createdByUser = new MinimalFantasyCriticUser(CreatedByUserID.Value, CreatedByUserDisplayName!, CreatedByUserEmailAddress!);
        }

        MinimalFantasyCriticUser? cancelledByUser = null;
        if (CancelledByUserID.HasValue)
        {
            cancelledByUser = new MinimalFantasyCriticUser(CancelledByUserID.Value, CancelledByUserDisplayName!, CancelledByUserEmailAddress!);
        }

        var jobTypeWithRunType = new FantasyCriticJobTypeWithRunType(FantasyCriticJobType.FromValue(JobType),
            FantasyCriticJobRunType.FromValue(RunType), FantasyCriticJobSeverity.FromValue(Severity));

        return new FantasyCriticJob(JobID, jobTypeWithRunType, createdByUser, FantasyCriticJobStatus.FromValue(Status),
            DetailedStatus, ErrorMessage, ScheduledFor, CreatedAt, StartedAt, FinishedAt, CancelledAt, cancelledByUser);
    }
}
