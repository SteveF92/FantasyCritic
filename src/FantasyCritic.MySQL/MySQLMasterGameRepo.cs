using System.Data;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.MySQL.Entities;
using Serilog;

namespace FantasyCritic.MySQL;

public class MySQLMasterGameRepo : IMasterGameRepo
{
    private static readonly ILogger _logger = Log.ForContext<MySQLMasterGameRepo>();

    private readonly string _connectionString;
    private readonly IReadOnlyFantasyCriticUserStore _userStore;
    private readonly IClock _clock;

    private IReadOnlyList<MasterGameTag>? _tagCache;
    private Dictionary<Guid, MasterGame>? _masterGamesCache;
    private Dictionary<int, Dictionary<Guid, MasterGameYear>> _masterGameYearsCache;

    public MySQLMasterGameRepo(RepositoryConfiguration configuration, IReadOnlyFantasyCriticUserStore userStore, IClock clock)
    {
        _connectionString = configuration.ConnectionString;
        _userStore = userStore;
        _masterGameYearsCache = new Dictionary<int, Dictionary<Guid, MasterGameYear>>();
        _clock = clock;
    }

    public void ClearMasterGameCache()
    {
        _masterGamesCache = null;
    }

    public void ClearMasterGameYearCache()
    {
        _masterGameYearsCache = new Dictionary<int, Dictionary<Guid, MasterGameYear>>();
    }

    public async Task<IReadOnlyList<MasterGame>> GetMasterGames()
    {
        if (_masterGamesCache is not null)
        {
            return _masterGamesCache.Values.ToList();
        }

        await using var connection = new MySqlConnection(_connectionString);
        var resultsSet = await connection.QueryMultipleAsync("sp_getmastergames", commandType: CommandType.StoredProcedure);
        var masterGameResults = await resultsSet.ReadAsync<MasterGameEntity>();
        var tagResults = await resultsSet.ReadAsync<MasterGameTagEntity>();
        var masterSubGameResults = await resultsSet.ReadAsync<MasterSubGameEntity>();
        var masterGameTagResults = await resultsSet.ReadAsync<MasterGameHasTagEntity>();

        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);

        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
        var masterGames = new List<MasterGame>();
        foreach (var entity in masterGameResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGame domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGames.Add(domain);
        }

