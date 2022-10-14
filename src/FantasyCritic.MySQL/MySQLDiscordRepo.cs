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

    public async Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID, int year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(leagueID, guildID, channelID);
        var existingLeague = await GetLeagueChannel(guildID, channelID, year);
        var sql = existingLeague == null
            ? "INSERT INTO tbl_discord_leaguechannel (LeagueID, GuildID, ChannelID) VALUES (@LeagueID, @GuildID, @ChannelID)"
            : "UPDATE tbl_discord_leaguechannel SET LeagueID=@LeagueID, GuildID=@GuildID, ChannelID=@ChannelID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task DeleteLeagueChannel(ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            guildID,
            channelID
        };
        var sql = "DELETE FROM tbl_discord_leaguechannel WHERE GuildID=@guildID AND ChannelID=@channelID";
        await connection.ExecuteAsync(sql, queryObject);
    }

    public async Task<IReadOnlyList<MinimalLeagueChannel>> GetAllLeagueChannels()
    {
        await using var connection = new MySqlConnection(_connectionString);
        const string sql =
            "select LeagueID, GuildID, ChannelID from tbl_discord_leaguechannel";

        var leagueChannels = await connection.QueryAsync<LeagueChannelEntity>(sql);
        return leagueChannels.Select(l => new MinimalLeagueChannel(l.LeagueID, l.GuildID, l.ChannelID)).ToList();
    }

    public async Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, int year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            guildID,
            channelID
        };

        const string leagueChannelSQL =
            "select LeagueID, GuildID, ChannelID from tbl_discord_leaguechannel WHERE GuildID = @guildID AND ChannelID = @channelID";

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

        return leagueChannelEntity.ToDomain(leagueYear);
    }
}
