using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Lib.Interfaces;

public interface IJobRepo
{
    Task<FantasyCriticJob?> GetJob(Guid jobID);
    Task<IReadOnlyList<FantasyCriticJob>> GetJobs(int page, int count, FantasyCriticJobType? jobType);
    Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs();
    Task<IReadOnlyList<FantasyCriticJobTypeWithRunType>> GetJobTypeRunTypes();

    /// <returns>False if a job for the same scheduled slot already exists. Manual jobs have no slot, so they always return true.</returns>
    Task<bool> CreateJob(FantasyCriticJob job);

    /// <summary>Queues a manual run. The single place manual runs check <see cref="FantasyCriticJobRunType.AllowsManual"/>.</summary>
    /// <returns>Failure if the job type's RunType does not allow manual runs.</returns>
    Task<Result<FantasyCriticJob>> EnqueueJob(FantasyCriticJobType jobType, IMinimalFantasyCriticUser createdByUser, Instant createdAt);
    Task<bool> StartJob(FantasyCriticJob job, Instant startTime);
    Task CompleteJob(FantasyCriticJob job, Instant finishTime);
    Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus);
    Task ErrorJob(FantasyCriticJob job, string errorMessage, Instant finishTime);
    Task<bool> CancelJob(FantasyCriticJob job, Instant cancellationTime);
    Task<bool> CancelQueuedJob(FantasyCriticJob job, string reason, Instant cancellationTime);
    Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime);

    /// <returns>False if the job was no longer Queued or Running (already resolved, or another request beat this one).</returns>
    Task<bool> RequestCancellation(FantasyCriticJob job, IMinimalFantasyCriticUser cancelledByUser, Instant requestedAt);
}
