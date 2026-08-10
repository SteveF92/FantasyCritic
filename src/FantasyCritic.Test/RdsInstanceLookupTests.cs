using System.Collections.Generic;
using FantasyCritic.RdsSnapshotManager.Configuration;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RdsInstanceLookupTests
{
    private static Dictionary<string, RdsInstanceOptions> BuildInstances() => new()
    {
        ["production"] = new RdsInstanceOptions
        {
            InstanceName = "fantasy-critic-rds",
            ConnectionString = "Server=prod;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;",
            EnableWriteOperations = false,
            DefaultSnapshotSource = true
        },
        ["beta"] = new RdsInstanceOptions
        {
            InstanceName = "fantasy-critic-beta-rds",
            ConnectionString = "Server=beta;Database=fantasycritic;Uid=fantasycritic-admin;Pwd=secret;",
            EnableWriteOperations = true,
            DefaultSnapshotSource = false
        }
    };

    [Test]
    public void TryResolve_ReturnsInstanceForKnownKey()
    {
        var result = RdsInstanceLookup.TryResolve(BuildInstances(), "beta");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.InstanceName, Is.EqualTo("fantasy-critic-beta-rds"));
        }
    }

    [Test]
    public void TryResolve_FailsForUnknownKey()
    {
        var result = RdsInstanceLookup.TryResolve(BuildInstances(), "staging");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("staging"));
        }
    }

    [Test]
    public void TryResolveWriteEnabled_SucceedsForWriteEnabledInstance()
    {
        var result = RdsInstanceLookup.TryResolveWriteEnabled(BuildInstances(), "beta");

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void TryResolveWriteEnabled_FailsForWriteDisabledInstance()
    {
        var result = RdsInstanceLookup.TryResolveWriteEnabled(BuildInstances(), "production");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("disabled"));
        }
    }

    [Test]
    public void TryResolveWriteEnabled_FailsForUnknownKey()
    {
        var result = RdsInstanceLookup.TryResolveWriteEnabled(BuildInstances(), "staging");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("staging"));
        }
    }

    [Test]
    public void GetDefaultSnapshotSource_ReturnsFlaggedInstance()
    {
        var defaultSource = RdsInstanceLookup.GetDefaultSnapshotSource(BuildInstances());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(defaultSource.InstanceName, Is.EqualTo("fantasy-critic-rds"));
        }
    }
}
