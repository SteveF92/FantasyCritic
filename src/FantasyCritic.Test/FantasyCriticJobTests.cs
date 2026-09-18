using System;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Jobs;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class FantasyCriticJobTests
{
    private static readonly Instant CreatedAt = Instant.FromUtc(2026, 9, 13, 0, 0, 0);
    private static readonly Instant ScheduledSlot = Instant.FromUtc(2026, 9, 13, 0, 0, 0);

    [TestCase("Cron", false, true)]
    [TestCase("ManualOrCron", true, true)]
    [TestCase("Manual", true, false)]
    [TestCase("Disabled", false, false)]
    public void AllowedByRunType_MatchesRunTypeForManualAndCronRuns(string runTypeName, bool manualAllowed, bool cronAllowed)
    {
        var runType = FantasyCriticJobRunType.FromValue(runTypeName);
        var user = new MinimalFantasyCriticUser(Guid.NewGuid(), "Admin", "admin@example.com");

        var manualJob = CreateJob(runType, user, scheduledFor: null);
        var cronJob = CreateJob(runType, createdByUser: null, ScheduledSlot);

        Assert.Multiple(() =>
        {
            Assert.That(manualJob.AllowedByRunType, Is.EqualTo(manualAllowed));
            Assert.That(cronJob.AllowedByRunType, Is.EqualTo(cronAllowed));
        });
    }

    [Test]
    public void ManualRunWithNoUser_IsNotTreatedAsCron()
    {
        var job = CreateJob(FantasyCriticJobRunType.Cron, createdByUser: null, scheduledFor: null);

        Assert.Multiple(() =>
        {
            Assert.That(job.IsCronRun, Is.False);
            Assert.That(job.AllowedByRunType, Is.False);
        });
    }

    private static FantasyCriticJob CreateJob(FantasyCriticJobRunType runType, IMinimalFantasyCriticUser? createdByUser, Instant? scheduledFor)
    {
        return new FantasyCriticJob(Guid.NewGuid(), new FantasyCriticJobTypeWithRunType(FantasyCriticJobType.ExpireTrades, runType, FantasyCriticJobSeverity.Info), createdByUser, FantasyCriticJobStatus.Queued,
            null, null, scheduledFor, CreatedAt, null, null);
    }
}
