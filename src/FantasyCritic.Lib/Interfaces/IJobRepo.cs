using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Lib.Interfaces;

public interface IJobRepo
{
    Task<IReadOnlyList<FantasyCriticJob>> GetRecentJobs(int count);
    Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs();
    Task<bool> StartJob(FantasyCriticJob job, Instant startTime);
    Task CompleteJob(FantasyCriticJob job, Instant finishTime);
    Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus);
    Task ErrorJob(FantasyCriticJob job, Exception error, Instant finishTime);
    Task<bool> CancelJob(FantasyCriticJob job, Instant cancellationTime);
    Task<bool> CancelQueuedJob(FantasyCriticJob job, string reason, Instant cancellationTime);
    Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime);
}
