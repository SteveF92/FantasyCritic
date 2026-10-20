using FantasyCritic.Web.Utilities;
using FantasyCritic.Worker;
using NUnit.Framework;

namespace FantasyCritic.ConfigurationTests;

/// <summary>
/// The web app references neither the worker nor the bot, so GrafanaLogLinks repeats their app labels and the worker's
/// flow names. These say when it has fallen behind them.
/// </summary>
[TestFixture]
public class GrafanaLogLinksHostTests
{
    [TestCase("web", GrafanaLogLinks.WebApp)]
    [TestCase("worker", GrafanaLogLinks.WorkerApp)]
    [TestCase("discordbot", GrafanaLogLinks.DiscordBotApp)]
    public void AppLabels_MatchEachHostsAppsettings(string host, string app)
    {
        Assert.That(ShippedAppSettings.Load(host)["Grafana:Loki:Labels:app"], Is.EqualTo(app));
    }

    [Test]
    public void WorkerFlows_MatchTheWorkers()
    {
        Assert.That(new[] { GrafanaLogLinks.JobRunnerFlow, GrafanaLogLinks.SchedulerFlow, GrafanaLogLinks.CancellerFlow }, Is.EquivalentTo(WorkerLogging.Flows));
    }
}
