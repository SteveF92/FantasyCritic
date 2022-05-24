using Microsoft.AspNetCore.Hosting;
using Dapper.NodaTime;
using FantasyCritic.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

string allLogPath = @"C:\FantasyCritic\Logs\log-all";
string warnLogPath = @"C:\FantasyCritic\Logs\log-warning";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(allLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
    .WriteTo.File(warnLogPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning, retainedFileCountLimit: 10)
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