        _masterGamesCache = masterGames.ToDictionary(x => x.MasterGameID, y => y);
        return masterGames;
    }

    public async Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year)
    {
        if (_masterGameYearsCache.ContainsKey(year))
        {
            return _masterGameYearsCache[year].Values.ToList();
        }

        await using var connection = new MySqlConnection(_connectionString);
        var resultsSet = await connection.QueryMultipleAsync("sp_getmastergameyears", new { P_Year = year }, commandType: CommandType.StoredProcedure);
        var masterGameResults = await resultsSet.ReadAsync<MasterGameYearEntity>();
        var tagResults = await resultsSet.ReadAsync<MasterGameTagEntity>();
        var masterSubGameResults = await resultsSet.ReadAsync<MasterSubGameEntity>();
        var masterGameTagResults = await resultsSet.ReadAsync<MasterGameHasTagEntity>();

        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);

        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
        List<MasterGameYear> masterGames = new List<MasterGameYear>();
        foreach (var entity in masterGameResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGames.Add(domain);
        }

        _masterGameYearsCache[year] = masterGames.ToDictionary(x => x.MasterGame.MasterGameID, y => y);

        return masterGames;
    }

    public async Task<MasterGame?> GetMasterGame(Guid masterGameID)
    {
        if (_masterGamesCache is null)
        {
            await GetMasterGames();
        }

        return _masterGamesCache!.GetValueOrDefault(masterGameID);
    }

    public async Task<MasterGameYear?> GetMasterGameYear(Guid masterGameID, int year)
    {
        if (!_masterGameYearsCache.ContainsKey(year))
        {
            await GetMasterGameYears(year);
        }

        return _masterGameYearsCache[year].GetValueOrDefault(masterGameID);
    }

    public async Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
    {
        string setFirstTimestamp = "";
        if (!masterGame.FirstCriticScoreTimestamp.HasValue && openCriticGame.Score.HasValue)
        {
            setFirstTimestamp = ", FirstCriticScoreTimestamp = CURRENT_TIMESTAMP ";
        }

        string sql = $"update tbl_mastergame set CriticScore = @criticScore, HasAnyReviews = @hasAnyReviews {setFirstTimestamp}, OpenCriticSlug = @openCriticSlug where MasterGameID = @masterGameID";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql,
            new
            {
                masterGameID = masterGame.MasterGameID,
                criticScore = openCriticGame.Score,
                hasAnyReviews = openCriticGame.HasAnyReviews,
                openCriticSlug = openCriticGame.Slug
            });
    }

    public async Task UpdateGGStats(MasterGame masterGame, GGGame ggGame)
    {
        const string sql = """
                           update tbl_mastergame set GGCoverArtFileName = @ggCoverArtFileName, GGSlug = @ggSlug where MasterGameID = @masterGameID;
                           update tbl_caching_mastergameyear set GGCoverArtFileName = @ggCoverArtFileName, GGSlug = @ggSlug where MasterGameID = @masterGameID;
                           """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql,
            new
            {
                masterGameID = masterGame.MasterGameID,
                ggCoverArtFileName = ggGame.CoverArtFileName,
                ggSlug = ggGame.Slug
            });
    }

    public async Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
    {
        DateTime? releaseDate = null;
        if (openCriticGame.ReleaseDate.HasValue)
        {
            releaseDate = openCriticGame.ReleaseDate.Value.ToDateTimeUnspecified();
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_mastergame_subgame set ReleaseDate = @releaseDate, CriticScore = @criticScore where MasterSubGameID = @masterSubGameID",
            new
            {
                masterSubGameID = masterSubGame.MasterSubGameID,
                releaseDate,
                criticScore = openCriticGame.Score
            });
    }

    public async Task CreateMasterGame(MasterGame masterGame)
    {
        const string masterGameCreateSQL = "insert into tbl_mastergame" +
                                           "(MasterGameID,GameName,EstimatedReleaseDate,MinimumReleaseDate,MaximumReleaseDate,EarlyAccessReleaseDate,InternationalReleaseDate,AnnouncementDate," +
                                           "ReleaseDate,OpenCriticID,GGToken,CriticScore,HasAnyReviews,Notes,BoxartFileName,GGCoverArtFileName," +
                                           "FirstCriticScoreTimestamp,DoNotRefreshDate,DoNotRefreshAnything,UseSimpleEligibility,DelayContention,ShowNote,AddedTimestamp,AddedByUserID) VALUES " +
                                           "(@MasterGameID,@GameName,@EstimatedReleaseDate,@MinimumReleaseDate,@MaximumReleaseDate,@EarlyAccessReleaseDate,@InternationalReleaseDate,@AnnouncementDate," +
                                           "@ReleaseDate,@OpenCriticID,@GGToken,@CriticScore,@HasAnyReviews,@Notes,@BoxartFileName,@GGCoverArtFileName," +
                                           "@FirstCriticScoreTimestamp,@DoNotRefreshDate,@DoNotRefreshAnything,@UseSimpleEligibility,@DelayContention,@ShowNote,@AddedTimestamp,@AddedByUserID);";

        var entity = new MasterGameEntity(masterGame);
        var tagEntities = masterGame.Tags.Select(x => new MasterGameHasTagEntity(masterGame, x));
        var excludeFields = new List<string>() { "TimeAdded" };
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(masterGameCreateSQL, entity, transaction);
        await connection.BulkInsertAsync<MasterGameHasTagEntity>(tagEntities, "tbl_mastergame_hastag", 500, transaction, excludeFields);
        await transaction.CommitAsync();
    }

    public async Task EditMasterGame(MasterGame masterGame, IEnumerable<MasterGameChangeLogEntry> changeLogEntries)
    {
        const string editSQL = "UPDATE tbl_mastergame SET " +
                               "GameName = @GameName, " +
                               "EstimatedReleaseDate = @EstimatedReleaseDate, " +
                               "MinimumReleaseDate = @MinimumReleaseDate, " +
                               "MaximumReleaseDate = @MaximumReleaseDate, " +
                               "EarlyAccessReleaseDate = @EarlyAccessReleaseDate, " +
                               "InternationalReleaseDate = @InternationalReleaseDate, " +
                               "AnnouncementDate = @AnnouncementDate, " +
                               "ReleaseDate = @ReleaseDate, " +
                               "OpenCriticID = @OpenCriticID, " +
                               "GGToken = @GGToken, " +
                               "CriticScore = @CriticScore, " +
                               "Notes = @Notes, " +
                               "BoxartFileName = @BoxartFileName, " +
                               "GGCoverArtFileName = @GGCoverArtFileName, " +
                               "FirstCriticScoreTimestamp = @FirstCriticScoreTimestamp, " +
                               "DoNotRefreshDate = @DoNotRefreshDate, " +
                               "DoNotRefreshAnything = @DoNotRefreshAnything, " +
                               "UseSimpleEligibility = @UseSimpleEligibility, " +
                               "DelayContention = @DelayContention, " +
                               "ShowNote = @ShowNote " +
                               "WHERE MasterGameID = @MasterGameID;";

        const string deleteTagsSQL = "delete from tbl_mastergame_hastag where MasterGameID = @MasterGameID;";

        var entity = new MasterGameEntity(masterGame);
        var changeLogEntities = changeLogEntries.Select(x => new MasterGameChangeLogEntity(x)).ToList();
        var tagEntities = masterGame.Tags.Select(x => new MasterGameHasTagEntity(masterGame, x));

        var excludeFields = new List<string>() { "TimeAdded" };
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(editSQL, entity, transaction);
        await connection.ExecuteAsync(deleteTagsSQL, new { masterGame.MasterGameID }, transaction);
        await connection.BulkInsertAsync<MasterGameChangeLogEntity>(changeLogEntities, "tbl_mastergame_changelog", 500, transaction);
        await connection.BulkInsertAsync<MasterGameHasTagEntity>(tagEntities, "tbl_mastergame_hastag", 500, transaction, excludeFields);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year)
    {
        const string sql = "select distinct MasterGameID from tbl_league_publishergame " +
                        "join tbl_league_publisher on(tbl_league_publisher.PublisherID = tbl_league_publishergame.PublisherID) " +
                        "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                        "where Year = @year and tbl_league.TestLeague = 0 and tbl_league.IsDeleted = 0 and MasterGameID IS NOT NULL;";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<Guid> guids = await connection.QueryAsync<Guid>(sql, new { year });
        return guids.ToList();
    }

    public async Task<IReadOnlyList<MasterGameChangeLogEntry>> GetMasterGameChangeLog(MasterGame masterGame)
    {
        var users = await _userStore.GetAllUsers();
        var userDictionary = users.ToDictionary(x => x.Id);

        const string sql = "select * from tbl_mastergame_changelog where MasterGameID = @MasterGameID order by Timestamp";
        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<MasterGameChangeLogEntity> entities = await connection.QueryAsync<MasterGameChangeLogEntity>(sql, new { masterGame.MasterGameID });

        return entities.Select(entity => entity.ToDomain(masterGame, userDictionary[entity.ChangedByUserID])).ToList();
    }

    public async Task<IReadOnlyList<MasterGameChangeLogEntry>> GetRecentMasterGameChanges()
    {
        var users = await _userStore.GetAllUsers();
        var userDictionary = users.ToDictionary(x => x.Id);

        var masterGames = await GetMasterGames();
        var masterGameDictionary = masterGames.ToDictionary(x => x.MasterGameID);

        const string sql = "select * from tbl_mastergame_changelog order by Timestamp desc limit 100";
        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<MasterGameChangeLogEntity> entities = await connection.QueryAsync<MasterGameChangeLogEntity>(sql);

        var domains = entities
            .Select(entity => entity.ToDomain(masterGameDictionary[entity.MasterGameID], userDictionary[entity.ChangedByUserID]))
            .ToList();

        return domains;
    }

    public async Task CreateMasterGameRequest(MasterGameRequest domainRequest)
    {
        var entity = new MasterGameRequestEntity(domainRequest);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_mastergame_request(RequestID,UserID,RequestTimestamp,RequestNote,GameName,SteamID,OpenCriticID,GGToken,ReleaseDate,EstimatedReleaseDate," +
            "Answered,ResponseTimestamp,ResponseNote,ResponseUserID,MasterGameID,Hidden) VALUES " +
            "(@RequestID,@UserID,@RequestTimestamp,@RequestNote,@GameName,@SteamID,@OpenCriticID,@GGToken,@ReleaseDate,@EstimatedReleaseDate," +
            "@Answered,@ResponseTimestamp,@ResponseNote,@ResponseUserID,@MasterGameID,@Hidden);",
            entity);
    }

    public async Task CreateMasterGameChangeRequest(MasterGameChangeRequest domainRequest)
    {
        var entity = new MasterGameChangeRequestEntity(domainRequest);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_mastergame_changerequest(RequestID,UserID,RequestTimestamp,RequestNote,MasterGameID,OpenCriticID,GGToken,Answered,ResponseTimestamp,ResponseNote,ResponseUserID,Hidden) VALUES " +
            "(@RequestID,@UserID,@RequestTimestamp,@RequestNote,@MasterGameID,@OpenCriticID,@GGToken,@Answered,@ResponseTimestamp,@ResponseNote,@ResponseUserID,@Hidden);",
            entity);
    }

    public async Task<IReadOnlyList<MasterGameRequest>> GetAllMasterGameRequests()
    {
        const string sql = "select * from tbl_mastergame_request where Answered = 0";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<MasterGameRequestEntity> entities = await connection.QueryAsync<MasterGameRequestEntity>(sql);
        return await ConvertMasterGameRequestEntities(entities);
    }

    public async Task<IReadOnlyList<MasterGameChangeRequest>> GetAllMasterGameChangeRequests()
    {
        const string sql = "select * from tbl_mastergame_changerequest where Answered = 0";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<MasterGameChangeRequestEntity> entities = await connection.QueryAsync<MasterGameChangeRequestEntity>(sql);
        return await ConvertMasterGameChangeRequestEntities(entities);
    }

    public async Task<int> GetNumberOutstandingCorrections(MasterGame masterGame)
    {
        const string sql = "select count(*) from tbl_mastergame_changerequest where MasterGameID = @masterGameID AND Answered = 0";

        var queryObject = new
        {
            masterGameID = masterGame.MasterGameID
        };

        await using var connection = new MySqlConnection(_connectionString);
        int count = await connection.QuerySingleAsync<int>(sql, queryObject);
        return count;
    }

    public async Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime,
        string responseNote, FantasyCriticUser responseUser, MasterGame? masterGame)
    {
        Guid? masterGameID = null;
        if (masterGame is not null)
        {
            masterGameID = masterGame.MasterGameID;
        }
        const string sql = "update tbl_mastergame_request set Answered = 1, ResponseTimestamp = @responseTime, " +
                           "ResponseNote = @responseNote, ResponseUserID = @responseUserID, MasterGameID = @masterGameID where RequestID = @requestID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql,
            new
            {
                requestID = masterGameRequest.RequestID,
                masterGameID,
                responseTime = responseTime.ToDateTimeUtc(),
                responseNote,
                responseUserID = responseUser.Id
            });
    }

    public async Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime,
        FantasyCriticUser responseUser, string responseNote)
    {
        const string sql = "update tbl_mastergame_changerequest set Answered = 1, ResponseTimestamp = @responseTime, " +
                           "ResponseNote = @responseNote, ResponseUserID = @responseUserID where RequestID = @requestID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql,
            new
            {
                requestID = masterGameRequest.RequestID,
                responseTime = responseTime.ToDateTimeUtc(),
                responseNote,
                responseUserID = responseUser.Id
            });
    }

    public async Task<IReadOnlyList<MasterGameRequest>> GetMasterGameRequestsForUser(FantasyCriticUser user)
    {
        const string sql = "select * from tbl_mastergame_request where UserID = @userID and Hidden = 0";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<MasterGameRequestEntity> entities = await connection.QueryAsync<MasterGameRequestEntity>(sql, new { userID = user.Id });
        return await ConvertMasterGameRequestEntities(entities);
    }

    public async Task<IReadOnlyList<MasterGameChangeRequest>> GetMasterGameChangeRequestsForUser(FantasyCriticUser user)
    {
        const string sql = "select * from tbl_mastergame_changerequest where UserID = @userID and Hidden = 0";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<MasterGameChangeRequestEntity> entities = await connection.QueryAsync<MasterGameChangeRequestEntity>(sql, new { userID = user.Id });
        return await ConvertMasterGameChangeRequestEntities(entities);
    }

    private async Task<IReadOnlyList<MasterGameRequest>> ConvertMasterGameRequestEntities(IEnumerable<MasterGameRequestEntity> entities)
    {
        var masterGames = await GetMasterGames();
        var users = await _userStore.GetAllUsers();
        var userDictionary = users.ToDictionary(x => x.Id);
        List<MasterGameRequest> domainRequests = new List<MasterGameRequest>();
        foreach (var entity in entities)
        {
            MasterGame? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = masterGames.Single(x => x.MasterGameID == entity.MasterGameID.Value);
            }

            MasterGameRequest domain = entity.ToDomain(userDictionary[entity.UserID], masterGame, entity.ResponseUserID.HasValue ? userDictionary[entity.ResponseUserID.Value] : null);
            domainRequests.Add(domain);
        }

        return domainRequests;
    }

    private async Task<IReadOnlyList<MasterGameChangeRequest>> ConvertMasterGameChangeRequestEntities(IEnumerable<MasterGameChangeRequestEntity> entities)
    {
        var masterGames = await GetMasterGames();
        var users = await _userStore.GetAllUsers();
        var userDictionary = users.ToDictionary(x => x.Id);
        List<MasterGameChangeRequest> domainRequests = new List<MasterGameChangeRequest>();
        foreach (var entity in entities)
        {
            var masterGame = masterGames.Single(x => x.MasterGameID == entity.MasterGameID);
            MasterGameChangeRequest domain = entity.ToDomain(userDictionary[entity.UserID], masterGame, entity.ResponseUserID.HasValue ? userDictionary[entity.ResponseUserID.Value] : null);
            domainRequests.Add(domain);
        }

        return domainRequests;
    }

    public async Task<MasterGameRequest?> GetMasterGameRequest(Guid requestID)
    {
        const string sql = "select * from tbl_mastergame_request where RequestID = @requestID";

        await using var connection = new MySqlConnection(_connectionString);
        MasterGameRequestEntity? entity = await connection.QuerySingleOrDefaultAsync<MasterGameRequestEntity?>(sql, new { requestID });
        if (entity == null)
        {
            return null;
        }

        MasterGame? masterGame = null;
        if (entity.MasterGameID.HasValue)
        {
            masterGame = await GetMasterGame(entity.MasterGameID.Value);
        }

        var user = await _userStore.FindByIdOrThrowAsync(entity.UserID, CancellationToken.None);
        FantasyCriticUser? responseUser = await _userStore.GetUserThatMightExist(entity.ResponseUserID);

        return entity.ToDomain(user, masterGame, responseUser);
    }

    public async Task<MasterGameChangeRequest?> GetMasterGameChangeRequest(Guid requestID)
    {
        const string sql = "select * from tbl_mastergame_changerequest where RequestID = @requestID";

        await using var connection = new MySqlConnection(_connectionString);
        MasterGameChangeRequestEntity? entity = await connection.QuerySingleOrDefaultAsync<MasterGameChangeRequestEntity?>(sql, new { requestID });
        if (entity == null)
        {
            return null;
        }

        var masterGame = await GetMasterGame(entity.MasterGameID);

        if (masterGame is null)
        {
            throw new Exception($"Something has gone horribly wrong with master game change requests. ID: {requestID}");
        }

        var user = await _userStore.FindByIdOrThrowAsync(entity.UserID, CancellationToken.None);
        FantasyCriticUser? responseUser = await _userStore.GetUserThatMightExist(entity.ResponseUserID);

        return entity.ToDomain(user, masterGame, responseUser);
    }



    public async Task DeleteMasterGameRequest(MasterGameRequest request)
    {
        var deleteObject = new
        {
            requestID = request.RequestID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "delete from tbl_mastergame_request where RequestID = @requestID;",
            deleteObject);
    }

    public async Task DeleteMasterGameChangeRequest(MasterGameChangeRequest request)
    {
        var deleteObject = new
        {
            requestID = request.RequestID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "delete from tbl_mastergame_changerequest where RequestID = @requestID;",
            deleteObject);
    }

    public async Task DismissMasterGameRequest(MasterGameRequest masterGameRequest)
    {
        var dismissObject = new
        {
            requestID = masterGameRequest.RequestID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "update tbl_mastergame_request SET Hidden = 1 where RequestID = @requestID;",
            dismissObject);
    }

    public async Task DismissMasterGameChangeRequest(MasterGameChangeRequest request)
    {
        var dismissObject = new
        {
            requestID = request.RequestID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "update tbl_mastergame_changerequest SET Hidden = 1 where RequestID = @requestID;",
            dismissObject);
    }

    public async Task LinkToOpenCritic(MasterGame masterGame, int openCriticID)
    {
        var linkObject = new
        {
            masterGameID = masterGame.MasterGameID,
            openCriticID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "update tbl_mastergame SET OpenCriticID = @openCriticID where MasterGameID = @masterGameID;",
            linkObject);
    }

    public async Task LinkToGG(MasterGame masterGame, string ggToken)
    {
        var linkObject = new
        {
            masterGameID = masterGame.MasterGameID,
            ggToken
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "update tbl_mastergame SET GGToken = @ggToken where MasterGameID = @masterGameID;",
            linkObject);
    }

    public async Task UpdateReleaseDateEstimates(LocalDate tomorrow)
    {
        _logger.Information("Updating Release Date Estimates");

        const string sql = "UPDATE tbl_mastergame SET MinimumReleaseDate = ReleaseDate, MaximumReleaseDate = ReleaseDate, EstimatedReleaseDate = ReleaseDate where ReleaseDate is not NULL;";
        const string sql2 = "UPDATE tbl_mastergame SET MinimumReleaseDate = @tomorrow WHERE MinimumReleaseDate < @tomorrow AND ReleaseDate IS NULL;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(sql, transaction: transaction);
        await connection.ExecuteAsync(sql2, new { tomorrow = tomorrow.ToDateTimeUnspecified() }, transaction);
        await transaction.CommitAsync();
    }

    public async Task UpdateCalculatedStats(IEnumerable<MasterGameCalculatedStats> calculatedStats, int year)
    {
        List<MasterGameYearEntity> masterGameYearEntities = calculatedStats.Select(x => new MasterGameYearEntity(x)).ToList();

        var excludeFields = new List<string>() { "DoNotRefreshDate", "DoNotRefreshAnything", "AddedByUserDisplayName" };
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync("delete from tbl_caching_mastergameyear where Year = @year", new { year }, transaction);
        await connection.BulkInsertAsync<MasterGameYearEntity>(masterGameYearEntities, "tbl_caching_mastergameyear", 500, transaction, excludeFields);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<MasterGameTag>> GetMasterGameTags()
    {
        if (_tagCache is not null)
        {
            return _tagCache;
        }

        const string sql = "select * from tbl_mastergame_tag;";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<MasterGameTagEntity> entities = await connection.QueryAsync<MasterGameTagEntity>(sql);
        var tags = entities.Select(x => x.ToDomain()).ToList();
        _tagCache = tags;
        return tags;
    }

    public async Task<IReadOnlyDictionary<string, MasterGameTag>> GetMasterGameTagDictionary()
    {
        var tags = await GetMasterGameTags();
        return tags.ToDictionary(x => x.Name);
    }

    public async Task UpdateCodeBasedTags(IReadOnlyDictionary<MasterGame, IReadOnlyList<MasterGameTag>> tagsToAdd)
    {
        const string deleteExistingTagsSQL = "DELETE tbl_mastergame_hastag FROM tbl_mastergame_hastag " +
                                             "JOIN tbl_mastergame_tag ON tbl_mastergame_hastag.TagName = tbl_mastergame_tag.Name " +
                                             "JOIN tbl_mastergame ON tbl_mastergame_hastag.MasterGameID = tbl_mastergame.MasterGameID " +
                                             "WHERE tbl_mastergame_tag.HasCustomCode " +
                                             "AND (EarlyAccessReleaseDate IS NOT NULL OR InternationalReleaseDate IS NOT NULL)";

        var tagEntities = tagsToAdd
            .SelectMany(masterGame => masterGame.Value, (masterGame, tag) => new MasterGameHasTagEntity(masterGame.Key, tag))
            .ToList();

        var excludeFields = new List<string>() { "TimeAdded" };
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteExistingTagsSQL, transaction: transaction);
        await connection.BulkInsertAsync<MasterGameHasTagEntity>(tagEntities, "tbl_mastergame_hastag", 500, transaction, excludeFields);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<LocalDate>> GetProcessingDatesForTopBidsAndDrops()
    {
        const string sql = "select distinct ProcessDate from tbl_caching_topbidsanddrops";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<LocalDate> dates = await connection.QueryAsync<LocalDate>(sql);
        return dates.ToList();
    }

    public async Task<IReadOnlyList<TopBidsAndDropsGame>> GetTopBidsAndDrops(LocalDate processingDate)
    {
        const string sql = "select * from tbl_caching_topbidsanddrops where ProcessDate = @processingDate";
        var param = new
        {
            processingDate
        };

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<TopBidsAndDropsEntity> entities = await connection.QueryAsync<TopBidsAndDropsEntity>(sql, param);

        List<TopBidsAndDropsGame> domains = new List<TopBidsAndDropsGame>();

        foreach (var entity in entities)
        {
            if (!_masterGameYearsCache.ContainsKey(entity.Year))
            {
                await GetMasterGameYears(entity.Year);
            }

            var masterGameYear = _masterGameYearsCache[entity.Year][entity.MasterGameID];
            domains.Add(entity.ToDomain(masterGameYear));
        }

        return domains;
    }

    public async Task<IReadOnlyList<LeagueYearWithMasterGame>> GetLeagueYearsWithMasterGameForUser(Guid userID,
        Guid masterGameID)
    {
        const string sql = "SELECT p.LeagueID, l.LeagueName, p.`Year`, pg.CounterPick FROM tbl_league_publishergame pg " +
                           "JOIN tbl_league_publisher p ON pg.PublisherID = p.PublisherID " +
                           "JOIN tbl_league l ON p.LeagueID = l.LeagueID " +
                           "WHERE p.UserID = @userID " +
                           "AND pg.MasterGameID=@masterGameID;";

        await using var connection = new MySqlConnection(_connectionString);
        var leagueYearsWithMasterGameEntities = await connection.QueryAsync<LeagueYearsWithMasterGameEntity>(sql, new { userID = userID, masterGameID = masterGameID });
        return leagueYearsWithMasterGameEntities.Select(l => l.ToDomain()).ToList();
    }

    public async Task<MasterGame> GetTestMasterGame(int year)
    {
        var masterGameYear = await GetTestMasterGameYear(year);

        return masterGameYear.MasterGame;
    }

    public async Task<MasterGameYear> GetTestMasterGameYear(int year)
    {
        var allMasterGameYears = await GetMasterGameYears(year);

        var mostPopularUnreleasedGame = allMasterGameYears
            .Where(x => !x.MasterGame.IsReleased(_clock.GetToday()))
            .Where(x => x.MasterGame.ReleaseDate.HasValue)
            .MaxBy(x => x.DateAdjustedHypeFactor);

        return mostPopularUnreleasedGame ?? throw new Exception("No unreleased games found for the test master game year.");
    }
}
