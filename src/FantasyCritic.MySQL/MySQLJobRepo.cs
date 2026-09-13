using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.MySQL;

public class MySQLJobRepo : IJobRepo
{
    private readonly string _connectionString;

    public MySQLJobRepo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<FantasyCriticJob>> GetRecentJobs(int count)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> StartJob(FantasyCriticJob job, Instant startTime)
    {
        throw new NotImplementedException();
    }

    public async Task CompleteJob(FantasyCriticJob job, Instant startTime)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus)
    {
        throw new NotImplementedException();
    }

    public async Task ErrorJob(FantasyCriticJob job, Exception error, Instant startTime)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CancelJob(FantasyCriticJob job, Instant cancellationTime)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CancelQueuedJob(FantasyCriticJob job, string reason, Instant cancellationTime)
    {
        //Conditional on Status = 'Queued', so it can't race the runner's claim. Reason goes in DetailedStatus.
        throw new NotImplementedException();
    }

    public async Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime)
    {
        throw new NotImplementedException();
    }
}
