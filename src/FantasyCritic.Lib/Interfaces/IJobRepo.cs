using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Lib.Interfaces;

public interface IJobRepo
{
    Task<IReadOnlyList<FantasyCriticJob>> GetRecentJobs(int count);
    Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs();
    Task StartJob(FantasyCriticJob job, Instant startTime);
    Task CompleteJob(FantasyCriticJob job, Instant startTime);
    Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus);
    Task ErrorJob(FantasyCriticJob job, Exception error, Instant startTime);
    Task CancelJob(FantasyCriticJob job, Instant cancellationTime);
    Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime);
}
