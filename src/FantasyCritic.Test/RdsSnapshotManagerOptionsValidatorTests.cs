using System.Collections.Generic;
using FantasyCritic.RdsSnapshotManager.Configuration;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RdsSnapshotManagerOptionsValidatorTests
{
    private static RdsInstanceOptions MakeInstance(bool enableWriteOperations = false, bool defaultSnapshotSource = false) => new()
    {
        InstanceName = "some-instance",
        ConnectionString = "Server=localhost;Database=fantasycritic;Uid=fantasycritic;Pwd=secret;",
        EnableWriteOperations = enableWriteOperations,
        DefaultSnapshotSource = defaultSnapshotSource
    };

    private static RdsSnapshotManagerOptions MakeOptions(Dictionary<string, RdsInstanceOptions> instances) => new()
    {
        RdsInstances = instances
    };

    [Test]
    public void Validate_AcceptsWellFormedConfiguration()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: false, defaultSnapshotSource: true),
            ["beta"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: false)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_RejectsEmptyInstanceDictionary()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>());

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("No RDS instances"));
        }
    }

    [Test]
    public void Validate_RejectsMissingInstanceName()
    {
        var instance = MakeInstance(defaultSnapshotSource: true, enableWriteOperations: true);
        instance.InstanceName = "";
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions> { ["production"] = instance });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("instanceName"));
        }
    }

    [Test]
    public void Validate_RejectsMissingConnectionString()
    {
        var instance = MakeInstance(defaultSnapshotSource: true, enableWriteOperations: true);
        instance.ConnectionString = "   ";
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions> { ["production"] = instance });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("connectionString"));
        }
    }

    [Test]
    public void Validate_RejectsZeroDefaultSnapshotSources()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: false)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("found 0"));
        }
    }

    [Test]
    public void Validate_RejectsMultipleDefaultSnapshotSources()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: true),
            ["beta"] = MakeInstance(enableWriteOperations: true, defaultSnapshotSource: true)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("found 2"));
        }
    }

    [Test]
    public void Validate_RejectsZeroWriteEnabledInstances()
    {
        var options = MakeOptions(new Dictionary<string, RdsInstanceOptions>
        {
            ["production"] = MakeInstance(enableWriteOperations: false, defaultSnapshotSource: true)
        });

        var result = RdsSnapshotManagerOptionsValidator.Validate(options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("enableWriteOperations"));
        }
    }
}
