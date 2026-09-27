using FantasyCritic.Lib.Jobs;
using FantasyCritic.Lib.SharedSerialization.API;
using FantasyCritic.Web.Utilities;

namespace FantasyCritic.Web.Models.Responses;

public class ServiceMonitorViewModel
{
    public ServiceMonitorViewModel(Instant checkedAt, bool workerShouldPullNewJobs, WorkerState workerState, ServiceHealthReport webHealth, ServiceHealthReport workerHealth,
        ServiceHealthReport discordBotHealth, GrafanaLogLinks logLinks)
    {
        CheckedAt = checkedAt;
        WorkerShouldPullNewJobs = workerShouldPullNewJobs;
        WorkerState = workerState.Value;
        Web = new ServiceHealthViewModel("Web", webHealth, logLinks.ForWeb());
        Worker = new ServiceHealthViewModel("Worker", workerHealth, logLinks.ForWorker());
        DiscordBot = new ServiceHealthViewModel("Discord Bot", discordBotHealth, logLinks.ForDiscordBot());
    }

    public Instant CheckedAt { get; }
    public bool WorkerShouldPullNewJobs { get; }
    public string WorkerState { get; }
    public ServiceHealthViewModel Web { get; }
    public ServiceHealthViewModel Worker { get; }
    public ServiceHealthViewModel DiscordBot { get; }
}

public class ServiceHealthViewModel
{
    public ServiceHealthViewModel(string name, ServiceHealthReport report, IReadOnlyList<LogLink> logLinks)
    {
        Name = name;
        Status = report.Status;
        Description = report.Description;
        Details = report.Details.Select(x => new ServiceHealthDetailViewModel(x)).ToList();
        LogLinks = logLinks.Select(x => new LogLinkViewModel(x)).ToList();
    }

    public string Name { get; }
    public string Status { get; }
    public string? Description { get; }
    public IReadOnlyList<ServiceHealthDetailViewModel> Details { get; }
    public IReadOnlyList<LogLinkViewModel> LogLinks { get; }
}

public class ServiceHealthDetailViewModel
{
    public ServiceHealthDetailViewModel(ServiceHealthDetail detail)
    {
        Label = detail.Label;
        Text = detail.Text;
        Time = detail.Time;
    }

    public string Label { get; }
    public string? Text { get; }
    public Instant? Time { get; }
}

public class LogLinkViewModel
{
    public LogLinkViewModel(LogLink domain)
    {
        Label = domain.Label;
        Url = domain.Url;
        Level = domain.Level?.ToString();
    }

    public string Label { get; }
    public string Url { get; }
    public string? Level { get; }
}
