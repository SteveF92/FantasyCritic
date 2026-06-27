using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class DatabaseEmptyCheckerTests
{
    [Test]
    public void EmptyDatabaseQuery_TargetsConfiguredSchema()
    {
        Assert.That(DatabaseEmptyChecker.BuildTableCountQuery("fantasycritic"), Does.Contain("fantasycritic"));
    }

    [Test]
    public void CreateDatabaseQuery_UsesIfNotExists()
    {
        Assert.That(
            DatabaseEmptyChecker.BuildCreateDatabaseQuery("fantasycritic-fromsnapshot"),
            Is.EqualTo("CREATE DATABASE IF NOT EXISTS `fantasycritic-fromsnapshot`;"));
    }

    [Test]
    public void EnsureAppUserGrantsQuery_IncludesTempTablePermission()
    {
        Assert.That(
            DatabaseEmptyChecker.BuildEnsureAppUserGrantsQuery("fantasycritic-fromsnapshot"),
            Does.Contain("CREATE TEMPORARY TABLES")
            .And.Contain("fantasycritic-fromsnapshot")
            .And.Contain("'fantasycritic'@'%'"));
    }
}
