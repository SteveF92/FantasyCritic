using System.Data;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Domain.Calculations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.MySQL.Entities.Identity;
using FantasyCritic.MySQL.Entities.Trades;
using Serilog;
using FantasyCritic.SharedSerialization.Database;
using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.MySQL;

public class MySQLFantasyCriticRepo : IFantasyCriticRepo
{
    private static readonly ILogger _logger = Log.ForContext<MySQLFantasyCriticRepo>();

    private readonly string _connectionString;
    private readonly IReadOnlyFantasyCriticUserStore _userStore;
    private readonly IMasterGameRepo _masterGameRepo;

    private IReadOnlyList<SupportedYear>? _supportedYearCache;

    public MySQLFantasyCriticRepo(RepositoryConfiguration configuration, IReadOnlyFantasyCriticUserStore userStore, IMasterGameRepo masterGameRepo)
    {
        _connectionString = configuration.ConnectionString;
        _userStore = userStore;
        _masterGameRepo = masterGameRepo;
    }

    public async Task<League?> GetLeague(Guid id)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            leagueID = id
        };

        const string leagueSQL = "select * from vw_league where LeagueID = @leagueID and IsDeleted = 0;";
        LeagueEntity leagueEntity = await connection.QuerySingleOrDefaultAsync<LeagueEntity>(leagueSQL, queryObject);
        if (leagueEntity is null)
        {
            return null;
        }

        FantasyCriticUser manager = await _userStore.FindByIdOrThrowAsync(leagueEntity.LeagueManager, CancellationToken.None);

        const string leagueYearSQL = "select Year from tbl_league_year where LeagueID = @leagueID;";
        IEnumerable<int> years = await connection.QueryAsync<int>(leagueYearSQL, queryObject);
        League league = leagueEntity.ToDomain(manager, years);
        return league;
    }

    private async Task<IReadOnlyList<League>> GetAllLeagues(bool includeDeleted = false)
    {
        await using var connection = new MySqlConnection(_connectionString);
        string sql = "select * from vw_league where IsDeleted = 0;";
        if (includeDeleted)
        {
            sql = "select * from vw_league;";
        }

        var leagueEntities = await connection.QueryAsync<LeagueEntity>(sql);

        IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tbl_league_year");
        var leagueYearLookup = yearEntities.ToLookup(x => x.LeagueID);
        List<League> leagues = new List<League>();
        var allUsers = await _userStore.GetAllUsers();
        var userDictionary = allUsers.ToDictionary(x => x.Id);
        var oneShotLeagues = await GetLeaguesWithMostRecentYearOneShot();
        foreach (var leagueEntity in leagueEntities)
        {
            FantasyCriticUser manager = userDictionary[leagueEntity.LeagueManager];
            IEnumerable<int> years = leagueYearLookup[leagueEntity.LeagueID].Select(x => x.Year);
            League league = leagueEntity.ToDomain(manager, years);
            leagues.Add(league);
        }

        return leagues;
    }

    public async Task<LeagueYear?> GetLeagueYear(Guid leagueID, int year)
    {
        var param = new
        {
            P_LeagueID = leagueID,
            P_Year = year
        };

        await using var connection = new MySqlConnection(_connectionString);
        using var resultSets = await connection.QueryMultipleAsync("sp_getleagueyear", param, commandType: CommandType.StoredProcedure);
        var leagueEntity = await resultSets.ReadSingleOrDefaultAsync<LeagueEntity>();
        if (leagueEntity is null)
        {
            return null;
        }

        var years = await resultSets.ReadAsync<int>();
        var leagueYearEntity = await resultSets.ReadSingleOrDefaultAsync<LeagueYearEntity>();
        if (leagueYearEntity is null)
        {
            return null;
        }

        var leagueTagEntities = await resultSets.ReadAsync<LeagueYearTagEntity>();
        var specialGameSlotEntities = await resultSets.ReadAsync<SpecialGameSlotEntity>();
        var eligibilityOverrideEntities = await resultSets.ReadAsync<EligibilityOverrideEntity>();
        var tagOverrideEntities = await resultSets.ReadAsync<TagOverrideEntity>();
        var usersInLeagueEntities = await resultSets.ReadAsync<FantasyCriticUserEntity>();
        var publisherEntities = await resultSets.ReadAsync<PublisherEntity>();
        var publisherGameEntities = await resultSets.ReadAsync<PublisherGameEntity>();
        var formerPublisherGameEntities = await resultSets.ReadAsync<FormerPublisherGameEntity>();

        var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
        var supportedYear = await GetSupportedYear(year);
        var winningUser = await _userStore.GetUserThatMightExist(leagueYearEntity.WinningUserID);
        var manager = await _userStore.FindByIdOrThrowAsync(leagueEntity.LeagueManager, CancellationToken.None);

        var league = leagueEntity.ToDomain(manager, years);
        var leagueYearKey = new LeagueYearKey(leagueID, year);
        var domainLeagueTags = ConvertLeagueTagEntities(leagueTagEntities, tagDictionary);
        var domainSpecialGameSlots = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(specialGameSlotEntities, tagDictionary);
        var specialGameSlotsForLeagueYear = domainSpecialGameSlots[leagueYearKey];
        var domainEligibilityOverrides = await ConvertEligibilityOverrideEntities(eligibilityOverrideEntities);
        var domainTagOverrides = await ConvertTagOverrideEntities(tagOverrideEntities, tagDictionary);
        var publishers = await ConvertPublisherEntities(usersInLeagueEntities, publisherEntities, publisherGameEntities,
            formerPublisherGameEntities, year);
        
        LeagueYear leagueYear = leagueYearEntity.ToDomain(league, supportedYear, domainEligibilityOverrides, domainTagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear,
            winningUser, publishers);
        return leagueYear;
    }

    public async Task<LeagueYearKey?> GetLeagueYearKeyForPublisherID(Guid publisherID)
    {
        const string sql = "select LeagueID, Year from tbl_league_publisher where PublisherID = @publisherID";
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            publisherID
        };

        LeagueYearKeyEntity entity = await connection.QuerySingleOrDefaultAsync<LeagueYearKeyEntity>(sql, queryObject);
        if (entity is null)
        {
            return null;
        }

        return new LeagueYearKey(entity.LeagueID, entity.Year);
    }

    public async Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year, bool includeDeleted = false)
    {
        var allLeagueTags = await GetLeagueYearTagEntities(year);
        var allSpecialGameSlots = await GetSpecialGameSlotEntities(year);
        var leagueTagsByLeague = allLeagueTags.ToLookup(x => x.LeagueID);
        var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
        var domainSpecialGameSlots = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(allSpecialGameSlots, tagDictionary);
        var supportedYear = await GetSupportedYear(year);
        var allEligibilityOverrides = await GetAllEligibilityOverrides(year);
        var allTagOverrides = await GetAllTagOverrides(year);
        var leagues = await GetAllLeagues(includeDeleted);
        var leaguesDictionary = leagues.ToDictionary(x => x.LeagueID);
        var allPublishersForYear = await GetAllPublishersForYear(year, includeDeleted);
        var publisherLookup = allPublishersForYear.ToLookup(x => x.LeagueYearKey);

        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            year
        };

        IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tbl_league_year where Year = @year", queryObject);
        List<LeagueYear> leagueYears = new List<LeagueYear>();

        foreach (var entity in yearEntities)
        {
            if (!leaguesDictionary.TryGetValue(entity.LeagueID, out var league))
            {
                _logger.Debug($"Cannot find league (probably deleted) LeagueID: {entity.LeagueID}");
                continue;
            }

            if (!allEligibilityOverrides.TryGetValue(entity.LeagueID, out var eligibilityOverrides))
            {
                eligibilityOverrides = new List<EligibilityOverride>();
            }

            if (!allTagOverrides.TryGetValue(entity.LeagueID, out var tagOverrides))
            {
                tagOverrides = new List<TagOverride>();
            }

            var leagueYearKey = new LeagueYearKey(entity.LeagueID, entity.Year);
            var domainLeagueTags = ConvertLeagueTagEntities(leagueTagsByLeague[entity.LeagueID], tagDictionary);
            var specialGameSlotsForLeagueYear = domainSpecialGameSlots[leagueYearKey];

            var winningUser = await _userStore.GetUserThatMightExist(entity.WinningUserID);
            var publishers = publisherLookup[leagueYearKey];
            LeagueYear leagueYear = entity.ToDomain(league, supportedYear, eligibilityOverrides, tagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear, winningUser, publishers);
            leagueYears.Add(leagueYear);
        }

        return leagueYears;
    }

    public async Task<IReadOnlyList<LeagueYear>> GetActiveLeagueYears(IEnumerable<Guid> leagueIDs)
    {
        var supportedYears = await GetSupportedYears();
        var activeYears = supportedYears.Where(x => !x.Finished).ToList();
        List<LeagueYear> requestedLeagueYears = new List<LeagueYear>();

        var leagueIDSet = leagueIDs.ToHashSet();
        foreach (var activeYear in activeYears)
        {
            var allLeagueYears = await GetLeagueYears(activeYear.Year);
            var requestedLeagueYearsForYear = allLeagueYears.Where(x => leagueIDSet.Contains(x.League.LeagueID));
            requestedLeagueYears.AddRange(requestedLeagueYearsForYear);
        }

        return requestedLeagueYears;
    }

    public async Task UpdatePublisherGameCalculatedStats(IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> calculatedStats)
    {
        const string sql = "update tbl_league_publishergame SET FantasyPoints = @FantasyPoints where PublisherGameID = @PublisherGameID;";
        List<PublisherGameUpdateEntity> updateEntities = calculatedStats.Select(x => new PublisherGameUpdateEntity(x)).ToList();
        var updateBatches = updateEntities.Chunk(1000).ToList();
        var longTimeoutConnectionString = ConnectionStringUtilities.GetLongTimeoutConnectionString(_connectionString, Duration.FromSeconds(60));
        await using var connection = new MySqlConnection(longTimeoutConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        for (var index = 0; index < updateBatches.Count; index++)
        {
            _logger.Information($"Running publisher game update {index + 1}/{updateBatches.Count}");
            var batch = updateBatches[index];
            await connection.ExecuteAsync(sql, batch, transaction);
        }

        await transaction.CommitAsync();
    }

    public async Task UpdateLeagueWinners(IReadOnlyDictionary<LeagueYearKey, FantasyCriticUser> winningUsers)
    {
        const string sql = "update tbl_league_year SET WinningUserID = @WinningUserID WHERE LeagueID = @LeagueID AND Year = @Year AND WinningUserID is null;";
        List<LeagueYearWinnerUpdateEntity> updateEntities = winningUsers.Select(x => new LeagueYearWinnerUpdateEntity(x)).ToList();
        var updateBatches = updateEntities.Chunk(1000).ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        for (var index = 0; index < updateBatches.Count; index++)
        {
            _logger.Information($"Running league year winner update {index + 1}/{updateBatches.Count}");
            var batch = updateBatches[index];
            await connection.ExecuteAsync(sql, batch, transaction);
        }

        await transaction.CommitAsync();
    }

    public async Task FullyRemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame)
    {
        const string sql = "delete from tbl_league_publishergame where PublisherGameID = @publisherGameID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var removed = await connection.ExecuteAsync(sql, new { publisherGameID = publisherGame.PublisherGameID }, transaction);
        if (removed != 1)
        {
            await transaction.RollbackAsync();
        }

        await MakePublisherGameSlotsConsistent(leagueYear, publisher, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task ManagerRemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame, FormerPublisherGame formerPublisherGame, LeagueAction leagueAction)
    {
        const string sql = "delete from tbl_league_publishergame where PublisherGameID = @publisherGameID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var removed = await connection.ExecuteAsync(sql, new { publisherGameID = publisherGame.PublisherGameID }, transaction);
        if (removed != 1)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Removing game failed: {publisher.PublisherID} | {publisherGame.GameName}");
        }

        await MakePublisherGameSlotsConsistent(leagueYear, publisher, connection, transaction);
        await AddFormerPublisherGames(new List<FormerPublisherGame>() { formerPublisherGame }, connection, transaction);
        await AddLeagueAction(leagueAction, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task SuperDropGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame, FormerPublisherGame formerPublisherGame, LeagueAction leagueAction)
    {
        const string sql = "delete from tbl_league_publishergame where PublisherGameID = @publisherGameID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var removed = await connection.ExecuteAsync(sql, new { publisherGameID = publisherGame.PublisherGameID }, transaction);
        if (removed != 1)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Removing game failed: {publisher.PublisherID} | {publisherGame.GameName}");
        }

        await MakePublisherGameSlotsConsistent(leagueYear, publisher, connection, transaction);
        await AddFormerPublisherGames(new List<FormerPublisherGame>() { formerPublisherGame }, connection, transaction);
        await AddLeagueAction(leagueAction, connection, transaction);
        await DecrementSuperDropsAvailable(publisher, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
    {
        const string sql = "update tbl_league_publishergame SET ManualCriticScore = @manualCriticScore where PublisherGameID = @publisherGameID;";
        var updateObject = new { publisherGameID = publisherGame.PublisherGameID, manualCriticScore };
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, updateObject);
    }

    public async Task ManuallySetWillNotRelease(PublisherGame publisherGame, bool willNotRelease)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_league_publishergame SET ManualWillNotRelease = @willNotRelease where PublisherGameID = @publisherGameID;",
            new { publisherGameID = publisherGame.PublisherGameID, willNotRelease });
    }

    public async Task CreatePickupBid(PickupBid currentBid)
    {
        var entity = new PickupBidEntity(currentBid);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_league_pickupbid(BidID,PublisherID,MasterGameID,ConditionalDropMasterGameID,CounterPick,Timestamp,Priority,BidAmount,AllowIneligibleSlot,Successful) VALUES " +
            "(@BidID,@PublisherID,@MasterGameID,@ConditionalDropMasterGameID,@CounterPick,@Timestamp,@Priority,@BidAmount,@AllowIneligibleSlot,@Successful);",
            entity);
    }

    public async Task EditPickupBid(PickupBid bid, PublisherGame? conditionalDropPublisherGame, uint bidAmount, bool allowIneligibleSlot)
    {
        string sql =
            """
            UPDATE tbl_league_pickupbid SET
            ConditionalDropMasterGameID = @ConditionalDropMasterGameID,
            BidAmount = @BidAmount,
            AllowIneligibleSlot = @AllowIneligibleSlot
            WHERE BidID = @BidID;
            """;

        var entity = new PickupBidEntity(bid, conditionalDropPublisherGame, bidAmount);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task<FantasyCriticUser?> GetLeagueYearWinner(Guid leagueID, int year)
    {
        const string sql = "select * from tbl_league_year where LeagueID = @leagueID and Year = @year";
        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            leagueID,
            year
        };

        LeagueYearEntity yearEntity = await connection.QuerySingleOrDefaultAsync<LeagueYearEntity>(sql, queryObject);
        if (yearEntity == null)
        {
            return null;
        }

        var winningUser = await _userStore.GetUserThatMightExist(yearEntity.WinningUserID);
        return winningUser;
    }

    public async Task RemovePickupBid(PickupBid pickupBid)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync("delete from tbl_league_pickupbid where BidID = @BidID",
            new { pickupBid.BidID }, transaction);
        await connection.ExecuteAsync(
            "update tbl_league_pickupbid SET Priority = Priority - 1 where PublisherID = @publisherID and Successful is NULL and Priority > @oldPriority",
            new { publisherID = pickupBid.Publisher.PublisherID, oldPriority = pickupBid.Priority }, transaction);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<PickupBid>> GetActivePickupBids(LeagueYear leagueYear, Publisher publisher)
    {
        var publisherGameDictionary = publisher.PublisherGames
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = publisher.FormerPublisherGames
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        const string sql = "select * from vw_league_pickupbid where PublisherID = @publisherID and Successful is NULL";
        var queryObject = new
        {
            publisherID = publisher.PublisherID
        };
        await using var connection = new MySqlConnection(_connectionString);
        var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, queryObject);
        List<PickupBid> domainBids = new List<PickupBid>();
        foreach (var bidEntity in bidEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(bidEntity.MasterGameID);
            PublisherGame? conditionalDropPublisherGame = await GetConditionalDropPublisherGame(bidEntity, leagueYear.Year, publisherGameDictionary, formerPublisherGameDictionary);
            PickupBid domain = bidEntity.ToDomain(publisher, masterGame, conditionalDropPublisherGame, leagueYear);
            domainBids.Add(domain);
        }

        return domainBids;
    }

    public async Task<IReadOnlyList<PickupBid>> GetActivePickupBids(LeagueYear leagueYear)
    {
        var publisherDictionary = leagueYear.Publishers.ToDictionary(x => x.PublisherID);

        var publisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.FormerPublisherGames)
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        const string sql = "select * from vw_league_pickupbid where LeagueID = @leagueID and Year = @year and Successful is NULL";
        var queryObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year,
        };
        await using var connection = new MySqlConnection(_connectionString);
        var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, queryObject);
        List<PickupBid> domainBids = new List<PickupBid>();
        foreach (var bidEntity in bidEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(bidEntity.MasterGameID);
            PublisherGame? conditionalDropPublisherGame = await GetConditionalDropPublisherGame(bidEntity, leagueYear.Year, publisherGameDictionary, formerPublisherGameDictionary);
            var publisher = publisherDictionary[bidEntity.PublisherID];
            PickupBid domain = bidEntity.ToDomain(publisher, masterGame, conditionalDropPublisherGame, leagueYear);
            domainBids.Add(domain);
        }

        return domainBids;
    }

    public async Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year, IReadOnlyList<LeagueYear> leagueYears)
    {
        var leagueYearDictionary = leagueYears.ToDictionary(x => x.Key);
        var allPublishersForYear = leagueYears.SelectMany(x => x.Publishers).ToList();
        var publisherDictionary = allPublishersForYear.ToDictionary(x => x.PublisherID);

        var publisherGameDictionary = allPublishersForYear
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = allPublishersForYear
            .SelectMany(x => x.FormerPublisherGames)
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        const string sql = "select * from vw_league_pickupbid where Year = @year AND Successful is NULL";
        var queryObject = new
        {
            year
        };
        await using var connection = new MySqlConnection(_connectionString);
        var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, queryObject);
        List<PickupBid> domainBids = new List<PickupBid>();
        foreach (var bidEntity in bidEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(bidEntity.MasterGameID);
            var publisher = publisherDictionary[bidEntity.PublisherID];
            var leagueYear = leagueYearDictionary[publisher.LeagueYearKey];
            PublisherGame? conditionalDropPublisherGame = await GetConditionalDropPublisherGame(bidEntity, leagueYear.Year, publisherGameDictionary, formerPublisherGameDictionary);
            PickupBid domain = bidEntity.ToDomain(publisher, masterGame, conditionalDropPublisherGame, leagueYear);
            domainBids.Add(domain);
        }

        var lookup = domainBids.ToLookup(x => x.LeagueYear);
        Dictionary<LeagueYear, List<PickupBid>> finalDictionary = new Dictionary<LeagueYear, List<PickupBid>>();
        foreach (var leagueYear in leagueYears)
        {
            finalDictionary[leagueYear] = lookup[leagueYear].ToList();
        }

        return finalDictionary.SealDictionary();
    }

    public async Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(int year, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        var processedBids = await GetPickupBids(year, allLeagueYears, true);
        var list = processedBids.SelectMany(x => x.Value).ToList();
        return list;
    }

    public async Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(LeagueYear leagueYear)
    {
        var publisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = leagueYear.Publishers
            .SelectMany(x => x.FormerPublisherGames)
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        const string sql = "select * from vw_league_pickupbid where LeagueID = @leagueID and Year = @year and Successful IS NOT NULL";
        var queryObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year
        };

        var publisherDictionary = leagueYear.Publishers.ToDictionary(x => x.PublisherID);

        await using var connection = new MySqlConnection(_connectionString);
        var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, queryObject);
        List<PickupBid> domainBids = new List<PickupBid>();
        foreach (var bidEntity in bidEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(bidEntity.MasterGameID);
            PublisherGame? conditionalDropPublisherGame = await GetConditionalDropPublisherGame(bidEntity, leagueYear.Year, publisherGameDictionary, formerPublisherGameDictionary);
            PickupBid domain = bidEntity.ToDomain(publisherDictionary[bidEntity.PublisherID], masterGame, conditionalDropPublisherGame, leagueYear);
            domainBids.Add(domain);
        }

        return domainBids;
    }

    private async Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetPickupBids(int year, IReadOnlyList<LeagueYear> allLeagueYears, bool processed)
    {
        var leagueYearDictionary = allLeagueYears.ToDictionary(x => x.Key);
        var allPublishersForYear = allLeagueYears.SelectMany(x => x.Publishers).ToList();
        var publisherDictionary = allPublishersForYear.ToDictionary(x => x.PublisherID, y => y);

        string sql = "select * from vw_league_pickupbid where Year = @year and Successful is NULL and IsDeleted = 0";
        if (processed)
        {
            sql = "select * from vw_league_pickupbid where Year = @year and Successful is NOT NULL and IsDeleted = 0";
        }

        var publisherGameDictionary = allPublishersForYear
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = allPublishersForYear
            .SelectMany(x => x.FormerPublisherGames)
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        await using var connection = new MySqlConnection(_connectionString);
        var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, new { year });

        Dictionary<LeagueYear, List<PickupBid>> pickupBidsByLeagueYear = allLeagueYears.ToDictionary(x => x, _ => new List<PickupBid>());

        foreach (var bidEntity in bidEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(bidEntity.MasterGameID);
            var publisher = publisherDictionary[bidEntity.PublisherID];
            var leagueYear = leagueYearDictionary[publisher.LeagueYearKey];
            PublisherGame? conditionalDropPublisherGame = await GetConditionalDropPublisherGame(bidEntity, leagueYear.Year, publisherGameDictionary, formerPublisherGameDictionary);
            PickupBid domainPickup = bidEntity.ToDomain(publisher, masterGame, conditionalDropPublisherGame, leagueYear);
            pickupBidsByLeagueYear[leagueYear].Add(domainPickup);
        }

        IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> finalDictionary = pickupBidsByLeagueYear.SealDictionary();
        return finalDictionary;
    }

    public async Task<IReadOnlyList<DropRequest>> GetProcessedDropRequests(LeagueYear leagueYear)
    {
        const string sql = "select * from vw_league_droprequest where LeagueID = @leagueID and Year = @year and Successful IS NOT NULL";
        var queryObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year
        };

        var publisherDictionary = leagueYear.Publishers.ToDictionary(x => x.PublisherID);

        await using var connection = new MySqlConnection(_connectionString);
        var dropEntities = await connection.QueryAsync<DropRequestEntity>(sql, queryObject);
        List<DropRequest> domainDrops = new List<DropRequest>();
        foreach (var dropEntity in dropEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(dropEntity.MasterGameID);
            DropRequest domain = dropEntity.ToDomain(publisherDictionary[dropEntity.PublisherID], masterGame, leagueYear);
            domainDrops.Add(domain);
        }

        return domainDrops;
    }

    public async Task<DropRequest?> GetDropRequest(Guid dropRequestID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var dropRequestEntity = await connection.QuerySingleOrDefaultAsync<DropRequestEntity>("select * from tbl_league_droprequest where DropRequestID = @dropRequestID", new { dropRequestID });
        if (dropRequestEntity == null)
        {
            return null;
        }

        var publisher = await GetPublisherOrThrow(dropRequestEntity.PublisherID);
        var masterGame = await _masterGameRepo.GetMasterGameOrThrow(dropRequestEntity.MasterGameID);
        var leagueYear = await this.GetLeagueYearOrThrow(publisher.LeagueYearKey.LeagueID, publisher.LeagueYearKey.Year);

        DropRequest domain = dropRequestEntity.ToDomain(publisher, masterGame, leagueYear);
        return domain;
    }

    public async Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var queuedEntities = await connection.QueryAsync<QueuedGameEntity>("select * from tbl_league_publisherqueue where PublisherID = @publisherID",
            new { publisherID = publisher.PublisherID });

        List<QueuedGame> domainQueue = new List<QueuedGame>();
        foreach (var queuedEntity in queuedEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(queuedEntity.MasterGameID);

            QueuedGame domain = queuedEntity.ToDomain(publisher, masterGame);
            domainQueue.Add(domain);
        }

        return domainQueue;
    }

    public async Task QueueGame(QueuedGame queuedGame)
    {
        var entity = new QueuedGameEntity(queuedGame);
        const string sql = "insert into tbl_league_publisherqueue(PublisherID,MasterGameID,`Ranking`) VALUES (@PublisherID,@MasterGameID,@Ranking);";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task RemoveQueuedGame(QueuedGame queuedGame)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        const string deleteSQL = "delete from tbl_league_publisherqueue where PublisherID = @PublisherID AND MasterGameID = @MasterGameID";
        const string alterRankSQL = "update tbl_league_pickupbid SET Priority = Priority - 1 where PublisherID = @publisherID and Successful is NULL and Priority > @oldPriority";
        await connection.ExecuteAsync(deleteSQL, new { queuedGame.Publisher.PublisherID, queuedGame.MasterGame.MasterGameID }, transaction);
        await connection.ExecuteAsync(alterRankSQL, new { publisherID = queuedGame.Publisher.PublisherID, oldPriority = queuedGame.Rank }, transaction);
        await transaction.CommitAsync();
    }

    public async Task SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks)
    {
        int tempPosition = queueRanks.Select(x => x.Value).Max() + 1;
        const string updateSQL = "update tbl_league_publisherqueue set `Ranking` = @ranking where `PublisherID` = @PublisherID AND `MasterGameID` = @MasterGameID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        foreach (var queuedGame in queueRanks)
        {
            var tempParams = new
            {
                queuedGame.Key.MasterGame.MasterGameID,
                queuedGame.Key.Publisher.PublisherID,
                ranking = tempPosition
            };
            await connection.ExecuteAsync(updateSQL, tempParams, transaction);
            tempPosition++;
        }


        foreach (var queuedGame in queueRanks)
        {
            var realParams = new
            {
                queuedGame.Key.MasterGame.MasterGameID,
                queuedGame.Key.Publisher.PublisherID,
                ranking = queuedGame.Value
            };
            await connection.ExecuteAsync(updateSQL, realParams, transaction);
        }

        await transaction.CommitAsync();
    }

    public async Task AddLeagueAction(LeagueAction action)
    {
        LeagueActionEntity entity = new LeagueActionEntity(action);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_league_action(PublisherID,Timestamp,ActionType,Description,ManagerAction) VALUES " +
            "(@PublisherID,@Timestamp,@ActionType,@Description,@ManagerAction);", entity);
    }

    public async Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
    {
        string sql =
            """
            select tbl_league_action.PublisherID,
                   tbl_league_action.Timestamp,
                   tbl_league_action.ActionType,
                   tbl_league_action.Description,
                   tbl_league_action.ManagerAction
            from   tbl_league_action
                   join tbl_league_publisher
                     on ( tbl_league_action.PublisherID = tbl_league_publisher.PublisherID )
            where  tbl_league_publisher.LeagueID = @leagueID
                   and tbl_league_publisher.Year = @leagueYear; 
            """;

        var param = new
        {
            leagueID = leagueYear.League.LeagueID,
            leagueYear = leagueYear.Year
        };

        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<LeagueActionEntity>(sql, param);

        List<LeagueAction> leagueActions = entities.Select(x => x.ToDomain(leagueYear.GetPublisherByIDOrFakePublisher(x.PublisherID))).ToList();
        return leagueActions;
    }

    public async Task<IReadOnlyList<LeagueAction>> GetLeagueActions(int year)
    {
        var leagueYears = await GetLeagueYears(year);
        var publisherDictionary = leagueYears.SelectMany(x => x.Publishers).ToDictionary(x => x.PublisherID, x => x);
        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<LeagueActionEntity>(
            "select tbl_league_action.PublisherID, tbl_league_action.Timestamp, tbl_league_action.ActionType, tbl_league_action.Description, tbl_league_action.ManagerAction from tbl_league_action " +
            "join tbl_league_publisher on (tbl_league_action.PublisherID = tbl_league_publisher.PublisherID) " +
            "where tbl_league_publisher.Year = @leagueYear;",
            new
            {
                leagueYear = year
            });

        var leagueActions = new List<LeagueAction>();
        foreach (var entity in entities)
        {
            if (publisherDictionary.TryGetValue(entity.PublisherID, out var publisher))
            {
                leagueActions.Add(entity.ToDomain(publisher));
            }
        }
        
        return leagueActions;
    }

    public async Task ChangePublisherName(Publisher publisher, string publisherName)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_league_publisher SET PublisherName = @publisherName where PublisherID = @publisherID;",
            new { publisherID = publisher.PublisherID, publisherName });
    }

    public async Task ChangePublisherIcon(Publisher publisher, string? publisherIcon)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_league_publisher SET PublisherIcon = @publisherIcon where PublisherID = @publisherID;",
            new
            {
                publisherID = publisher.PublisherID,
                publisherIcon
            });
    }

    public async Task ChangePublisherSlogan(Publisher publisher, string? publisherSlogan)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_league_publisher SET PublisherSlogan = @publisherSlogan where PublisherID = @publisherID;",
            new
            {
                publisherID = publisher.PublisherID,
                publisherSlogan
            });
    }

    public async Task SetAutoDraft(Publisher publisher, bool autoDraft)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_league_publisher SET AutoDraft = @autoDraft where PublisherID = @publisherID;",
            new { publisherID = publisher.PublisherID, autoDraft });
    }

    public async Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague, bool customRulesLeague)
    {
        string sql = "update tbl_league SET LeagueName = @leagueName, PublicLeague = @publicLeague, TestLeague = @testLeague, CustomRulesLeague = @customRulesLeague where LeagueID = @leagueID;";
        var param = new {leagueID = league.LeagueID, leagueName, publicLeague, testLeague, customRulesLeague};
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, param);
    }

    public async Task StartDraft(LeagueYear leagueYear)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            $"update tbl_league_year SET PlayStatus = '{PlayStatus.Drafting.Value}', DraftStartedTimestamp = CURRENT_TIMESTAMP WHERE LeagueID = @leagueID and Year = @year",
            new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year
            });
    }

    public async Task CompleteDraft(LeagueYear leagueYear)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            $"update tbl_league_year SET PlayStatus = '{PlayStatus.DraftFinal.Value}' WHERE LeagueID = @leagueID and Year = @year",
            new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year
            });
    }

    public async Task ResetDraft(LeagueYear leagueYear, Instant timestamp)
    {
        const string gameDeleteSQL = "delete from tbl_league_publishergame where PublisherID in @publisherIDs";
        string draftResetSQL = $"update tbl_league_year SET PlayStatus = '{PlayStatus.NotStartedDraft.Value}' WHERE LeagueID = @leagueID and Year = @year";

        LeagueAction resetDraftAction = new LeagueAction(leagueYear.GetManagerPublisher()!, timestamp, "Draft Reset", "Draft was reset.", true);
        var paramsObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year,
            publisherIDs = leagueYear.Publishers.Select(x => x.PublisherID)
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(gameDeleteSQL, paramsObject, transaction);
        await connection.ExecuteAsync(draftResetSQL, paramsObject, transaction);
        await AddLeagueAction(resetDraftAction, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task SetDraftPause(LeagueYear leagueYear, bool pause)
    {
        string sql;
        if (pause)
        {
            sql = $"update tbl_league_year SET PlayStatus = '{PlayStatus.DraftPaused.Value}' WHERE LeagueID = @leagueID and Year = @year";
        }
        else
        {
            sql = $"update tbl_league_year SET PlayStatus = '{PlayStatus.Drafting.Value}' WHERE LeagueID = @leagueID and Year = @year";
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql,
            new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year
            });
    }

    public async Task<PickupBid?> GetPickupBid(Guid bidID)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var bidEntity = await connection.QuerySingleOrDefaultAsync<PickupBidEntity>("select * from tbl_league_pickupbid where BidID = @bidID", new { bidID });
        if (bidEntity == null)
        {
            return null;
        }

        var publisher = await GetPublisherOrThrow(bidEntity.PublisherID);
        var masterGame = await _masterGameRepo.GetMasterGameOrThrow(bidEntity.MasterGameID);
        var leagueYear = await this.GetLeagueYearOrThrow(publisher.LeagueYearKey.LeagueID, publisher.LeagueYearKey.Year);

        var publisherGameDictionary = publisher.PublisherGames
            .Where(x => x.MasterGame is not null)
            .ToLookup(x => (x.PublisherID, x.MasterGame!.MasterGame.MasterGameID));

        var formerPublisherGameDictionary = publisher.FormerPublisherGames
            .Where(x => x.PublisherGame.MasterGame is not null)
            .ToLookup(x => (x.PublisherGame.PublisherID, x.PublisherGame.MasterGame!.MasterGame.MasterGameID));

        PublisherGame? conditionalDropPublisherGame = await GetConditionalDropPublisherGame(bidEntity, leagueYear.Year, publisherGameDictionary, formerPublisherGameDictionary);

        PickupBid domain = bidEntity.ToDomain(publisher, masterGame, conditionalDropPublisherGame, leagueYear);
        return domain;
    }

    public async Task CreateLeague(League league, int initialYear, LeagueOptions options)
    {
        LeagueEntity entity = new LeagueEntity(league);
        LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, initialYear, options, PlayStatus.NotStartedDraft, false);
        var tagEntities = options.LeagueTags.Select(x => new LeagueYearTagEntity(league, initialYear, x));
        List<SpecialGameSlotEntity> slotEntities = options.SpecialGameSlots.SelectMany(slot => slot.Tags, (slot, tag) =>
            new SpecialGameSlotEntity(Guid.NewGuid(), league, initialYear, slot.SpecialSlotPosition, tag)).ToList();

        const string createLeagueSQL =
            """
            insert into tbl_league(LeagueID,LeagueName,LeagueManager,PublicLeague,TestLeague,CustomRulesLeague) VALUES
            (@LeagueID,@LeagueName,@LeagueManager,@PublicLeague,@TestLeague,@CustomRulesLeague);
            """;

        const string createLeagueYearSQL =
            """
            insert into tbl_league_year
            (LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,CounterPicksToDraft,FreeDroppableGames,WillNotReleaseDroppableGames,WillReleaseDroppableGames,DropOnlyDraftGames,
            GrantSuperDrops,CounterPicksBlockDrops,AllowMoveIntoIneligible,MinimumBidAmount,DraftSystem,PickupSystem,TiebreakSystem,ScoringSystem,TradingSystem,ReleaseSystem,PlayStatus,DraftOrderSet,
            CounterPickDeadlineMonth,CounterPickDeadlineDay,MightReleaseDroppableMonth,MightReleaseDroppableDay) VALUES
            (@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@CounterPicksToDraft,@FreeDroppableGames,@WillNotReleaseDroppableGames,@WillReleaseDroppableGames,
            @DropOnlyDraftGames,@GrantSuperDrops,@CounterPicksBlockDrops,@AllowMoveIntoIneligible,@MinimumBidAmount,@DraftSystem,@PickupSystem,@TiebreakSystem,@ScoringSystem,@TradingSystem,
            @ReleaseSystem,@PlayStatus,@DraftOrderSet,@CounterPickDeadlineMonth,@CounterPickDeadlineDay,@MightReleaseDroppableMonth,@MightReleaseDroppableDay);
            """;

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(createLeagueSQL, entity, transaction);
            await connection.ExecuteAsync(createLeagueYearSQL, leagueYearEntity, transaction);
            await connection.BulkInsertAsync<LeagueYearTagEntity>(tagEntities, "tbl_league_yearusestag", 500, transaction);
            await connection.BulkInsertAsync<SpecialGameSlotEntity>(slotEntities, "tbl_league_specialgameslot", 500, transaction);

            await transaction.CommitAsync();
        }

        await AddPlayerToLeague(league, league.LeagueManager);
    }

    public Task EditLeagueYear(LeagueYear leagueYear, IReadOnlyDictionary<Guid, int> slotAssignments) => EditLeagueYearInternal(leagueYear, slotAssignments, null);

    public Task EditLeagueYear(LeagueYear leagueYear, IReadOnlyDictionary<Guid, int> slotAssignments, LeagueAction settingsChangeAction) => EditLeagueYearInternal(leagueYear, slotAssignments, settingsChangeAction);

    private async Task EditLeagueYearInternal(LeagueYear leagueYear, IReadOnlyDictionary<Guid, int> slotAssignments, LeagueAction? settingsChangeAction)
    {
        LeagueYearEntity leagueYearEntity = new LeagueYearEntity(leagueYear.League, leagueYear.Year, leagueYear.Options, leagueYear.PlayStatus, leagueYear.DraftOrderSet);
        var tagEntities = leagueYear.Options.LeagueTags.Select(x => new LeagueYearTagEntity(leagueYear.League, leagueYear.Year, x));

        List<SpecialGameSlotEntity> slotEntities = leagueYear.Options.SpecialGameSlots.SelectMany(slot => slot.Tags, (slot, tag) =>
            new SpecialGameSlotEntity(Guid.NewGuid(), leagueYear.League, leagueYear.Year, slot.SpecialSlotPosition, tag)).ToList();

        const string editLeagueYearSQL =
            """
            UPDATE tbl_league_year SET StandardGames = @StandardGames, GamesToDraft = @GamesToDraft, CounterPicks = @CounterPicks, CounterPicksToDraft = @CounterPicksToDraft,
            FreeDroppableGames = @FreeDroppableGames, WillNotReleaseDroppableGames = @WillNotReleaseDroppableGames, WillReleaseDroppableGames = @WillReleaseDroppableGames,
            DropOnlyDraftGames = @DropOnlyDraftGames, GrantSuperDrops = @GrantSuperDrops, CounterPicksBlockDrops = @CounterPicksBlockDrops, AllowMoveIntoIneligible = @AllowMoveIntoIneligible, MinimumBidAmount = @MinimumBidAmount, DraftSystem = @DraftSystem,
            PickupSystem = @PickupSystem, TiebreakSystem = @TiebreakSystem, ScoringSystem = @ScoringSystem, TradingSystem = @TradingSystem, ReleaseSystem = @ReleaseSystem,
            CounterPickDeadlineMonth = @CounterPickDeadlineMonth, CounterPickDeadlineDay = @CounterPickDeadlineDay, MightReleaseDroppableMonth = @MightReleaseDroppableMonth, MightReleaseDroppableDay = @MightReleaseDroppableDay
            WHERE LeagueID = @LeagueID and Year = @Year;
            """;

        const string deleteTagsSQL = "delete from tbl_league_yearusestag where LeagueID = @leagueID AND Year = @year;";
        const string deleteSpecialSlotsSQL = "delete from tbl_league_specialgameslot where LeagueID = @leagueID AND Year = @year;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(editLeagueYearSQL, leagueYearEntity, transaction);
        await connection.ExecuteAsync(deleteTagsSQL, new { leagueID = leagueYear.League.LeagueID, year = leagueYear.Year }, transaction);
        await connection.ExecuteAsync(deleteSpecialSlotsSQL, new { leagueID = leagueYear.League.LeagueID, year = leagueYear.Year }, transaction);
        await OrganizeSlots(leagueYear, slotAssignments, connection, transaction);
        await connection.BulkInsertAsync<LeagueYearTagEntity>(tagEntities, "tbl_league_yearusestag", 500, transaction);
        await connection.BulkInsertAsync<SpecialGameSlotEntity>(slotEntities, "tbl_league_specialgameslot", 500, transaction);
        if (settingsChangeAction is not null)
        {
            await AddLeagueAction(settingsChangeAction, connection, transaction);
        }
        await transaction.CommitAsync();
    }

    public async Task AddNewLeagueYear(League league, int year, LeagueOptions options)
    {
        LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, year, options, PlayStatus.NotStartedDraft, false);
        var tagEntities = options.LeagueTags.Select(x => new LeagueYearTagEntity(league, year, x));

        List<SpecialGameSlotEntity> slotEntities = options.SpecialGameSlots.SelectMany(slot => slot.Tags, (slot, tag) =>
            new SpecialGameSlotEntity(Guid.NewGuid(), league, year, slot.SpecialSlotPosition, tag)).ToList();

        const string newLeagueYearSQL =
            """
            insert into tbl_league_year(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,CounterPicksToDraft,FreeDroppableGames,WillNotReleaseDroppableGames,WillReleaseDroppableGames,DropOnlyDraftGames,
            DraftSystem,PickupSystem,TiebreakSystem,ScoringSystem,TradingSystem,ReleaseSystem,PlayStatus,DraftOrderSet,CounterPickDeadlineMonth,CounterPickDeadlineDay,MightReleaseDroppableMonth,MightReleaseDroppableDay) VALUES
            (@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@CounterPicksToDraft,@FreeDroppableGames,@WillNotReleaseDroppableGames,@WillReleaseDroppableGames,@DropOnlyDraftGames,
            @DraftSystem,@PickupSystem,@TiebreakSystem,@ScoringSystem,@TradingSystem,@ReleaseSystem,@PlayStatus,@DraftOrderSet,@CounterPickDeadlineMonth,@CounterPickDeadlineDay,@MightReleaseDroppableMonth,@MightReleaseDroppableDay);
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(newLeagueYearSQL, leagueYearEntity, transaction);
        await connection.BulkInsertAsync<LeagueYearTagEntity>(tagEntities, "tbl_league_yearusestag", 500, transaction);
        await connection.BulkInsertAsync<SpecialGameSlotEntity>(slotEntities, "tbl_league_specialgameslot", 500, transaction);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(Guid leagueID)
    {
        var query = new
        {
            leagueID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<FantasyCriticUserEntity>(
            "select tbl_user.* from tbl_user join tbl_league_hasuser on (tbl_user.UserID = tbl_league_hasuser.UserID) where tbl_league_hasuser.LeagueID = @leagueID;",
            query);

        var users = results.Select(x => x.ToDomain()).ToList();
        return users;
    }

    public async Task<IReadOnlyList<FantasyCriticUserRemovable>> GetUsersWithRemoveStatus(League league)
    {
        var param = new
        {
            P_LeagueID = league.LeagueID
        };

        await using var connection = new MySqlConnection(_connectionString);
        using var resultSets = await connection.QueryMultipleAsync("sp_getusersinleague", param, commandType: CommandType.StoredProcedure);
        var userEntities = await resultSets.ReadAsync<FantasyCriticUserEntity>();
        var playStatuses = await resultSets.ReadAsync<LeagueYearStatusEntity>();
        var userYears = await resultSets.ReadAsync<LeagueYearUserEntity>();

        var usersInLeague = userEntities.Select(x => x.ToDomain()).ToList();

        var domains = ConvertUserRemovableEntities(league, userYears, playStatuses, usersInLeague);
        return domains;
    }

    private IReadOnlyList<FantasyCriticUserRemovable> ConvertUserRemovableEntities(League league, IEnumerable<LeagueYearUserEntity> userYears,
        IEnumerable<LeagueYearStatusEntity> playStatuses, List<FantasyCriticUser> usersInLeague)
    {
        var userYearsDictionary = new Dictionary<int, HashSet<Guid>>();
        foreach (var userYear in userYears)
        {
            if (!userYearsDictionary.ContainsKey(userYear.Year))
            {
                userYearsDictionary[userYear.Year] = new HashSet<Guid>();
            }

            userYearsDictionary[userYear.Year].Add(userYear.UserID);
        }

        var startedYears = playStatuses
            .Where(x => x.PlayStatus != PlayStatus.NotStartedDraft.Value)
            .Select(x => x.Year)
            .ToList();

        List<FantasyCriticUserRemovable> usersWithStatus = new List<FantasyCriticUserRemovable>();
        foreach (var user in usersInLeague)
        {
            bool userRemovable = !league.LeagueManager.Equals(user);
            foreach (var year in startedYears)
            {
                var userPlayedInYear = userYearsDictionary[year].Contains(user.Id);
                if (userPlayedInYear)
                {
                    userRemovable = false;
                }
            }

            usersWithStatus.Add(new FantasyCriticUserRemovable(user, userRemovable));
        }

        return usersWithStatus;
    }

    public async Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(Guid leagueID, int year)
    {
        var query = new
        {
            leagueID,
            year
        };

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<FantasyCriticUserEntity>(
            "select tbl_user.* from tbl_user join tbl_league_activeplayer on (tbl_user.UserID = tbl_league_activeplayer.UserID) " +
            "where tbl_league_activeplayer.LeagueID = @leagueID AND year = @year;",
            query);

        var users = results.Select(x => x.ToDomain()).ToList();
        return users;
    }

    public async Task<CombinedLeagueYearUserStatus> GetCombinedLeagueYearUserStatus(LeagueYear leagueYear)
    {
        var param = new
        {
            P_LeagueID = leagueYear.League.LeagueID,
            P_Year = leagueYear.Year
        };

        await using var connection = new MySqlConnection(_connectionString);
        using var resultSets = await connection.QueryMultipleAsync("sp_getcombinedleagueyearuserstatus", param, commandType: CommandType.StoredProcedure);
        var userEntities = await resultSets.ReadAsync<FantasyCriticUserEntity>();
        var playStatuses = await resultSets.ReadAsync<LeagueYearStatusEntity>();
        var userYears = await resultSets.ReadAsync<LeagueYearUserEntity>();
        var activeUserEntities = await resultSets.ReadAsync<FantasyCriticUserEntity>();
        var inviteEntities = await resultSets.ReadAsync<LeagueInviteEntity>();

        var usersInLeague = userEntities.Select(x => x.ToDomain()).ToList();
        var activePlayersForLeagueYear = activeUserEntities.Select(x => x.ToDomain()).ToList();

        var usersWithRemoveStatus = ConvertUserRemovableEntities(leagueYear.League, userYears, playStatuses, usersInLeague);
        var leagueInvites = await ConvertLeagueInviteEntitiesForSingleLeague(leagueYear.League, inviteEntities);

        return new CombinedLeagueYearUserStatus(usersWithRemoveStatus, leagueInvites, activePlayersForLeagueYear);
    }

    public async Task SetPlayersActive(League league, int year, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers)
    {
        const string insertSQL = "insert into tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@year,@userID);";
        var insertObjects = mostRecentActivePlayers.Select(x => new
        {
            leagueID = league.LeagueID,
            userID = x.Id,
            year
        });

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(insertSQL, insertObjects);
    }

    public async Task SetPlayerActiveStatus(LeagueYear leagueYear, Dictionary<FantasyCriticUser, bool> usersToChange)
    {
        const string deleteActiveUserSQL = "delete from tbl_league_activeplayer where LeagueID = @leagueID and Year = @year and UserID = @userID;";
        const string insertSQL = "insert into tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@year,@userID);";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        foreach (var userToChange in usersToChange)
        {
            var paramsObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                userID = userToChange.Key.Id,
                leagueYear.Year
            };

            if (userToChange.Value)
            {
                await connection.ExecuteAsync(insertSQL, paramsObject, transaction);
            }
            else
            {
                await connection.ExecuteAsync(deleteActiveUserSQL, paramsObject, transaction);
            }
        }

        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league)
    {
        var query = new
        {
            leagueID = league.LeagueID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<FantasyCriticUserEntity>(
            "select tbl_user.* from tbl_user join tbl_user_followingleague on (tbl_user.UserID = tbl_user_followingleague.UserID) where tbl_user_followingleague.LeagueID = @leagueID;",
            query);

        var users = results.Select(x => x.ToDomain()).ToList();
        return users;
    }

    public async Task<bool> UserIsFollowingLeague(FantasyCriticUser currentUser, League league)
    {
        var query = new
        {
            userID = currentUser.Id,
            leagueID = league.LeagueID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var result = await connection.QuerySingleAsync<int>(
            @"
            select Count(*) from tbl_user
                join tbl_user_followingleague on (tbl_user.UserID = tbl_user_followingleague.UserID)
                where tbl_user_followingleague.LeagueID = @leagueID AND tbl_user.UserID = @userID;
            ", query);

        return result > 0;
    }

    public async Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user)
    {
        IEnumerable<LeagueEntity> leagueEntities;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            var queryObject = new
            {
                userID = user.Id,
            };

            const string sql = "select vw_league.*, tbl_league_hasuser.Archived from vw_league join tbl_league_hasuser on (vw_league.LeagueID = tbl_league_hasuser.LeagueID) where tbl_league_hasuser.UserID = @userID and IsDeleted = 0;";
            leagueEntities = await connection.QueryAsync<LeagueEntity>(sql, queryObject);
        }

        IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
        return leagues;
    }

    public async Task<IReadOnlySet<Guid>> GetLeaguesWithMostRecentYearOneShot()
    {
        const string sql = "SELECT distinct LeagueID " +
                           "FROM (SELECT LeagueID, YEAR, OneShotMode, ROW_NUMBER() OVER (PARTITION BY LeagueID ORDER BY Year DESC) ranked_order FROM tbl_caching_leagueyear) subQuery " +
                           "WHERE subQuery.ranked_order = 1 AND subQuery.OneShotMode = 1;";
        await using var connection = new MySqlConnection(_connectionString);
        var leagueIDs = await connection.QueryAsync<Guid>(sql);
        return leagueIDs.ToHashSet();
    }

    public async Task<IReadOnlyList<LeagueYear>> GetLeagueYearsForUser(FantasyCriticUser user, int year)
    {
        var allLeagueTags = await GetLeagueYearTagEntities(year);
        var allSpecialGameSlots = await GetSpecialGameSlotEntities(year);
        var leagueTagsByLeague = allLeagueTags.ToLookup(x => x.LeagueID);
        var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
        var domainSpecialGameSlots = SpecialGameSlotEntity.ConvertSpecialGameSlotEntities(allSpecialGameSlots, tagDictionary);
        var allEligibilityOverrides = await GetAllEligibilityOverrides(year);
        var allTagOverrides = await GetAllTagOverrides(year);
        var supportedYear = await GetSupportedYear(year);
        var allPublishersForYear = await GetAllPublishersForYear(year);
        var publisherLookup = allPublishersForYear.ToLookup(x => x.LeagueYearKey);

        await using var connection = new MySqlConnection(_connectionString);
        var queryObject = new
        {
            year,
            userID = user.Id
        };

        const string sql = "select tbl_league_year.* from tbl_league_year " +
                     "join tbl_league_hasuser on (tbl_league_hasuser.LeagueID = tbl_league_year.LeagueID) " +
                     "where tbl_league_year.Year = @year and tbl_league_hasuser.UserID = @userID";
        IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>(sql, queryObject);
        List<LeagueYear> leagueYears = new List<LeagueYear>();
        IReadOnlyList<League> leagues = await GetLeaguesForUser(user);
        Dictionary<Guid, League> leaguesDictionary = leagues.ToDictionary(x => x.LeagueID, y => y);

        foreach (var entity in yearEntities)
        {
            if (!leaguesDictionary.TryGetValue(entity.LeagueID, out var league))
            {
                _logger.Debug($"Cannot find league (probably deleted) LeagueID: {entity.LeagueID}");
                continue;
            }

            if (!allEligibilityOverrides.TryGetValue(entity.LeagueID, out var eligibilityOverrides))
            {
                eligibilityOverrides = new List<EligibilityOverride>();
            }

            if (!allTagOverrides.TryGetValue(entity.LeagueID, out var tagOverrides))
            {
                tagOverrides = new List<TagOverride>();
            }

            var leagueYearKey = new LeagueYearKey(entity.LeagueID, entity.Year);
            var domainLeagueTags = ConvertLeagueTagEntities(leagueTagsByLeague[entity.LeagueID], tagDictionary);
            var specialGameSlotsForLeagueYear = domainSpecialGameSlots[leagueYearKey];

            var winningUser = await _userStore.GetUserThatMightExist(entity.WinningUserID);
            var publishers = publisherLookup[leagueYearKey];
            LeagueYear leagueYear = entity.ToDomain(league, supportedYear, eligibilityOverrides, tagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear,
                winningUser, publishers);
            leagueYears.Add(leagueYear);
        }

        return leagueYears;
    }

    public async Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<LeagueYearKey>>> GetUsersWithLeagueYearsWithPublisher()
    {
        const string sql = "SELECT UserID, LeagueID, YEAR FROM tbl_league_publisher;";

        IEnumerable<UserActiveLeaguesEntity> entities;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            entities = await connection.QueryAsync<UserActiveLeaguesEntity>(sql);
        }

        var allUsers = await _userStore.GetAllUsers();
        Dictionary<FantasyCriticUser, List<LeagueYearKey>> userDictionary = allUsers.ToDictionary(x => x, _ => new List<LeagueYearKey>());
        var entitiesByUserID = entities.ToLookup(x => x.UserID);
        foreach (var user in allUsers)
        {
            var entitiesForUserID = entitiesByUserID[user.Id];
            userDictionary[user] = entitiesForUserID.Select(x => new LeagueYearKey(x.LeagueID, x.Year)).ToList();
        }

        return userDictionary.SealDictionary();
    }


    public async Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser currentUser)
    {
        var query = new
        {
            userID = currentUser.Id
        };

        IEnumerable<LeagueEntity> leagueEntities;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            const string sql = "select vw_league.* from vw_league join tbl_user_followingleague on (vw_league.LeagueID = tbl_user_followingleague.LeagueID) " +
                      "where tbl_user_followingleague.UserID = @userID and IsDeleted = 0;";
            leagueEntities = await connection.QueryAsync<LeagueEntity>(
                sql,
                query);
        }

        IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
        return leagues;
    }

    public async Task FollowLeague(League league, FantasyCriticUser user)
    {
        var userAddObject = new
        {
            leagueID = league.LeagueID,
            userID = user.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_user_followingleague(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject);
    }

    public async Task UnfollowLeague(League league, FantasyCriticUser user)
    {
        var deleteObject = new
        {
            leagueID = league.LeagueID,
            userID = user.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "delete from tbl_user_followingleague where LeagueID = @leagueID and UserID = @userID;",
            deleteObject);
    }

    public async Task<LeagueInvite?> GetInvite(Guid inviteID)
    {
        var query = new
        {
            inviteID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<LeagueInviteEntity>(
            "select * from tbl_league_invite where tbl_league_invite.InviteID = @inviteID",
            query);
        return await ConvertLeagueInviteEntity(entity);
    }

    public async Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser currentUser)
    {
        var query = new
        {
            email = currentUser.Email,
            userID = currentUser.Id
        };

        IEnumerable<LeagueInviteEntity> inviteEntities;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            inviteEntities = await connection.QueryAsync<LeagueInviteEntity>(
                "select * from tbl_league_invite where tbl_league_invite.EmailAddress = @email OR tbl_league_invite.UserID = @userID;",
                query);
        }

        var leagueInvites = await ConvertLeagueInviteEntities(inviteEntities);
        return leagueInvites;
    }

    public async Task SaveInvite(LeagueInvite leagueInvite)
    {
        var entity = new LeagueInviteEntity(leagueInvite);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_league_invite(InviteID,LeagueID,EmailAddress,UserID) VALUES (@inviteID, @leagueID, @emailAddress, @userID);",
            entity);
    }

    public async Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league)
    {
        var query = new
        {
            leagueID = league.LeagueID
        };

        IEnumerable<LeagueInviteEntity> invites;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            invites = await connection.QueryAsync<LeagueInviteEntity>(
                "select * from tbl_league_invite where tbl_league_invite.LeagueID = @leagueID;",
                query);
        }

        var leagueInvites = await ConvertLeagueInviteEntitiesForSingleLeague(league, invites);
        return leagueInvites;
    }

    public async Task AcceptInvite(LeagueInvite leagueInvite, FantasyCriticUser user)
    {
        await AddPlayerToLeague(leagueInvite.League, user);
        await DeleteInvite(leagueInvite);
    }

    public async Task DeleteInvite(LeagueInvite invite)
    {
        var deleteObject = new
        {
            inviteID = invite.InviteID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "delete from tbl_league_invite where InviteID = @inviteID;",
            deleteObject);
    }

    public async Task SaveInviteLink(LeagueInviteLink inviteLink)
    {
        LeagueInviteLinkEntity entity = new LeagueInviteLinkEntity(inviteLink);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_league_invitelink(InviteID,LeagueID,InviteCode,Active) VALUES " +
            "(@InviteID,@LeagueID,@InviteCode,@Active);",
            entity);
    }

    public async Task DeactivateInviteLink(LeagueInviteLink inviteLink)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_league_invitelink SET Active = 0 where InviteID = @inviteID;", new { inviteID = inviteLink.InviteID });
    }

    public async Task<IReadOnlyList<LeagueInviteLink>> GetInviteLinks(League league)
    {
        var query = new
        {
            leagueID = league.LeagueID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<LeagueInviteLinkEntity>("select * from tbl_league_invitelink where LeagueID = @leagueID;", query);

        var inviteLinks = results.Select(x => x.ToDomain(league)).ToList();
        return inviteLinks;
    }

    public async Task<LeagueInviteLink?> GetInviteLinkByInviteCode(Guid inviteCode)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<LeagueInviteLinkEntity>("select * from tbl_league_invitelink where InviteCode = @inviteCode and Active = 1;", new { inviteCode });

        if (result is null)
        {
            return null;
        }

        var league = await this.GetLeagueOrThrow(result.LeagueID);
        return result.ToDomain(league);
    }

    public async Task ReassignPublisher(LeagueYear leagueYear, Publisher publisherToReassign, FantasyCriticUser newUser)
    {
        string addUserSQL = "insert into tbl_league_hasuser (LeagueID,UserID) VALUES (@LeagueID,@NewUserID);";
        string setUserActiveSQL = "insert into tbl_league_activeplayer (LeagueID,Year,UserID) VALUES (@LeagueID,@Year,@NewUserID);";
        string reassignPublisherSQL = "update tbl_league_publisher SET UserID = @NewUserID WHERE LeagueID = @LeagueID AND PublisherID = @PublisherID";
        string setUserInactiveSQL = "delete from tbl_league_activeplayer WHERE LeagueID = @LeagueID AND Year = @Year AND UserID = @OldUserID;";

        var param = new
        {
            LeagueID = leagueYear.League.LeagueID,
            Year = leagueYear.Year,
            PublisherID = publisherToReassign.PublisherID,
            OldUserID = publisherToReassign.User.Id,
            NewUserID = newUser.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(addUserSQL, param, transaction);
        await connection.ExecuteAsync(setUserActiveSQL, param, transaction);
        await connection.ExecuteAsync(reassignPublisherSQL, param, transaction);
        await connection.ExecuteAsync(setUserInactiveSQL, param, transaction);

        await transaction.CommitAsync();
    }

    public async Task SetArchiveStatusForUser(League league, bool archive, FantasyCriticUser user)
    {
        const string updateSQL = "update tbl_league_hasuser SET Archived = @archive WHERE LeagueID = @leagueID AND UserID = @userID;";
        var parameters = new { leagueID = league.LeagueID, userID = user.Id, archive };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(updateSQL, parameters);
    }

    public async Task FullyRemovePublisher(LeagueYear leagueYear, Publisher deletePublisher)
    {
        const string deleteSQL = "delete from tbl_league_publisher where PublisherID = @publisherID;";
        const string deleteQueueSQL = "delete from tbl_league_publisherqueue where PublisherID = @publisherID;";
        const string deleteHistorySQL = "delete from tbl_league_action where PublisherID = @publisherID;";
        const string deletePublisherGameSQL = "delete from tbl_league_publishergame WHERE PublisherID = @publisherID;";
        const string deleteFormerPublisherGameSQL = "delete from tbl_league_formerpublishergame WHERE PublisherID = @publisherID;";
        const string deletePublisherBidsSQL = "delete from tbl_league_pickupbid WHERE PublisherID = @publisherID;";
        const string deletePublisherDropsSQL = "delete from tbl_league_droprequest WHERE PublisherID = @publisherID;";
        const string updateProposerTradeSQL = "UPDATE tbl_league_trade SET ProposerPublisherID = null WHERE ProposerPublisherID = @publisherID;";
        const string updateCounterPartyTradeSQL = "UPDATE tbl_league_trade SET CounterPartyPublisherID = null WHERE CounterPartyPublisherID = @publisherID;";
        const string fixDraftOrderSQL = "update tbl_league_publisher SET DraftPosition = @draftPosition where PublisherID = @publisherID;";

        var remainingOrderedPublishers = leagueYear.GetAllPublishersExcept(deletePublisher).OrderBy(x => x.DraftPosition).ToList();
        IEnumerable<SetDraftOrderEntity> setDraftOrderEntities = remainingOrderedPublishers.Select((publisher, index) => new SetDraftOrderEntity(publisher.PublisherID, index + 1));

        var deleteObject = new
        {
            publisherID = deletePublisher.PublisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteQueueSQL, deleteObject, transaction);
        await connection.ExecuteAsync(deleteHistorySQL, deleteObject, transaction);
        await connection.ExecuteAsync(deletePublisherGameSQL, deleteObject, transaction);
        await connection.ExecuteAsync(deleteFormerPublisherGameSQL, deleteObject, transaction);
        await connection.ExecuteAsync(deletePublisherBidsSQL, deleteObject, transaction);
        await connection.ExecuteAsync(deletePublisherDropsSQL, deleteObject, transaction);
        await connection.ExecuteAsync(updateProposerTradeSQL, deleteObject, transaction);
        await connection.ExecuteAsync(updateCounterPartyTradeSQL, deleteObject, transaction);
        await connection.ExecuteAsync(deleteSQL, deleteObject, transaction);
        await connection.ExecuteAsync(fixDraftOrderSQL, setDraftOrderEntities, transaction);
        await transaction.CommitAsync();
    }

    public async Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
    {
        const string deleteUserSQL = "delete from tbl_league_hasuser where LeagueID = @leagueID and UserID = @userID;";
        const string deleteActiveUserSQL = "delete from tbl_league_activeplayer where LeagueID = @leagueID and UserID = @userID;";

        var userDeleteObject = new
        {
            leagueID = league.LeagueID,
            userID = removeUser.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteActiveUserSQL, userDeleteObject, transaction);
        await connection.ExecuteAsync(deleteUserSQL, userDeleteObject, transaction);
        await transaction.CommitAsync();
    }

    public async Task TransferLeagueManager(League league, FantasyCriticUser newManager)
    {
        const string sql = "UPDATE tbl_league SET LeagueManager = @newManagerUserID WHERE LeagueID = @leagueID;";

        var transferObject = new
        {
            leagueID = league.LeagueID,
            newManagerUserID = newManager.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, transferObject);
    }

    public async Task CreatePublisher(Publisher publisher)
    {
        const string publisherCreateSQL =
            "insert into tbl_league_publisher(PublisherID,PublisherName,PublisherIcon,PublisherSlogan,LeagueID,Year,UserID,DraftPosition,Budget,FreeGamesDropped,WillNotReleaseGamesDropped,WillReleaseGamesDropped,SuperDropsAvailable) VALUES " +
            "(@PublisherID,@PublisherName,@PublisherIcon,@PublisherSlogan,@LeagueID,@Year,@UserID,@DraftPosition,@Budget,@FreeGamesDropped,@WillNotReleaseGamesDropped,@WillReleaseGamesDropped,@SuperDropsAvailable);";
        const string setFlagSQL = "update tbl_league_year SET DraftOrderSet = 0 WHERE LeagueID = @LeagueID AND Year = @Year;";

        var entity = new PublisherEntity(publisher);
        var leagueYearKey = new LeagueYearKeyEntity(publisher.LeagueYearKey);
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(publisherCreateSQL, entity, transaction);
        await connection.ExecuteAsync(setFlagSQL, leagueYearKey, transaction);

        await transaction.CommitAsync();
    }

    private async Task<IReadOnlyList<Publisher>> ConvertPublisherEntities(IEnumerable<FantasyCriticUserEntity> userInLeagueEntities, IEnumerable<PublisherEntity> publisherEntities,
        IEnumerable<PublisherGameEntity> publisherGameEntities, IEnumerable<FormerPublisherGameEntity> formerPublisherGameEntities, int year)
    {
        var usersInLeague = userInLeagueEntities.Select(x => x.ToDomain()).ToDictionary(x => x.Id);
        IReadOnlyList<PublisherGame> domainGames = await ConvertPublisherGameEntities(publisherGameEntities, year);
        IReadOnlyList<FormerPublisherGame> domainFormerGames = await ConvertFormerPublisherGameEntities(formerPublisherGameEntities, year);

        List<Publisher> domainPublishers = new List<Publisher>();
        foreach (var entity in publisherEntities)
        {
            var gamesForPublisher = domainGames.Where(x => x.PublisherID == entity.PublisherID);
            var formerGamesForPublisher = domainFormerGames.Where(x => x.PublisherGame.PublisherID == entity.PublisherID);
            var user = usersInLeague[entity.UserID];
            var domainPublisher = entity.ToDomain(user, gamesForPublisher, formerGamesForPublisher);
            domainPublishers.Add(domainPublisher);
        }

        return domainPublishers;
    }

    private async Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year, bool includeDeleted = false)
    {
        var query = new
        {
            year
        };

        var allUsers = await _userStore.GetAllUsers();
        var usersDictionary = allUsers.ToDictionary(x => x.Id, y => y);


        string sql = "select tbl_league_publisher.* from tbl_league_publisher " +
                     "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                     "where tbl_league_publisher.Year = @year and tbl_league.IsDeleted = 0;";

        if (includeDeleted)
        {
            sql = "select tbl_league_publisher.* from tbl_league_publisher " +
                  "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                  "where tbl_league_publisher.Year = @year;";
        }

        await using var connection = new MySqlConnection(_connectionString);
        var publisherEntities = await connection.QueryAsync<PublisherEntity>(sql, query);
        IReadOnlyList<PublisherGame> allDomainGames = await GetAllPublisherGamesForYear(year, includeDeleted);
        IReadOnlyList<FormerPublisherGame> allDomainFormerGames = await GetAllFormerPublisherGamesForYear(year, includeDeleted);
        var domainGameLookup = allDomainGames.ToLookup(x => x.PublisherID);
        var domainFormerGameLookup = allDomainFormerGames.ToLookup(x => x.PublisherGame.PublisherID);

        List<Publisher> publishers = new List<Publisher>();
        foreach (var entity in publisherEntities)
        {
            var user = usersDictionary[entity.UserID];
            var domainGames = domainGameLookup[entity.PublisherID];
            var domainFormerGames = domainFormerGameLookup[entity.PublisherID];
            var domainPublisher = entity.ToDomain(user, domainGames, domainFormerGames);
            publishers.Add(domainPublisher);
        }

        return publishers;
    }

    private async Task<IReadOnlyList<PublisherGame>> GetAllPublisherGamesForYear(int year, bool includeDeleted = false, MySqlConnection? connection = null, MySqlTransaction? transaction = null)
    {
        var query = new
        {
            year
        };

        string sql = "select tbl_league_publishergame.* from tbl_league_publishergame " +
                     "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                     "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                     "where tbl_league_publisher.Year = @year and IsDeleted = 0;";

        if (includeDeleted)
        {
            sql = "select tbl_league_publishergame.* from tbl_league_publishergame " +
                  "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                  "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                  "where tbl_league_publisher.Year = @year;";
        }

        bool shouldDispose = false;
        if (connection is null)
        {
            connection = new MySqlConnection(_connectionString);
            shouldDispose = true;
        }

        IEnumerable<PublisherGameEntity> gameEntities = await connection.QueryAsync<PublisherGameEntity>(sql, query, transaction);

        List<PublisherGame> domainGames = new List<PublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        if (shouldDispose)
        {
            connection.Dispose();
        }

        return domainGames;
    }

    private async Task<IReadOnlyList<FormerPublisherGame>> GetAllFormerPublisherGamesForYear(int year, bool includeDeleted = false, MySqlConnection? connection = null, MySqlTransaction? transaction = null)
    {
        var query = new
        {
            year
        };

        string sql = "select tbl_league_formerpublishergame.* from tbl_league_formerpublishergame " +
                     "join tbl_league_publisher on (tbl_league_formerpublishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                     "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                     "where tbl_league_publisher.Year = @year and IsDeleted = 0;";

        if (includeDeleted)
        {
            sql = "select tbl_league_formerpublishergame.* from tbl_league_formerpublishergame " +
                  "join tbl_league_publisher on (tbl_league_formerpublishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                  "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                  "where tbl_league_publisher.Year = @year;";
        }

        bool shouldDispose = false;
        if (connection is null)
        {
            connection = new MySqlConnection(_connectionString);
            shouldDispose = true;
        }

        IEnumerable<FormerPublisherGameEntity> gameEntities = await connection.QueryAsync<FormerPublisherGameEntity>(sql, query, transaction);

        List<FormerPublisherGame> domainGames = new List<FormerPublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        if (shouldDispose)
        {
            connection.Dispose();
        }

        return domainGames;
    }

    private async Task<Publisher?> GetPublisher(Guid publisherID)
    {
        var query = new
        {
            publisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        PublisherEntity publisherEntity = await connection.QuerySingleOrDefaultAsync<PublisherEntity>(
            "select tbl_league_publisher.* from tbl_league_publisher " +
            "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
            "where tbl_league_publisher.PublisherID = @publisherID and IsDeleted = 0;",
            query);

        if (publisherEntity == null)
        {
            return null;
        }

        IReadOnlyList<PublisherGame> domainGames = await GetPublisherGames(publisherEntity.PublisherID, publisherEntity.Year);
        IReadOnlyList<FormerPublisherGame> domainFormerGames = await GetFormerPublisherGames(publisherEntity.PublisherID, publisherEntity.Year);
        var user = await _userStore.FindByIdOrThrowAsync(publisherEntity.UserID, CancellationToken.None);
        var domainPublisher = publisherEntity.ToDomain(user, domainGames, domainFormerGames);
        return domainPublisher;
    }

    private async Task<Publisher> GetPublisherOrThrow(Guid publisherID)
    {
        var publisher = await GetPublisher(publisherID);
        if (publisher is null)
        {
            throw new Exception($"Publisher not found: {publisherID}");
        }

        return publisher;
    }

    public async Task AddPublisherGame(PublisherGame publisherGame)
    {
        PublisherGameEntity entity = new PublisherGameEntity(publisherGame);

        const string sql =
            "insert into tbl_league_publishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore," +
            "ManualWillNotRelease,FantasyPoints,MasterGameID,SlotNumber,DraftPosition,OverallDraftPosition,BidAmount) VALUES " +
            "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore," +
            "@ManualWillNotRelease,@FantasyPoints,@MasterGameID,@SlotNumber,@DraftPosition,@OverallDraftPosition,@BidAmount);";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task CreateTrade(Trade trade)
    {
        var tradeEntity = new TradeEntity(trade);
        var proposerComponents = trade.ProposerMasterGames.Select(x => new TradeComponentEntity(trade.TradeID, TradingParty.Proposer, x)).ToList();
        var counterPartyComponents = trade.CounterPartyMasterGames.Select(x => new TradeComponentEntity(trade.TradeID, TradingParty.CounterParty, x)).ToList();
        var allTradeComponents = proposerComponents.Concat(counterPartyComponents).ToList();

        const string baseTableSQL = "INSERT INTO tbl_league_trade(TradeID,LeagueID,Year,ProposerPublisherID,CounterPartyPublisherID,ProposerBudgetSendAmount,CounterPartyBudgetSendAmount," +
                              "Message,ProposedTimestamp,AcceptedTimestamp,CompletedTimestamp,Status) VALUES" +
                              "(@TradeID, @LeagueID, @Year, @ProposerPublisherID, @CounterPartyPublisherID, @ProposerBudgetSendAmount," +
                              "@CounterPartyBudgetSendAmount, @Message, @ProposedTimestamp, @AcceptedTimestamp, @CompletedTimestamp, @Status);";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(baseTableSQL, tradeEntity, transaction);
        await connection.BulkInsertAsync<TradeComponentEntity>(allTradeComponents, "tbl_league_tradecomponent", 500, transaction);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<Trade>> GetTradesForLeague(LeagueYear leagueYear)
    {
        const string baseTableSQL = "select * from tbl_league_trade WHERE LeagueID = @leagueID AND Year = @year;";
        const string componentTableSQL = "select tbl_league_tradecomponent.* from tbl_league_tradecomponent " +
                                         "join tbl_league_trade ON tbl_league_tradecomponent.TradeID = tbl_league_trade.TradeID " +
                                         "WHERE LeagueID = @leagueID AND Year = @year;";
        const string voteTableSQL = "select tbl_league_tradevote.* from tbl_league_tradevote " +
                                    "join tbl_league_trade ON tbl_league_tradevote.TradeID = tbl_league_trade.TradeID " +
                                    "WHERE LeagueID = @leagueID AND Year = @year;";

        var queryObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year
        };

        await using var connection = new MySqlConnection(_connectionString);
        var tradeEntities = await connection.QueryAsync<TradeEntity>(baseTableSQL, queryObject);
        var componentEntities = await connection.QueryAsync<TradeComponentEntity>(componentTableSQL, queryObject);
        var voteEntities = await connection.QueryAsync<TradeVoteEntity>(voteTableSQL, queryObject);

        var componentLookup = componentEntities.ToLookup(x => x.TradeID);
        var voteLookup = voteEntities.ToLookup(x => x.TradeID);

        List<Trade> domainTrades = new List<Trade>();
        foreach (var tradeEntity in tradeEntities)
        {
            Publisher proposer = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.ProposerPublisherID);
            Publisher counterParty = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.CounterPartyPublisherID);

            var components = componentLookup[tradeEntity.TradeID];
            List<MasterGameYearWithCounterPick> proposerMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
            List<MasterGameYearWithCounterPick> counterPartyMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
            foreach (var component in components)
            {
                var masterGameYear = await _masterGameRepo.GetMasterGameYear(component.MasterGameID, leagueYear.Year);
                if (masterGameYear is null)
                {
                    throw new Exception($"Invalid master game when getting trade: {tradeEntity.TradeID}");
                }

                var domainComponent = new MasterGameYearWithCounterPick(masterGameYear, component.CounterPick);
                if (component.CurrentParty == TradingParty.Proposer.Value)
                {
                    proposerMasterGameYearWithCounterPicks.Add(domainComponent);
                }
                else if (component.CurrentParty == TradingParty.CounterParty.Value)
                {
                    counterPartyMasterGameYearWithCounterPicks.Add(domainComponent);
                }
                else
                {
                    throw new Exception($"Invalid party when getting trade: {tradeEntity.TradeID}");
                }
            }

            var votes = voteLookup[tradeEntity.TradeID];
            List<TradeVote> tradeVotes = new List<TradeVote>();
            foreach (var vote in votes)
            {
                var user = leagueYear.Publishers.Single(x => x.User.Id == vote.UserID).User;
                var domainVote = new TradeVote(tradeEntity.TradeID, user, vote.Approved, vote.Comment, vote.Timestamp);
                tradeVotes.Add(domainVote);
            }

            domainTrades.Add(tradeEntity.ToDomain(leagueYear, proposer, counterParty, proposerMasterGameYearWithCounterPicks, counterPartyMasterGameYearWithCounterPicks, tradeVotes));
        }

        return domainTrades;
    }

    public async Task<IReadOnlyList<Trade>> GetTradesForYear(int year)
    {
        var leagueYears = await GetLeagueYears(2022);
        var leagueYearDictionary = leagueYears.ToDictionary(x => x.League.LeagueID);

        const string baseTableSQL = "select * from tbl_league_trade WHERE Year = @year;";
        const string componentTableSQL = "select tbl_league_tradecomponent.* from tbl_league_tradecomponent " +
                                         "join tbl_league_trade ON tbl_league_tradecomponent.TradeID = tbl_league_trade.TradeID " +
                                         "WHERE Year = @year;";
        const string voteTableSQL = "select tbl_league_tradevote.* from tbl_league_tradevote " +
                                    "join tbl_league_trade ON tbl_league_tradevote.TradeID = tbl_league_trade.TradeID " +
                                    "WHERE Year = @year;";

        var queryObject = new
        {
            year
        };

        await using var connection = new MySqlConnection(_connectionString);
        var tradeEntities = await connection.QueryAsync<TradeEntity>(baseTableSQL, queryObject);
        var componentEntities = await connection.QueryAsync<TradeComponentEntity>(componentTableSQL, queryObject);
        var voteEntities = await connection.QueryAsync<TradeVoteEntity>(voteTableSQL, queryObject);

        var componentLookup = componentEntities.ToLookup(x => x.TradeID);
        var voteLookup = voteEntities.ToLookup(x => x.TradeID);

        List<Trade> domainTrades = new List<Trade>();
        foreach (var tradeEntity in tradeEntities)
        {
            var leagueYear = leagueYearDictionary[tradeEntity.LeagueID];
            Publisher proposer = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.ProposerPublisherID);
            Publisher counterParty = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.CounterPartyPublisherID);

            var components = componentLookup[tradeEntity.TradeID];
            List<MasterGameYearWithCounterPick> proposerMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
            List<MasterGameYearWithCounterPick> counterPartyMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
            foreach (var component in components)
            {
                var masterGameYear = await _masterGameRepo.GetMasterGameYear(component.MasterGameID, leagueYear.Year);
                if (masterGameYear is null)
                {
                    throw new Exception($"Invalid master game when getting trade: {tradeEntity.TradeID}");
                }

                var domainComponent = new MasterGameYearWithCounterPick(masterGameYear, component.CounterPick);
                if (component.CurrentParty == TradingParty.Proposer.Value)
                {
                    proposerMasterGameYearWithCounterPicks.Add(domainComponent);
                }
                else if (component.CurrentParty == TradingParty.CounterParty.Value)
                {
                    counterPartyMasterGameYearWithCounterPicks.Add(domainComponent);
                }
                else
                {
                    throw new Exception($"Invalid party when getting trade: {tradeEntity.TradeID}");
                }
            }

            var votes = voteLookup[tradeEntity.TradeID];
            List<TradeVote> tradeVotes = new List<TradeVote>();
            foreach (var vote in votes)
            {
                var user = await _userStore.FindByIdOrThrowAsync(vote.UserID, CancellationToken.None);
                var domainVote = new TradeVote(tradeEntity.TradeID, user, vote.Approved, vote.Comment, vote.Timestamp);
                tradeVotes.Add(domainVote);
            }

            domainTrades.Add(tradeEntity.ToDomain(leagueYear, proposer, counterParty, proposerMasterGameYearWithCounterPicks, counterPartyMasterGameYearWithCounterPicks, tradeVotes));
        }

        return domainTrades;
    }

    public async Task<Trade?> GetTrade(Guid tradeID)
    {
        const string baseTableSQL = "select * from tbl_league_trade WHERE TradeID = @tradeID;";
        const string componentTableSQL = "select tbl_league_tradecomponent.* from tbl_league_tradecomponent " +
                                         "join tbl_league_trade ON tbl_league_tradecomponent.TradeID = tbl_league_trade.TradeID " +
                                         "WHERE tbl_league_trade.TradeID = @tradeID;";
        const string voteTableSQL = "select tbl_league_tradevote.* from tbl_league_tradevote " +
                                    "join tbl_league_trade ON tbl_league_tradevote.TradeID = tbl_league_trade.TradeID " +
                                    "WHERE tbl_league_trade.TradeID = @tradeID;";

        var queryObject = new
        {
            tradeID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var tradeEntity = await connection.QuerySingleOrDefaultAsync<TradeEntity>(baseTableSQL, queryObject);
        if (tradeEntity is null)
        {
            return null;
        }

        var componentEntities = await connection.QueryAsync<TradeComponentEntity>(componentTableSQL, queryObject);
        var voteEntities = await connection.QueryAsync<TradeVoteEntity>(voteTableSQL, queryObject);

        var componentLookup = componentEntities.ToLookup(x => x.TradeID);
        var voteLookup = voteEntities.ToLookup(x => x.TradeID);

        var leagueYear = await this.GetLeagueYearOrThrow(tradeEntity.LeagueID, tradeEntity.Year);
        Publisher proposer = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.ProposerPublisherID);
        Publisher counterParty = leagueYear.GetPublisherByIDOrFakePublisher(tradeEntity.CounterPartyPublisherID);

        var components = componentLookup[tradeEntity.TradeID];
        List<MasterGameYearWithCounterPick> proposerMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
        List<MasterGameYearWithCounterPick> counterPartyMasterGameYearWithCounterPicks = new List<MasterGameYearWithCounterPick>();
        foreach (var component in components)
        {
            var masterGameYear = await _masterGameRepo.GetMasterGameYear(component.MasterGameID, leagueYear.Key.Year);
            if (masterGameYear is null)
            {
                throw new Exception($"Invalid master game when getting trade: {tradeEntity.TradeID}");
            }

            var domainComponent = new MasterGameYearWithCounterPick(masterGameYear, component.CounterPick);
            if (component.CurrentParty == TradingParty.Proposer.Value)
            {
                proposerMasterGameYearWithCounterPicks.Add(domainComponent);
            }
            else if (component.CurrentParty == TradingParty.CounterParty.Value)
            {
                counterPartyMasterGameYearWithCounterPicks.Add(domainComponent);
            }
            else
            {
                throw new Exception($"Invalid party when getting trade: {tradeEntity.TradeID}");
            }
        }

        var votes = voteLookup[tradeEntity.TradeID];
        List<TradeVote> tradeVotes = new List<TradeVote>();
        foreach (var vote in votes)
        {
            var user = await _userStore.FindByIdOrThrowAsync(vote.UserID, CancellationToken.None);
            var domainVote = new TradeVote(tradeEntity.TradeID, user, vote.Approved, vote.Comment, vote.Timestamp);
            tradeVotes.Add(domainVote);
        }

        var domain = tradeEntity.ToDomain(leagueYear, proposer, counterParty, proposerMasterGameYearWithCounterPicks, counterPartyMasterGameYearWithCounterPicks, tradeVotes);

        return domain;
    }

    public async Task<Trade> EditTradeStatus(Trade trade, TradeStatus status, Instant? acceptedTimestamp, Instant? completedTimestamp)
    {
        await using var connection = new MySqlConnection(_connectionString);
        return await EditTradeStatus(trade, status, acceptedTimestamp, completedTimestamp, connection);
    }

    public async Task ExpireTrades(List<Trade> tradesToExpire, Instant expireTimestamp)
    {
        await using var connection = new MySqlConnection(_connectionString);
        string sql = $"update tbl_league_trade set Status = 'Expired', CompletedTimestamp = @expireTimestamp where TradeID IN @tradeIDs";
        var param = new { tradeIDs = tradesToExpire.Select(x => x.TradeID), expireTimestamp };
        await connection.ExecuteAsync(sql, param);
    }

    private static async Task<Trade> EditTradeStatus(Trade trade, TradeStatus status, Instant? acceptedTimestamp, Instant? completedTimestamp, MySqlConnection connection, MySqlTransaction? transaction = null)
    {
        string updateSection = "";
        if (acceptedTimestamp.HasValue)
        {
            updateSection = ", AcceptedTimestamp = @acceptedTimestamp";
        }
        if (completedTimestamp.HasValue)
        {
            updateSection = ", CompletedTimestamp = @completedTimestamp";
        }

        string sql = $"update tbl_league_trade set Status = @status {updateSection} where TradeID = @tradeID";
        var paramsObject = new
        {
            tradeID = trade.TradeID,
            status = status.Value,
            acceptedTimestamp,
            completedTimestamp
        };
        await connection.ExecuteAsync(sql, paramsObject, transaction);

        var updatedTrade = trade.UpdateTrade(status, acceptedTimestamp, completedTimestamp);
        return updatedTrade;
    }

    public async Task AddTradeVote(TradeVote vote)
    {
        TradeVoteEntity entity = new TradeVoteEntity(vote);

        const string sql =
            "insert into tbl_league_tradevote (TradeID,UserID,Approved,Comment,Timestamp) VALUES " +
            "(@TradeID,@UserID,@Approved,@Comment,@Timestamp);";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteTradeVote(Trade trade, FantasyCriticUser user)
    {
        const string sql = "delete from tbl_league_tradevote where TradeID = @tradeID AND UserID = @userID;";
        var paramsObject = new
        {
            tradeID = trade.TradeID,
            userID = user.Id
        };
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, paramsObject);
    }

    public async Task<Trade> ExecuteTrade(ExecutedTrade executedTrade)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var updatedTrade = await EditTradeStatus(executedTrade.Trade, TradeStatus.Executed, null, executedTrade.CompletionTime, connection, transaction);
        await AddLeagueActions(executedTrade.LeagueActions, connection, transaction);

        await UpdatePublisherBudgetsAndDroppedGames(executedTrade.UpdatedPublishers, connection, transaction);

        var flatRemovedPublisherGames = executedTrade.RemovedPublisherGames.Select(x => x.PublisherGame).ToList();
        await DeletePublisherGames(flatRemovedPublisherGames, connection, transaction);
        await AddFormerPublisherGames(executedTrade.RemovedPublisherGames, connection, transaction);
        await AddPublisherGames(executedTrade.AddedPublisherGames, connection, transaction);
        await MakePublisherGameSlotsConsistent(executedTrade.Trade.LeagueYear, executedTrade.UpdatedPublishers, connection, transaction);

        await transaction.CommitAsync();
        return updatedTrade;
    }

    public async  Task<IReadOnlyList<SpecialAuction>> GetAllActiveSpecialAuctions()
    {
        const string sql = "select * from tbl_league_specialauction where Processed = 0;";

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<SpecialAuctionEntity>(sql);

        List<SpecialAuction> domains = new List<SpecialAuction>();
        foreach (var result in results)
        {
            var masterGame = await _masterGameRepo.GetMasterGameYearOrThrow(result.MasterGameID, result.Year);
            domains.Add(result.ToDomain(masterGame));
        }

        return domains;
    }

    public async Task<IReadOnlyList<SpecialAuction>> GetSpecialAuctions(LeagueYear leagueYear)
    {
        const string sql = "select * from tbl_league_specialauction where LeagueID = @LeagueID AND Year = @Year;";
        var key = new LeagueYearKeyEntity(leagueYear.Key);

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<SpecialAuctionEntity>(sql, key);

        List<SpecialAuction> domains = new List<SpecialAuction>();
        foreach (var result in results)
        {
            var masterGame = await _masterGameRepo.GetMasterGameYearOrThrow(result.MasterGameID, leagueYear.Year);
            domains.Add(result.ToDomain(masterGame));
        }

        return domains;
    }

    public async Task CreateSpecialAuction(SpecialAuction specialAuction, LeagueAction action)
    {
        const string sql =
            "INSERT INTO tbl_league_specialauction(SpecialAuctionID,LeagueID,Year,MasterGameID,CreationTime,ScheduledEndTime,Processed) " +
            "VALUES (@SpecialAuctionID,@LeagueID,@Year,@MasterGameID,@CreationTime,@ScheduledEndTime,@Processed)";

        var entity = new SpecialAuctionEntity(specialAuction);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(sql, entity, transaction);
        await AddLeagueAction(action, connection, transaction);

        await transaction.CommitAsync();
    }

    public async Task CancelSpecialAuction(SpecialAuction specialAuction, LeagueAction action)
    {
        const string sql = "DELETE FROM tbl_league_specialauction WHERE LeagueID = @LeagueID AND Year = @Year AND MasterGameID = @MasterGameID;";
        var entity = new SpecialAuctionEntity(specialAuction);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(sql, entity, transaction);
        await AddLeagueAction(action, connection, transaction);

        await transaction.CommitAsync();
    }

    public async Task GrantSuperDrops(IEnumerable<Publisher> publishersToGrantSuperDrop, IEnumerable<LeagueAction> superDropActions)
    {
        const string sql = "UPDATE tbl_league_publisher SET SuperDropsAvailable = SuperDropsAvailable + 1 WHERE PublisherID in @publisherIDs";
        var updateObject = new
        {
            publisherIDs = publishersToGrantSuperDrop.Select(x => x.PublisherID).ToList()
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(sql, updateObject, transaction);
        await AddLeagueActions(superDropActions, connection, transaction);

        await transaction.CommitAsync();
    }

    public async Task ManualMakePublisherGameSlotsConsistent(int year)
    {
        var leagueYears = await GetLeagueYears(year);
        var publisherPairs = leagueYears.SelectMany(leagueYear => leagueYear.Publishers, (leagueYear, publisher) => new LeagueYearPublisherPair(leagueYear, publisher)).ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await MakePublisherGameSlotsConsistent(publisherPairs, connection, transaction);
        await transaction.CommitAsync();
    }

    private static Task MakePublisherGameSlotsConsistent(LeagueYear leagueYear, IEnumerable<Publisher> publishersToUpdate, MySqlConnection connection, MySqlTransaction transaction)
    {
        var pairs = publishersToUpdate.Select(x => new LeagueYearPublisherPair(leagueYear, x));
        return MakePublisherGameSlotsConsistent(pairs, connection, transaction);
    }

    private static Task MakePublisherGameSlotsConsistent(LeagueYear leagueYear, Publisher publisher, MySqlConnection connection, MySqlTransaction transaction)
    {
        return MakePublisherGameSlotsConsistent(new List<LeagueYearPublisherPair>() { new LeagueYearPublisherPair(leagueYear, publisher) }, connection, transaction);
    }

    private static async Task MakePublisherGameSlotsConsistent(IEnumerable<LeagueYearPublisherPair> publisherPairs, MySqlConnection connection, MySqlTransaction transaction)
    {
        var specialSlotPublisherIDs = publisherPairs.Where(x => x.LeagueYear.Options.HasSpecialSlots).Select(x => x.Publisher.PublisherID).ToHashSet();

        int tempSlotNumber = 1000;
        var preRunUpdates = new List<PublisherGameSlotNumberUpdateEntity>();
        var finalUpdates = new List<PublisherGameSlotNumberUpdateEntity>();
        foreach (var publisherPair in publisherPairs)
        {
            int slotNumber = 0;
            if (!specialSlotPublisherIDs.Contains(publisherPair.Publisher.PublisherID))
            {
                var standardGames = publisherPair.Publisher.PublisherGames
                    .Where(x => !x.CounterPick)
                    .OrderBy(x => x.Timestamp).ThenBy(x => x.SlotNumber);
                foreach (var standardGame in standardGames)
                {
                    preRunUpdates.Add(new PublisherGameSlotNumberUpdateEntity(standardGame.PublisherGameID, tempSlotNumber));
                    finalUpdates.Add(new PublisherGameSlotNumberUpdateEntity(standardGame.PublisherGameID, slotNumber));
                    tempSlotNumber++;
                    slotNumber++;
                }
            }

            //Counter Picks don't have special slots, so they should always be consistent.
            slotNumber = 0;
            var counterPicks = publisherPair.Publisher.PublisherGames
                .Where(x => x.CounterPick)
                .OrderBy(x => x.SlotNumber);
            foreach (var counterPick in counterPicks)
            {
                preRunUpdates.Add(new PublisherGameSlotNumberUpdateEntity(counterPick.PublisherGameID, tempSlotNumber));
                finalUpdates.Add(new PublisherGameSlotNumberUpdateEntity(counterPick.PublisherGameID, slotNumber));
                tempSlotNumber++;
                slotNumber++;
            }
        }

        const string sql = "UPDATE tbl_league_publishergame SET SlotNumber = @SlotNumber WHERE PublisherGameID = @PublisherGameID;";
        await connection.ExecuteAsync(sql, preRunUpdates, transaction);
        await connection.ExecuteAsync(sql, finalUpdates, transaction);
    }

    private static async Task OrganizeSlots(LeagueYear leagueYear, IReadOnlyDictionary<Guid, int> slotAssignments, MySqlConnection connection, MySqlTransaction transaction)
    {
        if (!slotAssignments.Any())
        {
            return;
        }

        int tempSlotNumber = 1000;
        List<PublisherGameSlotNumberUpdateEntity> preRunUpdates = new List<PublisherGameSlotNumberUpdateEntity>();
        List<PublisherGameSlotNumberUpdateEntity> finalUpdates = new List<PublisherGameSlotNumberUpdateEntity>();
        foreach (var publisher in leagueYear.Publishers)
        {
            var nonCounterPicks = publisher.PublisherGames.Where(x => !x.CounterPick);
            foreach (var publisherGame in nonCounterPicks)
            {
                var foundGame = slotAssignments.TryGetValue(publisherGame.PublisherGameID, out var newSlotNumber);
                if (!foundGame)
                {
                    string error =
                        $"Cannot figure out slots for LeagueID: {publisher.LeagueYearKey.LeagueID} PublisherID: {publisher.PublisherID} " +
                        $"PublisherGameID: {publisherGame.PublisherGameID} GameName: {publisherGame.GameName}";
                    throw new Exception(error);
                }

                preRunUpdates.Add(new PublisherGameSlotNumberUpdateEntity(publisherGame.PublisherGameID, tempSlotNumber));
                finalUpdates.Add(new PublisherGameSlotNumberUpdateEntity(publisherGame.PublisherGameID, newSlotNumber));
                tempSlotNumber++;
            }
        }

        const string sql = "UPDATE tbl_league_publishergame SET SlotNumber = @SlotNumber WHERE PublisherGameID = @PublisherGameID;";
        await connection.ExecuteAsync(sql, preRunUpdates, transaction);
        await connection.ExecuteAsync(sql, finalUpdates, transaction);
    }

    public async Task ReorderPublisherGames(Publisher publisher, Dictionary<int, Guid?> slotStates)
    {
        var realOrderEntities = new List<PublisherGameSlotNumberUpdateEntity>();
        var tempOrderEntities = new List<PublisherGameSlotNumberUpdateEntity>();

        int tempSlotNumber = 10_000;
        foreach (var slotState in slotStates)
        {
            tempOrderEntities.Add(new PublisherGameSlotNumberUpdateEntity(slotState.Value, tempSlotNumber));
            realOrderEntities.Add(new PublisherGameSlotNumberUpdateEntity(slotState.Value, slotState.Key));
            tempSlotNumber++;
        }

        const string sql = "UPDATE tbl_league_publishergame SET SlotNumber = @SlotNumber WHERE PublisherGameID = @PublisherGameID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(sql, tempOrderEntities, transaction);
        await connection.ExecuteAsync(sql, realOrderEntities, transaction);
        await transaction.CommitAsync();
    }

    public async Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_league_publishergame set MasterGameID = @masterGameID where PublisherGameID = @publisherGameID",
            new
            {
                masterGameID = masterGame.MasterGameID,
                publisherGameID = publisherGame.PublisherGameID
            });
    }

    public async Task MergeMasterGame(MasterGame removeMasterGame, MasterGame mergeIntoMasterGame)
    {
        const string mergeSQL =
            "UPDATE tbl_league_droprequest SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_eligibilityoverride SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_pickupbid SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_pickupbid SET ConditionalDropMasterGameID = @mergeIntoMasterGameID WHERE ConditionalDropMasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_publishergame SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_formerpublishergame SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_publisherqueue SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_tradecomponent SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_league_specialauction SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_mastergame_changerequest SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_mastergame_request SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_mastergame_subgame SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
            "UPDATE tbl_royale_publishergame SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;";

        const string removeGameSQL = "DELETE FROM tbl_mastergame WHERE MasterGameID = @removeMasterGameID;";
        const string removeTagsSQL = "DELETE FROM tbl_mastergame_hastag WHERE MasterGameID = @removeMasterGameID;";

        var requestObject = new
        {
            removeMasterGameID = removeMasterGame.MasterGameID,
            mergeIntoMasterGameID = mergeIntoMasterGame.MasterGameID,
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string queueSQL = "select * from tbl_league_publisherqueue where MasterGameID in @masterGameIDs;";
        var queueQueryParam = new
        {
            masterGameIDs = new List<Guid>()
            {
                removeMasterGame.MasterGameID,
                mergeIntoMasterGame.MasterGameID
            }
        };

        var queueEntities = await connection.QueryAsync<QueuedGameEntity>(queueSQL, queueQueryParam);
        var groupedByPublisher = queueEntities.GroupBy(x => x.PublisherID);
        var publishersIDsWithBothGames = groupedByPublisher.Where(x => x.Count() == 2).Select(x => x.Key);
        const string fixQueueSQL = "DELETE FROM tbl_league_publisherqueue where MasterGameID = @removeMasterGameID AND PublisherID IN @publishersIDsWithBothGames";
        var fixQueueObject = new
        {
            removeMasterGameID = removeMasterGame.MasterGameID,
            publishersIDsWithBothGames
        };

        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(fixQueueSQL, fixQueueObject, transaction);
        await connection.ExecuteAsync(mergeSQL, requestObject, transaction);
        await connection.ExecuteAsync(removeTagsSQL, requestObject, transaction);
        await connection.ExecuteAsync(removeGameSQL, requestObject, transaction);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
    {
        if (_supportedYearCache is not null)
        {
            return _supportedYearCache;
        }
        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<SupportedYearEntity>("select * from tbl_meta_supportedyear;");
        var supportedYears = results.Select(x => x.ToDomain()).ToList();
        _supportedYearCache = supportedYears;
        return supportedYears;
    }

    public async Task<SupportedYear> GetSupportedYear(int year)
    {
        var supportedYears = await GetSupportedYears();
        var supportedYear = supportedYears.Single(x => x.Year == year);
        return supportedYear;
    }

    private async Task<IReadOnlyList<League>> ConvertLeagueEntitiesToDomain(IEnumerable<LeagueEntity> leagueEntities)
    {
        var relevantUserIDs = leagueEntities.Select(x => x.LeagueManager).Distinct();
        var relevantUsers = await _userStore.GetUsers(relevantUserIDs);
        var userDictionary = relevantUsers.ToDictionary(x => x.Id);

        const string sql = "select * from tbl_league_year where LeagueID in @leagueIDs";
        var queryObject = new
        {
            leagueIDs = leagueEntities.Select(x => x.LeagueID)
        };
        await using var connection = new MySqlConnection(_connectionString);
        List<League> leagues = new List<League>();
        IEnumerable<LeagueYearEntity> allLeagueYears = await connection.QueryAsync<LeagueYearEntity>(sql, queryObject);
        var leagueYearLookup = allLeagueYears.ToLookup(x => x.LeagueID);
        var oneShotLeagues = await GetLeaguesWithMostRecentYearOneShot();

        foreach (var leagueEntity in leagueEntities)
        {
            FantasyCriticUser manager = userDictionary[leagueEntity.LeagueManager];
            IEnumerable<int> years = leagueYearLookup[leagueEntity.LeagueID].Select(x => x.Year);
            League league = leagueEntity.ToDomain(manager, years);
            leagues.Add(league);
        }

        return leagues;
    }

    public async Task<IReadOnlyList<PublisherGame>> GetPublisherGames(Guid publisherID, int leagueYear)
    {
        var query = new
        {
            publisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<PublisherGameEntity> gameEntities = await connection.QueryAsync<PublisherGameEntity>(
            "select tbl_league_publishergame.* from tbl_league_publishergame " +
            "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
            "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
            "where tbl_league_publishergame.PublisherID = @publisherID and IsDeleted = 0;",
            query);

        List<PublisherGame> domainGames = new List<PublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, leagueYear);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    private async Task<IReadOnlyList<FormerPublisherGame>> GetFormerPublisherGames(Guid publisherID, int leagueYear)
    {
        var query = new
        {
            publisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<FormerPublisherGameEntity> gameEntities = await connection.QueryAsync<FormerPublisherGameEntity>(
            "select tbl_league_formerpublishergame.* from tbl_league_formerpublishergame " +
            "join tbl_league_publisher on (tbl_league_formerpublishergame.PublisherID = tbl_league_publisher.PublisherID) " +
            "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
            "where tbl_league_formerpublishergame.PublisherID = @publisherID and IsDeleted = 0;",
            query);

        List<FormerPublisherGame> domainGames = new List<FormerPublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, leagueYear);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    private async Task<IReadOnlyList<PublisherGame>> ConvertPublisherGameEntities(IEnumerable<PublisherGameEntity> gameEntities, int year)
    {
        List<PublisherGame> domainGames = new List<PublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    private async Task<IReadOnlyList<FormerPublisherGame>> ConvertFormerPublisherGameEntities(IEnumerable<FormerPublisherGameEntity> gameEntities, int year)
    {
        List<FormerPublisherGame> domainGames = new List<FormerPublisherGame>();
        foreach (var entity in gameEntities)
        {
            MasterGameYear? masterGame = null;
            if (entity.MasterGameID.HasValue)
            {
                masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
            }

            domainGames.Add(entity.ToDomain(masterGame));
        }

        return domainGames;
    }

    public async Task SetDraftOrder(IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions, LeagueAction leagueAction)
    {
        const string updateDraftOrderSQL = "update tbl_league_publisher SET DraftPosition = @draftPosition where PublisherID = @publisherID;";
        const string setFlagSQL = "update tbl_league_year SET DraftOrderSet = 1 WHERE LeagueID = @LeagueID AND Year = @Year;";
        var leagueYearKey = new LeagueYearKeyEntity(leagueAction.Publisher.LeagueYearKey);
        var tempPositions = draftPositions.Select(x => new SetDraftOrderEntity(x.Key.PublisherID, x.Value + 100));
        var finalPositions = draftPositions.Select(x => new SetDraftOrderEntity(x.Key.PublisherID, x.Value));

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(updateDraftOrderSQL, tempPositions, transaction);
        await connection.ExecuteAsync(updateDraftOrderSQL, finalPositions, transaction);
        await AddLeagueAction(leagueAction, connection, transaction);
        await connection.ExecuteAsync(setFlagSQL, leagueYearKey, transaction);

        await transaction.CommitAsync();
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<EligibilityOverride>>> GetAllEligibilityOverrides(int year)
    {
        const string sql = "select tbl_league_eligibilityoverride.* from tbl_league_eligibilityoverride " +
                     "join tbl_league on (tbl_league.LeagueID = tbl_league_eligibilityoverride.LeagueID) " +
                     "where Year = @year and IsDeleted = 0;";
        var queryObject = new
        {
            year
        };

        IEnumerable<EligibilityOverrideEntity> results;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            results = await connection.QueryAsync<EligibilityOverrideEntity>(sql, queryObject);
        }

        List<Tuple<Guid, EligibilityOverride>> domainObjects = new List<Tuple<Guid, EligibilityOverride>>();
        foreach (var result in results)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(result.MasterGameID);
            EligibilityOverride domain = result.ToDomain(masterGame);
            domainObjects.Add(new Tuple<Guid, EligibilityOverride>(result.LeagueID, domain));
        }

        var dictionary = domainObjects
            .GroupBy(x => x.Item1)
            .ToDictionary(x => x.Key, y => (IReadOnlyList<EligibilityOverride>)y.Select(z => z.Item2).ToList());
        return dictionary;
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<TagOverride>>> GetAllTagOverrides(int year)
    {
        const string sql = "select tbl_league_tagoverride.* from tbl_league_tagoverride " +
                     "join tbl_league on (tbl_league.LeagueID = tbl_league_tagoverride.LeagueID) " +
                     "where Year = @year and IsDeleted = 0;";
        var queryObject = new
        {
            year
        };

        IEnumerable<TagOverrideEntity> results;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            results = await connection.QueryAsync<TagOverrideEntity>(sql, queryObject);
        }

        var allTags = await _masterGameRepo.GetMasterGameTags();
        var tagDictionary = allTags.ToDictionary(x => x.Name);

        List<Tuple<Guid, TagOverride>> domainObjects = new List<Tuple<Guid, TagOverride>>();

        var groupedResults = results.GroupBy(x => (x.LeagueID, x.Year, x.MasterGameID));
        foreach (var resultGroup in groupedResults)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(resultGroup.Key.MasterGameID);
            List<MasterGameTag> tagsForGroup = new List<MasterGameTag>();
            foreach (var result in resultGroup)
            {
                var fullTag = tagDictionary[result.TagName];
                tagsForGroup.Add(fullTag);
            }

            TagOverride domain = new TagOverride(masterGame, tagsForGroup);
            domainObjects.Add(new Tuple<Guid, TagOverride>(resultGroup.Key.LeagueID, domain));
        }

        var dictionary = domainObjects.GroupBy(x => x.Item1).ToDictionary(x => x.Key, y => (IReadOnlyList<TagOverride>)y.Select(z => z.Item2).ToList());
        return dictionary;
    }

    public async Task DeleteEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await DeleteEligibilityOverride(leagueYear, masterGame, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task SetEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, bool eligible)
    {
        const string sql = "insert into tbl_league_eligibilityoverride(LeagueID,Year,MasterGameID,Eligible) VALUES " +
                     "(@leagueID,@year,@masterGameID,@eligible)";

        var insertObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year,
            masterGameID = masterGame.MasterGameID,
            eligible
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await DeleteEligibilityOverride(leagueYear, masterGame, connection, transaction);
        await connection.ExecuteAsync(sql, insertObject, transaction);
        await transaction.CommitAsync();
    }

    public async Task SetTagOverride(LeagueYear leagueYear, MasterGame masterGame, IEnumerable<MasterGameTag> requestedTags)
    {
        const string deleteSQL = "DELETE from tbl_league_tagoverride where LeagueID = @leagueID AND Year = @year AND MasterGameID = @masterGameID;";

        var deleteObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year,
            masterGameID = masterGame.MasterGameID
        };

        var insertEntities = requestedTags.Select(x => new TagOverrideEntity()
        {
            LeagueID = leagueYear.League.LeagueID,
            Year = leagueYear.Year,
            MasterGameID = masterGame.MasterGameID,
            TagName = x.Name
        });

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteSQL, deleteObject, transaction);
        if (insertEntities.Any())
        {
            await connection.BulkInsertAsync(insertEntities, "tbl_league_tagoverride", 500, transaction);
        }
        await transaction.CommitAsync();
    }

    private static async Task DeleteEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql = "delete from tbl_league_eligibilityoverride where LeagueID = @leagueID and Year = @year and MasterGameID = @masterGameID;";
        var queryObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year,
            masterGameID = masterGame.MasterGameID
        };

        await connection.ExecuteAsync(sql, queryObject, transaction);
    }

    public async Task<SystemWideValues> GetSystemWideValues()
    {
        await using var connection = new MySqlConnection(_connectionString);
        var positionPoints = await connection.QueryAsync<AveragePositionPointsEntity>("select * from tbl_caching_averagepositionpoints;");
        var result = await connection.QuerySingleAsync<SystemWideValuesEntity>("select * from tbl_caching_systemwidevalues;");
        return result.ToDomain(positionPoints.Select(x => x.ToDomain()));
    }

    public async Task<SystemWideSettings> GetSystemWideSettings()
    {
        await using var connection = new MySqlConnection(_connectionString);
        var result = await connection.QuerySingleAsync<SystemWideSettingsEntity>("select * from tbl_meta_systemwidesettings;");
        return result.ToDomain();
    }

    public async Task<SiteCounts> GetSiteCounts()
    {
        await using var connection = new MySqlConnection(_connectionString);
        var result = await connection.QuerySingleAsync<SiteCountsEntity>("select * from vw_meta_sitecounts;");
        return result.ToDomain();
    }

    public async Task SetActionProcessingMode(bool modeOn)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("update tbl_meta_systemwidesettings set ActionProcessingMode = @modeOn;", new { modeOn });
    }

    public async Task EditPublisher(EditPublisherRequest editValues, LeagueAction leagueAction)
    {
        string sql = "update tbl_league_publisher SET ";
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("publisherID", editValues.Publisher.PublisherID);

        if (editValues.NewPublisherName is not null)
        {
            sql += "PublisherName = @publisherName,";
            parameters.Add("publisherName", editValues.NewPublisherName);
        }
        if (editValues.Budget.HasValue)
        {
            sql += "Budget = @budget,";
            parameters.Add("budget", editValues.Budget.Value);
        }
        if (editValues.WillNotReleaseGamesDropped.HasValue)
        {
            sql += "WillNotReleaseGamesDropped = @willNotReleaseGamesDropped,";
            parameters.Add("willNotReleaseGamesDropped", editValues.WillNotReleaseGamesDropped.Value);
        }
        if (editValues.WillReleaseGamesDropped.HasValue)
        {
            sql += "WillReleaseGamesDropped = @willReleaseGamesDropped,";
            parameters.Add("willReleaseGamesDropped", editValues.WillReleaseGamesDropped.Value);
        }
        if (editValues.FreeGamesDropped.HasValue)
        {
            sql += "FreeGamesDropped = @freeGamesDropped,";
            parameters.Add("freeGamesDropped", editValues.FreeGamesDropped.Value);
        }
        if (editValues.SuperDropsAvailable.HasValue)
        {
            sql += "SuperDropsAvailable = @superDropsAvailable,";
            parameters.Add("superDropsAvailable", editValues.SuperDropsAvailable.Value);
        }

        sql = sql.TrimEnd(',');
        sql += " WHERE PublisherID = @publisherID";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(sql, parameters, transaction);
        await AddLeagueAction(leagueAction, connection, transaction);
        await transaction.CommitAsync();
    }

    public async Task DeletePublisher(Publisher publisher)
    {
        var deleteObject = new
        {
            publisherID = publisher.PublisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "delete from tbl_league_publisher where PublisherID = @publisherID;",
            deleteObject);
    }

    public async Task DeleteLeagueYear(LeagueYear leagueYear)
    {
        var deleteObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "delete from tbl_league_year where LeagueID = @leagueID and Year = @year;",
            deleteObject);
    }

    public async Task DeleteLeague(League league)
    {
        var deleteObject = new
        {
            leagueID = league.LeagueID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("delete from tbl_league where LeagueID = @leagueID;", deleteObject);
    }

    public async Task DeleteLeagueActions(Publisher publisher)
    {
        var deleteObject = new
        {
            publisherID = publisher.PublisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("delete from tbl_league_action where PublisherID = @publisherID;", deleteObject);
    }

    public async Task<IReadOnlyList<ActionProcessingSetMetadata>> GetActionProcessingSets()
    {
        const string sql = "select * from tbl_meta_actionprocessingset;";
        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<ActionProcessingSetEntity>(sql);

        var domains = results.Select(x => x.ToDomain()).ToList();
        return domains;
    }

    public async Task SaveProcessedActionResults(FinalizedActionProcessingResults actionProcessingResults)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await CreateProcessSet(actionProcessingResults, connection, transaction);
        await MarkBidStatus(actionProcessingResults.Results.SuccessBids, true, actionProcessingResults.ProcessSetID, connection, transaction);
        await MarkBidStatus(actionProcessingResults.Results.FailedBids, false, actionProcessingResults.ProcessSetID, connection, transaction);

        await MarkDropStatus(actionProcessingResults.Results.SuccessDrops, true, actionProcessingResults.ProcessSetID, connection, transaction);
        await MarkDropStatus(actionProcessingResults.Results.FailedDrops, false, actionProcessingResults.ProcessSetID, connection, transaction);

        await AddLeagueActions(actionProcessingResults.Results.LeagueActions, connection, transaction);
        await UpdatePublisherBudgetsAndDroppedGames(actionProcessingResults.Results.PublisherStateSet.Publishers, connection, transaction);

        var flatRemovedPublisherGames = actionProcessingResults.Results.RemovedPublisherGames.Select(x => x.PublisherGame).ToList();
        await DeletePublisherGames(flatRemovedPublisherGames, connection, transaction);
        await AddFormerPublisherGames(actionProcessingResults.Results.RemovedPublisherGames, connection, transaction);
        await AddPublisherGames(actionProcessingResults.Results.AddedPublisherGames, connection, transaction);

        var publisherPairsToAdjust = actionProcessingResults.Results.GetAllAffectedPublisherPairs();
        await MakePublisherGameSlotsConsistent(publisherPairsToAdjust, connection, transaction);
        await MarkSpecialAuctionsFinalized(actionProcessingResults.SpecialAuctionsProcessed, connection, transaction);

        await transaction.CommitAsync();
    }

    private static Task MarkSpecialAuctionsFinalized(IEnumerable<SpecialAuction> specialAuctionsProcessed, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql = "UPDATE tbl_league_specialauction SET Processed = 1 WHERE SpecialAuctionID IN @specialAuctionIDs";
        var paramsObject = new
        {
            specialAuctionIDs = specialAuctionsProcessed.Select(x => x.SpecialAuctionID).ToList()
        };

        return connection.ExecuteAsync(sql, paramsObject, transaction);
    }

    public async Task UpdateSystemWideValues(SystemWideValues systemWideValues)
    {
        const string deleteSQL = "delete from tbl_caching_systemwidevalues;";
        const string deletePositionsSQL = "delete from tbl_caching_averagepositionpoints;";
        const string insertSQL = "INSERT into tbl_caching_systemwidevalues VALUES (@AverageStandardGamePoints,@AveragePickupOnlyStandardGamePoints,@AverageCounterPickPoints);";

        SystemWideValuesEntity entity = new SystemWideValuesEntity(systemWideValues);
        var positionEntities = systemWideValues.AverageStandardGamePointsByPickPosition.Select(x => new AveragePositionPointsEntity(x)).ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(deleteSQL, transaction: transaction);
        await connection.ExecuteAsync(deletePositionsSQL, transaction: transaction);
        await connection.ExecuteAsync(insertSQL, entity, transaction);
        await connection.BulkInsertAsync(positionEntities, "tbl_caching_averagepositionpoints", 500, transaction);
        await transaction.CommitAsync();
    }

    public async Task SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
    {
        int tempPosition = bidPriorities.Select(x => x.Value).Max() + 1;
        var tempEntities = bidPriorities.Select(x => new PickupBidPriorityUpdateEntity(x.Key.BidID, tempPosition++)).ToList();
        var finalEntities = bidPriorities.Select(x => new PickupBidPriorityUpdateEntity(x.Key.BidID, x.Value)).ToList();
        const string sql = "update tbl_league_pickupbid set Priority = @Priority where BidID = @BidID";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(sql, tempEntities, transaction);
        await connection.ExecuteAsync(sql, finalEntities, transaction);
        await transaction.CommitAsync();
    }

    public async Task CreateDropRequest(DropRequest currentDropRequest)
    {
        var entity = new DropRequestEntity(currentDropRequest);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "insert into tbl_league_droprequest(DropRequestID,PublisherID,MasterGameID,Timestamp,Successful) VALUES " +
            "(@DropRequestID,@PublisherID,@MasterGameID,@Timestamp,@Successful);",
            entity);
    }

    public async Task RemoveDropRequest(DropRequest dropRequest)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync("delete from tbl_league_droprequest where DropRequestID = @DropRequestID", new { dropRequest.DropRequestID });
    }

    public async Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(LeagueYear leagueYear, Publisher publisher)
    {
        await using var connection = new MySqlConnection(_connectionString);
        var dropEntities = await connection.QueryAsync<DropRequestEntity>("select * from tbl_league_droprequest where PublisherID = @publisherID and Successful is NULL",
            new { publisherID = publisher.PublisherID });

        List<DropRequest> domainDrops = new List<DropRequest>();
        foreach (var dropEntity in dropEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(dropEntity.MasterGameID);
            DropRequest domain = dropEntity.ToDomain(publisher, masterGame, leagueYear);
            domainDrops.Add(domain);
        }

        return domainDrops;
    }

    public async Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(int year, IReadOnlyList<LeagueYear> allLeagueYears)
    {
        var leagueDictionary = allLeagueYears.ToDictionary(x => x.Key);
        var publisherDictionary = allLeagueYears.SelectMany(x => x.Publishers).ToDictionary(x => x.PublisherID, y => y);

        await using var connection = new MySqlConnection(_connectionString);
        var dropEntities = await connection.QueryAsync<DropRequestEntity>("select * from vw_league_droprequest where Year = @year and Successful is NULL and IsDeleted = 0", new { year });

        Dictionary<LeagueYear, List<DropRequest>> dropRequestsByLeagueYear = allLeagueYears.ToDictionary(x => x, _ => new List<DropRequest>());

        foreach (var dropEntity in dropEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(dropEntity.MasterGameID);
            var publisher = publisherDictionary[dropEntity.PublisherID];
            var leagueYear = leagueDictionary[publisher.LeagueYearKey];
            DropRequest domainDrop = dropEntity.ToDomain(publisher, masterGame, leagueYear);
            dropRequestsByLeagueYear[leagueYear].Add(domainDrop);
        }

        return dropRequestsByLeagueYear.SealDictionary();
    }

    private static Task CreateProcessSet(FinalizedActionProcessingResults actionProcessingResults, MySqlConnection connection, MySqlTransaction transaction)
    {
        var entity = new ActionProcessingSetEntity(actionProcessingResults);
        return connection.ExecuteAsync(
            "insert into tbl_meta_actionprocessingset(ProcessSetID,ProcessTime,ProcessName) VALUES " +
            "(@ProcessSetID,@ProcessTime,@ProcessName);", entity, transaction);
    }

    private static Task MarkBidStatus(IEnumerable<IProcessedBid> bids, bool success, Guid processSetID, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql = "update tbl_league_pickupbid SET Successful = @Successful, ProcessSetID = @ProcessSetID, Outcome = @Outcome, ProjectedPointsAtTimeOfBid = @ProjectedPointsAtTimeOfBid where BidID = @BidID;";
        var entities = bids.Select(x => new PickupBidEntity(x, success, processSetID));
        return connection.ExecuteAsync(sql, entities, transaction);
    }

    private static Task MarkDropStatus(IEnumerable<DropRequest> drops, bool success, Guid processSetID, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql = "update tbl_league_droprequest SET Successful = @Successful, ProcessSetID = @ProcessSetID where DropRequestID = @DropRequestID;";
        var entities = drops.Select(x => new DropRequestEntity(x, success, processSetID));
        return connection.ExecuteAsync(sql, entities, transaction);
    }

    private static Task UpdatePublisherBudgetsAndDroppedGames(IEnumerable<Publisher> updatedPublishers, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql = "update tbl_league_publisher SET Budget = @Budget, FreeGamesDropped = @FreeGamesDropped, " +
                     "WillNotReleaseGamesDropped = @WillNotReleaseGamesDropped, WillReleaseGamesDropped = @WillReleaseGamesDropped where PublisherID = @PublisherID;";
        var entities = updatedPublishers.Select(x => new PublisherEntity(x));
        return connection.ExecuteAsync(sql, entities, transaction);
    }

    private static Task DecrementSuperDropsAvailable(Publisher publisher, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql = "update tbl_league_publisher SET SuperDropsAvailable = SuperDropsAvailable - 1 where PublisherID = @PublisherID;";
        var paramsObject = new
        {
            publisherID = publisher.PublisherID
        };
        return connection.ExecuteAsync(sql, paramsObject, transaction);
    }

    private static Task AddLeagueAction(LeagueAction action, MySqlConnection connection, MySqlTransaction transaction) => AddLeagueActions(new List<LeagueAction>() { action }, connection, transaction);

    private static Task AddLeagueActions(IEnumerable<LeagueAction> actions, MySqlConnection connection, MySqlTransaction transaction)
    {
        var entities = actions.Select(x => new LeagueActionEntity(x));
        return connection.ExecuteAsync(
            "insert into tbl_league_action(PublisherID,Timestamp,ActionType,Description,ManagerAction) VALUES " +
            "(@PublisherID,@Timestamp,@ActionType,@Description,@ManagerAction);", entities, transaction);
    }

    private static Task AddPublisherGames(IEnumerable<PublisherGame> publisherGames, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql =
            "insert into tbl_league_publishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore," +
            "ManualWillNotRelease,FantasyPoints,MasterGameID,SlotNumber,DraftPosition,OverallDraftPosition,BidAmount,AcquiredInTradeID) VALUES " +
            "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore," +
            "@ManualWillNotRelease,@FantasyPoints,@MasterGameID,@SlotNumber,@DraftPosition,@OverallDraftPosition,@BidAmount,@AcquiredInTradeID);";
        var entities = publisherGames.Select(x => new PublisherGameEntity(x));
        return connection.ExecuteAsync(sql, entities, transaction);
    }

    private static Task AddFormerPublisherGames(IEnumerable<FormerPublisherGame> publisherGames, MySqlConnection connection, MySqlTransaction transaction)
    {
        const string sql =
            "insert into tbl_league_formerpublishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore," +
            "ManualWillNotRelease,FantasyPoints,MasterGameID,DraftPosition,OverallDraftPosition,BidAmount,AcquiredInTradeID,RemovedTimestamp,RemovedNote) VALUES " +
            "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore," +
            "@ManualWillNotRelease,@FantasyPoints,@MasterGameID,@DraftPosition,@OverallDraftPosition,@BidAmount,@AcquiredInTradeID,@RemovedTimestamp,@RemovedNote);";
        var entities = publisherGames.Select(x => new FormerPublisherGameEntity(x));
        return connection.ExecuteAsync(sql, entities, transaction);
    }

    private static Task DeletePublisherGames(IEnumerable<PublisherGame> publisherGames, MySqlConnection connection, MySqlTransaction transaction)
    {
        var publisherGameIDs = publisherGames.Select(x => x.PublisherGameID);
        return connection.ExecuteAsync(
            "delete from tbl_league_publishergame where PublisherGameID in @publisherGameIDs;", new { publisherGameIDs }, transaction);
    }

    public async Task AddPlayerToLeague(League league, FantasyCriticUser inviteUser)
    {
        var mostRecentYear = await this.GetLeagueYearOrThrow(league.LeagueID, league.Years.Max());
        bool mostRecentYearNotStarted = !mostRecentYear.PlayStatus.PlayStarted;

        var userAddObject = new
        {
            leagueID = league.LeagueID,
            userID = inviteUser.Id,
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync("insert into tbl_league_hasuser(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject, transaction);
        if (mostRecentYearNotStarted)
        {
            var userActiveObject = new
            {
                leagueID = league.LeagueID,
                userID = inviteUser.Id,
                activeYear = mostRecentYear.Year
            };
            await connection.ExecuteAsync("insert into tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@activeYear,@userID);", userActiveObject, transaction);
        }
        await transaction.CommitAsync();
    }

    private async Task<IReadOnlyList<LeagueInvite>> ConvertLeagueInviteEntities(IEnumerable<LeagueInviteEntity> entities)
    {
        List<LeagueInvite> leagueInvites = new List<LeagueInvite>();
        foreach (var entity in entities)
        {
            var league = await this.GetLeagueOrThrow(entity.LeagueID);
            if (entity.UserID.HasValue)
            {
                FantasyCriticUser user = await _userStore.FindByIdOrThrowAsync(entity.UserID.Value, CancellationToken.None);
                leagueInvites.Add(entity.ToDomain(league, user));
            }
            else
            {
                leagueInvites.Add(entity.ToDomain(league));
            }
        }

        return leagueInvites;
    }

    private async Task<IReadOnlyList<LeagueInvite>> ConvertLeagueInviteEntitiesForSingleLeague(League league, IEnumerable<LeagueInviteEntity> entities)
    {
        List<LeagueInvite> leagueInvites = new List<LeagueInvite>();
        foreach (var entity in entities)
        {
            if (entity.UserID.HasValue)
            {
                FantasyCriticUser user = await _userStore.FindByIdOrThrowAsync(entity.UserID.Value, CancellationToken.None);
                leagueInvites.Add(entity.ToDomain(league, user));
            }
            else
            {
                leagueInvites.Add(entity.ToDomain(league));
            }
        }

        return leagueInvites;
    }

    private async Task<LeagueInvite> ConvertLeagueInviteEntity(LeagueInviteEntity entity)
    {
        var league = await this.GetLeagueOrThrow(entity.LeagueID);
        if (league is null)
        {
            throw new Exception($"Cannot find league for league (should never happen) LeagueID: {entity.LeagueID}");
        }

        if (entity.UserID.HasValue)
        {
            FantasyCriticUser user = await _userStore.FindByIdOrThrowAsync(entity.UserID.Value, CancellationToken.None);
            return entity.ToDomain(league, user);
        }

        return entity.ToDomain(league);
    }

    private async Task<IReadOnlyList<LeagueYearTagEntity>> GetLeagueYearTagEntities(int year)
    {
        const string sql = "select * from tbl_league_yearusestag where Year = @year;";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<LeagueYearTagEntity> entities = await connection.QueryAsync<LeagueYearTagEntity>(sql, new { year });
        return entities.ToList();
    }

    private async Task<IReadOnlyList<LeagueYearTagEntity>> GetLeagueYearTagEntities(Guid leagueID, int year)
    {
        const string sql = "select * from tbl_league_yearusestag where LeagueID = @leagueID AND Year = @year;";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<LeagueYearTagEntity> entities = await connection.QueryAsync<LeagueYearTagEntity>(sql, new { leagueID, year });
        return entities.ToList();
    }

    private async Task<IReadOnlyList<SpecialGameSlotEntity>> GetSpecialGameSlotEntities(int year)
    {
        const string sql = "select * from tbl_league_specialgameslot where Year = @year;";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<SpecialGameSlotEntity> entities = await connection.QueryAsync<SpecialGameSlotEntity>(sql, new { year });
        return entities.ToList();
    }

    private async Task<IReadOnlyList<SpecialGameSlotEntity>> GetSpecialGameSlotEntities(Guid leagueID, int year)
    {
        const string sql = "select * from tbl_league_specialgameslot where LeagueID = @leagueID AND Year = @year;";

        await using var connection = new MySqlConnection(_connectionString);
        IEnumerable<SpecialGameSlotEntity> entities = await connection.QueryAsync<SpecialGameSlotEntity>(sql, new { leagueID, year });
        return entities.ToList();
    }

    private static IReadOnlyList<LeagueTagStatus> ConvertLeagueTagEntities(IEnumerable<LeagueYearTagEntity> leagueTags, IReadOnlyDictionary<string, MasterGameTag> tagOptions)
    {
        var domains = leagueTags.Select(x => x.ToDomain(tagOptions[x.Tag])).ToList();
        return domains;
    }

    private async Task<List<TagOverride>> ConvertTagOverrideEntities(IEnumerable<TagOverrideEntity> tagOverrideEntities, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        var allMasterGames = await _masterGameRepo.GetMasterGames();
        var masterGameDictionary = allMasterGames.ToDictionary(x => x.MasterGameID);
        List<TagOverride> domainTagOverrides = new List<TagOverride>();
        var entitiesByMasterGameID = tagOverrideEntities.GroupBy(x => x.MasterGameID);
        foreach (var entitySet in entitiesByMasterGameID)
        {
            var masterGame = masterGameDictionary[entitySet.Key];
            List<MasterGameTag> tagsForMasterGame = new List<MasterGameTag>();
            foreach (var entity in entitySet)
            {
                var fullTag = tagDictionary[entity.TagName];
                tagsForMasterGame.Add(fullTag);
            }

            domainTagOverrides.Add(new TagOverride(masterGame, tagsForMasterGame));
        }

        return domainTagOverrides;
    }

    private async Task<List<EligibilityOverride>> ConvertEligibilityOverrideEntities(IEnumerable<EligibilityOverrideEntity> eligibilityOverrideEntities)
    {
        List<EligibilityOverride> domainEligibilityOverrides = new List<EligibilityOverride>();
        foreach (var entity in eligibilityOverrideEntities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameOrThrow(entity.MasterGameID);
            EligibilityOverride domain = entity.ToDomain(masterGame);
            domainEligibilityOverrides.Add(domain);
        }

        return domainEligibilityOverrides;
    }

    public async Task PostNewManagerMessage(LeagueYear leagueYear, ManagerMessage message)
    {
        var entity = new ManagerMessageEntity(leagueYear, message);
        const string sql = "INSERT INTO tbl_league_managermessage(MessageID,LeagueID,Year,MessageText,IsPublic,Timestamp,Deleted) VALUES " +
                     "(@MessageID,@LeagueID,@Year,@MessageText,@IsPublic,@Timestamp,0);";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task<IReadOnlyList<ManagerMessage>> GetManagerMessages(LeagueYear leagueYear)
    {
        const string messageSQL = "select * from tbl_league_managermessage where LeagueID = @leagueID AND Year = @year AND Deleted = 0;";
        var queryObject = new
        {
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year
        };

        const string dismissSQL = "select * from tbl_league_managermessagedismissal where MessageID in @messageIDs;";

        IEnumerable<ManagerMessageEntity> messageEntities;
        IEnumerable<ManagerMessageDismissalEntity> dismissalEntities;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            messageEntities = await connection.QueryAsync<ManagerMessageEntity>(messageSQL, queryObject);

            var messageIDs = messageEntities.Select(x => x.MessageID);
            var dismissQueryObject = new
            {
                messageIDs
            };
            dismissalEntities = await connection.QueryAsync<ManagerMessageDismissalEntity>(dismissSQL, dismissQueryObject);
        }

        List<ManagerMessage> domainMessages = new List<ManagerMessage>();
        var dismissalLookup = dismissalEntities.ToLookup(x => x.MessageID);
        foreach (var messageEntity in messageEntities)
        {
            var dismissedUserIDs = dismissalLookup[messageEntity.MessageID].Select(x => x.UserID);
            domainMessages.Add(messageEntity.ToDomain(dismissedUserIDs));
        }

        return domainMessages;
    }

    public async Task<Result> DeleteManagerMessage(LeagueYear leagueYear, Guid messageId)
    {
        const string sql = "UPDATE tbl_league_managermessage SET Deleted = 1 WHERE MessageID = @messageId AND LeagueID = @leagueID AND Year = @year;";
        var paramsObject = new
        {
            messageId,
            leagueID = leagueYear.League.LeagueID,
            year = leagueYear.Year
        };

        await using (var connection = new MySqlConnection(_connectionString))
        {
            var rowsDeleted = await connection.ExecuteAsync(sql, paramsObject);
            if (rowsDeleted != 1)
            {
                return Result.Failure("Invalid request");
            }
        }

        return Result.Success();
    }

    public async Task<Result> DismissManagerMessage(Guid messageId, Guid userId)
    {
        const string sql = "INSERT IGNORE INTO `tbl_league_managermessagedismissal` " +
                     "(`MessageID`, `UserID`) VALUES(@messageId, @userID);";
        await using (var connection = new MySqlConnection(_connectionString))
        {
            var rowsAdded = await connection.ExecuteAsync(sql, new { messageId, userId });
            if (rowsAdded != 1)
            {
                return Result.Failure("Invalid request");
            }
        }

        return Result.Success();
    }

    public async Task FinishYear(SupportedYear supportedYear)
    {
        const string sql = "UPDATE tbl_meta_supportedyear SET Finished = 1 WHERE Year = @year;";

        var finishObject = new
        {
            year = supportedYear.Year
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, finishObject);
    }

    private async Task<PublisherGame?> GetConditionalDropPublisherGame(PickupBidEntity bidEntity, int year,
        ILookup<(Guid PublisherID, Guid MasterGameID), PublisherGame> publisherGameLookup,
        ILookup<(Guid PublisherID, Guid MasterGameID), FormerPublisherGame> formerPublisherGameLookup)
    {
        if (!bidEntity.ConditionalDropMasterGameID.HasValue)
        {
            return null;
        }

        var currentPublisherGames = publisherGameLookup[(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)].ToList();
        if (currentPublisherGames.Any())
        {
            return currentPublisherGames.WhereMax(x => x.Timestamp).First();
        }

        var formerPublisherGames = formerPublisherGameLookup[(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)].ToList();
        if (formerPublisherGames.Any())
        {
            return formerPublisherGames.WhereMax(x => x.PublisherGame.Timestamp).First().PublisherGame;
        }

        var conditionalDropGame = await _masterGameRepo.GetMasterGameYearOrThrow(bidEntity.ConditionalDropMasterGameID.Value, year);
        var fakePublisherGame = new PublisherGame(bidEntity.PublisherID, Guid.NewGuid(),
            conditionalDropGame.MasterGame.GameName, bidEntity.Timestamp,
            false, null, false, null, conditionalDropGame, 0, null, null, null, null);

        return fakePublisherGame;
    }

    public async Task UpdateLeagueYearCache(IEnumerable<LeagueYear> allLeagueYears)
    {
        var leagueYearEntities = allLeagueYears.Select(x => new LeagueYearCacheEntity(x));
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync("delete from tbl_caching_leagueyear", transaction: transaction);
        await connection.BulkInsertAsync<LeagueYearCacheEntity>(leagueYearEntities, "tbl_caching_leagueyear", 500, transaction);
        await transaction.CommitAsync();
    }
}
