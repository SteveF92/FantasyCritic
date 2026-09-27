using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Configuration;
using Microsoft.Extensions.Configuration;

namespace FantasyCritic.Hosting;

public static class HostOptionsExtensions
{
    /// <summary>
    /// Validates a host's bound options, so a host that cannot start fails naming every key it is missing. Hosts bind
    /// and validate in two steps, building their logger from the bound options in between, so that a failure here
    /// reaches Loki as well.
    /// </summary>
    public static Result<T> ToValidOptions<T>(this T? options, FantasyCriticEnvironment environment) where T : class, IHostOptions
    {
        if (options is null)
        {
            return Result.Failure<T>("No configuration was loaded.");
        }

        return options.Validate(environment).Map(() => options);
    }

    public static Result<T> GetValidOptions<T>(this IConfiguration configuration, FantasyCriticEnvironment environment) where T : class, IHostOptions
    {
        return configuration.Get<T>().ToValidOptions(environment);
    }
}
