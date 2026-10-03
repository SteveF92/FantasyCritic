using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.MySQL;

public sealed class MySQLDatabaseDumper : IDatabaseDumper
{
    private readonly string _connectionString;
    private readonly MysqldumpRunner _mysqldumpRunner;

    public MySQLDatabaseDumper(string connectionString, MysqldumpRunner mysqldumpRunner)
    {
        _connectionString = connectionString;
        _mysqldumpRunner = mysqldumpRunner;
    }

    public async Task<Result> DumpToGzipFile(string outputFilePath, CancellationToken cancellationToken)
    {
        return await _mysqldumpRunner.DumpToGzipFile(_connectionString, outputFilePath, cancellationToken);
    }
}
