using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Configuration;

namespace FantasyCritic.CommandLine;

public sealed record CommandLineOptions : IHostOptions
{
    public required ConnectionStringsOptions ConnectionStrings { get; init; }
    public required AwsRegionOptions Aws { get; init; }
    public required GrafanaOptions Grafana { get; init; }

    public Result Validate(FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Section(nameof(ConnectionStrings), ConnectionStrings)
            .Section(nameof(Aws), Aws)
            .Section(nameof(Grafana), Grafana)
            .ToResult();
    }
}
