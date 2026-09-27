using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace FantasyCritic.Test;

/// <summary>
/// The registration methods take typed sections, so passing the wrong section does not compile. These check the part the
/// compiler cannot: that each value lands in the record the services read it from. Nothing here is resolved beyond those
/// records, so no service is constructed.
/// </summary>
[TestFixture]
public class ServiceRegistrationTests
{
    [Test]
    public void Core_PutsTheConnectionStringTokenAndBaseAddressWhereServicesReadThem()
    {
        var services = new ServiceCollection();

        services.AddFantasyCriticCore(
            new ConnectionStringsOptions { DefaultConnection = "the-connection-string" },
            new DiscordOptions { BotToken = "the-bot-token" },
            "https://the-base-address",
            new TestHostEnvironment(Environments.Production));

        using var provider = services.BuildServiceProvider();
        var repository = provider.GetRequiredService<RepositoryConfiguration>();
        var discord = provider.GetRequiredService<FantasyCriticDiscordConfiguration>();
        var environment = provider.GetRequiredService<EnvironmentConfiguration>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(repository.ConnectionString, Is.EqualTo("the-connection-string"));
            Assert.That(discord.BotToken, Is.EqualTo("the-bot-token"));
            Assert.That(discord.BaseAddress, Is.EqualTo("https://the-base-address"));
            Assert.That(discord.IsDevelopment, Is.False);
            Assert.That(environment.BaseAddress, Is.EqualTo("https://the-base-address"));
            Assert.That(environment.IsProduction, Is.True);
            Assert.That(environment.IntegrationTestMode, Is.False, "only the integration tests turn it on, by overriding this record");
        }
    }

    [Test]
    public void AdminServices_RegisterWebsPatreonLoginOptions_AsThePatreonOptionsTheServiceTakes()
    {
        var services = new ServiceCollection();
        var patreon = new PatreonAuthOptions { ClientId = "the-client", ClientSecret = "the-secret", CampaignId = "12345" };

        services.AddFantasyCriticAdminServices(
            new AwsOptions { Region = "us-east-1", RdsInstanceName = "the-instance" },
            new OpenCriticOptions { ApiKey = "the-key" },
            patreon);

        using var provider = services.BuildServiceProvider();

        Assert.That(provider.GetRequiredService<PatreonOptions>(), Is.SameAs(patreon));
    }
}
