using System.IO;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.DapperTypeMaps;
using FantasyCritic.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FantasyCritic.IntegrationTests;

public sealed class FantasyCriticWebApplicationFactory : WebApplicationFactory<Program>
{
    // Called once before any instance is created — safe to call repeatedly (idempotent).
    static FantasyCriticWebApplicationFactory()
    {
        DapperNodaTimeSetup.SetupDapperNodaTimeMappings();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Development so Program.cs skips AWS Secrets Manager,
        // and HostingExtensions.cs skips scheduled background tasks.
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            // Load test-specific overrides on top of appsettings.json.
            // appsettings.Testing.json is copied to the test output dir by the csproj.
            var outputDir = Path.GetDirectoryName(
                typeof(FantasyCriticWebApplicationFactory).Assembly.Location)!;

            configBuilder.AddJsonFile(
                Path.Combine(outputDir, "appsettings.Testing.json"),
                optional: true,
                reloadOnChange: false);

            // Per-machine local overrides — gitignored, never committed.
            configBuilder.AddJsonFile(
                Path.Combine(outputDir, "appsettings.Testing.Local.json"),
                optional: true,
                reloadOnChange: false);
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the Postmark sender with a no-op so registration doesn't make
            // outbound HTTP calls. Everything else (Discord, schedulers, RequireConfirmedAccount)
            // is already handled by the Development environment + appsettings.json defaults.
            services.RemoveAll<IEmailSender>();
            services.AddScoped<IEmailSender, NullEmailSender>();
        });
    }
}
