namespace FantasyCritic.Lib.Interfaces;

//Somewhere database dumps are kept, such as a bucket. Keys come from BackupRemoteKeyBuilder and are relative: each location puts them under its own prefix.
public interface IDatabaseArchiveLocation
{
    string Name { get; }
    Task Upload(string localFilePath, string key, CancellationToken cancellationToken);
}
