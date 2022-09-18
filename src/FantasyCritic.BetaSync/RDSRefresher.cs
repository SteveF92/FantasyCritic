using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using FantasyCritic.Lib.Extensions;
using NodaTime;
using Serilog;

namespace FantasyCritic.BetaSync;

public class RDSRefresher
{
    private readonly AmazonRDSClient _rdsClient;
    private readonly string _sourceRdsName;
    private readonly string _destinationRdsName;

    public RDSRefresher(string sourceRDSName, string destinationRDSName)
    {
        _sourceRdsName = sourceRDSName;
        _destinationRdsName = destinationRDSName;
        _rdsClient = new AmazonRDSClient();
    }

    public async Task CopySourceToDestination()
    {
        Log.Information("Starting up copy process.");
        DBInstance destinationDB = await GetDBInstanceByIdentifier(_destinationRdsName);
        Log.Information($"Destination Database: {destinationDB.DBInstanceIdentifier}");

        DBSnapshot snapshotChosen = await SelectDBSnapshot(_sourceRdsName);

        Log.Information($"Source Snapshot: {snapshotChosen.DBSnapshotIdentifier}");

        string newNameForOldServer = await RenameOldInstance(destinationDB);

        await RestoreFromSnapshot(destinationDB, snapshotChosen);

        DBInstance oldServer = await GetDBInstanceByIdentifier(newNameForOldServer);
        await DeleteInstance(oldServer);

        Log.Information("Process Complete.");
    }

    private async Task<DBSnapshot> SelectDBSnapshot(string sourceRDSName)
    {
        var snapshotResponses = await _rdsClient.DescribeDBSnapshotsAsync();
        Console.WriteLine("Select a snapshot");
        var snapshotsForInstance = snapshotResponses.DBSnapshots.Where(x => x.DBInstanceIdentifier == sourceRDSName).OrderByDescending(x => x.OriginalSnapshotCreateTime).Take(10).ToList();
        for (int index = 0; index < snapshotsForInstance.Count; index++)
        {
            DBSnapshot snapshot = snapshotsForInstance[index];
            Console.WriteLine($"{index}: {snapshot.DBInstanceIdentifier}|{snapshot.DBSnapshotIdentifier}");
        }

        string? snapshotSelection = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(snapshotSelection))
        {
            throw new Exception("Invalid selection.");
        }

        DBSnapshot snapshotChosen = snapshotsForInstance[Convert.ToInt32(snapshotSelection)];
        Log.Information($"Selected snapshot: {snapshotChosen.DBSnapshotIdentifier}");
        return snapshotChosen;
    }

    private async Task<string> RenameOldInstance(DBInstance instance)
    {
        string newName = instance.DBInstanceIdentifier + "-old";
        Log.Information($"Renaming {instance.DBInstanceIdentifier} to {newName}");
        ModifyDBInstanceRequest modifyDBInstanceRequest = new ModifyDBInstanceRequest()
        {
            DBInstanceIdentifier = instance.DBInstanceIdentifier,
            NewDBInstanceIdentifier = newName,
            ApplyImmediately = true
        };

        ModifyDBInstanceResponse modifyResponse = await _rdsClient.ModifyDBInstanceAsync(modifyDBInstanceRequest);

        await WaitForDBToHaveName(instance.DbiResourceId, newName);
        Log.Information("Rename successful.");
        return newName;
    }

    private async Task<DBInstance> RestoreFromSnapshot(DBInstance instance, DBSnapshot snapshot)
    {
        Log.Information("Creating new instance from snapshot.");
        RestoreDBInstanceFromDBSnapshotRequest restoreRequest = new RestoreDBInstanceFromDBSnapshotRequest()
        {
            DBSnapshotIdentifier = snapshot.DBSnapshotIdentifier,
            LicenseModel = instance.LicenseModel,
            DBInstanceClass = instance.DBInstanceClass,
            MultiAZ = instance.MultiAZ,
            StorageType = instance.StorageType,

            DBInstanceIdentifier = instance.DBInstanceIdentifier,

            DBSubnetGroupName = instance.DBSubnetGroup.DBSubnetGroupName,
            AvailabilityZone = instance.AvailabilityZone,

            AutoMinorVersionUpgrade = instance.AutoMinorVersionUpgrade,
            VpcSecurityGroupIds = instance.VpcSecurityGroups.Select(x => x.VpcSecurityGroupId).ToList()
        };

        var restoreResponse = await _rdsClient.RestoreDBInstanceFromDBSnapshotAsync(restoreRequest);

        await WaitForDBToBeAvailable(restoreResponse.DBInstance.DbiResourceId);

        Log.Information("Creation Successful.");

        return restoreResponse.DBInstance;
    }

    private async Task DeleteInstance(DBInstance instance)
    {
        Log.Information("Deleting old instance.");

        var deleteRequest = new DeleteDBInstanceRequest()
        {
            DBInstanceIdentifier = instance.DBInstanceIdentifier,
            SkipFinalSnapshot = true
        };
        await _rdsClient.DeleteDBInstanceAsync(deleteRequest);
    }

    private async Task<DBInstance> GetDBInstanceByIdentifier(string identifier)
    {
        var instanceResponse = await _rdsClient.DescribeDBInstancesAsync();

        DBInstance? instanceChosen = instanceResponse.DBInstances.SingleOrDefault(x => x.DBInstanceIdentifier == identifier);
        if (instanceChosen is null)
        {
            throw new Exception($"RDS instance not found: {identifier}");
        }

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
