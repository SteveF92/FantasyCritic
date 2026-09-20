using System;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Worker;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class WorkerHealthCheckTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 9, 19, 12, 0, 0);

    [Test]
    public void NeverPolled_IsUnhealthy()
    {
        var result = WorkerHealthCheck.Evaluate(new WorkerStatusSnapshot(null, null, null), Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }

    [Test]
    public void PolledRecently_IsHealthy()
    {
        var status = new WorkerStatusSnapshot(Now - Duration.FromSeconds(5), true, null);

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void PolledExactlyAtTheLimit_IsHealthy()
    {
        var status = new WorkerStatusSnapshot(Now - WorkerHealthCheck.MaximumPollAge, true, null);

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void PollIsStale_IsUnhealthy()
    {
        var status = new WorkerStatusSnapshot(Now - WorkerHealthCheck.MaximumPollAge - Duration.FromSeconds(1), true, null);

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }

    [Test]
    public void PollIsStaleButAJobIsRunning_IsHealthy()
    {
        //The runner does not poll while it is inside a job, so a long job must not read as a dead worker.
        var status = new WorkerStatusSnapshot(Now - Duration.FromHours(2), true, CreateJob());

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void NotPullingNewJobs_IsHealthyAndSaysSo()
    {
        var status = new WorkerStatusSnapshot(Now - Duration.FromSeconds(5), false, null);

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(result.Description, Does.Contain("WorkerShouldPullNewJobs"));
            Assert.That(result.Data["workerShouldPullNewJobs"], Is.EqualTo("False"));
        });
    }

    private static FantasyCriticJob CreateJob()
    {
        return new FantasyCriticJob(Guid.NewGuid(), new FantasyCriticJobTypeWithRunType(FantasyCriticJobType.ExpireTrades, FantasyCriticJobRunType.Cron, FantasyCriticJobSeverity.Info), null, FantasyCriticJobStatus.Running,
            null, null, Now, Now, Now, null, null, null);
    }
}
