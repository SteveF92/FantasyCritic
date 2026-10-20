using FantasyCritic.Web.Utilities;
using NUnit.Framework;

namespace FantasyCritic.ConfigurationTests;

[TestFixture]
public class GrafanaLogLinksAppLabelTests
{
    //The web app references neither host, so it repeats their labels. These say when it has fallen behind them.
    [TestCase("web", GrafanaLogLinks.WebApp)]
    [TestCase("worker", GrafanaLogLinks.WorkerApp)]
    [TestCase("discordbot", GrafanaLogLinks.DiscordBotApp)]
    public void AppLabels_MatchEachHostsAppsettings(string host, string app)
    {
        Assert.That(ShippedAppSettings.Load(host)["Grafana:Loki:Labels:app"], Is.EqualTo(app));
    }
}
