namespace FantasyCritic.Lib.Interfaces;

//Keys are relative: each location puts them under its own prefix.
public interface IDatabaseArchiveLocation
{
    string Name { get; }
    Task Upload(string localFilePath, string key, CancellationToken cancellationToken);
}
