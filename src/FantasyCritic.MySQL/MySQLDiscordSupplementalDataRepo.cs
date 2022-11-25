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

        string sql = "SELECT DISTINCT SubQuery.LeagueID FROM " +
            "(SELECT distinct LeagueID FROM tbl_league_publisher " +
            "JOIN tbl_league_publishergame ON tbl_league_publisher.PublisherID = tbl_league_publishergame.PublisherID " +
            "WHERE YEAR = 2022 AND MasterGameID = @MasterGameID)" +
            "UNION " +
            "SELECT distinct LeagueID FROM tbl_league_publisher " +
            "JOIN tbl_league_formerpublishergame ON tbl_league_publisher.PublisherID = tbl_league_formerpublishergame.PublisherID " +
            "WHERE YEAR = 2022 AND MasterGameID = @MasterGameID)" +
            ") AS SubQuery";

        var result = await connection.QueryAsync<Guid>(sql, queryObject);
        return result.ToHashSet();
    }
}
