using System;
using System.Linq;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.Web.Utilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class JobLogLinksTests
{
    private static readonly Guid JobID = Guid.Parse("6c6b74d3-3fb7-4b81-aded-d73d68ca01dc");
    private static readonly Instant CreatedAt = Instant.FromUtc(2026, 9, 26, 19, 55, 23);
    private static readonly GrafanaLogsOptions Options = new() { Url = "https://fantasycritic.grafana.net/", DataSource = "grafanacloud-logs" };

    [Test]
    public void FinishedJob_LinksToItsJobIDInItsEnvironment_AroundItsRun()
    {
        var job = CreateJob(finishedAt: CreatedAt + Duration.FromMinutes(5));

        var url = new JobLogLinks(Options, "Staging").ForJob(job)!;
        var query = ParseQuery(url);

        Assert.Multiple(() =>
        {
            Assert.That(url, Does.StartWith("https://fantasycritic.grafana.net/a/grafana-lokiexplore-app/explore/env/Staging/logs?"));
            Assert.That(query.Where(x => x.Name == "var-filters").Select(x => x.Value), Is.EqualTo(new[] { "env|=|Staging", "app|=|fantasycritic-worker" }));
            Assert.That(query.Single(x => x.Name == "var-fields").Value,
                Is.EqualTo("JobID|=|{\"parser\":\"json\"__gfc__\"value\":\"6c6b74d3-3fb7-4b81-aded-d73d68ca01dc\"},6c6b74d3-3fb7-4b81-aded-d73d68ca01dc"));
            Assert.That(query.Single(x => x.Name == "var-ds").Value, Is.EqualTo("grafanacloud-logs"));
            Assert.That(query.Single(x => x.Name == "from").Value, Is.EqualTo((CreatedAt - Duration.FromMinutes(1)).ToUnixTimeMilliseconds().ToString()));
            Assert.That(query.Single(x => x.Name == "to").Value, Is.EqualTo((CreatedAt + Duration.FromMinutes(6)).ToUnixTimeMilliseconds().ToString()));
        });
    }

    [Test]
    public void UnfinishedJob_RunsToNow()
    {
        var url = new JobLogLinks(Options, "Production").ForJob(CreateJob(finishedAt: null))!;

        Assert.That(ParseQuery(url).Single(x => x.Name == "to").Value, Is.EqualTo("now"));
    }

    [Test]
    public void NoLokiEnvironment_HasNoLink()
    {
        Assert.That(new JobLogLinks(Options, null).ForJob(CreateJob(finishedAt: null)), Is.Null);
    }

    [Test]
    public void NoUrl_HasNoLink()
    {
        Assert.That(new JobLogLinks(Options with { Url = "" }, "Production").ForJob(CreateJob(finishedAt: null)), Is.Null);
    }

    private static (string Name, string Value)[] ParseQuery(string url)
    {
        return url.Split('?', 2)[1].Split('&')
            .Select(x => x.Split('=', 2))
            .Select(x => (x[0], Uri.UnescapeDataString(x[1])))
            .ToArray();
    }

    private static FantasyCriticJob CreateJob(Instant? finishedAt)
    {
        return new FantasyCriticJob(JobID, new FantasyCriticJobTypeWithRunType(FantasyCriticJobType.RefreshCaches, FantasyCriticJobRunType.Cron, FantasyCriticJobSeverity.Info), null,
            finishedAt.HasValue ? FantasyCriticJobStatus.Complete : FantasyCriticJobStatus.Running, null, null, null, CreatedAt, CreatedAt, finishedAt, null, null);
    }
}
