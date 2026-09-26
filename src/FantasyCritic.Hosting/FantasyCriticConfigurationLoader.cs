using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.AWS;
using FantasyCritic.Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FantasyCritic.Hosting;

public static class FantasyCriticConfigurationLoader
{
    private const string SecretAppName = "fantasyCritic";

    public static Task<IConfigurationRoot> Load(IHostEnvironment environment)
    {
        return Load(environment.EnvironmentName, environment.ContentRootPath);
    }

    /// <summary>
    /// appsettings.json, then user secrets, then outside Development the environment's secret store, then environment
    /// variables. Each wins over the ones before it.
    /// </summary>
    public static async Task<IConfigurationRoot> Load(string environmentName, string contentRootPath)
    {
        var builder = CreateLocalConfiguration(contentRootPath);

        if (FantasyCriticEnvironments.FromName(environmentName) != FantasyCriticEnvironment.Development)
        {
            var region = GetSecretStoreRegion(contentRootPath, environmentName);
            var awsStore = new SecretsManagerConfigurationStore(region, SecretAppName, GetSecretEnvironmentName(environmentName));
            var awsString = await awsStore.GetConfiguration();

            builder.AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(awsString)));
        }

        // Override JSON / AWS secrets (last wins) — required for Docker Compose and hosting env vars.
        builder.AddEnvironmentVariables();

        return builder.Build();
    }

    /// <summary>
    /// The secret store is found by region, so the region comes from everything but the store. That includes
    /// environment variables, which win here as they do in the full configuration, so the region compose sets is the
    /// one used.
    /// </summary>
    internal static string GetSecretStoreRegion(string contentRootPath, string environmentName)
    {
        var location = CreateLocalConfiguration(contentRootPath).AddEnvironmentVariables().Build().Get<SecretStoreLocation>();
        if (location is null)
        {
            throw new InvalidOperationException("The secret store cannot be found: no configuration was loaded.");
        }

        var validation = location.Validate(FantasyCriticEnvironments.FromName(environmentName));
        if (validation.IsFailure)
        {
            throw new InvalidOperationException($"The secret store cannot be found. {validation.Error}");
        }

        return location.Aws.Region;
    }

    private static IConfigurationBuilder CreateLocalConfiguration(string contentRootPath)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(contentRootPath)
            .AddJsonFile("appsettings.json");

        var userSecretsAssembly = Assembly.GetEntryAssembly();
        if (userSecretsAssembly is not null)
        {
            builder.AddUserSecrets(userSecretsAssembly, true);
        }

        return builder;
    }

    private static string GetSecretEnvironmentName(string environmentName)
        => string.Equals(environmentName, Environments.Staging, StringComparison.OrdinalIgnoreCase) ? "beta" : environmentName;

    /// <summary>
    /// What the loader needs from configuration before it can read the secret store.
    /// </summary>
    private sealed record SecretStoreLocation
    {
        public required AwsRegionOptions Aws { get; init; }

        public Result Validate(FantasyCriticEnvironment environment)
        {
            return new MissingConfiguration(environment)
                .Section(nameof(Aws), Aws)
                .ToResult();
        }
    }
}
