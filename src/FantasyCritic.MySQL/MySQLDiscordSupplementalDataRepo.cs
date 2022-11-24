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

    public async Task<bool> GameInLeagueOrFormerlyInLeague(MasterGameYear masterGameYear, Guid leagueID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            masterGameYear.MasterGame.MasterGameID,
            masterGameYear.Year,
            LeagueID = leagueID
        };

        string sql = "SELECT DISTINCT SubQuery.GameExists FROM " +
                     "(SELECT 1 AS GameExists FROM tbl_league_publisher " +
                     "JOIN tbl_league_publishergame ON tbl_league_publisher.PublisherID = tbl_league_publishergame.PublisherID " +
                     "WHERE LeagueID = @LeagueID AND YEAR = @Year AND MasterGameID = @MasterGameID " +
                     "UNION " +
                     "SELECT 1 AS GameExists FROM tbl_league_publisher " +
                     "JOIN tbl_league_formerpublishergame ON tbl_league_publisher.PublisherID = tbl_league_formerpublishergame.PublisherID " +
                     "WHERE LeagueID = @LeagueID AND YEAR = @Year AND MasterGameID = @MasterGameID) AS SubQuery";

        var result = await connection.QuerySingleOrDefaultAsync<int?>(sql, queryObject);
        return result is not null;
    }
}
