using FantasyCritic.Lib.Jobs;
using NodaTime;

namespace FantasyCritic.Worker;

public record WorkerLoopFailure(Instant FailingSince, string Error);

/// <summary>
/// How one of the worker's three loops last fared. Each loop catches its own failures and tries again, so this is what
/// tells the health check that one keeps failing.
/// </summary>
public record WorkerLoopStatus(Instant? LastSucceededAt, WorkerLoopFailure? Failure)
{
    public static readonly WorkerLoopStatus NotStarted = new(null, null);

    public WorkerLoopStatus Succeeded(Instant time) => new(time, null);

    //Keeps the time of the first failure in a run of them, so the health report can say how long it has been failing.
    public WorkerLoopStatus Failed(Instant time, string error) => this with { Failure = new WorkerLoopFailure(Failure?.FailingSince ?? time, error) };
}

public record WorkerStatusSnapshot(WorkerLoopStatus JobRunner, WorkerLoopStatus Scheduler, WorkerLoopStatus Canceller, bool? WorkerShouldPullNewJobs,
    FantasyCriticJob? RunningJob);

/// <summary>
/// What the worker's loops last did, for the health check to read. A singleton: the loops write it from their own threads
/// and the health endpoint reads it from a request thread.
/// </summary>
public class WorkerStatus
{
    private readonly IClock _clock;
    private readonly Lock _lock = new();
    private WorkerStatusSnapshot _current = new(WorkerLoopStatus.NotStarted, WorkerLoopStatus.NotStarted, WorkerLoopStatus.NotStarted, null, null);

    public WorkerStatus(IClock clock)
    {
        _clock = clock;
    }

    public WorkerStatusSnapshot Current
    {
        get
        {
            lock (_lock)
            {
                return _current;
            }
        }
    }

    public void RecordWorkerShouldPullNewJobs(bool workerShouldPullNewJobs) => Update(x => x with { WorkerShouldPullNewJobs = workerShouldPullNewJobs });

    public void RecordJobRunnerSuccess() => Update(x => x with { JobRunner = x.JobRunner.Succeeded(_clock.GetCurrentInstant()) });
    public void RecordJobRunnerFailure(string error) => Update(x => x with { JobRunner = x.JobRunner.Failed(_clock.GetCurrentInstant(), error) });

    public void RecordSchedulerSuccess() => Update(x => x with { Scheduler = x.Scheduler.Succeeded(_clock.GetCurrentInstant()) });
    public void RecordSchedulerFailure(string error) => Update(x => x with { Scheduler = x.Scheduler.Failed(_clock.GetCurrentInstant(), error) });

    public void RecordCancellerSuccess() => Update(x => x with { Canceller = x.Canceller.Succeeded(_clock.GetCurrentInstant()) });
    public void RecordCancellerFailure(string error) => Update(x => x with { Canceller = x.Canceller.Failed(_clock.GetCurrentInstant(), error) });

    public void RecordJobStarted(FantasyCriticJob job) => Update(x => x with { RunningJob = job });
    public void RecordJobFinished() => Update(x => x with { RunningJob = null });

    private void Update(Func<WorkerStatusSnapshot, WorkerStatusSnapshot> update)
    {
        lock (_lock)
        {
            _current = update(_current);
        }
    }
}
