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

    public async Task SetLeagueChannel(Guid leagueId, string channelId, int year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(leagueId, channelId);
        var existingLeague = await GetLeagueChannel(channelId, year);
        var sql = existingLeague == null
            ? "INSERT INTO tbl_discord_leaguechannel (LeagueID, ChannelID) VALUES (@LeagueID, @ChannelID)"
            : "UPDATE tbl_discord_leaguechannel SET LeagueID=@LeagueID, ChannelID=@ChannelID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task DeleteLeagueChannel(string channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            channelID
        };
        var sql = "DELETE FROM tbl_discord_leaguechannel WHERE ChannelID=@ChannelID";
        await connection.ExecuteAsync(sql, queryObject);
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
