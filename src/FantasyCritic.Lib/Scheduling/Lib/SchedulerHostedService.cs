using FantasyCritic.Lib.Scheduling.Lib.Cron;

namespace FantasyCritic.Lib.Scheduling.Lib;

public class SchedulerHostedService : HostedService
{
    public event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException;

    private readonly List<SchedulerTaskWrapper> _scheduledTasks = new List<SchedulerTaskWrapper>();

    public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks)
    {
        var referenceTime = DateTime.UtcNow;

        foreach (var scheduledTask in scheduledTasks)
        {
            _scheduledTasks.Add(new SchedulerTaskWrapper(CrontabSchedule.Parse(scheduledTask.Schedule), scheduledTask, referenceTime));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteOnceAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }

    private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        var taskFactory = new TaskFactory(TaskScheduler.Current);
        var referenceTime = DateTime.UtcNow;

        var tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime)).ToList();

        foreach (var taskThatShouldRun in tasksThatShouldRun)
        {
            taskThatShouldRun.Increment();

            await taskFactory.StartNew(
                async () =>
                {
                    try
                    {
                        await taskThatShouldRun.Task.ExecuteAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        var args = new UnobservedTaskExceptionEventArgs(
                            ex as AggregateException ?? new AggregateException(ex));

                        UnobservedTaskException?.Invoke(this, args);

                        if (!args.Observed)
                        {
                            throw;
                        }
                    }
                },
                cancellationToken);
        }
    }

    private class SchedulerTaskWrapper
    {
        public SchedulerTaskWrapper(CrontabSchedule schedule, IScheduledTask task, DateTime nextRunTime)
        {
            Schedule = schedule;
            Task = task;
            NextRunTime = nextRunTime;
        }

        public CrontabSchedule Schedule { get; }
        public IScheduledTask Task { get; }

        public DateTime LastRunTime { get; private set; }
        public DateTime NextRunTime { get; private set; }

        public void Increment()
        {
            LastRunTime = NextRunTime;
            NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
        }

        public bool ShouldRun(DateTime currentTime)
        {
            return NextRunTime < currentTime && LastRunTime != NextRunTime;
        }
    }
}
