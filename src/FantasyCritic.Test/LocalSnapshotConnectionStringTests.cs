using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LocalSnapshotConnectionStringTests
{
    private const string LocalDockerConnectionString =
        "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;SslMode=Required;charset=utf8;";

    [Test]
    public void BuildSnapshotConnectionString_OverridesDatabaseName()
    {
        string result = LocalSnapshotConnectionString.BuildSnapshotConnectionString(LocalDockerConnectionString);

        Assert.That(result, Does.Contain($"Database={LocalSnapshotDatabaseNames.SnapshotDatabase}"));
        Assert.That(result, Does.Not.Contain("Database=fantasycritic;"));
    }

    [Test]
    public void BuildSnapshotConnectionString_PreservesHostPortAndCredentials()
    {
        string result = LocalSnapshotConnectionString.BuildSnapshotConnectionString(LocalDockerConnectionString);

        Assert.That(result, Does.Contain("Server=localhost"));
        Assert.That(result, Does.Contain("Port=3307"));
        Assert.That(result, Does.Contain("Uid=fantasycritic-admin"));
    }
}
