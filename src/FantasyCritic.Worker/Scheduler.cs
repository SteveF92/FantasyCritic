using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using NodaTime;

namespace FantasyCritic.Worker;

//Decides what is due and inserts it into tbl_job. It never runs anything, and talks to the runner only through that table,
//so it can move to its own process by moving one AddHostedService line.
public class Scheduler : BackgroundService
{
    //The longest the scheduler sleeps, so a RunType changed in the database takes effect within a minute even when nothing is due for hours.
    private static readonly Duration MaxPollInterval = Duration.FromMinutes(1);
    private static readonly Duration MinPollInterval = Duration.FromMilliseconds(100);

    //How late a slot can still be enqueued. Comfortably covers a deploy or a crash-restart; beyond it the slot is skipped,
    //so Thursday's public bidding emails don't go out on Saturday because the worker was down, or because the type was just switched to cron.
    private static readonly Duration MisfireThreshold = Duration.FromMinutes(30);

    private readonly ILogger<Scheduler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IClock _clock;

    //Schedules come from the registry, not from handler instances, so scheduling never constructs a handler or its dependencies.
    private readonly IReadOnlyDictionary<FantasyCriticJobType, FantasyCriticJobSchedule> _schedules;

    //The most recent slot reported as missed for each type, so a skipped weekly slot warns once instead of every tick until the next one.
    private readonly Dictionary<FantasyCriticJobType, Instant> _reportedMisfires = new();

    public Scheduler(ILogger<Scheduler> logger, IServiceProvider serviceProvider, IClock clock, FantasyCriticJobRegistry jobRegistry)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _clock = clock;
        _schedules = jobRegistry.Schedules;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Only consulted for a type that has never been scheduled. Anchoring it here, not earlier, means starting the worker never runs anything by itself.
        var startedAt = _clock.GetCurrentInstant();

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = MaxPollInterval;
            try
            {
                await EnqueueDueJobs(startedAt);
                delay = GetDelayUntilNextOccurrence();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduler failed while enqueueing due jobs.");
            }

            try
            {
                await Task.Delay(delay.ToTimeSpan(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task EnqueueDueJobs(Instant startedAt)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();

        var jobTypeRunTypes = (await jobRepo.GetJobTypeRunTypes()).ToDictionary(x => x.JobType);
        var lastScheduledTimes = await jobRepo.GetLastScheduledTimes();
        var now = _clock.GetCurrentInstant();

        foreach (var (jobType, schedule) in _schedules)
        {
            //No row in tbl_job_type means the FK would reject the insert anyway. Startup reconciliation is what reports that.
            if (!jobTypeRunTypes.TryGetValue(jobType, out var jobTypeWithRunType) || !jobTypeWithRunType.RunType.AllowsCron)
            {
                continue;
            }

            //One type failing to enqueue shouldn't hold up the others.
            try
            {
                //The last enqueued slot, not process start, is what the next one is computed from. That is what makes a deploy during a slot catch it up.
                var after = lastScheduledTimes.TryGetValue(jobType, out var lastScheduledFor) ? lastScheduledFor : startedAt;
                await EnqueueIfDue(jobTypeWithRunType, schedule, after, now, jobRepo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduler failed to enqueue job type {JobType}.", jobType);
            }
        }
    }

    private async Task EnqueueIfDue(FantasyCriticJobTypeWithRunType jobTypeWithRunType, FantasyCriticJobSchedule schedule, Instant after, Instant now, IJobRepo jobRepo)
    {
        var jobType = jobTypeWithRunType.JobType;

        var slot = schedule.GetLatestOccurrence(after, now);
        if (slot is null)
        {
            return;
        }

        var lateness = now - slot.Value;
        if (lateness > MisfireThreshold)
        {
            ReportMisfire(jobType, schedule, slot.Value, lateness);
            return;
        }

        var job = new FantasyCriticJob(Guid.NewGuid(), jobTypeWithRunType, createdByUser: null, FantasyCriticJobStatus.Queued,
            detailedStatus: null, errorMessage: null, scheduledFor: slot.Value, createdAt: now, startedAt: null, finishedAt: null);

        var created = await jobRepo.CreateJob(job);
        if (created)
        {
            _logger.LogInformation("Enqueued job {Job} for scheduled slot {ScheduledFor} ({Schedule}).", job, slot.Value, schedule);
        }
        else
        {
            //Another scheduler got there first, such as the old container during a deploy overlap. The slot is enqueued, which is all that matters.
            _logger.LogDebug("Slot {ScheduledFor} for {JobType} was already enqueued.", slot.Value, jobType);
        }
    }

    private void ReportMisfire(FantasyCriticJobType jobType, FantasyCriticJobSchedule schedule, Instant slot, Duration lateness)
    {
        if (_reportedMisfires.TryGetValue(jobType, out var reportedSlot) && reportedSlot == slot)
        {
            return;
        }

        _reportedMisfires[jobType] = slot;
        _logger.LogWarning("Skipped {JobType} slot {ScheduledFor} ({Schedule}): it came due {Lateness} ago, past the {MisfireThreshold} misfire threshold.",
            jobType, slot, schedule, lateness, MisfireThreshold);
    }

    //Wakes at the next slot across every scheduled type, whatever its RunType, rather than polling on a fixed interval,
    //so a 20:00 job is enqueued at 20:00 and not up to a minute later.
    private Duration GetDelayUntilNextOccurrence()
    {
        var now = _clock.GetCurrentInstant();
        var nextOccurrence = _schedules.Values.Min(x => x.GetNextOccurrence(now));
        var untilNextOccurrence = nextOccurrence - now;

        if (untilNextOccurrence > MaxPollInterval)
        {
            return MaxPollInterval;
        }

        return untilNextOccurrence < MinPollInterval ? MinPollInterval : untilNextOccurrence;
    }
}
