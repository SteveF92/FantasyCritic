using System.Collections.Generic;
using FantasyCritic.Lib.DependencyInjection;
using NUnit.Framework;
using Serilog.Events;
using Serilog;

namespace FantasyCritic.Test;
[SetUpFixture]
internal class Startup
{
    private const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";

    [OneTimeSetUp]
    public void Setup()
    {
        var loggingPaths = new LoggingPaths();
        ConfigureLogging(loggingPaths);
    }

    private static void ConfigureLogging(LoggingPaths loggingPaths)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(loggingPaths.AllLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3, outputTemplate: outputTemplate)
            .WriteTo.File(loggingPaths.WarnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10, outputTemplate: outputTemplate)
            .WriteTo.Logger(config =>
            {
                config.Filter
                    .ByIncludingOnly(logEvent =>
                    {
                        var sourceContext = logEvent.Properties.GetValueOrDefault("SourceContext");
                        var sourceContextString = sourceContext?.ToString();
                        return sourceContextString != null && sourceContextString.StartsWith("\"FantasyCritic");
                    })
                    .WriteTo.File(loggingPaths.MyLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5, outputTemplate: outputTemplate);
            })
            .CreateLogger();
    }
}
