using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using Dapper.NodaTime;
using FantasyCritic.Web;
using Microsoft.AspNetCore.Builder;
using NLog;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("init main");
    DapperNodaTimeSetup.Register();

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseNLog();
    builder.WebHost.UseIIS();

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException") // https://github.com/dotnet/runtime/issues/60600
{
    logger.Fatal(ex, "Unhandled exception");
}
finally
{
    logger.Info("Shut down complete");
    LogManager.Shutdown();
}
