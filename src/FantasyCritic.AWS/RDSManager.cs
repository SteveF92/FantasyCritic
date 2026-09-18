using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.AWS;

public class RDSManager : IRDSManager
{
    private readonly string _instanceName;

    public RDSManager(string instanceName)
    {
        _instanceName = instanceName;
    }

    public async Task SnapshotRDS(string snapshotIdentifier, CancellationToken cancellationToken)
    {
        var validation = RdsSnapshotIdentifierValidator.Validate(snapshotIdentifier);
        if (validation.IsFailure)
        {
            throw new InvalidOperationException(validation.Error);
        }

        using AmazonRDSClient rdsClient = new AmazonRDSClient();
        CreateDBSnapshotRequest request = new CreateDBSnapshotRequest(snapshotIdentifier, _instanceName);
        await rdsClient.CreateDBSnapshotAsync(request, cancellationToken);
    }

    public async Task<DatabaseSnapshotInfo> GetSnapshot(string snapshotIdentifier, CancellationToken cancellationToken)
    {
        using AmazonRDSClient rdsClient = new AmazonRDSClient();
        DescribeDBSnapshotsRequest request = new DescribeDBSnapshotsRequest()
        {
            DBInstanceIdentifier = _instanceName,
            DBSnapshotIdentifier = snapshotIdentifier
        };
        DescribeDBSnapshotsResponse snaps = await rdsClient.DescribeDBSnapshotsAsync(request, cancellationToken);
        return ToDomain(snaps.DBSnapshots.Single());
    }

    public async Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots()
    {
        using AmazonRDSClient rdsClient = new AmazonRDSClient();
        DescribeDBSnapshotsRequest request = new DescribeDBSnapshotsRequest()
        {
            DBInstanceIdentifier = _instanceName
        };
        DescribeDBSnapshotsResponse snaps = await rdsClient.DescribeDBSnapshotsAsync(request, CancellationToken.None);
        var orderedSnaps = snaps.DBSnapshots.OrderBy(x => x.PercentProgress).ThenByDescending(x => x.SnapshotCreateTime);
        var domainObjects = orderedSnaps
            .Select(ToDomain)
            .ToList();
        return domainObjects;
    }

    private static DatabaseSnapshotInfo ToDomain(DBSnapshot snapshot) =>
        new DatabaseSnapshotInfo(snapshot.DBSnapshotIdentifier,
            Instant.FromDateTimeUtc(snapshot.SnapshotCreateTime ?? DateTime.MinValue),
            snapshot.PercentProgress ?? 0,
            snapshot.Status);
}
