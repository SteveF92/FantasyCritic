using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LocalDatabaseConnectionGuardTests
{
    private const string LocalConnectionString =
        "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    private const string BetaConnectionString =
        "Server=example-beta-db.abc123.us-east-1.rds.amazonaws.com;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;SslMode=Required;charset=utf8;";

    private const string DumpConnectionString =
        "Server=example-prod-db.abc123.us-east-1.rds.amazonaws.com;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    [Test]
    public void ValidateForClean_AcceptsConfiguredLocalDockerConnection()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            LocalConnectionString,
            BetaConnectionString,
            DumpConnectionString);

        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("Server=127.0.0.1;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;")]
    [TestCase("Server=::1;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;")]
    public void ValidateForClean_AcceptsLocalhostVariants(string connectionString)
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            connectionString,
            BetaConnectionString,
            DumpConnectionString);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateForClean_RejectsRemoteConnectionString()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            BetaConnectionString,
            BetaConnectionString,
            DumpConnectionString);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("localhost").Or.Contain("remote"));
    }

    [Test]
    public void ValidateForClean_RejectsNonLocalPort()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            "Server=localhost;Port=3306;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;",
            BetaConnectionString,
            DumpConnectionString);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("3307"));
    }

    [Test]
    public void ValidateForClean_RejectsWhenLocalMatchesBetaConnectionString()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            LocalConnectionString,
            LocalConnectionString,
            DumpConnectionString);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("beta connection string"));
    }

    [Test]
    public void ValidateForClean_RejectsWhenLocalMatchesDumpConnectionString()
    {
        var result = LocalDatabaseConnectionGuard.ValidateForClean(
            LocalConnectionString,
            BetaConnectionString,
            LocalConnectionString);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("dump connection string"));
    }
}
