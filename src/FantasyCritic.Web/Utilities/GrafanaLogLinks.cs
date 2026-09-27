using System.Diagnostics.CodeAnalysis;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Web.Utilities;

public record LogLink(string Label, string Url);

/// <summary>
/// Links to logs in Grafana's Logs Drilldown app, for the admin console: a job's, or a service's recent ones.
/// </summary>
public class GrafanaLogLinks
{
    //The hosts' Loki "app" labels and the worker's flows, from their appsettings and WorkerLogging. The web app references neither host,
    //so these are repeated here, and GrafanaLogLinksTests compares them with the originals.
    public const string WebApp = "fantasycritic-web";
    public const string WorkerApp = "fantasycritic-worker";
    public const string DiscordBotApp = "fantasycritic-discordbot";
    public const string JobRunnerFlow = "JobRunner";
    public const string SchedulerFlow = "Scheduler";
    public const string CancellerFlow = "Canceller";

    //Logs from just before and after the job's own timestamps, since those are written a moment apart from its log lines.
    private static readonly Duration Margin = Duration.FromMinutes(1);

    private readonly GrafanaLogsOptions _options;
    private readonly string? _lokiEnvironment;

    /// <param name="lokiEnvironment">The "env" label the hosts' logs carry, or null where nothing is sent to Loki.</param>
    public GrafanaLogLinks(GrafanaLogsOptions options, string? lokiEnvironment)
    {
        _options = options;
        _lokiEnvironment = lokiEnvironment;
    }

    [MemberNotNullWhen(true, nameof(_lokiEnvironment))]
    private bool Enabled => _lokiEnvironment is not null && !string.IsNullOrWhiteSpace(_options.Url);

    public string? ForJob(FantasyCriticJob job)
    {
        if (!Enabled)
        {
            return null;
        }

        var from = (job.CreatedAt - Margin).ToUnixTimeMilliseconds().ToString();
        var lastTimestamp = new[] { job.FinishedAt, job.CancelledAt }.Max();
        var to = lastTimestamp.HasValue ? (lastTimestamp.Value + Margin).ToUnixTimeMilliseconds().ToString() : "now";

        //The worker puts JobID on every line it logs while handling a job, handler included. Drilldown writes a field
        //filter as "name|operator|{parser and value as JSON},value", with the JSON's commas escaped as __gfc__.
        var jobID = job.JobID.ToString();
        var jobIDFilter = $"JobID|=|{{\"parser\":\"json\"__gfc__\"value\":\"{jobID}\"}},{jobID}";

        //Oldest first, so a job reads from start to finish.
        return BuildUrl(_lokiEnvironment, from, to, [$"app|=|{WorkerApp}"], jobIDFilter, "Ascending");
    }

    public IReadOnlyList<LogLink> ForWeb() => ForService(("Logs", WebApp, null));
    public IReadOnlyList<LogLink> ForDiscordBot() => ForService(("Logs", DiscordBotApp, null));

    public IReadOnlyList<LogLink> ForWorker() => ForService(
        ("All", WorkerApp, null),
        ("Job Runner", WorkerApp, JobRunnerFlow),
        ("Scheduler", WorkerApp, SchedulerFlow),
        ("Canceller", WorkerApp, CancellerFlow));

    private IReadOnlyList<LogLink> ForService(params (string Label, string App, string? Flow)[] links)
    {
        if (!Enabled)
        {
            return [];
        }

        //The last hour, newest first: what the service has been doing lately.
        var serviceLinks = new List<LogLink>();
        foreach (var link in links)
        {
            var labelFilters = new List<string> { $"app|=|{link.App}" };
            if (link.Flow is not null)
            {
                labelFilters.Add($"{FantasyCriticLogging.FlowProperty}|=|{link.Flow}");
            }

            serviceLinks.Add(new LogLink(link.Label, BuildUrl(_lokiEnvironment, "now-1h", "now", labelFilters, null, "Descending")));
        }

        return serviceLinks;
    }

    private string BuildUrl(string lokiEnvironment, string from, string to, IReadOnlyList<string> labelFilters, string? fieldFilter, string sortOrder)
    {
        var parameters = new List<(string Name, string Value)>
        {
            ("patterns", "[]"),
            ("from", from),
            ("to", to),
            ("var-lineFormat", ""),
            ("var-ds", _options.DataSource),
            ("var-filters", $"env|=|{lokiEnvironment}")
        };
        parameters.AddRange(labelFilters.Select(x => ("var-filters", x)));
        parameters.AddRange(
        [
            ("var-fields", fieldFilter ?? ""),
            ("var-levels", ""),
            ("var-metadata", ""),
            ("var-jsonFields", ""),
            ("var-patterns", ""),
            ("var-lineFilterV2", ""),
            ("var-lineFilters", ""),
            ("displayedFields", "[\"Message\"]"),
            ("urlColumns", "[]"),
            ("visualizationType", "\"logs\""),
            ("timezone", "browser"),
            ("var-all-fields", fieldFilter ?? ""),
            ("userDisplayedFields", "false"),
            ("sortOrder", $"\"{sortOrder}\""),
            ("wrapLogMessage", "false"),
            ("prettifyLogMessage", "false")
        ]);

        var query = string.Join("&", parameters.Select(x => $"{x.Name}={Uri.EscapeDataString(x.Value)}"));
        var baseUrl = _options.Url.TrimEnd('/');
        return $"{baseUrl}/a/grafana-lokiexplore-app/explore/env/{Uri.EscapeDataString(lokiEnvironment)}/logs?{query}";
    }
}
