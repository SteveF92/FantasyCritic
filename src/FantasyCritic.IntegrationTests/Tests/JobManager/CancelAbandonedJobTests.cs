using System;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.JobManager;

/// <summary>
/// The canceller settles a started job that no runner holds, so that cancelling a stuck job in the console clears it.
/// The test host has no worker, so these call the repo the way the canceller does, against the real job table.
/// </summary>
[TestFixture]
public class CancelAbandonedJobTests : IntegrationTestBase
{
    private const string Reason = "No worker was running this job.";

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

    [Test]
    public async Task CancellingStartedJob_IsSettledAndKeepsItsLastProgress()
    {
        var queuedJob = await _adminSession.Admin.ExpireTradesAsync();
        await StartJob(queuedJob.JobID);
        await UpdateDetailedStatus(queuedJob.JobID, "Halfway through.");
        await _adminSession.JobManager.CancelJobAsync(new CancelJobRequest { JobID = queuedJob.JobID });

        var cancelled = await CancelAbandonedJob(queuedJob.JobID);
        var job = await GetJob(queuedJob.JobID);

        Assert.Multiple(() =>
        {
            Assert.That(cancelled, Is.True);
            Assert.That(job.Status, Is.EqualTo(FantasyCriticJobStatus.CancelledInProgress));
            Assert.That(job.ErrorMessage, Is.EqualTo(Reason));
            Assert.That(job.DetailedStatus, Is.EqualTo("Halfway through."));
            Assert.That(job.FinishedAt, Is.Not.Null);
        });
    }

    [Test]
    public async Task RunningJobNobodyCancelled_IsLeftAlone()
    {
        //A stuck Running row is only settled once someone cancels it.
        var queuedJob = await _adminSession.Admin.ExpireTradesAsync();
        await StartJob(queuedJob.JobID);

        bool cancelled;
        FantasyCriticJob job;
        try
        {
            cancelled = await CancelAbandonedJob(queuedJob.JobID);
            job = await GetJob(queuedJob.JobID);
        }
        finally
        {
            //A job left Running would block every later manual run of its type.
            await CompleteJob(queuedJob.JobID);
        }

        Assert.Multiple(() =>
        {
            Assert.That(cancelled, Is.False);
            Assert.That(job.Status, Is.EqualTo(FantasyCriticJobStatus.Running));
        });
    }

    [Test]
    public async Task JobItsRunnerSettledFirst_IsLeftAlone()
    {
        //The job finished between the canceller reading it and looking for its token.
        var queuedJob = await _adminSession.Admin.ExpireTradesAsync();
        await StartJob(queuedJob.JobID);
        await _adminSession.JobManager.CancelJobAsync(new CancelJobRequest { JobID = queuedJob.JobID });
        await CancelInProgressJob(queuedJob.JobID);

        var cancelled = await CancelAbandonedJob(queuedJob.JobID);
        var job = await GetJob(queuedJob.JobID);

        Assert.Multiple(() =>
        {
            Assert.That(cancelled, Is.False);
            Assert.That(job.Status, Is.EqualTo(FantasyCriticJobStatus.CancelledInProgress));
            Assert.That(job.ErrorMessage, Is.Null);
        });
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

    private async Task UpdateDetailedStatus(Guid jobID, string detailedStatus)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var job = await jobRepo.GetJob(jobID);
        await jobRepo.UpdateDetailedStatusForJob(job!, detailedStatus);
    }

    private async Task CompleteJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var job = await jobRepo.GetJob(jobID);
        await jobRepo.CompleteJob(job!, clock.GetCurrentInstant());
    }

    private async Task CancelInProgressJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var job = await jobRepo.GetJob(jobID);
        await jobRepo.CancelInProgressJob(job!, clock.GetCurrentInstant());
    }

    private async Task<bool> CancelAbandonedJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var job = await jobRepo.GetJob(jobID);
        return await jobRepo.CancelAbandonedJob(job!, Reason, clock.GetCurrentInstant());
    }

    private async Task<FantasyCriticJob> GetJob(Guid jobID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var job = await jobRepo.GetJob(jobID);
        return job!;
    }
}
