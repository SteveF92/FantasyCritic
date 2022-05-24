using Microsoft.AspNetCore.Hosting;
using Dapper.NodaTime;
using FantasyCritic.Web;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(@"C:\FantasyCritic\Logs\log-all", rollingInterval: RollingInterval.Day)
    .WriteTo.File(@"C:\FantasyCritic\Logs\log-warning", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning)
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
