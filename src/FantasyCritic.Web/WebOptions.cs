using FantasyCritic.Lib.Configuration;

namespace FantasyCritic.Web;

public sealed record WebOptions : IHostOptions
{
    public required ConnectionStringsOptions ConnectionStrings { get; init; }
    public required AwsOptions Aws { get; init; }
    public required PostmarkOptions Postmark { get; init; }
    public required OpenCriticOptions OpenCritic { get; init; }
    public required AuthenticationOptions Authentication { get; init; }
    public required DiscordOptions Discord { get; init; }
    public required string BaseAddress { get; init; }
    public required ServiceHealthOptions ServiceHealth { get; init; }
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
            .Section(nameof(ServiceHealth), ServiceHealth)
            .Section(nameof(Grafana), Grafana)
            .ToResult();
    }
}
