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
}
