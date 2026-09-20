using FantasyCritic.Lib.Jobs;
using NodaTime;

namespace FantasyCritic.Worker;

public record WorkerStatusSnapshot(Instant? LastPollTime, bool? WorkerShouldPullNewJobs, FantasyCriticJob? RunningJob);

/// <summary>
/// What the job runner last did, for the health check to read. A singleton: the runner writes it from its own thread
/// and the health endpoint reads it from a request thread.
/// </summary>
public class WorkerStatus
{
    private readonly Lock _lock = new();
    private WorkerStatusSnapshot _current = new(null, null, null);

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

    public void RecordPoll(Instant pollTime, bool workerShouldPullNewJobs)
    {
        lock (_lock)
        {
            _current = _current with { LastPollTime = pollTime, WorkerShouldPullNewJobs = workerShouldPullNewJobs };
        }
    }

    public void RecordJobStarted(FantasyCriticJob job)
    {
        lock (_lock)
        {
            _current = _current with { RunningJob = job };
        }
    }

    public void RecordJobFinished()
    {
        lock (_lock)
        {
            _current = _current with { RunningJob = null };
        }
    }
}
