using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;

namespace FantasyCritic.MySQL;
public class MySQLDiscordSupplementalDataRepo : IDiscordSupplementalDataRepo
{
    private readonly string _connectionString;

    public MySQLDiscordSupplementalDataRepo(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public async Task<IReadOnlySet<Guid>> GetLeaguesWithOrFormerlyWithGame(MasterGame masterGame, int year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            masterGame.MasterGameID,
            year
        };

        string sql = "SELECT DISTINCT SubQuery.LeagueID " +
            "FROM   (SELECT LeagueID " +
            "        FROM   tbl_league_publisher " +
            "               JOIN tbl_league_publishergame " +
            "                 ON tbl_league_publisher.publisherid = " +
            "                    tbl_league_publishergame.publisherid " +
            "        WHERE  Year = @year " +
            "               AND MasterGameID = @MasterGameID " +
            "       UNION " +
            "        SELECT LeagueID " +
            "        FROM   tbl_league_publisher " +
            "               JOIN tbl_league_formerpublishergame " +
            "                 ON tbl_league_publisher.publisherid = " +
            "                    tbl_league_formerpublishergame.publisherid " +
            "       WHERE  Year = @year " +
            "               AND MasterGameID = @MasterGameID) AS SubQuery ";

        var result = await connection.QueryAsync<Guid>(sql, queryObject);
        return result.ToHashSet();
    }

    public async Task<ILookup<Guid, Guid>> GetLeaguesWithOrFormerlyWithGames(IEnumerable<MasterGame> masterGamesReleasingToday, int year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            masterGameIDs = masterGamesReleasingToday.Select(x => x.MasterGameID),
            year
        };

        string sql = "SELECT DISTINCT SubQuery.LeagueID, SubQuery.MasterGameID " +
                     "FROM   (SELECT LeagueID, MasterGameID " +
                     "        FROM   tbl_league_publisher " +
                     "               JOIN tbl_league_publishergame " +
                     "                 ON tbl_league_publisher.publisherid = " +
                     "                    tbl_league_publishergame.publisherid " +
                     "        WHERE  Year = @year " +
                     "               AND MasterGameID IN @masterGameIDs " +
                     "       UNION " +
                     "        SELECT LeagueID, MasterGameID " +
                     "        FROM   tbl_league_publisher " +
                     "               JOIN tbl_league_formerpublishergame " +
                     "                 ON tbl_league_publisher.publisherid = " +
                     "                    tbl_league_formerpublishergame.publisherid " +
                     "       WHERE  Year = @year " +
                     "               AND MasterGameID IN @masterGameIDs) AS SubQuery ";

        var result = await connection.QueryAsync<CurrentLeagueYearHasGameEntity>(sql, queryObject);
        return result.ToLookup(x => x.LeagueID, y => y.MasterGameID);
    }
}
