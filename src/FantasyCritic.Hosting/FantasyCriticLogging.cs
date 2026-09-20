using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace FantasyCritic.Hosting;

public static class FantasyCriticLogging
{
    public const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";

    //For a process that runs several independent loops. Set through a log scope; events carrying it also get their own file and a Loki label.
    public const string FlowProperty = "Flow";

    /// <param name="consoleToStandardError">For a command line tool whose standard output is its answer, read by a script. Logs go to standard error instead.</param>
    public static LoggerConfiguration CreateConfiguration(LoggingPaths loggingPaths, LogEventLevel microsoftMinimumLevel, bool consoleToStandardError = false)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", microsoftMinimumLevel)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(standardErrorFromLevel: consoleToStandardError ? LogEventLevel.Verbose : null)
            .WriteTo.File(loggingPaths.AllLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3, outputTemplate: OutputTemplate)
            .WriteTo.File(loggingPaths.WarnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10, outputTemplate: OutputTemplate);
    }

    public static LoggerConfiguration WriteToApplicationLogFile(this LoggerConfiguration loggerConfiguration, LoggingPaths loggingPaths)
    {
        return loggerConfiguration.WriteTo.Logger(config =>
        {
            config.Filter
                .ByIncludingOnly(logEvent =>
                {
                    var sourceContext = logEvent.Properties.GetValueOrDefault("SourceContext");
                    var sourceContextString = sourceContext?.ToString();
                    return sourceContextString != null && sourceContextString.StartsWith("\"FantasyCritic");
                })
                .WriteTo.File(loggingPaths.MyLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5, outputTemplate: OutputTemplate);
        });
    }

    public static LoggerConfiguration WriteToFlowLogFiles(this LoggerConfiguration loggerConfiguration, LoggingPaths loggingPaths, IEnumerable<string> flows)
    {
        foreach (var flow in flows)
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Logger(config =>
            {
                config.Filter
                    .ByIncludingOnly(logEvent => logEvent.Properties.GetValueOrDefault(FlowProperty) is ScalarValue { Value: string eventFlow } && eventFlow == flow)
                    .WriteTo.File(loggingPaths.GetFlowLogPath(flow), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5, outputTemplate: OutputTemplate);
            });
        }

        return loggerConfiguration;
    }

    public static LoggerConfiguration WriteToGrafanaLoki(this LoggerConfiguration loggerConfiguration, IHostEnvironment environment, IConfiguration configuration)
    {
        var lokiUri = configuration["Grafana:Loki:Uri"];
        var lokiUserId = configuration["Grafana:Loki:UserId"];
        var lokiApiToken = configuration["Grafana:Loki:ApiToken"];
        if (string.IsNullOrWhiteSpace(lokiUri) ||
            string.IsNullOrWhiteSpace(lokiUserId) ||
            string.IsNullOrWhiteSpace(lokiApiToken))
        {
            return loggerConfiguration;
        }

        var labelSection = configuration.GetSection("Grafana:Loki:Labels");
        var lokiLabels = labelSection.GetChildren()
            .Where(c => !string.IsNullOrWhiteSpace(c.Key) && !string.IsNullOrWhiteSpace(c.Value))
            .Where(c => !string.Equals(c.Key, "env", StringComparison.OrdinalIgnoreCase))
            .Select(c => new LokiLabel { Key = c.Key, Value = c.Value! })
            .Append(new LokiLabel { Key = "env", Value = environment.EnvironmentName })
            .ToArray();

        return loggerConfiguration.WriteTo.GrafanaLoki(
            uri: lokiUri,
            credentials: new LokiCredentials
            {
                Login = lokiUserId,
                Password = lokiApiToken
            },
            labels: lokiLabels,
            //Only low-cardinality properties belong here: each distinct label value is its own Loki stream.
            //High-cardinality ones like JobID stay in the JSON line, queryable with | json.
            propertiesAsLabels: ["SourceContext", FlowProperty]);

    }
}
