using FantasyCritic.Lib.Utilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class DatabaseSnapshotNamesTests
{
    //Saturday 2026-09-19 8:00:05pm Eastern (EDT), which is already Sunday in UTC.
    private static readonly Instant SaturdayEveningEastern = Instant.FromUtc(2026, 9, 20, 0, 0, 5);

    [Test]
    public void Admin_UsesEasternDateAndTimeToTheSecond()
    {
        Assert.That(DatabaseSnapshotNames.Admin(SaturdayEveningEastern), Is.EqualTo("admin-snap-2026-09-19-200005"));
    }

    [Test]
    public void PreActionProcessing_UsesEasternDate()
    {
        Assert.That(DatabaseSnapshotNames.PreActionProcessing(SaturdayEveningEastern), Is.EqualTo("pre-action-processing-snap-2026-09-19"));
    }

    [Test]
    public void Names_AreValidRdsSnapshotIdentifiers()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RdsSnapshotIdentifierValidator.Validate(DatabaseSnapshotNames.Admin(SaturdayEveningEastern)).IsSuccess, Is.True);
            Assert.That(RdsSnapshotIdentifierValidator.Validate(DatabaseSnapshotNames.PreActionProcessing(SaturdayEveningEastern)).IsSuccess, Is.True);
        });
    }
}
