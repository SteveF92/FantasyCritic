using FantasyCritic.Lib.Utilities;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class SnapshotArchiveNamesTests
{
    [Test]
    public void BuildTemporaryInstanceIdentifier_PrefixesSnapshotIdentifier()
    {
        var identifier = SnapshotArchiveNames.BuildTemporaryInstanceIdentifier("adminsnap-2024-01-01-e");
        Assert.That(identifier, Is.EqualTo("archive-adminsnap-2024-01-01-e"));
    }

    [Test]
    public void BuildTemporaryInstanceIdentifier_TruncatesToRdsLimit()
    {
        var identifier = SnapshotArchiveNames.BuildTemporaryInstanceIdentifier(new string('a', 100));
        Assert.That(identifier, Has.Length.EqualTo(63));
    }

    [Test]
    public void BuildTemporaryInstanceIdentifier_DoesNotEndWithHyphenAfterTruncation()
    {
        //"archive-" is 8 characters, so the 63rd character is this identifier's 55th, a hyphen.
        var snapshotIdentifier = new string('a', 54) + "-bbbbbbbbbb";
        var identifier = SnapshotArchiveNames.BuildTemporaryInstanceIdentifier(snapshotIdentifier);
        Assert.That(identifier, Is.EqualTo("archive-" + new string('a', 54)));
    }

    [Test]
    public void BuildKey_NestsDumpUnderSnapshotFolder()
    {
        var key = SnapshotArchiveNames.BuildKey("before-net10", "fantasycritic");
        Assert.That(key, Is.EqualTo("rds-snapshots/before-net10/before-net10-fantasycritic.sql.gz"));
    }

    [Test]
    public void BuildManifestKey_SitsBesideDumps()
    {
        var key = SnapshotArchiveNames.BuildManifestKey("before-net10");
        Assert.That(key, Is.EqualTo("rds-snapshots/before-net10/manifest.json"));
    }

    [TestCase("information_schema", true)]
    [TestCase("mysql", true)]
    [TestCase("performance_schema", true)]
    [TestCase("sys", true)]
    [TestCase("fantasycritic", false)]
    public void IsSystemSchema_RecognizesMySqlSchemas(string schema, bool expected)
    {
        Assert.That(SnapshotArchiveNames.IsSystemSchema(schema), Is.EqualTo(expected));
    }
}
