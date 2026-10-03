namespace FantasyCritic.Lib.Interfaces;

//Dumps the whole database it was built for, schema and data, to a gzipped SQL file.
public interface IDatabaseDumper
{
    //What the dump files are named after: the RDS instance, as the snapshot manager names its dumps.
    string InstanceName { get; }

    Task<Result> DumpToGzipFile(string outputFilePath, CancellationToken cancellationToken);
}
