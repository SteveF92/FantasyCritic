using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using NodaTime;
using Serilog;

namespace FantasyCritic.AWS;

//Restores snapshots onto short-lived, tagged instances, and deletes only instances carrying that tag.
public sealed class RdsTemporaryRestoreService
{
    private static readonly ILogger _logger = Log.ForContext<RdsTemporaryRestoreService>();

    private const string PurposeTagKey = "fc-purpose";
    private const string PurposeTagValue = "snapshot-archive";
    //Offered for every MySQL version from 5.7 to 8.4.
    private const string InstanceClass = "db.t3.micro";
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan AvailableTimeout = TimeSpan.FromHours(1);
    private static readonly IReadOnlySet<string> FailedStatuses = new HashSet<string>
    {
        "failed",
        "incompatible-network",
        "incompatible-option-group",
        "incompatible-parameters",
        "incompatible-restore",
        "inaccessible-encryption-credentials",
        "storage-full",
    };

    private readonly IAmazonRDS _rdsClient;

    public RdsTemporaryRestoreService(IAmazonRDS rdsClient)
    {
        _rdsClient = rdsClient;
    }

    public async Task<IReadOnlyList<ManualSnapshot>> GetManualSnapshots(CancellationToken cancellationToken)
    {
        var request = new DescribeDBSnapshotsRequest { SnapshotType = "manual" };
        List<ManualSnapshot> snapshots = [];
        await foreach (var snapshot in _rdsClient.Paginators.DescribeDBSnapshots(request).DBSnapshots.WithCancellation(cancellationToken))
        {
            snapshots.Add(new ManualSnapshot(
                snapshot.DBSnapshotIdentifier,
                snapshot.DBInstanceIdentifier,
                snapshot.Engine,
                snapshot.EngineVersion,
                Instant.FromDateTimeUtc(DateTime.SpecifyKind(snapshot.SnapshotCreateTime ?? DateTime.MinValue, DateTimeKind.Utc)),
                snapshot.MasterUsername,
                snapshot.Status));
        }

        return snapshots;
    }

    public async Task<IReadOnlyList<string>> GetTemporaryInstanceIdentifiers(CancellationToken cancellationToken)
    {
        List<string> identifiers = [];
        await foreach (var instance in _rdsClient.Paginators.DescribeDBInstances(new DescribeDBInstancesRequest()).DBInstances
                           .WithCancellation(cancellationToken))
        {
            if (HasPurposeTag(instance) && instance.DBInstanceStatus != "deleting")
            {
                identifiers.Add(instance.DBInstanceIdentifier);
            }
        }

        return identifiers;
    }

    public async Task<TemporaryInstanceEndpoint> RestoreToTemporaryInstance(string snapshotIdentifier, string instanceIdentifier,
        string networkTemplateInstanceIdentifier, string masterPassword, CancellationToken cancellationToken)
    {
        DBInstance template = await GetInstance(networkTemplateInstanceIdentifier, cancellationToken);

        _logger.Information("Restoring {Snapshot} to {Instance}", snapshotIdentifier, instanceIdentifier);
        await _rdsClient.RestoreDBInstanceFromDBSnapshotAsync(new RestoreDBInstanceFromDBSnapshotRequest
        {
            DBSnapshotIdentifier = snapshotIdentifier,
            DBInstanceIdentifier = instanceIdentifier,
            DBInstanceClass = InstanceClass,
            MultiAZ = false,
            PubliclyAccessible = template.PubliclyAccessible,
            DBSubnetGroupName = template.DBSubnetGroup.DBSubnetGroupName,
            VpcSecurityGroupIds = template.VpcSecurityGroups.Select(x => x.VpcSecurityGroupId).ToList(),
            AutoMinorVersionUpgrade = false,
            DeletionProtection = false,
            Tags = [new Tag { Key = PurposeTagKey, Value = PurposeTagValue }],
        }, cancellationToken);
        await WaitForAvailable(instanceIdentifier, cancellationToken);

        //A snapshot keeps the master password it was taken with, which may be long forgotten.
        _logger.Information("Resetting the master password on {Instance}", instanceIdentifier);
        await _rdsClient.ModifyDBInstanceAsync(new ModifyDBInstanceRequest
        {
            DBInstanceIdentifier = instanceIdentifier,
            MasterUserPassword = masterPassword,
            ApplyImmediately = true,
        }, cancellationToken);

        //The status stays available for a moment before the reset begins.
        await Task.Delay(PollInterval, cancellationToken);
        DBInstance instance = await WaitForAvailable(instanceIdentifier, cancellationToken);
        return new TemporaryInstanceEndpoint(instance.Endpoint.Address, instance.Endpoint.Port ?? 3306);
    }

    public async Task DeleteTemporaryInstance(string instanceIdentifier)
    {
        DBInstance instance = await GetInstance(instanceIdentifier, CancellationToken.None);
        if (!HasPurposeTag(instance))
        {
            throw new InvalidOperationException(
                $"Refusing to delete {instanceIdentifier}: it lacks the {PurposeTagKey}={PurposeTagValue} tag.");
        }

        _logger.Information("Deleting temporary instance {Instance}", instanceIdentifier);
        await _rdsClient.DeleteDBInstanceAsync(new DeleteDBInstanceRequest
        {
            DBInstanceIdentifier = instanceIdentifier,
            SkipFinalSnapshot = true,
            DeleteAutomatedBackups = true,
        }, CancellationToken.None);
    }

    private async Task<DBInstance> WaitForAvailable(string instanceIdentifier, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + AvailableTimeout;
        while (true)
        {
            DBInstance instance = await GetInstance(instanceIdentifier, cancellationToken);
            if (instance.DBInstanceStatus == "available")
            {
                return instance;
            }

            if (FailedStatuses.Contains(instance.DBInstanceStatus))
            {
                throw new InvalidOperationException($"{instanceIdentifier} entered status {instance.DBInstanceStatus}.");
            }

            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException(
                    $"{instanceIdentifier} was not available after {AvailableTimeout.TotalMinutes} minutes; last status {instance.DBInstanceStatus}.");
            }

            await Task.Delay(PollInterval, cancellationToken);
        }
    }

    private async Task<DBInstance> GetInstance(string instanceIdentifier, CancellationToken cancellationToken)
    {
        var response = await _rdsClient.DescribeDBInstancesAsync(new DescribeDBInstancesRequest
        {
            DBInstanceIdentifier = instanceIdentifier
        }, cancellationToken);

        return response.DBInstances?.SingleOrDefault()
               ?? throw new InvalidOperationException($"RDS instance not found: {instanceIdentifier}");
    }

    private static bool HasPurposeTag(DBInstance instance) =>
        instance.TagList?.Any(x => x.Key == PurposeTagKey && x.Value == PurposeTagValue) ?? false;
}
