using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Lib.Interfaces;

public interface IJobRepo
{
    Task<FantasyCriticJob?> GetJob(Guid jobID);
    /// <summary>Newest first.</summary>
    Task<IReadOnlyList<FantasyCriticJob>> GetJobs(int page, int count, FantasyCriticJobFilter filter);
    Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs();
    Task<IReadOnlyList<FantasyCriticJobTypeWithRunType>> GetJobTypeRunTypes();

    /// <returns>False if a job for the same scheduled slot already exists. Manual jobs have no slot, so they always return true.</returns>
    Task<bool> CreateJob(FantasyCriticJob job);

    /// <summary>Queues a manual run. The single place manual runs check <see cref="FantasyCriticJobRunType.AllowsManual"/>.</summary>
    /// <returns>Failure if the job type's RunType does not allow manual runs, or a job of the type is already queued, running or cancelling.</returns>
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

public class NoOpJobRepo : IJobRepo
{
    public Task<FantasyCriticJob?> GetJob(Guid jobID)
    {
        return Task.FromResult<FantasyCriticJob?>(null);
    }

    public Task<IReadOnlyList<FantasyCriticJob>> GetJobs(int page, int count, FantasyCriticJobFilter filter)
    {
        return Task.FromResult<IReadOnlyList<FantasyCriticJob>>([]);
    }

    public Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs()
    {
        return Task.FromResult<IReadOnlyList<FantasyCriticJob>>([]);
    }

    public Task<IReadOnlyList<FantasyCriticJobTypeWithRunType>> GetJobTypeRunTypes()
    {
        return Task.FromResult<IReadOnlyList<FantasyCriticJobTypeWithRunType>>([]);
    }

    public Task<bool> CreateJob(FantasyCriticJob job)
    {
        return Task.FromResult(true);
    }

    public Task<Result<FantasyCriticJob>> EnqueueJob(FantasyCriticJobType jobType, IMinimalFantasyCriticUser createdByUser, Instant createdAt)
    {
        return Task.FromResult(Result.Success(FantasyCriticJob.FakeJob));
    }

    public Task<bool> StartJob(FantasyCriticJob job, Instant startTime)
    {
        return Task.FromResult(true);
    }

    public Task CompleteJob(FantasyCriticJob job, Instant finishTime)
    {
        return Task.CompletedTask;
    }

    public Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus)
    {
        return Task.CompletedTask;
    }

    public Task ErrorJob(FantasyCriticJob job, string errorMessage, Instant finishTime)
    {
        return Task.CompletedTask;
    }

    public Task<bool> CancelJob(FantasyCriticJob job, Instant cancellationTime)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CancelQueuedJob(FantasyCriticJob job, string reason, Instant cancellationTime)
    {
        return Task.FromResult(true);
    }

    public Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime)
    {
        return Task.CompletedTask;
    }

    public Task<bool> RequestCancellation(FantasyCriticJob job, IMinimalFantasyCriticUser cancelledByUser, Instant requestedAt)
    {
        return Task.FromResult(true);
    }
}
