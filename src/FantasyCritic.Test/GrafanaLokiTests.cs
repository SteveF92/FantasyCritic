using System.Collections.Generic;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
using NUnit.Framework;
using Serilog;

namespace FantasyCritic.Test;

/// <summary>
/// Hosts build their Loki logger before validating their options, so that a host refusing to start says why in Loki.
/// The Grafana section can therefore be missing or half bound, and building the logger must not throw over it: that
/// would replace the message naming what is missing with a crash.
/// </summary>
[TestFixture]
public class GrafanaLokiTests
{
    private static IEnumerable<TestCaseData> UnvalidatedSections()
    {
        yield return new TestCaseData(null).SetName("No Grafana section");
        yield return new TestCaseData(new GrafanaOptions { Loki = null! }).SetName("No Loki section");
        yield return new TestCaseData(new GrafanaOptions
        {
            Loki = new LokiOptions { Uri = null!, UserId = "user", ApiToken = "token", Labels = null! }
        }).SetName("No Uri and no labels");
        yield return new TestCaseData(new GrafanaOptions
        {
            Loki = new LokiOptions { Uri = "https://loki.invalid", UserId = "user", ApiToken = "token", Labels = null! }
        }).SetName("Credentials but no labels");
        yield return new TestCaseData(new GrafanaOptions
        {
            Loki = new LokiOptions { Uri = "", UserId = "", ApiToken = "", Labels = new Dictionary<string, string> { ["app"] = "fantasycritic-test" } }
        }).SetName("Empty keys, as appsettings ships them");
    }

    [TestCaseSource(nameof(UnvalidatedSections))]
    public void UnvalidatedGrafanaSection_BuildsALoggerWithoutThrowing(GrafanaOptions? grafana)
    {
        Assert.DoesNotThrow(() =>
        {
            using var logger = new LoggerConfiguration().WriteToGrafanaLoki("Production", grafana).CreateLogger();
        });
    }
}
