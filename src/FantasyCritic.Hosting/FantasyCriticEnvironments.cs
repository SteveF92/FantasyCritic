using System;
using FantasyCritic.Lib.Configuration;
using Microsoft.Extensions.Hosting;

namespace FantasyCritic.Hosting;

public static class FantasyCriticEnvironments
{
    /// <summary>
    /// The environment a host runs in, for the hosts that set it themselves rather than leaving it to
    /// WebApplication.CreateBuilder(args). DOTNET_ENVIRONMENT wins, as it does there, so every host agrees.
    /// </summary>
    public static string NameFromEnvironmentVariables()
    {
        return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
               ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
               ?? Environments.Production;
    }

    /// <summary>
    /// ASP.NET's Staging is beta. Any other name but Development counts as production, as
    /// <see cref="FantasyCriticConfigurationLoader"/> treats it as deployed, so a misspelt name is validated strictly
    /// rather than leniently.
    /// </summary>
    public static FantasyCriticEnvironment FromName(string environmentName)
    {
        if (string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
        {
            return FantasyCriticEnvironment.Development;
        }

        if (string.Equals(environmentName, Environments.Staging, StringComparison.OrdinalIgnoreCase))
        {
            return FantasyCriticEnvironment.Beta;
        }

        return FantasyCriticEnvironment.Production;
    }

    public static FantasyCriticEnvironment GetFantasyCriticEnvironment(this IHostEnvironment environment)
    {
        return FromName(environment.EnvironmentName);
    }
}
