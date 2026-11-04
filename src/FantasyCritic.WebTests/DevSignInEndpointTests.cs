using FantasyCritic.Web.Development;
using NUnit.Framework;

namespace FantasyCritic.WebTests;

[TestFixture]
public class DevSignInEndpointTests
{
    private const string DockerDatabase = "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;";

    [TestCase("Development", "Server=localhost;Port=3307;Database=fantasycritic;")]
    [TestCase("Development", "Server=127.0.0.1;Port=3306;Database=fantasycritic;")]
    [TestCase("Development", "Server=::1;Port=3306;Database=fantasycritic;")]
    [TestCase("development", "Server=LOCALHOST;Port=3307;Database=fantasycritic;")]
    public void LocalDatabaseInDevelopment_IsAllowed(string environmentName, string connectionString)
    {
        Assert.That(DevSignInEndpoint.IsAllowed(environmentName, connectionString), Is.True);
    }

    [TestCase("Development", "Server=mysql;Port=3306;Database=fantasycritic;")]
    [TestCase("Development", "Server=fantasycritic.abc123.us-east-1.rds.amazonaws.com;Port=3306;Database=fantasycritic;")]
    [TestCase("Development", "Server=localhost,fantasycritic.abc123.us-east-1.rds.amazonaws.com;Port=3306;Database=fantasycritic;")]
    [TestCase("Development", "Port=3306;Database=fantasycritic;")]
    public void NonLocalDatabaseInDevelopment_IsRefused(string environmentName, string connectionString)
    {
        Assert.That(DevSignInEndpoint.IsAllowed(environmentName, connectionString), Is.False);
    }

    [TestCase("Production")]
    [TestCase("Staging")]
    [TestCase("")]
    public void LocalDatabaseOutsideDevelopment_IsRefused(string environmentName)
    {
        Assert.That(DevSignInEndpoint.IsAllowed(environmentName, DockerDatabase), Is.False);
    }
}
