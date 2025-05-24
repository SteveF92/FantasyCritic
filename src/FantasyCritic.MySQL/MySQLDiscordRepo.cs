using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL.Entities.Discord;

namespace FantasyCritic.MySQL;
public class MySQLDiscordRepo : IDiscordRepo
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IMasterGameRepo _masterGameRepo;
    private readonly IConferenceRepo _conferenceRepo;
    private readonly ICombinedDataRepo _combinedDataRepo;
    private readonly IClock _clock;
    private readonly string _connectionString;

    public MySQLDiscordRepo(RepositoryConfiguration configuration,
        IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, IConferenceRepo conferenceRepo, ICombinedDataRepo combinedDataRepo,
        IClock clock)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _masterGameRepo = masterGameRepo;
        _conferenceRepo = conferenceRepo;
        _combinedDataRepo = combinedDataRepo;
        _clock = clock;
        _connectionString = configuration.ConnectionString;
    }

    public async Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(guildID, channelID, leagueID, true, true, false, NotableMissSetting.ScoreUpdates, null);
        var existingChannel = await GetLeagueChannelEntity(guildID, channelID);
        var sql = existingChannel == null
            ? "INSERT INTO tbl_discord_leaguechannel (GuildID,ChannelID,LeagueID,ShowPickedGameNews,ShowEligibleGameNews,ShowIneligibleGameNews,NotableMissSetting) VALUES (@GuildID, @ChannelID, @LeagueID, @ShowPickedGameNews, @ShowEligibleGameNews, @NotableMissSetting)"
            : "UPDATE tbl_discord_leaguechannel SET LeagueID=@LeagueID WHERE ChannelID=@ChannelID AND GuildID=@GuildID";
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task SetConferenceChannel(Guid conferenceID, ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var conferenceChannelEntity = new ConferenceChannelEntity(guildID, channelID, conferenceID);
        var existingChannel = await GetConferenceChannelEntity(guildID, channelID);
        var sql = existingChannel == null
            ? "INSERT INTO tbl_discord_conferencechannel (GuildID,ChannelID,ConferenceID) VALUES (@GuildID, @ChannelID, @ConferenceID)"
            : "UPDATE tbl_discord_conferencechannel SET ConferenceID=@ConferenceID WHERE ChannelID=@ChannelID AND GuildID=@GuildID";
        await connection.ExecuteAsync(sql, conferenceChannelEntity);
    }

    public async Task SetLeagueGameNewsSetting(Guid leagueID, ulong guildID, ulong channelID, bool showPickedGameNews, bool showEligibleGameNews, bool showIneligibleGameNews, NotableMissSetting notableMissSetting)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var leagueChannelEntity = new LeagueChannelEntity(guildID, channelID, leagueID, showPickedGameNews, showEligibleGameNews, showIneligibleGameNews, notableMissSetting, null);
        var sql = """
                  UPDATE tbl_discord_leaguechannel SET
                  ShowPickedGameNews=@ShowPickedGameNews,
                  ShowEligibleGameNews=@ShowEligibleGameNews,
                  ShowIneligibleGameNews=@ShowIneligibleGameNews,
                  NotableMissSetting=@NotableMissSetting
                  WHERE LeagueID=@LeagueID AND GuildID=@GuildID AND ChannelID=@ChannelID;
                  """;
        await connection.ExecuteAsync(sql, leagueChannelEntity);
    }

    public async Task SetGameNewsSetting(ulong guildID, ulong channelID, GameNewsSetting gameNewsSetting)
    {
        bool deleting = gameNewsSetting.IsOff();
        var deleteSQL = "DELETE FROM tbl_discord_gamenewschannel where GuildID=@GuildID AND ChannelID=@ChannelID;";

        var insertSQL = """
                        INSERT IGNORE INTO tbl_discord_gamenewschannel(GuildID,ChannelID,ShowAlreadyReleasedNews,ShowWillReleaseInYearNews,ShowMightReleaseInYearNews,ShowWillNotReleaseInYearNews,ShowScoreGameNews,ShowJustReleasedAnnouncements,ShowNewGameAnnouncements,ShowEditedGameNews) 
                        VALUES (@GuildID,@ChannelID,@ShowAlreadyReleasedNews,@ShowWillReleaseInYearNews,@ShowMightReleaseInYearNews,@ShowWillNotReleaseInYearNews,@ShowScoreGameNews,@ShowJustReleasedAnnouncements,@ShowNewGameAnnouncements,@ShowEditedGameNews);
                        """;
        var updateSQL = """
                        UPDATE tbl_discord_gamenewschannel SET
                        ShowAlreadyReleasedNews = @ShowAlreadyReleasedNews,
                        ShowWillReleaseInYearNews = @ShowWillReleaseInYearNews,
                        ShowMightReleaseInYearNews = @ShowMightReleaseInYearNews,
                        ShowWillNotReleaseInYearNews = @ShowWillNotReleaseInYearNews,
                        ShowScoreGameNews = @ShowScoreGameNews,
                        ShowJustReleasedAnnouncements = @ShowJustReleasedAnnouncements,
                        ShowNewGameAnnouncements = @ShowNewGameAnnouncements,
                        ShowEditedGameNews = @ShowEditedGameNews
                        where GuildID=@GuildID AND ChannelID=@ChannelID;
                        """;

        var selectTagsSQL = "SELECT * from tbl_discord_gamenewschannelskiptag where GuildID=@guildID AND ChannelID=@channelID;";
        var deleteTagsSQL = "DELETE from tbl_discord_gamenewschannelskiptag where GuildID=@guildID AND ChannelID=@channelID;";
        var gameNewsChannelEntity = new GameNewsChannelEntity(guildID, channelID, gameNewsSetting);

        var param = new
        {
            guildID,
            channelID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var masterGameTagEntities = await connection.QueryAsync<GameNewsChannelSkippedTagEntity>(selectTagsSQL, param);

        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteTagsSQL, param, transaction);
        await connection.ExecuteAsync(deleteSQL, gameNewsChannelEntity, transaction);
        if (deleting)
        {
            await connection.ExecuteAsync(deleteSQL, gameNewsChannelEntity, transaction);
        }
        else
        {
            await connection.ExecuteAsync(insertSQL, gameNewsChannelEntity, transaction);
            await connection.ExecuteAsync(updateSQL, gameNewsChannelEntity, transaction);
            await connection.BulkInsertAsync(masterGameTagEntities, "tbl_discord_gamenewschannelskiptag", 500, transaction);
        }
        await transaction.CommitAsync();
    }

    public async Task SetSkippedGameNewsTags(ulong guildID, ulong channelID, IEnumerable<MasterGameTag> skippedTags)
    {
        var param = new
        {
            guildID,
            channelID
        };
        const string deleteTagsSQL = "delete from tbl_discord_gamenewschannelskiptag where GuildID=@guildID AND ChannelID=@channelID;";

        var tagEntities = skippedTags.Select(x => new GameNewsChannelSkippedTagEntity(guildID, channelID, x));

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteTagsSQL, param, transaction);
        await connection.BulkInsertAsync<GameNewsChannelSkippedTagEntity>(tagEntities, "tbl_discord_gamenewschannelskiptag", 500, transaction);
        await transaction.CommitAsync();
    }

    public async Task SetBidAlertRoleId(Guid leagueID, ulong guildID, ulong channelID, ulong? bidAlertRoleID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var param = new
        {
            LeagueID = leagueID,
            GuildID = guildID,
            ChannelID = channelID,
            BidAlertRoleID = bidAlertRoleID
        };
        var sql = "UPDATE tbl_discord_leaguechannel SET BidAlertRoleID=@BidAlertRoleID WHERE LeagueID=@LeagueID AND GuildID=@GuildID AND ChannelID=@ChannelID";
        await connection.ExecuteAsync(sql, param);
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

    public async Task<bool> DeleteConferenceChannel(ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            guildID,
            channelID
        };
        var sql = "DELETE FROM tbl_discord_conferencechannel WHERE GuildID=@guildID AND ChannelID=@channelID";
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
        var possibleTags = await _masterGameRepo.GetMasterGameTags();

        await using var connection = new MySqlConnection(_connectionString);
        const string channelSQL = "select * from tbl_discord_gamenewschannel";
        const string tagSQL = "select * from tbl_discord_gamenewschannelskiptag";

        var channelEntities = await connection.QueryAsync<GameNewsChannelEntity>(channelSQL);
        var tagEntities = await connection.QueryAsync<GameNewsChannelSkippedTagEntity>(tagSQL);

        var tagLookup = tagEntities.ToLookup(x => new DiscordChannelKey(x.GuildID, x.ChannelID));
        List<GameNewsChannel> gameNewsChannels = new List<GameNewsChannel>();
        foreach (var channelEntity in channelEntities)
        {
            var tagAssociations = tagLookup[new DiscordChannelKey(channelEntity.GuildID, channelEntity.ChannelID)].Select(x => x.TagName).ToList();
            IReadOnlyList<MasterGameTag> tags = possibleTags
                .Where(x => tagAssociations.Contains(x.Name))
                .ToList();
            gameNewsChannels.Add(channelEntity.ToDomain(tags));
        }

        return gameNewsChannels;
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

    public async Task<IReadOnlyList<MinimalConferenceChannel>> GetConferenceChannels(Guid conferenceID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            conferenceID
        };

        const string conferenceChannelSQL = "select * from tbl_discord_conferencechannel WHERE ConferenceID = @conferenceID";

        var conferenceChannels = await connection.QueryAsync<ConferenceChannelEntity>(conferenceChannelSQL, queryObject);
        return conferenceChannels.Select(l => l.ToMinimalDomain()).ToList();
    }

    public async Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, IReadOnlyList<SupportedYear> supportedYears, int? year = null)
    {
        var leagueChannelEntity = await GetLeagueChannelEntity(guildID, channelID);
        if (leagueChannelEntity is null)
        {
            return null;
        }

        LeagueYear? leagueYear = null;

        if (year != null)
        {
            leagueYear = await _combinedDataRepo.GetLeagueYear(leagueChannelEntity.LeagueID, year.Value);
        }
        else
        {
            var league = await _fantasyCriticRepo.GetLeague(leagueChannelEntity.LeagueID);
            if (league is null)
            {
                return null;
            }
            var supportedYear = supportedYears
                .OrderBy(y => y.Year)
                .FirstOrDefault(y => !y.Finished && league.Years.Contains(y.Year));
            if (supportedYear == null)
            {
                return null;
            }

            leagueYear = await _combinedDataRepo.GetLeagueYear(leagueChannelEntity.LeagueID, supportedYear.Year);
        }

        return leagueYear is null
            ? null
            : leagueChannelEntity.ToDomain(leagueYear);
    }

    public async Task<ConferenceChannel?> GetConferenceChannel(ulong guildID, ulong channelID, IReadOnlyList<SupportedYear> supportedYears, int? year = null)
    {
        var conferenceChannelEntity = await GetConferenceChannelEntity(guildID, channelID);
        if (conferenceChannelEntity is null)
        {
            return null;
        }

        ConferenceYear? conferenceYear = null;

        if (year != null)
        {
            conferenceYear = await _conferenceRepo.GetConferenceYear(conferenceChannelEntity.ConferenceID, year.Value);
        }
        else
        {
            var conference = await _conferenceRepo.GetConference(conferenceChannelEntity.ConferenceID);
            if (conference is null)
            {
                return null;
            }
            var supportedYear = supportedYears
                .OrderBy(y => y.Year)
                .FirstOrDefault(y => !y.Finished && conference.Years.Contains(y.Year));
            if (supportedYear == null)
            {
                return null;
            }

            conferenceYear = await _conferenceRepo.GetConferenceYear(conferenceChannelEntity.ConferenceID, supportedYear.Year);
        }

        return conferenceYear is null
            ? null
            : conferenceChannelEntity.ToDomain(conferenceYear);
    }

    public async Task<GameNewsChannel?> GetGameNewsChannel(ulong guildID, ulong channelID)
    {
        var possibleTags = await _masterGameRepo.GetMasterGameTags();

        await using var connection = new MySqlConnection(_connectionString);
        const string channelSQL = "select * from tbl_discord_gamenewschannel WHERE GuildID = @guildID AND ChannelID = @channelID;";
        const string tagSQL = "select * from tbl_discord_gamenewschannelskiptag WHERE GuildID = @guildID AND ChannelID = @channelID;";

        var queryObject = new
        {
            guildID,
            channelID
        };

        var entity = await connection.QuerySingleOrDefaultAsync<GameNewsChannelEntity>(channelSQL, queryObject);
        var tagEntities = await connection.QueryAsync<GameNewsChannelSkippedTagEntity>(tagSQL, queryObject);

        var tagAssociations = tagEntities.Select(x => x.TagName).ToList();
        IReadOnlyList<MasterGameTag> tags = possibleTags
            .Where(x => tagAssociations.Contains(x.Name))
            .ToList();
        return entity?.ToDomain(tags);
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

    private async Task<ConferenceChannelEntity?> GetConferenceChannelEntity(ulong guildID, ulong channelID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            guildID,
            channelID
        };

        const string leagueChannelSQL = "select * from tbl_discord_conferencechannel WHERE GuildID = @guildID AND ChannelID = @channelID";

        var conferenceChannelEntity = await connection.QuerySingleOrDefaultAsync<ConferenceChannelEntity>(leagueChannelSQL, queryObject);
        return conferenceChannelEntity;
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
