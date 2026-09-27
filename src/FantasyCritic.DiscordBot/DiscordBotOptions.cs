using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Configuration;

namespace FantasyCritic.DiscordBot;

public sealed record DiscordBotOptions : IHostOptions
{
    public required ConnectionStringsOptions ConnectionStrings { get; init; }
    public required AwsRegionOptions Aws { get; init; }
    public required DiscordOptions Discord { get; init; }
    public required string BaseAddress { get; init; }
    public required GrafanaOptions Grafana { get; init; }

    public Result Validate(FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Section(nameof(ConnectionStrings), ConnectionStrings)
            .Section(nameof(Aws), Aws)
            //Answering commands is this host's whole job, so unlike the others it needs a real token even in Development.
            .Value($"{nameof(Discord)}:{nameof(DiscordOptions.BotToken)}", Discord?.BotToken)
            .Value(nameof(BaseAddress), BaseAddress)
            .Section(nameof(Grafana), Grafana)
            .ToResult();
    }
}
