using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using NodaTime;

namespace FantasyCritic.Worker;

public class Worker : BackgroundService
{
    private static readonly TimeSpan RunnerPollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan CancellerPollInterval = TimeSpan.FromSeconds(2);

    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly WorkerStatus _workerStatus;

    //The one piece of state shared between the two loops. A CancellationTokenSource can't cross a process boundary,
    //so the canceller needs the runner's own tokens to stop a job that has already started.
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _inFlightJobs = new();

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, WorkerStatus workerStatus)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _workerStatus = workerStatus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var executionLoop = Task.Run(async () => await JobRunnerLoop(stoppingToken), CancellationToken.None);
        var cancellationLoop = Task.Run(async () => await JobCancellationLoop(stoppingToken), CancellationToken.None);

        await Task.WhenAll(executionLoop, cancellationLoop);
    }

    private async Task JobRunnerLoop(CancellationToken stoppingToken)
    {
        using var flowScope = _logger.BeginFlowScope(WorkerLogging.JobRunnerFlow);
        while (!stoppingToken.IsCancellationRequested)
        {
            FantasyCriticJob? nextJob = null;
            try
            {
                nextJob = await GetNextQueuedJob();
                if (nextJob is null)
                {
                    await Task.Delay(RunnerPollInterval, stoppingToken);
                    continue;
                }

                await RunJob(nextJob, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (nextJob is not null)
                {
                    using var jobScope = _logger.BeginJobScope(nextJob);
                    _logger.LogError(ex, "Job runner loop failed while handling job {Job}.", nextJob);
                }
                else
                {
                    _logger.LogError(ex, "Job runner loop failed while looking for a job to run.");
                }

                await DelayUnlessStopping(RunnerPollInterval, stoppingToken);
            }
        }
    }

    private async Task JobCancellationLoop(CancellationToken stoppingToken)
    {
        using var flowScope = _logger.BeginFlowScope(WorkerLogging.CancellerFlow);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ResolveCancellationRequests();
                await Task.Delay(CancellerPollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job cancellation loop failed.");
                await DelayUnlessStopping(CancellerPollInterval, stoppingToken);
            }
        }
    }

    private async Task<FantasyCriticJob?> GetNextQueuedJob()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var fantasyCriticRepo = scope.ServiceProvider.GetRequiredService<IFantasyCriticRepo>();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var systemWideSettings = await fantasyCriticRepo.GetSystemWideSettings();
        _workerStatus.RecordPoll(clock.GetCurrentInstant(), systemWideSettings.WorkerShouldPullNewJobs);
        if (!systemWideSettings.WorkerShouldPullNewJobs)
        {
            _logger.LogInformation("Intentionally not running new jobs because WorkerShouldPullNewJobs is FALSE.");
            return null;
        }

        var incompleteJobs = await jobRepo.GetIncompleteJobs();
        var queuedJobs = incompleteJobs.Where(x => x.Status.Equals(FantasyCriticJobStatus.Queued)).ToList();

        //Enqueueing already refuses these, so one only turns up if its RunType changed while it waited.
        //Settle it now, or it sits Queued and runs whenever the type is next re-enabled.
        foreach (var job in queuedJobs.Where(x => !x.AllowedByRunType))
        {
            using var jobScope = _logger.BeginJobScope(job);
            try
            {
                await CancelDisallowedJob(job, jobRepo, clock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel job {Job}, which its RunType no longer allows.", job);
            }
        }

        return queuedJobs.Where(x => x.AllowedByRunType).MinBy(x => x.CreatedAt);
    }

    private async Task CancelDisallowedJob(FantasyCriticJob job, IJobRepo jobRepo, IClock clock)
    {
        var runKind = job.IsCronRun ? "cron" : "manual";
        var reason = $"Cancelled before starting: the job type's RunType changed to {job.RunType}, which does not allow {runKind} runs.";
        var cancelled = await jobRepo.CancelQueuedJob(job, reason, clock.GetCurrentInstant());
        if (cancelled)
        {
            _logger.LogWarning("Cancelled job {Job}: RunType {RunType} does not allow {RunKind} runs.", job, job.RunType, runKind);
        }
    }

    private async Task RunJob(FantasyCriticJob job, CancellationToken stoppingToken)
    {
        //Covers the handler too, so its logs carry the JobID without the handler doing anything.
        using var jobScope = _logger.BeginJobScope(job);

        //Linked to the stopping token so a shutdown cancels the running job the same way an admin request does.
        using var jobCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        //Registered before the claim, so there is no window where the row says Running but the canceller can't find the token.
        if (!_inFlightJobs.TryAdd(job.JobID, jobCancellationSource))
        {
            _logger.LogWarning("Job {Job} is already in flight on this worker.", job);
            return;
        }

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();

            var claimed = await jobRepo.StartJob(job, clock.GetCurrentInstant());
            if (!claimed)
            {
                //Cancelled before it began, or another worker claimed it during a deploy overlap.
                _logger.LogInformation("Job {Job} was no longer queued when claimed; skipping.", job);
                return;
            }

            _logger.LogInformation("Starting job {Job}.", job);
            _workerStatus.RecordJobStarted(job);

            var context = new FantasyCriticJobContext(job, jobRepo);
            var cancelledInProgress = false;
            Exception? jobError = null;
            Result result = Result.Success();
            try
            {
                var handler = scope.ServiceProvider.GetRequiredKeyedService<IJobHandler>(job.Type);
                result = await handler.Run(context, jobCancellationSource.Token);
            }
            catch (OperationCanceledException) when (jobCancellationSource.IsCancellationRequested)
            {
                cancelledInProgress = true;
            }
            catch (Exception ex)
            {
                jobError = ex;
            }

            if (cancelledInProgress)
            {
                await jobRepo.CancelInProgressJob(job, clock.GetCurrentInstant());
                _logger.LogWarning("Cancelled job {Job} while in progress.", job);
            }
            else if (jobError is not null)
            {
                await jobRepo.ErrorJob(job, jobError.ToString(), clock.GetCurrentInstant());
                _logger.LogError(jobError, "Job {Job} failed.", job);
            }
            else if (result.IsFailure)
            {
                await jobRepo.ErrorJob(job, result.Error, clock.GetCurrentInstant());
                _logger.LogWarning("Job {Job} refused to run: {Reason}", job, result.Error);
            }
            else
            {
                await jobRepo.CompleteJob(job, clock.GetCurrentInstant());
                _logger.LogInformation("Completed job {Job}.", job);
            }
        }
        finally
        {
            _inFlightJobs.TryRemove(job.JobID, out _);
            _workerStatus.RecordJobFinished();
        }
    }

    private async Task ResolveCancellationRequests()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var incompleteJobs = await jobRepo.GetIncompleteJobs();
        var cancellingJobs = incompleteJobs.Where(x => x.Status.Equals(FantasyCriticJobStatus.Cancelling));
        foreach (var job in cancellingJobs)
        {
            using var jobScope = _logger.BeginJobScope(job);

            //One job failing to cancel shouldn't hold up the others.
            try
            {
                await ResolveCancellationRequest(job, jobRepo, clock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve cancellation request for job {Job}.", job);
            }
        }
    }

    private async Task ResolveCancellationRequest(FantasyCriticJob job, IJobRepo jobRepo, IClock clock)
    {
        if (job.StartedAt is null)
        {
            //Conditional on still being unstarted. If it loses, the job started underneath us,
            //and the next tick finds a StartedAt and takes the token path instead.
            var cancelled = await jobRepo.CancelJob(job, clock.GetCurrentInstant());
            if (cancelled)
            {
                _logger.LogInformation("Cancelled job {Job} before it started.", job);
            }

            return;
        }

        //Trip the token and stop there: the runner writes CancelledInProgress once the job unwinds.
        //A job this worker doesn't hold belongs to another worker, or to one that died; either way it isn't ours to settle.
        if (!_inFlightJobs.TryGetValue(job.JobID, out var jobCancellationSource))
        {
            _logger.LogDebug("Job {Job} is cancelling but is not running on this worker.", job);
            return;
        }

        if (jobCancellationSource.IsCancellationRequested)
        {
            return;
        }

        _logger.LogInformation("Requesting cancellation of in-progress job {Job}.", job);
        try
        {
            await jobCancellationSource.CancelAsync();
        }
        catch (ObjectDisposedException)
        {
            //The job finished between the lookup and the cancel. Nothing left to stop.
        }
    }

    private static async Task DelayUnlessStopping(TimeSpan delay, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(delay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            //Shutting down; the loop condition handles the exit.
        }
    }
}
