using FantasyCritic.MySQL.Entities;
using Serilog;

namespace FantasyCritic.MySQL.SyncingRepos;

public class MySQLLocalSetupSyncer
{
    private static readonly ILogger _logger = Log.ForContext<MySQLLocalSetupSyncer>();

    private readonly string _connectionString;

    public MySQLLocalSetupSyncer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task UpsertSupportedYears(IEnumerable<SupportedYearEntity> years)
    {
        const string sql = """
            INSERT INTO tbl_meta_supportedyear
                (Year, OpenForCreation, OpenForPlay, OpenForBetaUsers, StartDate, Finished)
            VALUES
                (@Year, @OpenForCreation, @OpenForPlay, @OpenForBetaUsers, @StartDate, @Finished)
            ON DUPLICATE KEY UPDATE
                OpenForCreation  = VALUES(OpenForCreation),
                OpenForPlay      = VALUES(OpenForPlay),
                OpenForBetaUsers = VALUES(OpenForBetaUsers),
                StartDate        = VALUES(StartDate),
                Finished         = VALUES(Finished);
            """;

        var yearsList = years.ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, yearsList);
        _logger.Information("Upserted {Count} supported years.", yearsList.Count);
    }

    public async Task UpsertRoyaleYearQuarters(IEnumerable<RoyaleYearQuarterEntity> quarters)
    {
        const string sql = """
            INSERT INTO tbl_royale_supportedquarter
                (Year, Quarter, OpenForPlay, Finished, WinningUser)
            VALUES
                (@Year, @Quarter, @OpenForPlay, @Finished, NULL)
            ON DUPLICATE KEY UPDATE
                OpenForPlay = VALUES(OpenForPlay),
                Finished    = VALUES(Finished);
            """;

        var quartersList = quarters.ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, quartersList);
        _logger.Information("Upserted {Count} royale year quarters.", quartersList.Count);
    }
}
