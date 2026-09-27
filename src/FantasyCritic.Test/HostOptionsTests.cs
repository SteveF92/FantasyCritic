using System;
using System.Collections.Generic;
using FantasyCritic.CommandLine;
using FantasyCritic.DatabaseUpdater;
using FantasyCritic.DiscordBot;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Web;
using FantasyCritic.Worker;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace FantasyCritic.Test;

/// <summary>
/// Each host's whole shipped appsettings, bound into its options and validated in every environment: exactly what each
/// host refuses to start without.
/// </summary>
[TestFixture]
public class HostOptionsTests
{
    private const string WebBeta = "Missing configuration: Aws:RdsInstanceName, Postmark:ApiKey, OpenCritic:ApiKey, Discord:BotToken";
    private const string WebProduction = "Missing configuration: Aws:RdsInstanceName, Postmark:ApiKey, OpenCritic:ApiKey, "
                                         + "Authentication:Google:ClientId, Authentication:Google:ClientSecret, "
                                         + "Authentication:Microsoft:ClientId, Authentication:Microsoft:ClientSecret, "
                                         + "Authentication:Twitch:ClientId, Authentication:Twitch:ClientSecret, "
                                         + "Authentication:Patreon:ClientId, Authentication:Patreon:CampaignId, Authentication:Patreon:ClientSecret, "
                                         + "Authentication:Discord:ClientId, Authentication:Discord:ClientSecret, Discord:BotToken";
    private const string WorkerBeta = "Missing configuration: Aws:RdsInstanceName, Postmark:ApiKey, OpenCritic:ApiKey, Discord:BotToken";
    private const string WorkerProduction = "Missing configuration: Aws:RdsInstanceName, Postmark:ApiKey, OpenCritic:ApiKey, "
                                            + "Authentication:Patreon:ClientId, Authentication:Patreon:CampaignId, Discord:BotToken";
    private const string BotWithoutAToken = "Missing configuration: Discord:BotToken";

    // One blob per environment serves every host, so it holds every value any of them needs.
    private static readonly Dictionary<string, string?> SharedSecret = new()
    {
        ["Aws:RdsInstanceName"] = "fantasy-critic-rds",
        ["Postmark:ApiKey"] = "a-postmark-key",
        ["OpenCritic:ApiKey"] = "an-opencritic-key",
        ["Authentication:Google:ClientId"] = "google-id",
        ["Authentication:Google:ClientSecret"] = "google-secret",
        ["Authentication:Microsoft:ClientId"] = "microsoft-id",
        ["Authentication:Microsoft:ClientSecret"] = "microsoft-secret",
        ["Authentication:Twitch:ClientId"] = "twitch-id",
        ["Authentication:Twitch:ClientSecret"] = "twitch-secret",
        ["Authentication:Patreon:ClientId"] = "patreon-id",
        ["Authentication:Patreon:ClientSecret"] = "patreon-secret",
        ["Authentication:Patreon:CampaignId"] = "12345",
        ["Authentication:Discord:ClientId"] = "discord-id",
        ["Authentication:Discord:ClientSecret"] = "discord-secret",
        ["Discord:BotToken"] = "a-bot-token",
    };

    /// <returns>Why the host would refuse to start, or null if it would start.</returns>
    private static string? Refusal(string host, FantasyCriticEnvironment environment, IReadOnlyDictionary<string, string?>? secret = null)
    {
        var configuration = ShippedAppSettings.Load(host, secret);
        return host switch
        {
            "web" => ErrorOrNull(configuration.GetValidOptions<WebOptions>(environment)),
            "worker" => ErrorOrNull(configuration.GetValidOptions<WorkerOptions>(environment)),
            "discordbot" => ErrorOrNull(configuration.GetValidOptions<DiscordBotOptions>(environment)),
            "commandline" => ErrorOrNull(configuration.GetValidOptions<CommandLineOptions>(environment)),
            "databaseupdater" => ErrorOrNull(configuration.GetValidOptions<DatabaseUpdaterOptions>(environment)),
            _ => throw new ArgumentOutOfRangeException(nameof(host), host, null),
        };
    }

    private static string? ErrorOrNull<T>(CSharpFunctionalExtensions.Result<T> result) => result.IsSuccess ? null : result.Error;

