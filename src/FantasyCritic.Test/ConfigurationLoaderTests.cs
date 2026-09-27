using System;
using System.Collections.Generic;
using System.IO;
using FantasyCritic.Hosting;
using NUnit.Framework;

namespace FantasyCritic.Test;

/// <summary>
/// How the loader finds the secret store, and how hosts name their environment. These read the process's environment
/// variables, so each test sets the ones it needs and puts back whatever was there.
/// </summary>
[TestFixture]
public class ConfigurationLoaderTests
{
    private static readonly string[] VariablesTheseTestsSet = ["Aws__Region", "DOTNET_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT"];

    private readonly Dictionary<string, string?> _originalVariables = new();
    private string _contentRoot = null!;

    [SetUp]
    public void SaveVariablesAndMakeAContentRoot()
    {
        foreach (var variable in VariablesTheseTestsSet)
        {
            _originalVariables[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, null);
        }

        _contentRoot = Path.Combine(Path.GetTempPath(), $"fantasycritic-loader-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_contentRoot);
    }

    [TearDown]
    public void RestoreVariablesAndRemoveTheContentRoot()
    {
        foreach (var (variable, value) in _originalVariables)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }

        Directory.Delete(_contentRoot, recursive: true);
    }

    private void WriteAppSettings(string json)
    {
        File.WriteAllText(Path.Combine(_contentRoot, "appsettings.json"), json);
    }

    [Test]
    public void SecretStoreRegion_ComesFromAppSettings()
    {
        WriteAppSettings("""{ "Aws": { "Region": "us-east-1" } }""");

        Assert.That(FantasyCriticConfigurationLoader.GetSecretStoreRegion(_contentRoot, "Production"), Is.EqualTo("us-east-1"));
    }

    [Test]
    public void SecretStoreRegion_FromTheEnvironment_WinsOverAppSettings()
    {
        // As compose sets it. Before, the loader looked the region up without environment variables.
        WriteAppSettings("""{ "Aws": { "Region": "us-east-1" } }""");
        Environment.SetEnvironmentVariable("Aws__Region", "eu-west-2");

        Assert.That(FantasyCriticConfigurationLoader.GetSecretStoreRegion(_contentRoot, "Production"), Is.EqualTo("eu-west-2"));
    }

    [TestCase("""{ "Aws": { "Region": "" } }""", "Aws:Region")]
    [TestCase("""{ "Aws": { "Region": "secret" } }""", "Aws:Region")]
    // An empty block has no keys, so to configuration the section is not there at all.
    [TestCase("""{ "Aws": { } }""", "Aws")]
    public void MissingSecretStoreRegion_FailsNamingIt(string appSettings, string missingPath)
    {
        WriteAppSettings(appSettings);

        var exception = Assert.Throws<InvalidOperationException>(() => FantasyCriticConfigurationLoader.GetSecretStoreRegion(_contentRoot, "Production"));

        Assert.That(exception!.Message, Is.EqualTo($"The secret store cannot be found. Missing configuration: {missingPath}"));
    }

    [Test]
    public void EnvironmentName_PrefersDotnetEnvironment_AsWebApplicationCreateBuilderDoes()
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "FromDotnet");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "FromAspNetCore");

        Assert.That(FantasyCriticEnvironments.NameFromEnvironmentVariables(), Is.EqualTo("FromDotnet"));
    }

    [Test]
    public void EnvironmentName_FallsBackToAspNetCoreEnvironment_WhichIsWhatDeploymentsSet()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging");

        Assert.That(FantasyCriticEnvironments.NameFromEnvironmentVariables(), Is.EqualTo("Staging"));
    }

    [Test]
    public void EnvironmentName_IsProduction_WhenNeitherIsSet()
    {
        Assert.That(FantasyCriticEnvironments.NameFromEnvironmentVariables(), Is.EqualTo("Production"));
    }
}
