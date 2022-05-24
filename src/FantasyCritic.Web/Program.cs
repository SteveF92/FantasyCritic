using Microsoft.AspNetCore.Hosting;
using Dapper.NodaTime;
using FantasyCritic.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

string allLogPath = @"C:\FantasyCritic\Logs\log-all";
string myLogPath = @"C:\FantasyCritic\Logs\log-my";
string warnLogPath = @"C:\FantasyCritic\Logs\log-warning";
string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(allLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3, outputTemplate: outputTemplate)
    .WriteTo.File(warnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10, outputTemplate: outputTemplate)

    .WriteTo.Logger(config =>
    {
        config.Filter
            .ByExcluding(logEvent =>
            {
                if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
                {
                    var sourceContextString = sourceContext?.ToString();
                    if (sourceContextString is not null && sourceContextString.StartsWith("\"FantasyCritic"))
                    {
                        return false;
                    }
                }

                if (logEvent.Properties.TryGetValue("ActionName", out var actionName))
                {
                    var actionNameString = actionName?.ToString();
                    if (actionNameString is not null && actionNameString.StartsWith("\"FantasyCritic"))
                    {
                        return false;
                    }
                }

                return true;
            })
            .WriteTo.File(myLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5, outputTemplate: outputTemplate);
    })
    .CreateLogger();

try
{
    DapperNodaTimeSetup.Register();
    Log.Information("Starting web host");
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseIIS();
    builder.Host.UseSerilog();

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
