using FantasyCritic.Hosting;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Worker;

//The worker runs three independent flows in one process. These are log scopes rather than logger settings, so everything logged inside one
//is tagged: a handler's own injected logger, and the services it calls, pick up the flow and job without knowing about either.
public static class WorkerLogging
{
    public const string JobRunnerFlow = "JobRunner";
    public const string CancellerFlow = "Canceller";
    public const string SchedulerFlow = "Scheduler";

    public static IReadOnlyList<string> Flows { get; } = [JobRunnerFlow, CancellerFlow, SchedulerFlow];

    public static IDisposable? BeginFlowScope(this ILogger logger, string flow)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            [FantasyCriticLogging.FlowProperty] = flow
        });
    }

    public static IDisposable? BeginJobScope(this ILogger logger, FantasyCriticJob job)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["JobID"] = job.JobID,
            ["JobType"] = job.Type.Value
        });
    }
}
