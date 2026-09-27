namespace FantasyCritic.Lib.Jobs;

/// <summary>
/// What the admin monitor says about the worker. Turning the worker off only clears WorkerShouldPullNewJobs:
/// the process stays up, finishes the job it has (Draining) and then sits idle (Off).
/// </summary>
public class WorkerState : TypeSafeEnum<WorkerState>
{
    // Define values here.
    public static readonly WorkerState Unreachable = new WorkerState("Unreachable");
    public static readonly WorkerState Unhealthy = new WorkerState("Unhealthy");
    public static readonly WorkerState Running = new WorkerState("Running");
    public static readonly WorkerState Draining = new WorkerState("Draining");
    public static readonly WorkerState Off = new WorkerState("Off");

    // Constructor is private: values are defined within this class only!
    private WorkerState(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;

    /// <param name="workerReachable">Whether the worker answered its health endpoint at all.</param>
    /// <param name="workerHealthy">Whether that answer was anything but Unhealthy.</param>
    /// <param name="workerShouldPullNewJobs">The flag, read from the database rather than from the worker.</param>
    /// <param name="incompleteJobs">Also from the database, so Draining agrees with what a deploy's drain waits on.</param>
    public static WorkerState Determine(bool workerReachable, bool workerHealthy, bool workerShouldPullNewJobs, IEnumerable<FantasyCriticJob> incompleteJobs)
    {
        if (!workerReachable)
        {
            return Unreachable;
        }

        if (!workerHealthy)
        {
            return Unhealthy;
        }

        if (workerShouldPullNewJobs)
        {
            return Running;
        }

        var anyJobStillRunning = incompleteJobs.Any(x => x.Status.Equals(FantasyCriticJobStatus.Running) || x.Status.Equals(FantasyCriticJobStatus.Cancelling));
        return anyJobStillRunning ? Draining : Off;
    }
}
