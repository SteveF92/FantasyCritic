using System;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.JobManager;

[TestFixture]
public class WorkerPullFlagTests : IntegrationTestBase
{
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

    //Every other test that runs a job depends on this being on.
    [TearDown]
    public async Task TurnPullingBackOn()
    {
        await SetWorkerShouldPullNewJobs(true);
    }

    [Test]
    public async Task FlagOff_QueuedJobCannotBeClaimed_ThenRunsOnceFlagIsBackOn()
    {
        var queuedJob = await _adminSession.Admin.ExpireTradesAsync();

        await SetWorkerShouldPullNewJobs(false);
        var claimedWhileOff = await TryStartJob(queuedJob.JobID);
        var statusWhileOff = await GetJobStatus(queuedJob.JobID);
        await SetWorkerShouldPullNewJobs(true);

        if (claimedWhileOff)
        {
            //Settle it before failing. A job left Running would block every later manual run of its type.
            await CompleteJob(queuedJob.JobID);
        }
        else
        {
            await JobTestHelpers.RunQueuedJobAsync(Factory, queuedJob.JobID);
        }

        var statusAfterOn = await GetJobStatus(queuedJob.JobID);

        Assert.Multiple(() =>
        {
            Assert.That(claimedWhileOff, Is.False);
            Assert.That(statusWhileOff, Is.EqualTo(FantasyCriticJobStatus.Queued));
            Assert.That(statusAfterOn, Is.EqualTo(FantasyCriticJobStatus.Complete));
        });
    }

    [Test]
    public async Task Flag_RoundTripsThroughSystemWideSettings()
    {
        await SetWorkerShouldPullNewJobs(false);
        var whileOff = await GetWorkerShouldPullNewJobs();

        await SetWorkerShouldPullNewJobs(true);
        var whileOn = await GetWorkerShouldPullNewJobs();

        Assert.Multiple(() =>
        {
            Assert.That(whileOff, Is.False);
            Assert.That(whileOn, Is.True);
        });
    }

    private async Task SetWorkerShouldPullNewJobs(bool shouldPull)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var fantasyCriticRepo = scope.ServiceProvider.GetRequiredService<IFantasyCriticRepo>();
        await fantasyCriticRepo.SetWorkerShouldPullNewJobs(shouldPull);
    }

    private async Task<bool> GetWorkerShouldPullNewJobs()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var fantasyCriticRepo = scope.ServiceProvider.GetRequiredService<IFantasyCriticRepo>();
        var systemWideSettings = await fantasyCriticRepo.GetSystemWideSettings();
        return systemWideSettings.WorkerShouldPullNewJobs;
    }

    private async Task<bool> TryStartJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var job = await jobRepo.GetJob(jobID);
        return await jobRepo.StartJob(job!, clock.GetCurrentInstant());
    }

    private async Task CompleteJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var job = await jobRepo.GetJob(jobID);
        await jobRepo.CompleteJob(job!, clock.GetCurrentInstant());
    }

    private async Task<FantasyCriticJobStatus> GetJobStatus(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var job = await jobRepo.GetJob(jobID);
        return job!.Status;
    }
}
