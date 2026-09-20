using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Jobs;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class WorkerStateTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 9, 19, 12, 0, 0);

    [Test]
    public void Unreachable_WinsOverEverything()
    {
        var state = WorkerState.Determine(workerReachable: false, workerHealthy: true, workerShouldPullNewJobs: true, NoJobs());

        Assert.That(state, Is.EqualTo(WorkerState.Unreachable));
    }

    [Test]
    public void ReachableButUnhealthy_IsUnhealthy()
    {
        var state = WorkerState.Determine(workerReachable: true, workerHealthy: false, workerShouldPullNewJobs: true, NoJobs());

        Assert.That(state, Is.EqualTo(WorkerState.Unhealthy));
    }

    [Test]
    public void HealthyAndPulling_IsRunning()
    {
        var state = WorkerState.Determine(workerReachable: true, workerHealthy: true, workerShouldPullNewJobs: true, [CreateJob(FantasyCriticJobStatus.Running)]);

        Assert.That(state, Is.EqualTo(WorkerState.Running));
    }

    [TestCase("Running")]
    [TestCase("Cancelling")]
    public void NotPullingWithAJobStillGoing_IsDraining(string jobStatus)
    {
        var state = WorkerState.Determine(workerReachable: true, workerHealthy: true, workerShouldPullNewJobs: false, [CreateJob(FantasyCriticJobStatus.FromValue(jobStatus))]);

        Assert.That(state, Is.EqualTo(WorkerState.Draining));
    }

    [Test]
    public void NotPullingWithOnlyQueuedJobs_IsOff()
    {
        //Queued jobs are the backlog the scheduler keeps building. They are waiting, not running.
        var state = WorkerState.Determine(workerReachable: true, workerHealthy: true, workerShouldPullNewJobs: false, [CreateJob(FantasyCriticJobStatus.Queued)]);

        Assert.That(state, Is.EqualTo(WorkerState.Off));
    }

    [Test]
    public void NotPullingWithNoJobs_IsOff()
    {
        var state = WorkerState.Determine(workerReachable: true, workerHealthy: true, workerShouldPullNewJobs: false, NoJobs());

        Assert.That(state, Is.EqualTo(WorkerState.Off));
    }

    private static IReadOnlyList<FantasyCriticJob> NoJobs() => [];

    private static FantasyCriticJob CreateJob(FantasyCriticJobStatus status)
    {
        return new FantasyCriticJob(Guid.NewGuid(), new FantasyCriticJobTypeWithRunType(FantasyCriticJobType.ExpireTrades, FantasyCriticJobRunType.Cron, FantasyCriticJobSeverity.Info), null, status,
            null, null, Now, Now, null, null, null, null);
    }
}
