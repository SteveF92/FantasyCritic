namespace FantasyCritic.Lib.Interfaces;

public interface IDatabaseDumper
{
    //What the dump files are named after: the RDS instance, as the snapshot manager names its dumps.
    string InstanceName { get; }

    Task<Result> DumpToGzipFile(string outputFilePath, CancellationToken cancellationToken);
}
