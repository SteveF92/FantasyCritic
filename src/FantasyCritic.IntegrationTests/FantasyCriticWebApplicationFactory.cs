using System.Collections.Generic;
using System.IO;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.DapperTypeMaps;
using FantasyCritic.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace FantasyCritic.IntegrationTests;

public sealed class FantasyCriticWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Singleton capturing sender that stores confirmation-email bodies so tests can
    /// extract the confirmation link and call <see cref="ApiSession.ConfirmEmailAsync"/>.
    /// </summary>
    public CapturingEmailSender CapturingEmailSender { get; } = new CapturingEmailSender();
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

            // Hard overrides — added last so they always win over user secrets.
            // User secrets in Development mode can contain real credentials (e.g. a real
            // BotToken or a connection string pointing to beta RDS). These in-memory values
            // ensure tests only ever hit local infrastructure.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // "secret" is the sentinel value that HostingExtensions uses to skip
                // Discord bot registration (see HostingExtensions.cs:162).
                ["BotToken"] = "secret",
                // Always use local Docker MySQL, never any real database from user secrets.
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the Postmark sender with a capturing sender so tests can
            // extract confirmation links and follow them.
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(CapturingEmailSender);

            // Program.GetConfiguration() builds its own IConfigurationRoot (including user
            // secrets) and passes it directly into ConfigureServices. That means config values
            // like BotToken and the connection string may come from user secrets — pointing at
            // real infrastructure. We fix that here at the DI level.

            // Force the local Docker MySQL connection string, regardless of user secrets.
            services.RemoveAll<RepositoryConfiguration>();
            services.AddSingleton<RepositoryConfiguration>(_ => new RepositoryConfiguration(
                "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;",
                SystemClock.Instance));

            // Remove all IHostedService registrations. In Development mode the schedulers are
            // already not registered (gated by !IsDevelopment()), but the Discord bot may still
            // be registered if a real BotToken came in via user secrets.
            services.RemoveAll<IHostedService>();
        });
    }
}