    [TestCase("web", FantasyCriticEnvironment.Development, null)]
    [TestCase("web", FantasyCriticEnvironment.Beta, WebBeta)]
    [TestCase("web", FantasyCriticEnvironment.Production, WebProduction)]
    [TestCase("worker", FantasyCriticEnvironment.Development, null)]
    [TestCase("worker", FantasyCriticEnvironment.Beta, WorkerBeta)]
    [TestCase("worker", FantasyCriticEnvironment.Production, WorkerProduction)]
    [TestCase("discordbot", FantasyCriticEnvironment.Development, BotWithoutAToken)]
    [TestCase("discordbot", FantasyCriticEnvironment.Beta, BotWithoutAToken)]
    [TestCase("discordbot", FantasyCriticEnvironment.Production, BotWithoutAToken)]
    [TestCase("commandline", FantasyCriticEnvironment.Development, null)]
    [TestCase("commandline", FantasyCriticEnvironment.Beta, null)]
    [TestCase("commandline", FantasyCriticEnvironment.Production, null)]
    [TestCase("databaseupdater", FantasyCriticEnvironment.Development, null)]
    [TestCase("databaseupdater", FantasyCriticEnvironment.Beta, null)]
    [TestCase("databaseupdater", FantasyCriticEnvironment.Production, null)]
    public void ShippedAppSettings_OnTheirOwn(string host, FantasyCriticEnvironment environment, string? expectedRefusal)
    {
        Assert.That(Refusal(host, environment), Is.EqualTo(expectedRefusal));
    }

    [TestCase("web")]
    [TestCase("worker")]
    [TestCase("discordbot")]
    [TestCase("commandline")]
    [TestCase("databaseupdater")]
    public void EveryHost_StartsInProduction_OnTheSharedSecret(string host)
    {
        Assert.That(Refusal(host, FantasyCriticEnvironment.Production, SharedSecret), Is.Null);
    }

    [Test]
    public void DiscordBot_NeedsARealToken_EvenInDevelopment()
    {
        var secret = new Dictionary<string, string?> { ["Discord:BotToken"] = "a-development-bot-token" };

        Assert.That(Refusal("discordbot", FantasyCriticEnvironment.Development, secret), Is.Null);
    }

    [Test]
    public void MissingSection_IsReportedByItsName()
    {
        // The worker's appsettings without its Grafana block.
        var withoutGrafana = new ConfigurationBuilder()
            .AddInMemoryCollection(ConfigurationWithout(ShippedAppSettings.Load("worker"), "Grafana"))
            .Build();

        var result = withoutGrafana.GetValidOptions<WorkerOptions>(FantasyCriticEnvironment.Development);

        Assert.That(result.Error, Is.EqualTo("Missing configuration: Grafana"));
    }

    [Test]
    public void NoConfigurationAtAll_IsReported()
    {
        var result = new ConfigurationBuilder().Build().GetValidOptions<CommandLineOptions>(FantasyCriticEnvironment.Development);

        Assert.That(result.Error, Is.EqualTo("No configuration was loaded."));
    }

    [TestCase("Development", FantasyCriticEnvironment.Development)]
    [TestCase("development", FantasyCriticEnvironment.Development)]
    [TestCase("Staging", FantasyCriticEnvironment.Beta)]
    [TestCase("Production", FantasyCriticEnvironment.Production)]
    [TestCase("Prodution", FantasyCriticEnvironment.Production)]
    public void EnvironmentName_MapsStrictly_WhenItIsNotOneWeKnow(string environmentName, FantasyCriticEnvironment expected)
    {
        Assert.That(FantasyCriticEnvironments.FromName(environmentName), Is.EqualTo(expected));
    }

    private static IEnumerable<KeyValuePair<string, string?>> ConfigurationWithout(IConfiguration configuration, string section)
    {
        foreach (var pair in configuration.AsEnumerable())
        {
            if (!pair.Key.Equals(section, StringComparison.OrdinalIgnoreCase) && !pair.Key.StartsWith(section + ":", StringComparison.OrdinalIgnoreCase))
            {
                yield return pair;
            }
        }
    }
}
