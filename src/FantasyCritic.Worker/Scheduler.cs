using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using NodaTime;

namespace FantasyCritic.Worker;

public class Scheduler : BackgroundService
{
    private static readonly Duration MinimumSleepDuration = Duration.FromSeconds(5);

    private readonly ILogger<Scheduler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IClock _clock;
    private readonly FantasyCriticJobRegistry _jobRegistry;
    private readonly Dictionary<FantasyCriticJobType, Instant> _nextScheduledOccurrencePerJobType;

    public Scheduler(ILogger<Scheduler> logger, IServiceProvider serviceProvider, IClock clock, FantasyCriticJobRegistry jobRegistry)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _clock = clock;
        _jobRegistry = jobRegistry;
        _nextScheduledOccurrencePerJobType = new Dictionary<FantasyCriticJobType, Instant>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var flowScope = _logger.BeginFlowScope(WorkerLogging.SchedulerFlow);

        //Seed from 30 minutes ago so a slot that fell in a restart gap is still attempted.
        //The UNIQUE constraint makes this safe if the old worker already enqueued it.
        var startupSchedulingInstant = _clock.GetCurrentInstant();
        CalculateNextOccurrenceTimes(startupSchedulingInstant - Duration.FromMinutes(30));

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Scheduler running at: {time}", DateTimeOffset.Now);
            }

            await using var scope = _serviceProvider.CreateAsyncScope();
            var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepo>();

            var schedulingInstant = _clock.GetCurrentInstant();
            var updatedJobTypes = await jobRepo.GetJobTypeRunTypes();

            //Decide everything that is due before enqueueing anything, because whether a job runs can depend on what else is due.
            var dueSlots = new List<DueSlot>();
            foreach (var (jobType, schedule) in _jobRegistry.Schedules)
            {
                var jobTypeFromDatabase = updatedJobTypes.SingleOrDefault(x => x.JobType.Equals(jobType));
                if (jobTypeFromDatabase is null)
                {
                    throw new Exception($"Job Type {jobType} not found in database.");
                }

                if (!_nextScheduledOccurrencePerJobType.TryGetValue(jobType, out var nextScheduledOccurrenceForJobType))
                {
                    _logger.LogDebug("Nothing to queue for {JobType}", jobType);
                    continue;
                }

                var shouldSchedule = schedulingInstant >= nextScheduledOccurrenceForJobType;
                if (!shouldSchedule)
                {
                    _logger.LogDebug("We have not reached the next scheduled time for {JobType}, which is {NextJobTime}. Nothing to do.", jobType, nextScheduledOccurrenceForJobType);
                    continue;
                }

                if (!jobTypeFromDatabase.RunType.AllowsCron)
                {
                    _logger.LogWarning("Skipped slot {ScheduledFor} for {JobType}: it has a cron schedule in code, but its RunType {RunType} does not allow cron.",
                        nextScheduledOccurrenceForJobType, jobType, jobTypeFromDatabase.RunType);
                    continue;
                }

                dueSlots.Add(new DueSlot(jobTypeFromDatabase, schedule, nextScheduledOccurrenceForJobType));
            }

            var dueJobTypes = dueSlots.Select(x => x.JobType.JobType).ToHashSet();
            foreach (var dueSlot in dueSlots)
            {
                var jobType = dueSlot.JobType.JobType;
                var dueJobsToDeferTo = _jobRegistry.GetDueJobsToDeferTo(jobType, dueJobTypes);
                if (dueJobsToDeferTo.Any())
                {
                    _logger.LogInformation("Skipped slot {ScheduledFor} for {JobType}: deferring to {DueJobTypes}, due in the same wake.",
                        dueSlot.ScheduledFor, jobType, string.Join(", ", dueJobsToDeferTo));
                    continue;
                }

                var job = new FantasyCriticJob(Guid.NewGuid(), dueSlot.JobType, createdByUser: null, FantasyCriticJobStatus.Queued,
                    detailedStatus: null, errorMessage: null, scheduledFor: dueSlot.ScheduledFor, createdAt: schedulingInstant, startedAt: null, finishedAt: null,
                    cancelledAt: null, cancelledByUser: null);
                using var jobScope = _logger.BeginJobScope(job);

                var created = await jobRepo.CreateJob(job);
                if (created)
                {
                    _logger.LogInformation("Enqueued job {Job} for scheduled slot {ScheduledFor} ({Schedule}).", job, dueSlot.ScheduledFor, dueSlot.Schedule);
                }
                else
                {
                    //Another scheduler got there first, such as the old container during a deploy overlap. The slot is enqueued, which is all that matters.
                    _logger.LogWarning("Slot {ScheduledFor} for {JobType} was already enqueued - probably a deployment overlap.", dueSlot.ScheduledFor, jobType);
                }
            }

            CalculateNextOccurrenceTimes(schedulingInstant);

            var nextTaskTime = _nextScheduledOccurrencePerJobType.Values.Min();
            var beforeSleepInstant = _clock.GetCurrentInstant();
            //The buffer makes sure we are definitely after the scheduled time. The minimum handles a slow iteration or forward clock adjustment.
            var bufferedSleepDuration = (nextTaskTime - beforeSleepInstant).Plus(MinimumSleepDuration);
            var howLongToSleep = bufferedSleepDuration < MinimumSleepDuration ? MinimumSleepDuration : bufferedSleepDuration;
            await Task.Delay(howLongToSleep.ToTimeSpan(), stoppingToken);
        }
    }

    private void CalculateNextOccurrenceTimes(Instant schedulingInstant)
    {
        foreach (var (jobType, schedule) in _jobRegistry.Schedules)
        {
            Instant? nextOccurrence = schedule.GetNextOccurrence(schedulingInstant);
            if (nextOccurrence.HasValue)
            {
                _nextScheduledOccurrencePerJobType[jobType] = nextOccurrence.Value;
            }
            else
            {
                _nextScheduledOccurrencePerJobType.Remove(jobType);
            }
        }
    }

    private record DueSlot(FantasyCriticJobTypeWithRunType JobType, FantasyCriticJobSchedule Schedule, Instant ScheduledFor);
}
