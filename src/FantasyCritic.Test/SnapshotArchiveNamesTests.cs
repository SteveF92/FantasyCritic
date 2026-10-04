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

    [Test]
    public void GetSnapshotIdentifierIfApplicationSchemaKey_ReadsBuiltKey()
    {
        var key = SnapshotArchiveNames.BuildKey("before-net10", SnapshotArchiveNames.ApplicationSchema);
        Assert.That(SnapshotArchiveNames.GetSnapshotIdentifierIfApplicationSchemaKey(key), Is.EqualTo("before-net10"));
    }

    [TestCase("rds-snapshots/before-net10/before-net10-innodb.sql.gz")]
    [TestCase("rds-snapshots/before-net10/manifest.json")]
    [TestCase("rds-snapshots/before-net10/other-fantasycritic.sql.gz")]
    [TestCase("fantasy-critic-rds/2026-10-04/fantasy-critic-rds-2026-10-04-030016.sql.gz")]
    public void GetSnapshotIdentifierIfApplicationSchemaKey_IgnoresOtherKeys(string key)
    {
        Assert.That(SnapshotArchiveNames.GetSnapshotIdentifierIfApplicationSchemaKey(key), Is.Null);
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
