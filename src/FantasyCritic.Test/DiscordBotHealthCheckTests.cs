using Discord;
using FantasyCritic.DiscordBot;
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Data["connectionState"], Is.EqualTo("Connected"));
            Assert.That(result.Data["loginState"], Is.EqualTo("LoggedIn"));
            Assert.That(result.Data["latencyMilliseconds"], Is.EqualTo("40"));
            Assert.That(result.Data["guilds"], Is.EqualTo("3"));
        });
    }
}
