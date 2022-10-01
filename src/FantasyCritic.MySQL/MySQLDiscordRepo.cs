using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;

namespace FantasyCritic.MySQL;
public class MySQLDiscordRepo : IDiscordRepo
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly string _connectionString;

    public MySQLDiscordRepo(RepositoryConfiguration configuration, IFantasyCriticRepo fantasyCriticRepo)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _connectionString = configuration.ConnectionString;
    }

    public async Task<LeagueChannel?> GetLeagueChannel(string channelID, int year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            channelID
        };

        const string leagueChannelSQL =
            "select LeagueID, ChannelID from tbl_discord_leaguechannel WHERE ChannelID = @channelID";

        var leagueChannelEntity = await connection.QuerySingleOrDefaultAsync<LeagueChannelEntity>(leagueChannelSQL, queryObject);
        if (leagueChannelEntity is null)
        {
            return null;
        }

        var league = await _fantasyCriticRepo.GetLeague(leagueChannelEntity.LeagueID);
        if (league is null)
        {
            return null;
        }

        var leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, year);
        if (leagueYear is null)
        {
            return null;
        }

        var leagueChannel = leagueChannelEntity.ToDomain(leagueYear);
        return leagueChannel;
    }
}
