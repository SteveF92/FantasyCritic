using FantasyCritic.Lib.Jobs;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class ServiceMonitorViewModel
{
    public ServiceMonitorViewModel(Instant checkedAt, bool workerShouldPullNewJobs, WorkerState workerState, ServiceHealthReport workerHealth, ServiceHealthReport discordBotHealth)
    {
        CheckedAt = checkedAt;
        WorkerShouldPullNewJobs = workerShouldPullNewJobs;
        WorkerState = workerState.Value;
        Worker = new ServiceHealthViewModel("Worker", workerHealth);
        DiscordBot = new ServiceHealthViewModel("Discord Bot", discordBotHealth);
    }

    public Instant CheckedAt { get; }
    public bool WorkerShouldPullNewJobs { get; }
    public string WorkerState { get; }
    public ServiceHealthViewModel Worker { get; }
    public ServiceHealthViewModel DiscordBot { get; }
}

public class ServiceHealthViewModel
{
    public ServiceHealthViewModel(string name, ServiceHealthReport report)
    {
        Name = name;
        Status = report.Status;
        Description = report.Description;
        Details = report.Data;
    }

    public string Name { get; }
    public string Status { get; }
    public string? Description { get; }
    public IReadOnlyDictionary<string, string> Details { get; }
}
