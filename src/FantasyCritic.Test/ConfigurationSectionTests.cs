using System.Collections.Generic;
using FantasyCritic.Lib.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace FantasyCritic.Test;

/// <summary>
/// The configuration sections and the rules for when a value counts as missing. The binding tests read each host's
/// shipped appsettings, with a secret layered on top where a test needs one, as the secret store is at runtime.
/// </summary>
[TestFixture]
public class ConfigurationSectionTests
{
    private static T? Bind<T>(string host, string section, IReadOnlyDictionary<string, string?>? secret = null) where T : class
    {
        return ShippedAppSettings.Load(host, secret).GetSection(section).Get<T>();
    }

    [TestCase(FantasyCriticEnvironment.Development, FantasyCriticEnvironment.Development, true)]
    [TestCase(FantasyCriticEnvironment.Development, FantasyCriticEnvironment.Beta, false)]
    [TestCase(FantasyCriticEnvironment.Development, FantasyCriticEnvironment.Production, false)]
    [TestCase(FantasyCriticEnvironment.Beta, FantasyCriticEnvironment.Development, true)]
    [TestCase(FantasyCriticEnvironment.Beta, FantasyCriticEnvironment.Beta, true)]
    [TestCase(FantasyCriticEnvironment.Beta, FantasyCriticEnvironment.Production, false)]
    [TestCase(FantasyCriticEnvironment.Production, FantasyCriticEnvironment.Development, true)]
    [TestCase(FantasyCriticEnvironment.Production, FantasyCriticEnvironment.Beta, true)]
    [TestCase(FantasyCriticEnvironment.Production, FantasyCriticEnvironment.Production, true)]
    public void Placeholder_IsMissingFromTheEnvironmentTheKeyNamesOnward(FantasyCriticEnvironment environment, FantasyCriticEnvironment requiredFrom, bool missing)
    {
        var paths = new MissingConfiguration(environment).Value("Some:Key", MissingConfiguration.Placeholder, requiredFrom).Paths;

        Assert.That(paths, missing ? Is.EqualTo(new[] { "Some:Key" }) : Is.Empty);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void BlankValue_IsMissing_EvenBeforeItIsRequired(string? value)
    {
        var paths = new MissingConfiguration(FantasyCriticEnvironment.Development)
            .Value("Some:Key", value, FantasyCriticEnvironment.Production)
            .Paths;

        Assert.That(paths, Is.EqualTo(new[] { "Some:Key" }));
    }

    [Test]
    public void RealValue_IsNeverMissing_AndThePlaceholderIsMatchedInAnyCase()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new MissingConfiguration(FantasyCriticEnvironment.Production)
                .Value("Some:Key", "a-real-key", FantasyCriticEnvironment.Production).Paths, Is.Empty);
            Assert.That(new MissingConfiguration(FantasyCriticEnvironment.Production)
                .Value("Some:Key", "SECRET", FantasyCriticEnvironment.Production).Paths, Is.EqualTo(new[] { "Some:Key" }));
        }
    }

    [Test]
    public void Present_AcceptsEmpty_ButNotUnbound()
    {
        var paths = new MissingConfiguration(FantasyCriticEnvironment.Production)
            .Present("Empty", "")
            .Present("Unbound", null)
            .Paths;

        Assert.That(paths, Is.EqualTo(new[] { "Unbound" }));
    }

    [Test]
    public void SectionThatDidNotBind_IsReportedOnce_ByItsOwnPath()
    {
        // The worker has no ServiceHealth block, so the section binds to nothing at all.
        var serviceHealth = Bind<ServiceHealthOptions>("worker", "ServiceHealth");

        var paths = new MissingConfiguration(FantasyCriticEnvironment.Development).Section("ServiceHealth", serviceHealth).Paths;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serviceHealth, Is.Null);
            Assert.That(paths, Is.EqualTo(new[] { "ServiceHealth" }));
        }
    }

    [Test]
    public void ToResult_NamesEveryMissingPathAtOnce()
    {
        var failure = new MissingConfiguration(FantasyCriticEnvironment.Production)
            .Value("First", null)
            .Value("Fine", "a-real-value")
            .Value("Second", MissingConfiguration.Placeholder)
            .ToResult();
        var success = new MissingConfiguration(FantasyCriticEnvironment.Production)
            .Value("Fine", "a-real-value")
            .ToResult();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(failure.IsFailure, Is.True);
            Assert.That(failure.Error, Is.EqualTo("Missing configuration: First, Second"));
            Assert.That(success.IsSuccess, Is.True);
        }
    }

    [TestCase("web")]
    [TestCase("worker")]
    [TestCase("discordbot")]
    [TestCase("commandline")]
    public void DefaultConnection_BindsFromEveryHostThatHasOne(string host)
    {
        var connectionStrings = Bind<ConnectionStringsOptions>(host, "ConnectionStrings")!;

        Assert.That(connectionStrings.Validate("ConnectionStrings", FantasyCriticEnvironment.Production), Is.Empty);
    }

    [Test]
    public void AdminConnection_BindsFromTheMigrator()
    {
        var connectionStrings = Bind<AdminConnectionStringsOptions>("databaseupdater", "ConnectionStrings")!;

        Assert.That(connectionStrings.Validate("ConnectionStrings", FantasyCriticEnvironment.Production), Is.Empty);
    }

    [TestCase("web")]
    [TestCase("worker")]
    [TestCase("discordbot")]
    [TestCase("commandline")]
    [TestCase("databaseupdater")]
    public void RegionAndGrafana_BindFromEveryHost_AndKeepTheLokiLabelAsWritten(string host)
    {
        var aws = Bind<AwsRegionOptions>(host, "Aws")!;
        var grafana = Bind<GrafanaOptions>(host, "Grafana")!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(aws.Validate("Aws", FantasyCriticEnvironment.Production), Is.Empty);
            Assert.That(grafana.Validate("Grafana", FantasyCriticEnvironment.Production), Is.Empty);
            Assert.That(grafana.Loki.Uri, Is.Empty);
            Assert.That(grafana.Loki.Labels, Is.EqualTo(new Dictionary<string, string> { ["app"] = $"fantasycritic-{host}" }));
        }
    }

    [Test]
    public void LokiKeys_MayBeEmpty_ButMustBeBound()
    {
        var loki = new LokiOptions { Uri = null!, UserId = "", ApiToken = "", Labels = null! };

        Assert.That(loki.Validate("Grafana:Loki", FantasyCriticEnvironment.Production), Is.EqualTo(new[] { "Grafana:Loki:Uri", "Grafana:Loki:Labels" }));
    }

    [TestCase("web")]
    [TestCase("worker")]
    public void ServicePlaceholders_AreFineInDevelopment_AndMissingFromBeta(string host)
    {
        var aws = Bind<AwsOptions>(host, "Aws")!;
        var postmark = Bind<PostmarkOptions>(host, "Postmark")!;
        var openCritic = Bind<OpenCriticOptions>(host, "OpenCritic")!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(aws.Validate("Aws", FantasyCriticEnvironment.Development), Is.Empty);
            Assert.That(postmark.Validate("Postmark", FantasyCriticEnvironment.Development), Is.Empty);
            Assert.That(openCritic.Validate("OpenCritic", FantasyCriticEnvironment.Development), Is.Empty);

            Assert.That(aws.Validate("Aws", FantasyCriticEnvironment.Beta), Is.EqualTo(new[] { "Aws:RdsInstanceName" }));
            Assert.That(postmark.Validate("Postmark", FantasyCriticEnvironment.Beta), Is.EqualTo(new[] { "Postmark:ApiKey" }));
            // OpenCritic's placeholder used to be "key", which reads as a real value and would have let Beta start without one.
            Assert.That(openCritic.Validate("OpenCritic", FantasyCriticEnvironment.Beta), Is.EqualTo(new[] { "OpenCritic:ApiKey" }));
        }
    }

    [TestCase("web")]
    [TestCase("worker")]
    [TestCase("discordbot")]
    public void BotToken_IsFineInDevelopment_AndMissingFromBeta(string host)
    {
        var discord = Bind<DiscordOptions>(host, "Discord")!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(discord.Validate("Discord", FantasyCriticEnvironment.Development), Is.Empty);
            Assert.That(discord.Validate("Discord", FantasyCriticEnvironment.Beta), Is.EqualTo(new[] { "Discord:BotToken" }));
        }
    }

    [Test]
    public void WebAuthentication_IsFineUntilProduction_ThenNeedsEveryProvider()
    {
        var authentication = Bind<AuthenticationOptions>("web", "Authentication")!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(authentication.Validate("Authentication", FantasyCriticEnvironment.Development), Is.Empty);
            Assert.That(authentication.Validate("Authentication", FantasyCriticEnvironment.Beta), Is.Empty);
            Assert.That(authentication.Validate("Authentication", FantasyCriticEnvironment.Production), Is.EqualTo(new[]
            {
                "Authentication:Google:ClientId",
                "Authentication:Google:ClientSecret",
                "Authentication:Microsoft:ClientId",
                "Authentication:Microsoft:ClientSecret",
                "Authentication:Twitch:ClientId",
                "Authentication:Twitch:ClientSecret",
                "Authentication:Patreon:ClientId",
                "Authentication:Patreon:CampaignId",
                "Authentication:Patreon:ClientSecret",
                "Authentication:Discord:ClientId",
                "Authentication:Discord:ClientSecret",
            }));
        }
    }

    [Test]
    public void WorkerPatreon_BindsTheApiClientsKeys_FromTheSamePathAsWeb()
    {
        var patreon = Bind<PatreonOptions>("worker", "Authentication:Patreon")!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(patreon.Validate("Authentication:Patreon", FantasyCriticEnvironment.Beta), Is.Empty);
            Assert.That(patreon.Validate("Authentication:Patreon", FantasyCriticEnvironment.Production),
                Is.EqualTo(new[] { "Authentication:Patreon:ClientId", "Authentication:Patreon:CampaignId" }));
        }
    }

    [Test]
    public void SecretWithTheOldCasing_StillBindsToTheNewKeys()
    {
        // A secret written before the keys became PascalCase. Configuration keys are case-insensitive.
        var secret = new Dictionary<string, string?>
        {
            ["AWS:rdsInstanceName"] = "fantasy-critic-rds",
            ["opencritic:apikey"] = "a-real-key",
            ["DISCORD:BOTTOKEN"] = "a-real-token",
        };

        var aws = Bind<AwsOptions>("web", "Aws", secret)!;
        var openCritic = Bind<OpenCriticOptions>("web", "OpenCritic", secret)!;
        var discord = Bind<DiscordOptions>("web", "Discord", secret)!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(aws.RdsInstanceName, Is.EqualTo("fantasy-critic-rds"));
            Assert.That(aws.Validate("Aws", FantasyCriticEnvironment.Production), Is.Empty);
            Assert.That(openCritic.Validate("OpenCritic", FantasyCriticEnvironment.Production), Is.Empty);
            Assert.That(discord.Validate("Discord", FantasyCriticEnvironment.Production), Is.Empty);
        }
    }

    [Test]
    public void SecretWithTheCampaignIdAtAnOldPath_IsReportedUnderTheNewOne()
    {
        // The value is real but not where anything reads it, so the placeholder from appsettings wins.
        var secret = new Dictionary<string, string?>
        {
            ["Authentication:Patreon:ClientId"] = "a-real-client",
            ["Authentication:Patreon:ClientSecret"] = "a-real-secret",
            ["PatreonService:CampaignID"] = "12345",
            ["Patreon:CampaignID"] = "12345",
        };

        var patreon = Bind<PatreonAuthOptions>("web", "Authentication:Patreon", secret)!;

        Assert.That(patreon.Validate("Authentication:Patreon", FantasyCriticEnvironment.Production),
            Is.EqualTo(new[] { "Authentication:Patreon:CampaignId" }));
    }
}
