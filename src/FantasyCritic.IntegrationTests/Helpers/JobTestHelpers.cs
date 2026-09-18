using System;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// The test host has no worker, so a test that queues a job runs it here, in-process and synchronously.
/// This is the worker's claim → run → settle sequence without its polling, cancellation or error swallowing:
/// a job that fails throws, so the test fails at the job rather than at a later assertion.
/// </summary>
internal static class JobTestHelpers
{
    public static async Task RunQueuedJobAsync(FantasyCriticWebApplicationFactory factory, Guid jobID)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var job = await jobRepo.GetJob(jobID) ?? throw new InvalidOperationException($"Job {jobID} does not exist.");
        var claimed = await jobRepo.StartJob(job, clock.GetCurrentInstant());
        if (!claimed)
        {
            throw new InvalidOperationException($"Job {job} was not queued, so it could not be started.");
        }

        var handler = scope.ServiceProvider.GetRequiredKeyedService<IJobHandler>(job.Type);
        var context = new FantasyCriticJobContext(job, jobRepo);
        CSharpFunctionalExtensions.Result result;
        try
        {
            result = await handler.Run(context, CancellationToken.None);
        }
        catch (Exception ex)
        {
            await jobRepo.ErrorJob(job, ex.ToString(), clock.GetCurrentInstant());
            throw;
        }

        if (result.IsFailure)
        {
            await jobRepo.ErrorJob(job, result.Error, clock.GetCurrentInstant());
            throw new InvalidOperationException($"Job {job} refused to run: {result.Error}");
        }

        await jobRepo.CompleteJob(job, clock.GetCurrentInstant());
    }
}
