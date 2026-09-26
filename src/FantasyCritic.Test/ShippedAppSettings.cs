using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace FantasyCritic.Test;

/// <summary>
/// Each host's appsettings.json as it ships, linked into the test output by the project file.
/// </summary>
internal static class ShippedAppSettings
{
    /// <param name="secret">Layered on top, as the secret store is at runtime.</param>
    public static IConfiguration Load(string host, IReadOnlyDictionary<string, string?>? secret = null)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "Appsettings", $"{host}.json"));
        if (secret is not null)
        {
            builder.AddInMemoryCollection(secret);
        }

        return builder.Build();
    }
}
