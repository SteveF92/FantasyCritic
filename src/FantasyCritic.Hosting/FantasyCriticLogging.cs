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


    public static LoggerConfiguration CreateConfiguration(LoggingPaths loggingPaths, LogEventLevel microsoftMinimumLevel)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", microsoftMinimumLevel)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
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
            propertiesAsLabels: ["SourceContext"]);

    }
}
