using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Configuration;

/// <summary>
/// Web builds its services from configuration the factory cannot add to, so the factory changes the records Web
/// registered instead. These check that each change took, above all that a developer's real bot token, read from their
/// user secrets, never reaches Discord from a test run.
/// </summary>
[TestFixture]
public class TestFactoryConfigurationTests : IntegrationTestBase
{
    [Test]
    public void FactoryOverrides_ReachTheRecordsWebsServicesRead()
    {
        var discord = Factory.Services.GetRequiredService<FantasyCriticDiscordConfiguration>();
        var environment = Factory.Services.GetRequiredService<EnvironmentConfiguration>();
        var serviceHealth = Factory.Services.GetRequiredService<ServiceHealthOptions>();
        var repository = Factory.Services.GetRequiredService<RepositoryConfiguration>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(discord.BotToken, Is.EqualTo(MissingConfiguration.Placeholder));
            Assert.That(environment.IntegrationTestMode, Is.True);
            Assert.That(serviceHealth.WorkerUrl, Is.EqualTo("http://localhost:1"));
            Assert.That(serviceHealth.DiscordBotUrl, Is.EqualTo("http://localhost:1"));
            Assert.That(repository.ConnectionString, Does.Contain("Port=3307"));
        }
    }
}
