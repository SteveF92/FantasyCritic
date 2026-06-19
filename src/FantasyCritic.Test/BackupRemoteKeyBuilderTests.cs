using FantasyCritic.Lib.Utilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class BackupRemoteKeyBuilderTests
{
    [Test]
    public void Build_IncludesPrefixInstanceDateAndFileName()
    {
        var instant = Instant.FromUtc(2026, 6, 18, 15, 30, 0);
        var key = BackupRemoteKeyBuilder.Build("db-dumps/", "fantasy-critic-beta-rds", instant, "fantasy-critic-beta-rds-2026-06-18.sql.gz");
        Assert.That(key, Is.EqualTo("db-dumps/fantasy-critic-beta-rds/2026-06-18/fantasy-critic-beta-rds-2026-06-18.sql.gz"));
    }

    [Test]
    public void Build_NormalizesMissingTrailingSlashOnPrefix()
    {
        var instant = Instant.FromUtc(2026, 6, 18, 15, 30, 0);
        var key = BackupRemoteKeyBuilder.Build("db-dumps", "prod", instant, "prod.sql.gz");
        Assert.That(key, Is.EqualTo("db-dumps/prod/2026-06-18/prod.sql.gz"));
    }
}
