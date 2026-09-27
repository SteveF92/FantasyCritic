using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Hosting;
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

    /// <summary>
    /// The logger a host starts with, before its configuration is loaded. Anything that takes a logger from it before
    /// <see cref="ReplaceBootstrapLogger"/> — a static Log.ForContext field, or an ILogger the framework creates — follows
    /// it to the replacement, rather than writing to a closed logger.
    /// </summary>
    public static void UseBootstrapLogger(LoggerConfiguration configuration)
    {
        Log.Logger = configuration.CreateBootstrapLogger();
    }

    /// <summary>
    /// Replaces the bootstrap logger once configuration is loaded. The old one is closed before the new one is built:
    /// while it holds the log files open, the rolling file sink would start new files beside them.
    /// </summary>
    public static void ReplaceBootstrapLogger(Func<LoggerConfiguration> configure)
    {
        if (Log.Logger is not ReloadableLogger bootstrapLogger)
        {
            throw new InvalidOperationException($"{nameof(ReplaceBootstrapLogger)} needs the logger {nameof(UseBootstrapLogger)} creates.");
        }

        bootstrapLogger.Reload(_ => configure());
        Log.Logger = bootstrapLogger.Freeze();
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

    /// <param name="grafana">
    /// Bound but not yet validated: hosts build this logger before validating their options, so that a host refusing
    /// to start says why in Loki too. Any of it may be missing, and then Loki stays off. Empty keys turn it off as well.
    /// </param>
    public static LoggerConfiguration WriteToGrafanaLoki(this LoggerConfiguration loggerConfiguration, string environmentName, GrafanaOptions? grafana)
    {
        var loki = grafana?.Loki;
        if (loki is null ||
            string.IsNullOrWhiteSpace(loki.Uri) ||
            string.IsNullOrWhiteSpace(loki.UserId) ||
            string.IsNullOrWhiteSpace(loki.ApiToken))
        {
            return loggerConfiguration;
        }

        var lokiLabels = (loki.Labels ?? new Dictionary<string, string>())
            .Where(label => !string.IsNullOrWhiteSpace(label.Key) && !string.IsNullOrWhiteSpace(label.Value))
            .Where(label => !string.Equals(label.Key, "env", StringComparison.OrdinalIgnoreCase))
            .Select(label => new LokiLabel { Key = label.Key, Value = label.Value })
            .Append(new LokiLabel { Key = "env", Value = environmentName })
            .ToArray();

        return loggerConfiguration.WriteTo.GrafanaLoki(
            uri: loki.Uri,
            credentials: new LokiCredentials
            {
                Login = loki.UserId,
                Password = loki.ApiToken
            },
            labels: lokiLabels,
            //Only low-cardinality properties belong here: each distinct label value is its own Loki stream.
            //High-cardinality ones like JobID stay in the JSON line, queryable with | json.
            propertiesAsLabels: ["SourceContext", FlowProperty]);

    }
}
