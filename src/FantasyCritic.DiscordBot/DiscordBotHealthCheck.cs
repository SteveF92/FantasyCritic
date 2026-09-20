using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FantasyCritic.DiscordBot;

/// <summary>
/// Healthy means the gateway connection is up, which is what answering a slash command needs. Discord drops and
/// re-establishes that connection as a matter of routine, so Connecting is only Degraded (still a 200): it is the
/// compose healthcheck's retries that decide whether a reconnect has gone on too long.
/// </summary>
public sealed class DiscordBotHealthCheck : IHealthCheck
{
    private readonly DiscordSocketClient _client;

    public DiscordBotHealthCheck(DiscordSocketClient client)
    {
        _client = client;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Evaluate(_client.ConnectionState, _client.LoginState, _client.Latency, _client.Guilds.Count));
    }

    public static HealthCheckResult Evaluate(ConnectionState connectionState, LoginState loginState, int latencyMilliseconds, int guildCount)
    {
        var data = new Dictionary<string, object>
        {
            ["connectionState"] = connectionState.ToString(),
            ["loginState"] = loginState.ToString(),
            ["latencyMilliseconds"] = latencyMilliseconds.ToString(),
            ["guilds"] = guildCount.ToString()
        };

        return connectionState switch
        {
            ConnectionState.Connected => HealthCheckResult.Healthy("Connected to the Discord gateway.", data),
            ConnectionState.Connecting => HealthCheckResult.Degraded("Connecting to the Discord gateway.", data: data),
            _ => HealthCheckResult.Unhealthy($"Not connected to the Discord gateway ({connectionState}).", data: data)
        };
    }
}
