using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.MySQL;

public sealed class MySQLDatabaseDumper : IDatabaseDumper
{
    private readonly string _connectionString;
    private readonly MysqldumpRunner _mysqldumpRunner;

    public MySQLDatabaseDumper(string instanceName, string connectionString, MysqldumpRunner mysqldumpRunner)
    {
        InstanceName = instanceName;
        _connectionString = connectionString;
        _mysqldumpRunner = mysqldumpRunner;
    }

    public string InstanceName { get; }

    public async Task<Result> DumpToGzipFile(string outputFilePath, CancellationToken cancellationToken)
    {
        return await _mysqldumpRunner.DumpToGzipFile(_connectionString, outputFilePath, cancellationToken);
    }
}
