using System.Diagnostics;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.CommandLine;

/// <summary>
/// Lets a deploy wait for the running job instead of killing it. Each command does one thing; it is deploy.sh that
/// remembers whether the worker was pulling beforehand, so that a worker someone turned off by hand stays off.
/// </summary>
public class WorkerCommands
{
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(5);

    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IJobRepo _jobRepo;
    private readonly ILogger<WorkerCommands> _logger;

    public WorkerCommands(IFantasyCriticRepo fantasyCriticRepo, IJobRepo jobRepo, ILogger<WorkerCommands> logger)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _jobRepo = jobRepo;
        _logger = logger;
    }

    public async Task<bool> GetShouldPull()
    {
        var systemWideSettings = await _fantasyCriticRepo.GetSystemWideSettings();
        return systemWideSettings.WorkerShouldPullNewJobs;
    }

    public async Task StopPulling()
    {
        await _fantasyCriticRepo.SetWorkerShouldPullNewJobs(false);
        _logger.LogInformation("Turned WorkerShouldPullNewJobs off.");
    }

    public async Task StartPulling()
    {
        await _fantasyCriticRepo.SetWorkerShouldPullNewJobs(true);
        _logger.LogInformation("Turned WorkerShouldPullNewJobs on.");
    }

    /// <returns>True once nothing is Running or Cancelling. False if something still was when the timeout ran out.</returns>
    public async Task<bool> WaitUntilIdle(TimeSpan timeout, TimeSpan pollInterval)
    {
        _logger.LogInformation("Waiting up to {Timeout} for running jobs to finish.", timeout);

        //Real elapsed time rather than IClock: this is how long to keep waiting, not a domain timestamp.
        var waited = Stopwatch.StartNew();
        while (true)
        {
            //Waits before the first look as well. The claim checks the flag in the same statement, so nothing can
            //start once it is off, but a claim that was already in flight when it changed needs a moment to land.
            await Task.Delay(pollInterval);

            var stillRunning = await GetJobsStillRunning();
            if (stillRunning.Count == 0)
            {
                _logger.LogInformation("No jobs are running.");
                return true;
            }

            if (waited.Elapsed >= timeout)
            {
                _logger.LogError("Gave up after {Timeout}. Still running: {Jobs}.", timeout, string.Join("; ", stillRunning));
                return false;
            }

            _logger.LogInformation("Waiting on {Jobs}.", string.Join("; ", stillRunning));
        }
    }

    private async Task<IReadOnlyList<FantasyCriticJob>> GetJobsStillRunning()
    {
        var incompleteJobs = await _jobRepo.GetIncompleteJobs();
        return incompleteJobs
            .Where(x => x.Status.Equals(FantasyCriticJobStatus.Running) || x.Status.Equals(FantasyCriticJobStatus.Cancelling))
            .ToList();
    }
}
