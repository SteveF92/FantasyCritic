using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using Serilog;

namespace FantasyCritic.AWS;

public class RdsRestoreService
{
    private readonly AmazonRDSClient _rdsClient;

    public RdsRestoreService()
    {
        _rdsClient = new AmazonRDSClient();
    }

    public RdsRestoreService(AmazonRDSClient rdsClient)
    {
        _rdsClient = rdsClient;
    }

    public async Task CopySnapshotToInstance(string snapshotIdentifier, string destinationInstanceIdentifier)
    {
        Log.Information("Starting restore of {Snapshot} to {Destination}", snapshotIdentifier, destinationInstanceIdentifier);

        DBSnapshot snapshot = await AssertDBSnapshotByIdentifier(snapshotIdentifier);
        DBInstance? destinationDB = await GetDBInstanceByIdentifier(destinationInstanceIdentifier);

        if (destinationDB is not null)
        {
            Log.Information("Destination database: {Destination}", destinationDB.DBInstanceIdentifier);
            string newNameForOldServer = await RenameOldInstance(destinationDB);
            await RestoreFromSnapshot(destinationDB, snapshot);
            DBInstance oldServer = await AssertDBInstanceByIdentifier(newNameForOldServer);
            await DeleteInstance(oldServer);
        }
        else
        {
            Log.Information("Restoring snapshot to new instance {Destination}", destinationInstanceIdentifier);
            DBInstance productionDB = await AssertDBInstanceByIdentifier(snapshot.DBInstanceIdentifier);
            await RestoreFromSnapshot(productionDB, snapshot, destinationInstanceIdentifier);
        }

        Log.Information("Restore complete.");
    }

    private async Task<DBSnapshot> AssertDBSnapshotByIdentifier(string snapshotIdentifier)
    {
        var response = await _rdsClient.DescribeDBSnapshotsAsync(new DescribeDBSnapshotsRequest
        {
            DBSnapshotIdentifier = snapshotIdentifier
        });

        DBSnapshot? snapshot = response.DBSnapshots.SingleOrDefault();
        if (snapshot is null)
        {
            throw new InvalidOperationException($"RDS snapshot not found: {snapshotIdentifier}");
        }

        return snapshot;
    }

    private async Task<string> RenameOldInstance(DBInstance instance)
    {
        string newName = instance.DBInstanceIdentifier + "-old";
        Log.Information("Renaming {Instance} to {NewName}", instance.DBInstanceIdentifier, newName);
        ModifyDBInstanceRequest modifyDBInstanceRequest = new ModifyDBInstanceRequest()
        {
            DBInstanceIdentifier = instance.DBInstanceIdentifier,
            NewDBInstanceIdentifier = newName,
            ApplyImmediately = true
        };

        await _rdsClient.ModifyDBInstanceAsync(modifyDBInstanceRequest);

        await WaitForDBToHaveName(instance.DbiResourceId, newName);
        Log.Information("Rename successful.");
        return newName;
    }

    private async Task<DBInstance> RestoreFromSnapshot(DBInstance instance, DBSnapshot snapshot, string? newDBName = null)
    {
        if (newDBName is null)
        {
            newDBName = instance.DBInstanceIdentifier;
        }

        Log.Information("Creating new instance from snapshot.");
        RestoreDBInstanceFromDBSnapshotRequest restoreRequest = new RestoreDBInstanceFromDBSnapshotRequest()
        {
            DBSnapshotIdentifier = snapshot.DBSnapshotIdentifier,
            LicenseModel = instance.LicenseModel,
            DBInstanceClass = instance.DBInstanceClass,
            MultiAZ = false,
            StorageType = instance.StorageType,

            DBInstanceIdentifier = newDBName,

            DBSubnetGroupName = instance.DBSubnetGroup.DBSubnetGroupName,
            AvailabilityZone = instance.AvailabilityZone,

            AutoMinorVersionUpgrade = instance.AutoMinorVersionUpgrade,
            VpcSecurityGroupIds = instance.VpcSecurityGroups.Select(x => x.VpcSecurityGroupId).ToList()
        };

        var restoreResponse = await _rdsClient.RestoreDBInstanceFromDBSnapshotAsync(restoreRequest);

        await WaitForDBToBeAvailable(restoreResponse.DBInstance.DbiResourceId);

        Log.Information("Creation successful.");

        return restoreResponse.DBInstance;
    }

    private async Task DeleteInstance(DBInstance instance)
    {
        Log.Information("Deleting old instance {Instance}.", instance.DBInstanceIdentifier);

        var deleteRequest = new DeleteDBInstanceRequest()
        {
            DBInstanceIdentifier = instance.DBInstanceIdentifier,
            SkipFinalSnapshot = true
        };
        await _rdsClient.DeleteDBInstanceAsync(deleteRequest);
    }

    private async Task<DBInstance> AssertDBInstanceByIdentifier(string identifier)
    {
        var instanceResponse = await _rdsClient.DescribeDBInstancesAsync();

        DBInstance? instanceChosen = instanceResponse.DBInstances.SingleOrDefault(x => x.DBInstanceIdentifier == identifier);
        if (instanceChosen is null)
        {
            throw new InvalidOperationException($"RDS instance not found: {identifier}");
        }

        return instanceChosen;
    }

    private async Task<DBInstance?> GetDBInstanceByIdentifier(string identifier)
    {
        var instanceResponse = await _rdsClient.DescribeDBInstancesAsync();

        DBInstance? instanceChosen = instanceResponse.DBInstances.SingleOrDefault(x => x.DBInstanceIdentifier == identifier);
        return instanceChosen;
    }

    private async Task WaitForDBToHaveName(string id, string name)
    {
        while (true)
        {
            var instances = await _rdsClient.DescribeDBInstancesAsync();
            var instance = instances.DBInstances.SingleOrDefault(x => x.DbiResourceId == id);
            if (instance?.DBInstanceIdentifier == name)
            {
                break;
            }

            await Task.Delay(5000);
        }

        await Task.Delay(10000);

        while (true)
        {
            var instances = await _rdsClient.DescribeDBInstancesAsync();
            var instance = instances.DBInstances.SingleOrDefault(x => x.DbiResourceId == id);
            if (instance?.DBInstanceStatus == "available")
            {
                break;
            }

            await Task.Delay(5000);
        }
    }

    private async Task WaitForDBToBeAvailable(string id)
    {
        while (true)
        {
            var instances = await _rdsClient.DescribeDBInstancesAsync();
            var instance = instances.DBInstances.SingleOrDefault(x => x.DbiResourceId == id);
            if (instance?.DBInstanceStatus == "available")
            {
                break;
            }

            await Task.Delay(5000);
        }
    }
}
