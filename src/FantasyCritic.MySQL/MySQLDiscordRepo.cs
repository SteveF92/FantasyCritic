using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities;

namespace FantasyCritic.MySQL;
public class MySQLDiscordRepo : IDiscordRepo
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;
    private readonly string _connectionString;

    public MySQLDiscordRepo(RepositoryConfiguration configuration, IFantasyCriticRepo fantasyCriticRepo, IClock clock)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
        _connectionString = configuration.ConnectionString;
    }

    public async Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID, int year)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(leagueID, guildID, channelID, true, true, null);
        var existingLeague = await GetLeagueChannel(guildID, channelID, year);
        var sql = existingLeague == null
            ? "INSERT INTO tbl_discord_leaguechannel (LeagueID,GuildID,ChannelID,SendLeagueMasterGameUpdates,SendNotableMisses) VALUES (@LeagueID, @GuildID, @ChannelID, @SendLeagueMasterGameUpdates, @SendNotableMisses)"
            : "UPDATE tbl_discord_leaguechannel SET LeagueID=@LeagueID WHERE @ChannelID=@ChannelID AND @GuildID=@GuildID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task SetLeagueGameNewsSetting(Guid leagueID, ulong guildID, ulong channelID, bool sendLeagueMasterGameUpdates, bool sendNotableMisses)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(leagueID, guildID, channelID, sendLeagueMasterGameUpdates, sendNotableMisses, null);
        var sql = "UPDATE tbl_discord_leaguechannel SET SendLeagueMasterGameUpdates=@SendLeagueMasterGameUpdates, SendNotableMisses=@SendNotableMisses WHERE LeagueID=@LeagueID AND GuildID=@GuildID AND ChannelID=@ChannelID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task SetGameNewsSetting(ulong guildID, ulong channelID, GameNewsSetting gameNewsSetting)
    {
        bool deleting = gameNewsSetting.Equals(GameNewsSetting.Off);
        var deleteSQL = "DELETE FROM tbl_discord_gamenewschannel where GuildID=@GuildID AND ChannelID=@ChannelID;";
        var insertSQL = "INSERT IGNORE INTO tbl_discord_gamenewschannel(GuildID,ChannelID,GameNewsSetting) VALUES (@GuildID,@ChannelID,@GameNewsSetting);";
        var updateSQL = "UPDATE tbl_discord_gamenewschannel SET GameNewsSetting = @GameNewsSetting where GuildID=@GuildID AND ChannelID=@ChannelID;";
        var gameNewsChannelEntity = new GameNewsChannelEntity(guildID, channelID, gameNewsSetting);

        await using var connection = new MySqlConnection(_connectionString);
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteSQL, gameNewsChannelEntity, transaction);
        if (deleting)
        {
            await connection.ExecuteAsync(deleteSQL, gameNewsChannelEntity, transaction);
        }
        else
        {
            await connection.ExecuteAsync(insertSQL, gameNewsChannelEntity, transaction);
            await connection.ExecuteAsync(updateSQL, gameNewsChannelEntity, transaction);
        }
        await transaction.CommitAsync();
    }

    public async Task SetBidAlertRoleId(Guid leagueID, ulong guildID, ulong channelID, ulong? bidAlertRoleID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(leagueID, guildID, channelID, true, true, bidAlertRoleID);
        var sql = "UPDATE tbl_discord_leaguechannel SET BidAlertRoleID=@BidAlertRoleID WHERE LeagueID=@LeagueID AND GuildID=@GuildID AND ChannelID=@ChannelID";
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

    public async Task<IReadOnlyList<GameNewsChannel>> GetAllGameNewsChannels()
    {
        await using var connection = new MySqlConnection(_connectionString);
        const string sql = "select * from tbl_discord_gamenewschannel";

        var entities = await connection.QueryAsync<GameNewsChannelEntity>(sql);
        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<MinimalLeagueChannel>> GetLeagueChannels(Guid leagueID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            leagueID
        };

        const string leagueChannelSQL = "select * from tbl_discord_leaguechannel WHERE LeagueID = @leagueID";

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

    public async Task<GameNewsChannel?> GetGameNewsChannel(ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            guildID,
            channelID
        };

        const string sql = "select * from tbl_discord_gamenewschannel WHERE GuildID = @guildID AND ChannelID = @channelID";

        var entity = await connection.QuerySingleOrDefaultAsync<GameNewsChannelEntity>(sql, queryObject);
        return entity?.ToDomain();
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

        const string leagueChannelSQL = "select * from tbl_discord_leaguechannel WHERE GuildID = @guildID AND ChannelID = @channelID";

        var leagueChannelEntity = await connection.QuerySingleOrDefaultAsync<LeagueChannelEntity>(leagueChannelSQL, queryObject);
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

    private async Task<IReadOnlyList<int>> GetActiveYears()
    {
        string sql = "SELECT Year FROM tbl_meta_supportedyear WHERE Finished = 0;";
        await using var connection = new MySqlConnection(_connectionString);
        var activeYearsForLeague = await connection.QueryAsync<int>(sql);
        return activeYearsForLeague.ToList();
    }
}
