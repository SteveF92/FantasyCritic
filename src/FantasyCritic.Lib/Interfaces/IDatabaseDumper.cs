namespace FantasyCritic.Lib.Interfaces;

//Dumps the whole database it was built for, schema and data, to a gzipped SQL file.
public interface IDatabaseDumper
{
    Task<Result> DumpToGzipFile(string outputFilePath, CancellationToken cancellationToken);
}
