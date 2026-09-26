using FantasyCritic.Hosting;
using FantasyCritic.Lib.Jobs;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace FantasyCritic.Worker;

//The worker runs three independent flows in one process. These push onto Serilog's LogContext rather than opening ILogger scopes,
//because an ILogger scope only reaches loggers created through Microsoft.Extensions.Logging. Services with a static
//Log.ForContext logger would miss it. LogContext tags everything logged inside, whichever kind of logger writes it.
public static class WorkerLogging
{
    public const string JobRunnerFlow = "JobRunner";
    public const string CancellerFlow = "Canceller";
    public const string SchedulerFlow = "Scheduler";

    public static IReadOnlyList<string> Flows { get; } = [JobRunnerFlow, CancellerFlow, SchedulerFlow];

    public static IDisposable BeginFlowScope(string flow)
    {
        return LogContext.PushProperty(FantasyCriticLogging.FlowProperty, flow);
    }

    public static IDisposable BeginJobScope(FantasyCriticJob job)
    {
        return LogContext.Push(
            new PropertyEnricher("JobID", job.JobID),
            new PropertyEnricher("JobType", job.Type.Value));
    }
}
