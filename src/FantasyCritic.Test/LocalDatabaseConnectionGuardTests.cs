using System.Collections.Generic;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LocalDatabaseConnectionGuardTests
{
    private const string LocalConnectionString =
        "Server=localhost;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    private const string RemoteConnectionStringA =
        "Server=example-beta-db.abc123.us-east-1.rds.amazonaws.com;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;SslMode=Required;charset=utf8;";

    private const string RemoteConnectionStringB =
        "Server=example-prod-db.abc123.us-east-1.rds.amazonaws.com;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    private static readonly IReadOnlyList<string> RemoteConnectionStrings = [RemoteConnectionStringA, RemoteConnectionStringB];

    [Test]
    public void ValidateForClean_AcceptsConfiguredLocalDockerConnection()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, RemoteConnectionStrings);

        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("Server=127.0.0.1;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic;Pwd=secret;")]
    [TestCase("Server=::1;Port=3307;Database=fantasycritic-fromsnapshot;Uid=fantasycritic;Pwd=secret;")]
    public void ValidateForClean_AcceptsLocalhostVariants(string connectionString)
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(connectionString, RemoteConnectionStrings);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateForClean_RejectsRemoteConnectionString()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(RemoteConnectionStringA, RemoteConnectionStrings);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("localhost").Or.Contain("remote"));
        }
    }

    [Test]
    public void ValidateForClean_RejectsNonLocalPort()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            "Server=localhost;Port=3306;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;",
            RemoteConnectionStrings);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("3307"));
        }
    }

    [Test]
    public void ValidateForClean_RejectsWhenLocalMatchesAnyConfiguredRemoteConnectionString()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, [RemoteConnectionStringB, LocalConnectionString]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("remote RDS instance"));
        }
    }

    [Test]
    public void ValidateForClean_AcceptsWhenNoRemoteConnectionStringsConfigured()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, []);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateForClean_RejectsSeededDatabaseName()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;",
            RemoteConnectionStrings);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("fantasycritic"));
        }
        Assert.That(result.Error, Does.Contain("seeded").IgnoreCase);
    }

    [Test]
    public void ValidateForClean_AcceptsSnapshotDatabaseName()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(LocalConnectionString, RemoteConnectionStrings);

        Assert.That(result.IsSuccess, Is.True);
    }
}
