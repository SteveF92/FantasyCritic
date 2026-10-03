using FantasyCritic.Lib.Utilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class BackupRemoteKeyBuilderTests
{
    [Test]
    public void Build_IncludesInstanceDateAndFileName()
    {
        var instant = Instant.FromUtc(2026, 6, 18, 15, 30, 0);
        var key = BackupRemoteKeyBuilder.Build("fantasy-critic-beta-rds", instant, "fantasy-critic-beta-rds-2026-06-18.sql.gz");
        Assert.That(key, Is.EqualTo("fantasy-critic-beta-rds/2026-06-18/fantasy-critic-beta-rds-2026-06-18.sql.gz"));
    }

    [Test]
    public void Build_UsesTheEasternDate()
    {
        var instant = Instant.FromUtc(2026, 10, 4, 2, 0, 0);
        var key = BackupRemoteKeyBuilder.Build("prod", instant, "prod.sql.gz");
        Assert.That(key, Is.EqualTo("prod/2026-10-03/prod.sql.gz"));
    }

    [Test]
    public void Build_WithLocalDate_IncludesInstanceDateAndFileName()
    {
        var date = new LocalDate(2026, 6, 18);
        var key = BackupRemoteKeyBuilder.Build("fantasy-critic-beta-rds", date, "fantasy-critic-beta-rds-2026-06-18.sql.gz");
        Assert.That(key, Is.EqualTo("fantasy-critic-beta-rds/2026-06-18/fantasy-critic-beta-rds-2026-06-18.sql.gz"));
    }

    [Test]
    public void BuildFileName_UsesTheEasternTimeToTheSecond()
    {
        var instant = Instant.FromUtc(2026, 10, 4, 7, 0, 5);
        var fileName = BackupRemoteKeyBuilder.BuildFileName("fantasy-critic-rds", instant);
        Assert.That(fileName, Is.EqualTo("fantasy-critic-rds-2026-10-04-030005.sql.gz"));
    }

    [Test]
    public void WithPrefix_PutsTheKeyUnderThePrefix()
    {
        var key = BackupRemoteKeyBuilder.WithPrefix("db-dumps/", "prod/2026-06-18/prod.sql.gz");
        Assert.That(key, Is.EqualTo("db-dumps/prod/2026-06-18/prod.sql.gz"));
    }

    [Test]
    public void WithPrefix_NormalizesMissingTrailingSlash()
    {
        var key = BackupRemoteKeyBuilder.WithPrefix("db-dumps", "prod/2026-06-18/prod.sql.gz");
        Assert.That(key, Is.EqualTo("db-dumps/prod/2026-06-18/prod.sql.gz"));
    }

    [Test]
    public void WithPrefix_EmptyPrefixLeavesTheKeyAlone()
    {
        var key = BackupRemoteKeyBuilder.WithPrefix("", "prod/2026-06-18/prod.sql.gz");
        Assert.That(key, Is.EqualTo("prod/2026-06-18/prod.sql.gz"));
    }
}
