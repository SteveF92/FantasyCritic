using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Configuration;

namespace FantasyCritic.Worker;

public sealed record WorkerOptions : IHostOptions
{
    public required ConnectionStringsOptions ConnectionStrings { get; init; }
    public required AwsOptions Aws { get; init; }
    public required PostmarkOptions Postmark { get; init; }
    public required OpenCriticOptions OpenCritic { get; init; }
    public required WorkerAuthenticationOptions Authentication { get; init; }
    public required DiscordOptions Discord { get; init; }
    public required string BaseAddress { get; init; }
    public required GrafanaOptions Grafana { get; init; }

    public Result Validate(FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Section(nameof(ConnectionStrings), ConnectionStrings)
            .Section(nameof(Aws), Aws)
            .Section(nameof(Postmark), Postmark)
            .Section(nameof(OpenCritic), OpenCritic)
            .Section(nameof(Authentication), Authentication)
            .Section(nameof(Discord), Discord)
            .Value(nameof(BaseAddress), BaseAddress)
            .Section(nameof(Grafana), Grafana)
            .ToResult();
    }
}

/// <summary>
/// The worker's view of Authentication. It refreshes Plus roles from Patreon but logs nobody in, so it binds only the
/// Patreon API client's keys, at the same path Web reads them.
/// </summary>
public sealed record WorkerAuthenticationOptions : IOptionsSection
{
    public required PatreonOptions Patreon { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Section($"{path}:{nameof(Patreon)}", Patreon)
            .Paths;
    }
}
