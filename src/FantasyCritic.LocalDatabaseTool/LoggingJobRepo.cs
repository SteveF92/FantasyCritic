using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using NodaTime;
using Serilog;

namespace FantasyCritic.LocalDatabaseTool;

//Lets the tool run job utilities outside the job system: a FantasyCriticJobContext only ever writes status through this,
//so the status goes to the console. Anything else means a utility is doing more than the tool expects, so it throws.
internal class LoggingJobRepo : IJobRepo
{
    public Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus)
    {
        Log.Information("{DetailedStatus}", detailedStatus);
        return Task.CompletedTask;
    }

    public Task<FantasyCriticJob?> GetJob(Guid jobID) => throw new NotSupportedException();
    public Task<IReadOnlyList<FantasyCriticJob>> GetJobs(int page, int count, FantasyCriticJobFilter filter) => throw new NotSupportedException();
    public Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs() => throw new NotSupportedException();
    public Task<IReadOnlyList<FantasyCriticJobTypeWithRunType>> GetJobTypeRunTypes() => throw new NotSupportedException();
    public Task<bool> CreateJob(FantasyCriticJob job) => throw new NotSupportedException();
    public Task<Result<FantasyCriticJob>> EnqueueJob(FantasyCriticJobType jobType, IMinimalFantasyCriticUser createdByUser, Instant createdAt) => throw new NotSupportedException();
    public Task<bool> StartJob(FantasyCriticJob job, Instant startTime) => throw new NotSupportedException();
    public Task CompleteJob(FantasyCriticJob job, Instant finishTime) => throw new NotSupportedException();
    public Task ErrorJob(FantasyCriticJob job, string errorMessage, Instant finishTime) => throw new NotSupportedException();
    public Task<bool> CancelJob(FantasyCriticJob job, Instant cancellationTime) => throw new NotSupportedException();
    public Task<bool> CancelQueuedJob(FantasyCriticJob job, string reason, Instant cancellationTime) => throw new NotSupportedException();
    public Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime) => throw new NotSupportedException();
    public Task<bool> CancelAbandonedJob(FantasyCriticJob job, string reason, Instant cancellationTime) => throw new NotSupportedException();
    public Task<bool> RequestCancellation(FantasyCriticJob job, IMinimalFantasyCriticUser cancelledByUser, Instant requestedAt) => throw new NotSupportedException();
}
