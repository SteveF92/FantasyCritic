using System;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Lib.SharedSerialization.API;
using FantasyCritic.Worker;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class WorkerHealthCheckTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 9, 19, 12, 0, 0);
    private static readonly WorkerLoopStatus SucceededRecently = WorkerLoopStatus.NotStarted.Succeeded(Now - Duration.FromSeconds(5));

    [Test]
    public void NoLoopHasRun_IsUnhealthy()
    {
        var status = new WorkerStatusSnapshot(WorkerLoopStatus.NotStarted, WorkerLoopStatus.NotStarted, WorkerLoopStatus.NotStarted, null, null);

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }

    [Test]
    public void EveryLoopSucceededRecently_IsHealthy()
    {
        var result = WorkerHealthCheck.Evaluate(AllSucceeded(), Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void SchedulerHasNotRunYet_IsUnhealthy()
    {
        var status = AllSucceeded() with { Scheduler = WorkerLoopStatus.NotStarted };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(result.Description, Does.Contain("scheduler has not completed a pass"));
        });
    }

    [Test]
    public void JobRunnerPolledExactlyAtTheLimit_IsHealthy()
    {
        var status = AllSucceeded() with { JobRunner = WorkerLoopStatus.NotStarted.Succeeded(Now - WorkerHealthCheck.MaximumPollAge) };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void JobRunnerPollIsStale_IsUnhealthy()
    {
        var status = AllSucceeded() with { JobRunner = WorkerLoopStatus.NotStarted.Succeeded(Now - WorkerHealthCheck.MaximumPollAge - Duration.FromSeconds(1)) };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }

    [Test]
    public void JobRunnerPollIsStaleButAJobIsRunning_IsHealthy()
    {
        //The runner does not poll while it is inside a job, so a long job must not read as a dead worker.
        var status = AllSucceeded() with { JobRunner = WorkerLoopStatus.NotStarted.Succeeded(Now - Duration.FromHours(2)), RunningJob = CreateJob() };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void CancellerPassIsStale_IsUnhealthy()
    {
        var status = AllSucceeded() with { Canceller = WorkerLoopStatus.NotStarted.Succeeded(Now - WorkerHealthCheck.MaximumPollAge - Duration.FromSeconds(1)) };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }

    [Test]
    public void SchedulerLastSucceededLongAgo_IsHealthy()
    {
        //It sleeps until its next slot, so a long gap since its last pass is normal.
        var status = AllSucceeded() with { Scheduler = WorkerLoopStatus.NotStarted.Succeeded(Now - Duration.FromMinutes(10)) };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void SchedulerFailing_IsUnhealthyAndSaysWhy()
    {
        var status = AllSucceeded() with { Scheduler = SucceededRecently.Failed(Now, "Job Type ExpireTrades not found in database.") };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(result.Description, Is.EqualTo("The scheduler is failing: Job Type ExpireTrades not found in database."));
            Assert.That(result.Data["schedulerFailingSince"], Is.EqualTo(ServiceHealthDetail.FromTime("Scheduler failing since", Now)));
        });
    }

    [Test]
    public void CancellerFailingWhileAJobIsRunning_IsUnhealthy()
    {
        //A running job excuses the runner's poll age, nothing else.
        var status = AllSucceeded() with { Canceller = SucceededRecently.Failed(Now, "Unable to connect."), RunningJob = CreateJob() };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }

    [Test]
    public void SeveralLoopsFailing_NamesEach()
    {
        var status = AllSucceeded() with
        {
            JobRunner = SucceededRecently.Failed(Now, "Unable to connect."),
            Scheduler = SucceededRecently.Failed(Now, "Unable to connect.")
        };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.That(result.Description, Is.EqualTo("The job runner is failing: Unable to connect. The scheduler is failing: Unable to connect."));
    }

    [Test]
    public void RepeatedFailures_KeepTheFirstFailureTime()
    {
        var firstFailure = Now - Duration.FromMinutes(3);

        var loop = SucceededRecently.Failed(firstFailure, "First error.").Failed(Now, "Latest error.");

        Assert.That(loop.Failure, Is.EqualTo(new WorkerLoopFailure(firstFailure, "Latest error.")));
    }

    [Test]
    public void SuccessAfterFailure_ClearsTheFailure()
    {
        var loop = SucceededRecently.Failed(Now - Duration.FromMinutes(3), "Unable to connect.").Succeeded(Now);

        Assert.That(loop, Is.EqualTo(new WorkerLoopStatus(Now, null)));
    }

    [Test]
    public void NotPullingNewJobs_IsHealthyAndSaysSo()
    {
        var status = AllSucceeded() with { WorkerShouldPullNewJobs = false };

        var result = WorkerHealthCheck.Evaluate(status, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(result.Description, Does.Contain("WorkerShouldPullNewJobs"));
            Assert.That(result.Data["workerShouldPullNewJobs"], Is.EqualTo(ServiceHealthDetail.FromText("Pulling new jobs", "No")));
        });
    }

    [Test]
    public void LastSucceeded_IsSentAsATime()
    {
        var polled = WorkerHealthCheck.Evaluate(AllSucceeded(), Now);
        var neverPolled = WorkerHealthCheck.Evaluate(AllSucceeded() with { JobRunner = WorkerLoopStatus.NotStarted }, Now);

        Assert.Multiple(() =>
        {
            Assert.That(polled.Data["jobRunnerLastSucceededAt"], Is.EqualTo(ServiceHealthDetail.FromTime("Job runner last succeeded", SucceededRecently.LastSucceededAt!.Value)));
            Assert.That(neverPolled.Data["jobRunnerLastSucceededAt"], Is.EqualTo(ServiceHealthDetail.FromText("Job runner last succeeded", "Never")));
        });
    }

    private static WorkerStatusSnapshot AllSucceeded()
    {
        return new WorkerStatusSnapshot(SucceededRecently, SucceededRecently, SucceededRecently, true, null);
    }

    private static FantasyCriticJob CreateJob()
    {
        return new FantasyCriticJob(Guid.NewGuid(), new FantasyCriticJobTypeWithRunType(FantasyCriticJobType.ExpireTrades, FantasyCriticJobRunType.Cron, FantasyCriticJobSeverity.Info), null, FantasyCriticJobStatus.Running,
            null, null, Now, Now, Now, null, null, null);
    }
}
