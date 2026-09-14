using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Lib.Interfaces;

public interface IJobRepo
{
    Task<IReadOnlyList<FantasyCriticJob>> GetRecentJobs(int count);
    Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs();
    Task<IReadOnlyList<FantasyCriticJobTypeWithRunType>> GetJobTypeRunTypes();
    Task<IReadOnlyDictionary<FantasyCriticJobType, Instant>> GetLastScheduledTimes();

    /// <returns>False if a job for the same scheduled slot already exists. Manual jobs have no slot, so they always return true.</returns>
    Task<bool> CreateJob(FantasyCriticJob job);
    Task<bool> StartJob(FantasyCriticJob job, Instant startTime);
    Task CompleteJob(FantasyCriticJob job, Instant finishTime);
    Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus);
    Task ErrorJob(FantasyCriticJob job, string errorMessage, Instant finishTime);
    Task<bool> CancelJob(FantasyCriticJob job, Instant cancellationTime);
    Task<bool> CancelQueuedJob(FantasyCriticJob job, string reason, Instant cancellationTime);
    Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime);
}
