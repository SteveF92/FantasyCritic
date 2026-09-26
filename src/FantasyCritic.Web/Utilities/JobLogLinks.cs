using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.Jobs;

namespace FantasyCritic.Web.Utilities;

/// <summary>
/// Links to a job's logs in Grafana's Logs Drilldown app, for the admin console.
/// </summary>
public class JobLogLinks
{
    //The worker's Loki "app" label, from its appsettings.
    private const string WorkerApp = "fantasycritic-worker";

    //Logs from just before and after the job's own timestamps, since those are written a moment apart from its log lines.
    private static readonly Duration Margin = Duration.FromMinutes(1);

    private readonly GrafanaLogsOptions _options;
    private readonly string? _lokiEnvironment;

    /// <param name="lokiEnvironment">The "env" label the worker's logs carry, or null where nothing is sent to Loki.</param>
    public JobLogLinks(GrafanaLogsOptions options, string? lokiEnvironment)
    {
        _options = options;
        _lokiEnvironment = lokiEnvironment;
    }

    public string? ForJob(FantasyCriticJob job)
    {
        if (_lokiEnvironment is null || string.IsNullOrWhiteSpace(_options.Url))
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

        var parameters = new List<(string Name, string Value)>
        {
            ("patterns", "[]"),
            ("from", from),
            ("to", to),
            ("var-lineFormat", ""),
            ("var-ds", _options.DataSource),
            ("var-filters", $"env|=|{_lokiEnvironment}"),
            ("var-filters", $"app|=|{WorkerApp}"),
            ("var-fields", jobIDFilter),
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
            ("var-all-fields", jobIDFilter),
            ("userDisplayedFields", "false"),
            //Oldest first, so a job reads from start to finish.
            ("sortOrder", "\"Ascending\""),
            ("wrapLogMessage", "false"),
            ("prettifyLogMessage", "false")
        };

        var query = string.Join("&", parameters.Select(x => $"{x.Name}={Uri.EscapeDataString(x.Value)}"));
        var baseUrl = _options.Url.TrimEnd('/');
        return $"{baseUrl}/a/grafana-lokiexplore-app/explore/env/{Uri.EscapeDataString(_lokiEnvironment)}/logs?{query}";
    }
}
