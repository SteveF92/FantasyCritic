using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.MySQL;
public class MySQLDiscordSupplementalDataRepo : IDiscordSupplementalDataRepo
{
    private readonly string _connectionString;

    public MySQLDiscordSupplementalDataRepo(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public async Task<IReadOnlySet<Guid>> GetLeaguesWithOrFormerlyWithGame(MasterGameYear masterGameYear)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            masterGameYear.MasterGame.MasterGameID,
            masterGameYear.Year
        };

        string sql = "SELECT DISTINCT SubQuery.leagueid " +
            "FROM   (SELECT DISTINCT leagueid " +
            "        FROM   tbl_league_publisher " +
            "               JOIN tbl_league_publishergame " +
            "                 ON tbl_league_publisher.publisherid = " +
            "                    tbl_league_publishergame.publisherid " +
            "        WHERE  Year = @Year " +
            "               AND MasterGameID = @MasterGameID " +
            "       UNION " +
            "        SELECT DISTINCT leagueid " +
            "        FROM   tbl_league_publisher " +
            "               JOIN tbl_league_formerpublishergame " +
            "                 ON tbl_league_publisher.publisherid = " +
            "                    tbl_league_formerpublishergame.publisherid " +
            "       WHERE  Year = @Year " +
            "               AND MasterGameID = @MasterGameID) AS SubQuery ";

        var result = await connection.QueryAsync<Guid>(sql, queryObject);
        return result.ToHashSet();
    }
}
