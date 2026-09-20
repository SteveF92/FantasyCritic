using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;

namespace FantasyCritic.Worker;

/// <summary>
/// The runner loop catches a failed poll and carries on, so the process staying up proves nothing. What does is how
/// recently it last read the database. While a job runs the loop is inside that job and does not poll at all, so a
/// running job counts as healthy on its own.
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
            ["workerShouldPullNewJobs"] = status.WorkerShouldPullNewJobs?.ToString() ?? "Unknown",
            ["lastPollTime"] = status.LastPollTime?.ToString() ?? "Never",
            ["runningJob"] = status.RunningJob is null ? "None" : $"{status.RunningJob.Type} {status.RunningJob.JobID}"
        };

        if (status.RunningJob is not null)
        {
            return HealthCheckResult.Healthy($"Running {status.RunningJob.Type}.", data);
        }

        if (status.LastPollTime is null)
        {
            return HealthCheckResult.Unhealthy("The job runner has not polled the database yet.", data: data);
        }

        var pollAge = now - status.LastPollTime.Value;
        if (pollAge > MaximumPollAge)
        {
            return HealthCheckResult.Unhealthy($"The job runner last polled the database {(int)pollAge.TotalSeconds} seconds ago.", data: data);
        }

        if (status.WorkerShouldPullNewJobs == false)
        {
            return HealthCheckResult.Healthy("Idle. Not pulling new jobs because WorkerShouldPullNewJobs is off.", data);
        }

        return HealthCheckResult.Healthy("Waiting for jobs.", data);
    }
}
