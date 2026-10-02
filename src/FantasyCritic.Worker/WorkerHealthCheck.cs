using FantasyCritic.Lib.SharedSerialization.API;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;

namespace FantasyCritic.Worker;

/// <summary>
/// Each of the worker's three loops catches a failure and tries again, so the process staying up proves nothing. The worker
/// is healthy only while every loop's most recent attempt succeeded. A failure that a retry fixes clears by itself; one it
/// does not leaves the worker unhealthy, saying which loop and why, until someone looks.
/// </summary>
public sealed class WorkerHealthCheck : IHealthCheck
{
    public static readonly Duration MaximumPollAge = Duration.FromSeconds(60);

    private readonly WorkerStatus _workerStatus;
    private readonly IClock _clock;

    public WorkerHealthCheck(WorkerStatus workerStatus, IClock clock)
    {
        _workerStatus = workerStatus;
        _clock = clock;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Evaluate(_workerStatus.Current, _clock.GetCurrentInstant()));
    }

    public static HealthCheckResult Evaluate(WorkerStatusSnapshot status, Instant now)
    {
        var data = new Dictionary<string, object>
        {
            ["workerShouldPullNewJobs"] = ServiceHealthDetail.FromText("Pulling new jobs", status.WorkerShouldPullNewJobs switch
            {
                true => "Yes",
                false => "No",
                null => "Unknown"
            }),
            ["runningJob"] = ServiceHealthDetail.FromText("Running job", status.RunningJob is null ? "None" : $"{status.RunningJob.Type} ({status.RunningJob.JobID})")
        };
        AddLoopDetails(data, "jobRunner", "Job runner", status.JobRunner);
        AddLoopDetails(data, "scheduler", "Scheduler", status.Scheduler);
        AddLoopDetails(data, "canceller", "Canceller", status.Canceller);

        //The age limits catch a loop that hangs rather than fails. The runner does not poll while it is inside a job, so its
        //limit only applies while it is idle. The scheduler sleeps until its next slot, which can be ten minutes away, so it
        //has none: a failed attempt is the only thing that marks it.
        var problems = new List<string?>
        {
            FindProblem("job runner", status.JobRunner, now, status.RunningJob is null ? MaximumPollAge : null),
            FindProblem("scheduler", status.Scheduler, now, maximumAge: null),
            FindProblem("canceller", status.Canceller, now, MaximumPollAge)
        }.OfType<string>().ToList();

        if (problems.Count > 0)
        {
            return HealthCheckResult.Unhealthy(string.Join(" ", problems), data: data);
        }

        if (status.RunningJob is not null)
        {
            return HealthCheckResult.Healthy($"Running {status.RunningJob.Type}.", data);
        }

        if (status.WorkerShouldPullNewJobs == false)
        {
            return HealthCheckResult.Healthy("Idle. Not pulling new jobs because WorkerShouldPullNewJobs is off.", data);
        }

        return HealthCheckResult.Healthy("Waiting for jobs.", data);
    }

    private static string? FindProblem(string loopName, WorkerLoopStatus loop, Instant now, Duration? maximumAge)
    {
        if (loop.Failure is not null)
        {
            return $"The {loopName} is failing: {loop.Failure.Error}";
        }

        if (loop.LastSucceededAt is null)
        {
            return $"The {loopName} has not completed a pass yet.";
        }

        var age = now - loop.LastSucceededAt.Value;
        if (maximumAge.HasValue && age > maximumAge.Value)
        {
            return $"The {loopName} last completed a pass {(int)age.TotalSeconds} seconds ago.";
        }

        return null;
    }

    private static void AddLoopDetails(Dictionary<string, object> data, string key, string label, WorkerLoopStatus loop)
    {
        data[$"{key}LastSucceededAt"] = loop.LastSucceededAt is null
            ? ServiceHealthDetail.FromText($"{label} last succeeded", "Never")
            : ServiceHealthDetail.FromTime($"{label} last succeeded", loop.LastSucceededAt.Value);

        if (loop.Failure is not null)
        {
            data[$"{key}FailingSince"] = ServiceHealthDetail.FromTime($"{label} failing since", loop.Failure.FailingSince);
            data[$"{key}Error"] = ServiceHealthDetail.FromText($"{label} error", loop.Failure.Error);
        }
    }
}
