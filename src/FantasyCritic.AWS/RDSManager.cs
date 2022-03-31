using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using FantasyCritic.Lib.Extensions;
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

    public async Task SnapshotRDS(Instant snapshotTime)
    {
        using AmazonRDSClient rdsClient = new AmazonRDSClient();
        var date = snapshotTime.InZone(TimeExtensions.EasternTimeZone).LocalDateTime.Date;
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var random = Guid.NewGuid().ToString().Substring(0, 1);
        string snapName = "AdminSnap-" + dateString + "-" + random;

        CreateDBSnapshotRequest request = new CreateDBSnapshotRequest(snapName, _instanceName);
        await rdsClient.CreateDBSnapshotAsync(request, CancellationToken.None);
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
            .Select(x =>
                new DatabaseSnapshotInfo(x.DBSnapshotIdentifier,
                    Instant.FromDateTimeUtc(x.SnapshotCreateTime.ToUniversalTime()),
                    x.PercentProgress,
                    x.Status))
            .ToList();
        return domainObjects;
    }
}
