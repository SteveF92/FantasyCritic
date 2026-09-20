using System;
using System.Threading.Tasks;
using FantasyCritic.CommandLine;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.JobManager;

/// <summary>
/// The command line project's worker commands, which deploy.sh runs so that a deploy waits for the running job.
/// Run here against the real job table, with intervals short enough to keep the fixture quick.
/// </summary>
[TestFixture]
public class WorkerCommandsTests : IntegrationTestBase
{
    private static readonly TimeSpan ShortPollInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan ShortTimeout = TimeSpan.FromMilliseconds(500);

    private ApiSession _adminSession = null!;

    [OneTimeSetUp]
    public async Task LoginAsAdmin()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
    }

    [OneTimeTearDown]
    public void DisposeSession()
    {
        _adminSession.Dispose();
    }

    //Every other test that runs a job depends on the worker being on.
    [TearDown]
    public async Task TurnWorkerBackOn()
    {
        await _adminSession.Admin.TurnOnWorkerAsync();
    }

    [Test]
    public async Task StopAndStartPulling_AreWhatShouldPullReports()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var workerCommands = CreateWorkerCommands(scope.ServiceProvider);

        await workerCommands.StopPulling();
        var afterStop = await workerCommands.GetShouldPull();
        var monitorAfterStop = await _adminSession.Admin.GetServiceMonitorAsync();

        await workerCommands.StartPulling();
        var afterStart = await workerCommands.GetShouldPull();

        Assert.Multiple(() =>
        {
            Assert.That(afterStop, Is.False);
            //The same flag the admin console's buttons set.
            Assert.That(monitorAfterStop.WorkerShouldPullNewJobs, Is.False);
            Assert.That(afterStart, Is.True);
        });
    }

    [Test]
    public async Task WaitUntilIdle_NothingRunning_IsIdle()
    {
        var idle = await WaitUntilIdle(ShortTimeout);

        Assert.That(idle, Is.True);
    }

    [Test]
    public async Task WaitUntilIdle_QueuedJobsDoNotCount()
    {
        //Queued jobs are the backlog the scheduler keeps building while the worker is off. Nothing is running them.
        var queuedJob = await _adminSession.Admin.ExpireTradesAsync();

        bool idle;
        try
        {
            idle = await WaitUntilIdle(ShortTimeout);
        }
        finally
        {
            //A job left queued would block every later manual run of its type.
            await JobTestHelpers.RunQueuedJobAsync(Factory, queuedJob.JobID);
        }

        Assert.That(idle, Is.True);
    }

    [Test]
    public async Task WaitUntilIdle_JobStillRunningAtTimeout_IsNotIdleAndLeavesTheFlagAlone()
    {
        var queuedJob = await _adminSession.Admin.ExpireTradesAsync();
        await StartJob(queuedJob.JobID);

        bool idle;
        bool shouldPullAfterwards;
        try
        {
            idle = await WaitUntilIdle(ShortTimeout);
            shouldPullAfterwards = (await _adminSession.Admin.GetServiceMonitorAsync()).WorkerShouldPullNewJobs;
        }
        finally
        {
            //A job left Running would block every later manual run of its type.
            await CompleteJob(queuedJob.JobID);
        }

        Assert.Multiple(() =>
        {
            Assert.That(idle, Is.False);
            //Waiting only waits. Putting the flag back after a timeout is deploy.sh's decision.
            Assert.That(shouldPullAfterwards, Is.True);
        });
    }

    [Test]
    public async Task WaitUntilIdle_WaitsForTheRunningJobToFinish()
    {
        var queuedJob = await _adminSession.Admin.ExpireTradesAsync();
        await StartJob(queuedJob.JobID);

        var waiting = WaitUntilIdle(TimeSpan.FromSeconds(30));
        await Task.Delay(TimeSpan.FromMilliseconds(400));
        var finishedWhileJobWasRunning = waiting.IsCompleted;

        await CompleteJob(queuedJob.JobID);
        var idle = await waiting;

        Assert.Multiple(() =>
        {
            Assert.That(finishedWhileJobWasRunning, Is.False);
            Assert.That(idle, Is.True);
        });
    }

    private async Task<bool> WaitUntilIdle(TimeSpan timeout)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await CreateWorkerCommands(scope.ServiceProvider).WaitUntilIdle(timeout, ShortPollInterval);
    }

    private static WorkerCommands CreateWorkerCommands(IServiceProvider serviceProvider)
    {
        return new WorkerCommands(serviceProvider.GetRequiredService<IFantasyCriticRepo>(), serviceProvider.GetRequiredService<IJobRepo>(), NullLogger<WorkerCommands>.Instance);
    }

    private async Task StartJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var job = await jobRepo.GetJob(jobID);
        var claimed = await jobRepo.StartJob(job!, clock.GetCurrentInstant());
        if (!claimed)
        {
            throw new InvalidOperationException($"Job {jobID} could not be started.");
        }
    }

    private async Task CompleteJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var job = await jobRepo.GetJob(jobID);
        await jobRepo.CompleteJob(job!, clock.GetCurrentInstant());
    }
}
