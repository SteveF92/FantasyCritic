using Discord;
using FantasyCritic.DiscordBot;
using FantasyCritic.Lib.SharedSerialization.API;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class DiscordBotHealthCheckTests
{
    [TestCase(ConnectionState.Connected, HealthStatus.Healthy)]
    [TestCase(ConnectionState.Connecting, HealthStatus.Degraded)]
    [TestCase(ConnectionState.Disconnecting, HealthStatus.Unhealthy)]
    [TestCase(ConnectionState.Disconnected, HealthStatus.Unhealthy)]
    public void Status_FollowsTheGatewayConnection(ConnectionState connectionState, HealthStatus expectedStatus)
    {
        var result = DiscordBotHealthCheck.Evaluate(connectionState, LoginState.LoggedIn, 40, 3);

        Assert.That(result.Status, Is.EqualTo(expectedStatus));
    }

    [Test]
    public void Data_ReportsTheClientState()
    {
        var result = DiscordBotHealthCheck.Evaluate(ConnectionState.Connected, LoginState.LoggedIn, 40, 3);

        Assert.That(result.Data.Values, Is.EqualTo(new[]
        {
            ServiceHealthDetail.FromText("Connection", "Connected"),
            ServiceHealthDetail.FromText("Login", "LoggedIn"),
            ServiceHealthDetail.FromText("Latency", "40 ms"),
            ServiceHealthDetail.FromText("Servers", "3")
        }));
    }
}
