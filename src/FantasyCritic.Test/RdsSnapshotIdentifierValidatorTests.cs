using FantasyCritic.Lib.Utilities;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class RdsSnapshotIdentifierValidatorTests
{
    [TestCase("adminsnap-2026-06-18-a")]
    [TestCase("manual-backup-1")]
    public void Validate_AcceptsValidIdentifiers(string identifier)
    {
        var result = RdsSnapshotIdentifierValidator.Validate(identifier);
        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("UPPERCASE-NOT-ALLOWED")]
    [TestCase("has spaces")]
    [TestCase("has_underscores")]
    public void Validate_RejectsInvalidIdentifiers(string identifier)
    {
        var result = RdsSnapshotIdentifierValidator.Validate(identifier);
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public void Validate_RejectsTooLongIdentifier()
    {
        var identifier = new string('a', 256);
        var result = RdsSnapshotIdentifierValidator.Validate(identifier);
        Assert.That(result.IsFailure, Is.True);
    }
}
