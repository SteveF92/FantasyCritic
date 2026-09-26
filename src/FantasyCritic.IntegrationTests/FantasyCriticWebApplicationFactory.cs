using System;
using System.Linq;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL.DapperTypeMaps;
using FantasyCritic.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
        // Development, so the configuration loader reads appsettings and user secrets but not the secret store.
        builder.UseEnvironment("Development");

        // Web builds its services from the configuration its loader reads, which a factory cannot add to. So
        // everything a test needs to differ, or that must never reach real infrastructure from a developer's user
        // secrets, is changed here instead, on the records Web registered.
        builder.ConfigureTestServices(services =>
        {
            // Replace IClock with a controllable fake so tests can advance time via the API.
            var adjustableClock = new AdjustableClock();
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(adjustableClock);

            // Replace the Postmark sender with a capturing sender so tests can
            // extract confirmation links and follow them.
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(CapturingEmailSender);

            // Always the local Docker MySQL, and on the same AdjustableClock so the repositories see the tests' time.
            ReplaceRegistered<RepositoryConfiguration>(services, _ => new RepositoryConfiguration(
                "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;",
                adjustableClock));

            // Opens the endpoints that move the AdjustableClock.
            ReplaceRegistered<EnvironmentConfiguration>(services, environment => environment with { IntegrationTestMode = true });

            // DiscordPushService stays off with the placeholder token, whatever token a developer's user secrets hold.
            ReplaceRegistered<FantasyCriticDiscordConfiguration>(services, discord => discord with { BotToken = MissingConfiguration.Placeholder });

            // The admin monitor asks the worker and the Discord bot for their health over HTTP, and
            // the appsettings defaults are the ports a developer's own worker and bot listen on.
            // Point both at a port nothing listens on, so the result does not depend on what else
            // happens to be running on this machine.
            ReplaceRegistered<ServiceHealthOptions>(services, serviceHealth => serviceHealth with
            {
                WorkerUrl = "http://localhost:1",
                DiscordBotUrl = "http://localhost:1"
            });

            // Tests run the jobs and clock changes they need themselves, so nothing runs in the background.
            services.RemoveAll<IHostedService>();

            // There is no worker here, so tests run the jobs they queue with JobTestHelpers.
            services.AddFantasyCriticJobHandlers();
        });
    }

    /// <summary>
    /// Swaps a record Web registered as an instance for a changed copy, leaving the rest of what Web configured as it was.
    /// </summary>
    private static void ReplaceRegistered<T>(IServiceCollection services, Func<T, T> change) where T : class
    {
        var registered = services.Single(descriptor => descriptor.ServiceType == typeof(T)).ImplementationInstance as T
                         ?? throw new InvalidOperationException($"{typeof(T).Name} is not registered as an instance, so there is nothing to change.");
        services.RemoveAll<T>();
        services.AddSingleton(change(registered));
    }
}
