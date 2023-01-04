using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord.GameNewsSettings;
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
        var leagueChannelEntity = new LeagueChannelEntity(leagueID, guildID, channelID, new RelevantDiscordGameNewsSetting(), null);
        var existingLeague = await GetLeagueChannel(guildID, channelID, year);
        var sql = existingLeague == null
            ? "INSERT INTO tbl_discord_leaguechannel (LeagueID, GuildID, ChannelID, GameNewsSetting) VALUES (@LeagueID, @GuildID, @ChannelID, @GameNewsSetting)"
            : "UPDATE tbl_discord_leaguechannel SET LeagueID=@LeagueID, GuildID=@GuildID, ChannelID=@ChannelID, GameNewsSetting=@GameNewsSetting WHERE @ChannelID=@ChannelID AND @GuildID=@GuildID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task SetIsGameNewsSetting(Guid leagueID, ulong guildID, ulong channelID, DiscordGameNewsSetting gameNewsSetting)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(leagueID, guildID, channelID, gameNewsSetting, null);
        var sql = "UPDATE tbl_discord_leaguechannel SET GameNewsSetting=@GameNewsSetting WHERE LeagueID=@LeagueID AND GuildID=@GuildID AND ChannelID=@ChannelID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task SetPublicBidAlertRoleId(Guid leagueID, ulong guildID, ulong channelID, ulong? publicBidAlertRoleID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(leagueID, guildID, channelID, new RelevantDiscordGameNewsSetting(), publicBidAlertRoleID);
        var sql = "UPDATE tbl_discord_leaguechannel SET PublicBidAlertRoleID=@PublicBidAlertRoleID WHERE LeagueID=@LeagueID AND GuildID=@GuildID AND ChannelID=@ChannelID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task<bool> DeleteLeagueChannel(ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            guildID,
            channelID
        };
        var sql = "DELETE FROM tbl_discord_leaguechannel WHERE GuildID=@guildID AND ChannelID=@channelID";
        var rowsDeleted = await connection.ExecuteAsync(sql, queryObject);
        return rowsDeleted >= 1;
    }

    public async Task<IReadOnlyList<MinimalLeagueChannel>> GetAllLeagueChannels()
    {
        await using var connection = new MySqlConnection(_connectionString);
        const string sql = "select * from tbl_discord_leaguechannel";

        var leagueChannels = await connection.QueryAsync<LeagueChannelEntity>(sql);
        return leagueChannels.Select(l => l.ToMinimalDomain()).ToList();
    }

    public async Task<IReadOnlyList<MinimalLeagueChannel>> GetLeagueChannels(Guid leagueID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            leagueID
        };

        const string leagueChannelSQL =
            "select * from tbl_discord_leaguechannel WHERE LeagueID = @leagueID";

        var leagueChannels = await connection.QueryAsync<LeagueChannelEntity>(leagueChannelSQL, queryObject);
        return leagueChannels.Select(l => l.ToMinimalDomain()).ToList();
    }

    public async Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, IReadOnlyList<SupportedYear> supportedYears, int? year = null)
    {
        var leagueChannelEntity = await GetLeagueChannelEntity(guildID, channelID);
        if (leagueChannelEntity is null)
        {
            return null;
        }

        var league = await _fantasyCriticRepo.GetLeague(leagueChannelEntity.LeagueID);
        if (league is null)
        {
            return null;
        }

        LeagueYear? leagueYear = null;

        if (year != null)
        {
            leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, year.Value);
        }
        else
        {
            var supportedYear = supportedYears
                .OrderBy(y => y.Year)
                .FirstOrDefault(y => !y.Finished && league.Years.Contains(y.Year));
            if (supportedYear == null)
            {
                return null;
            }

            leagueYear = await _fantasyCriticRepo.GetLeagueYear(league, supportedYear.Year);
        }

        return leagueYear is null
            ? null
            : leagueChannelEntity.ToDomain(leagueYear);
    }

    public async Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, int year)
    {
        var leagueChannelEntity = await GetLeagueChannelEntity(guildID, channelID);
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
        return leagueYear is null
            ? null
            : leagueChannelEntity.ToDomain(leagueYear);
    }

    public async Task<MinimalLeagueChannel?> GetMinimalLeagueChannel(ulong guildID, ulong channelID)
    {
        var leagueChannelEntity = await GetLeagueChannelEntity(guildID, channelID);
        return leagueChannelEntity?.ToMinimalDomain();
    }

    private async Task<LeagueChannelEntity?> GetLeagueChannelEntity(ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            guildID,
            channelID
        };

        const string leagueChannelSQL =
            "select * from tbl_discord_leaguechannel WHERE GuildID = @guildID AND ChannelID = @channelID";

        var leagueChannelEntity =
            await connection.QuerySingleOrDefaultAsync<LeagueChannelEntity>(leagueChannelSQL, queryObject);
        return leagueChannelEntity;
    }

    public async Task RemoveAllLeagueChannelsForLeague(Guid leagueID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            leagueID
        };
        var sql = "DELETE FROM tbl_discord_leaguechannel WHERE LeagueID=@leagueID";
        await connection.ExecuteAsync(sql, queryObject);
    }
}
