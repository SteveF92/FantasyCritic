using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FantasyCritic.Lib;
using FantasyCritic.Lib.SharedSerialization.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FantasyCritic.Hosting;

public static class HealthEndpointExtensions
{
    /// <summary>
    /// GET /health for the processes that serve nothing else. Runs every registered check and answers 200 for Healthy or
    /// Degraded and 503 for Unhealthy, which is what `curl -f` in the compose healthcheck keys off.
    /// </summary>
    public static void MapFantasyCriticHealth(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteReport
        });
    }

    private static Task WriteReport(HttpContext context, HealthReport report)
    {
        var description = report.Entries.Values.Select(x => x.Description).FirstOrDefault(x => x is not null);
        var data = new Dictionary<string, string>();
        foreach (var entry in report.Entries.Values)
        {
            foreach (var item in entry.Data)
            {
                data[item.Key] = item.Value.ToString() ?? "";
            }
        }

        var serviceHealthReport = new ServiceHealthReport(report.Status.ToString(), description, data);
        context.Response.ContentType = "application/json";
        return JsonSerializer.SerializeAsync(context.Response.Body, serviceHealthReport, FantasyCriticJsonOptions.Default);
    }
}
