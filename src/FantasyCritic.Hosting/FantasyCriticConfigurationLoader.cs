using System.Reflection;
using FantasyCritic.AWS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FantasyCritic.Hosting;

public static class FantasyCriticConfigurationLoader
{
    private const string SecretAppName = "fantasyCritic";

    public static async Task<IConfigurationRoot> Load(IHostEnvironment environment)
    {
        var userSecretsAssembly = Assembly.GetExecutingAssembly();
        var builder = new ConfigurationBuilder()
            .SetBasePath(environment.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(userSecretsAssembly, true);

        if (!environment.IsDevelopment())
        {
            var preliminaryConfig = builder.Build();
            var awsRegion = preliminaryConfig["AWS:region"]!;

            string fixedEnvironmentName = GetSecretEnvironmentName(environment.EnvironmentName);
            var awsStore = new SecretsManagerConfigurationStore(awsRegion, SecretAppName, fixedEnvironmentName);
            var awsString = await awsStore.GetConfiguration();

            builder.AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(awsString)));
        }

        // Override JSON / AWS secrets (last wins) — required for Docker Compose and hosting env vars.
        builder.AddEnvironmentVariables();

        return builder.Build();
    }

    private static string GetSecretEnvironmentName(string environmentName)
        => string.Equals(environmentName, "Staging", StringComparison.OrdinalIgnoreCase) ? "beta" : environmentName;
}
