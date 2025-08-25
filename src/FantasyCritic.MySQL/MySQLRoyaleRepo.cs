using System.Data;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.Lib.SharedSerialization.Database;

namespace FantasyCritic.MySQL;

public class MySQLRoyaleRepo : IRoyaleRepo
{
    private readonly string _connectionString;
    private readonly IReadOnlyFantasyCriticUserStore _userStore;
    private readonly IMasterGameRepo _masterGameRepo;

    public MySQLRoyaleRepo(RepositoryConfiguration configuration, IReadOnlyFantasyCriticUserStore userStore, IMasterGameRepo masterGameRepo)
    {
        _connectionString = configuration.ConnectionString;
        _userStore = userStore;
        _masterGameRepo = masterGameRepo;
    }

    public async Task CreatePublisher(RoyalePublisher publisher)
    {
        RoyalePublisherEntity entity = new RoyalePublisherEntity(publisher);
        const string sql = "insert into tbl_royale_publisher (PublisherID,UserID,Year,Quarter,PublisherName,PublisherIcon,PublisherSlogan,Budget) " +
                           "VALUES (@PublisherID,@UserID,@Year,@Quarter,@PublisherName,@PublisherIcon,@PublisherSlogan,@Budget)";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task ChangePublisherName(RoyalePublisher publisher, string publisherName)
    {
        const string sql = "UPDATE tbl_royale_publisher SET PublisherName = @publisherName WHERE PublisherID = @publisherID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            publisherID = publisher.PublisherID,
            publisherName
        });
    }

    public async Task ChangePublisherIcon(RoyalePublisher publisher, string? publisherIcon)
    {
        const string sql = "UPDATE tbl_royale_publisher SET PublisherIcon = @publisherIcon WHERE PublisherID = @publisherID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            publisherID = publisher.PublisherID,
            publisherIcon
        });
    }

    public async Task ChangePublisherSlogan(RoyalePublisher publisher, string? publisherSlogan)
    {
        const string sql = "UPDATE tbl_royale_publisher SET PublisherSlogan = @publisherSlogan WHERE PublisherID = @publisherID;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            publisherID = publisher.PublisherID,
            publisherSlogan
        });
    }

    public async Task<RoyalePublisher?> GetPublisher(RoyaleYearQuarter yearQuarter, IVeryMinimalFantasyCriticUser user)
    {
        const string publisherGameSQL = """
                                        SELECT * FROM tbl_royale_publishergame
                                        JOIN tbl_royale_publisher ON tbl_royale_publishergame.PublisherID = tbl_royale_publisher.PublisherID
                                        WHERE UserID = @userID and Year = @year and Quarter = @quarter;
                                        """;
        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<RoyalePublisherEntity>(publisherGameSQL,
            new
            {
                userID = user.UserID,
                year = yearQuarter.YearQuarter.Year,
                quarter = yearQuarter.YearQuarter.Quarter
            });
        if (entity is null)
        {
            return null;
        }

        var publisherGames = await GetGamesForPublisher(entity.PublisherID, yearQuarter);
        var domain = entity.ToDomain(yearQuarter, publisherGames);
        return domain;
    }

    public async Task<RoyaleYearQuarterData?> GetRoyaleYearQuarterData(int year, int quarter)
    {
        var param = new
        {
            P_Year = year,
            P_Quarter = quarter
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getroyaleyearquarterdata", param, commandType: CommandType.StoredProcedure);
        var supportedQuarterEntities = resultSets.Read<RoyaleYearQuarterEntity>();
        var publisherEntities = resultSets.Read<RoyalePublisherEntity>();
        var publisherGameEntities = resultSets.Read<RoyalePublisherGameEntity>();

        //MasterGame Results
        var tagResults = resultSets.Read<MasterGameTagEntity>();
        var masterSubGameResults = resultSets.Read<MasterSubGameEntity>();
        var masterGameTagResults = resultSets.Read<MasterGameHasTagEntity>();
        var masterGameYearResults = resultSets.Read<MasterGameYearEntity>();

        await resultSets.DisposeAsync();
        await connection.DisposeAsync();

        var supportedQuarters = supportedQuarterEntities.Select(x => x.ToDomain()).ToList();
        var activeRoyaleQuarter = supportedQuarters.SingleOrDefault(x => x.YearQuarter.Year == year && x.YearQuarter.Quarter == quarter);
        if (activeRoyaleQuarter is null)
        {
            return null;
        }

        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);
        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();

        var masterGameYearDictionary = new Dictionary<Guid, MasterGameYear>();
        foreach (var entity in masterGameYearResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameYearDictionary.Add(domain.MasterGame.MasterGameID, domain);
        }

        List<RoyalePublisherGame> domainPublisherGames = new List<RoyalePublisherGame>();
        foreach (var entity in publisherGameEntities)
        {
            var masterGameYear = masterGameYearDictionary[entity.MasterGameID];
            var domain = entity.ToDomain(activeRoyaleQuarter.YearQuarter, masterGameYear);
            domainPublisherGames.Add(domain);
        }

        var publisherGameLookup = domainPublisherGames.ToLookup(x => x.PublisherID);

        List<RoyalePublisher> domainPublishers = new List<RoyalePublisher>();
        foreach (var entity in publisherEntities)
        {
            var gamesForPublisher = publisherGameLookup[entity.PublisherID];
            var domain = entity.ToDomain(activeRoyaleQuarter, gamesForPublisher);
            domainPublishers.Add(domain);
        }

        return new RoyaleYearQuarterData(supportedQuarters, activeRoyaleQuarter, domainPublishers);
    }

    public async Task<RoyalePublisherData?> GetPublisherData(Guid publisherID)
    {
        var param = new
        {
            P_PublisherID = publisherID
        };

        await using var connection = new MySqlConnection(_connectionString);
        await using var resultSets = await connection.QueryMultipleAsync("sp_getroyalepublisher", param, commandType: CommandType.StoredProcedure);

        var supportedQuarterEntities = resultSets.Read<RoyaleYearQuarterEntity>();
        var publisherEntity = resultSets.ReadSingleOrDefault<RoyalePublisherEntity>();
        if (publisherEntity is null)
        {
            return null;
        }

        var publisherGameEntities = resultSets.Read<RoyalePublisherGameEntity>();

        //MasterGame Results
        var tagResults = resultSets.Read<MasterGameTagEntity>();
        var masterSubGameResults = resultSets.Read<MasterSubGameEntity>();
        var masterGameTagResults = resultSets.Read<MasterGameHasTagEntity>();
        var masterGameYearResults = resultSets.Read<MasterGameYearEntity>();

        await resultSets.DisposeAsync();
        await connection.DisposeAsync();

        var supportedQuarters = supportedQuarterEntities.Select(x => x.ToDomain()).ToList();
        var quarterForPublisher = supportedQuarters.Single(x => x.YearQuarter.Year == publisherEntity.Year && x.YearQuarter.Quarter == publisherEntity.Quarter);

        var possibleTags = tagResults.Select(x => x.ToDomain()).ToDictionary(x => x.Name);
        var masterGameTagLookup = masterGameTagResults.ToLookup(x => x.MasterGameID);
        var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();

        var masterGameYearDictionary = new Dictionary<Guid, MasterGameYear>();
        foreach (var entity in masterGameYearResults)
        {
            var tags = masterGameTagLookup[entity.MasterGameID].Select(x => possibleTags[x.TagName]).ToList();
            var addedByUser = new VeryMinimalFantasyCriticUser(entity.AddedByUserID, entity.AddedByUserDisplayName);
            MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID), tags, addedByUser);
            masterGameYearDictionary.Add(domain.MasterGame.MasterGameID, domain);
        }

        List<RoyalePublisherGame> domainPublisherGames = new List<RoyalePublisherGame>();
        foreach (var entity in publisherGameEntities)
        {
            var masterGameYear = masterGameYearDictionary.GetValueOrDefault(entity.MasterGameID);
            if (masterGameYear is null)
            {
                var masterGame = await _masterGameRepo.GetMasterGame(entity.MasterGameID);
                masterGameYear = new MasterGameYear(masterGame!, publisherEntity.Year);
            }
            var domain = entity.ToDomain(quarterForPublisher.YearQuarter, masterGameYear);
            domainPublisherGames.Add(domain);
        }

        var quartersWonByUser = supportedQuarters.Where(x => x.WinningUser is not null && x.WinningUser.UserID == publisherEntity.UserID).ToList();

        var domainPublisher = publisherEntity.ToDomain(quarterForPublisher, domainPublisherGames);
        var domainPublisherData = new RoyalePublisherData(domainPublisher, quartersWonByUser, masterGameYearDictionary.Values.ToList(), possibleTags.Values.ToList());
        return domainPublisherData;
    }

    public async Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter)
    {
        var yearQuarter = await GetYearQuarter(year, quarter);
        if (yearQuarter is null)
        {
            return new List<RoyalePublisher>();
        }

        var users = await _userStore.GetAllUsers();
        var publisherGames = await GetAllPublisherGames(yearQuarter);
        var publisherGameLookup = publisherGames.ToLookup(x => x.PublisherID);

        const string publisherSQL = """
                                    select tbl_royale_publisher.*, tbl_user.DisplayName AS PublisherDisplayName from tbl_royale_publisher
                                    join tbl_user on tbl_user.UserID = tbl_royale_publisher.UserID
                                    where Year = @year and Quarter = @quarter;
                                    """;
        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<RoyalePublisherEntity>(publisherSQL, new { year, quarter });

        List<RoyalePublisher> domainPublishers = new List<RoyalePublisher>();
        foreach (var entity in entities)
        {
            var user = users.SingleOrDefault(x => x.Id == entity.UserID);
            if (user is null)
            {
                continue;
            }

            var gamesForPublisher = publisherGameLookup[entity.PublisherID];

            var domain = entity.ToDomain(yearQuarter, gamesForPublisher);
            domainPublishers.Add(domain);
        }

        return domainPublishers;
    }

    public async Task UpdateFantasyPoints(Dictionary<(Guid, Guid), decimal?> publisherGameScores)
    {
        List<RoyalePublisherScoreUpdateEntity> updateEntities = publisherGameScores.Select(x => new RoyalePublisherScoreUpdateEntity(x)).ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(
            "update tbl_royale_publishergame SET FantasyPoints = @FantasyPoints where PublisherID = @PublisherID AND MasterGameID = @MasterGameID;",
            updateEntities, transaction);
        await transaction.CommitAsync();
    }

    private async Task<IReadOnlyList<RoyalePublisherGame>> GetAllPublisherGames(RoyaleYearQuarter yearQuarter)
    {
        const string publisherGameSQL = """
                                        SELECT * FROM tbl_royale_publishergame
                                        JOIN tbl_royale_publisher ON tbl_royale_publishergame.PublisherID = tbl_royale_publisher.PublisherID
                                        WHERE tbl_royale_publisher.Year = @year AND tbl_royale_publisher.Quarter = @quarter;
                                        """;

        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<RoyalePublisherGameEntity>(publisherGameSQL,
            new
            {
                year = yearQuarter.YearQuarter.Year,
                quarter = yearQuarter.YearQuarter.Quarter
            });
        List<RoyalePublisherGame> domains = new List<RoyalePublisherGame>();
        foreach (var entity in entities)
        {
            var masterGameYear = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID, yearQuarter.YearQuarter.Year);
            if (masterGameYear is null)
            {
                var masterGame = await _masterGameRepo.GetMasterGameOrThrow(entity.MasterGameID);
                masterGameYear = new MasterGameYear(masterGame, yearQuarter.YearQuarter.Year);
            }
            var domain = entity.ToDomain(yearQuarter.YearQuarter, masterGameYear);
            domains.Add(domain);
        }
        return domains;
    }

    public async Task PurchaseGame(RoyalePublisherGame game)
    {
        const string gameAddSQL = "INSERT INTO tbl_royale_publishergame(PublisherID,MasterGameID,Timestamp,AmountSpent,AdvertisingMoney,FantasyPoints) VALUES " +
                                  "(@PublisherID,@MasterGameID,@Timestamp,@AmountSpent,@AdvertisingMoney,@FantasyPoints)";
        const string budgetDecreaseSQL = "UPDATE tbl_royale_publisher SET Budget = Budget - @amountSpent WHERE PublisherID = @publisherID";

        RoyalePublisherGameEntity entity = new RoyalePublisherGameEntity(game);
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(gameAddSQL, entity, transaction);
        await connection.ExecuteAsync(budgetDecreaseSQL, new { amountSpent = game.AmountSpent, publisherID = game.PublisherID }, transaction);
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters()
    {
        const string sql = """
                            select tbl_royale_supportedquarter.*, tbl_user.DisplayName as WinningUserDisplayName
                            from tbl_royale_supportedquarter
                            LEFT JOIN tbl_user on tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
                            ORDER BY Year, Quarter;
                           """;

        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<RoyaleYearQuarterEntity>(sql);
        return results.Select(x => x.ToDomain()).ToList();
    }

    private async Task<RoyaleYearQuarter?> GetYearQuarter(int year, int quarter)
    {
        const string sql = """
                           select tbl_royale_supportedquarter.*, tbl_user.DisplayName as WinningUserDisplayName
                           from tbl_royale_supportedquarter
                           LEFT JOIN tbl_user on tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
                           where Year = @year and Quarter = @quarter;
                           """;

        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<RoyaleYearQuarterEntity>(sql, new { year, quarter });
        var domain = entity?.ToDomain();
        return domain;
    }

    private async Task<IReadOnlyList<RoyalePublisherGame>> GetGamesForPublisher(Guid publisherID, RoyaleYearQuarter yearQuarter)
    {
        const string sql = "select * from tbl_royale_publishergame where PublisherID = @publisherID;";
        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<RoyalePublisherGameEntity>(sql,
            new
            {
                publisherID
            });
        List<RoyalePublisherGame> domains = new List<RoyalePublisherGame>();
        foreach (var entity in entities)
        {
            var masterGameYear = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID, yearQuarter.YearQuarter.Year);
            if (masterGameYear is null)
            {
                var masterGame = await _masterGameRepo.GetMasterGameOrThrow(entity.MasterGameID);
                masterGameYear = new MasterGameYear(masterGame, yearQuarter.YearQuarter.Year);
            }
            var domain = entity.ToDomain(yearQuarter.YearQuarter, masterGameYear);
            domains.Add(domain);
        }
        return domains;
    }

    public async Task SellGame(RoyalePublisherGame publisherGame, bool fullRefund)
    {
        var refund = publisherGame.AmountSpent;
        if (!fullRefund)
        {
            refund /= 2;
        }

        const string gameRemoveSQL = "DELETE FROM tbl_royale_publishergame WHERE PublisherID = @publisherID AND MasterGameID = @masterGameID";
        const string budgetIncreaseSQL = "UPDATE tbl_royale_publisher SET Budget = Budget + @amountGained WHERE PublisherID = @publisherID";
        var amountGained = refund + publisherGame.AdvertisingMoney;
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(gameRemoveSQL, new { masterGameID = publisherGame.MasterGame.MasterGame.MasterGameID, publisherID = publisherGame.PublisherID }, transaction);
        await connection.ExecuteAsync(budgetIncreaseSQL, new { amountGained, publisherID = publisherGame.PublisherID }, transaction);
        await transaction.CommitAsync();
    }

    public async Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney)
    {
        decimal amountToSpend = advertisingMoney - publisherGame.AdvertisingMoney;
        const string advertisingMoneySetSQL = "UPDATE tbl_royale_publishergame SET AdvertisingMoney = @advertisingMoney WHERE PublisherID = @publisherID AND MasterGameID = @masterGameID";
        const string budgetDecreaseSQL = "UPDATE tbl_royale_publisher SET Budget = Budget - @amountToSpend WHERE PublisherID = @publisherID";
        var masterGameID = publisherGame.MasterGame.MasterGame.MasterGameID;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(advertisingMoneySetSQL, new { advertisingMoney, publisherID = publisherGame.PublisherID, masterGameID }, transaction);
        await connection.ExecuteAsync(budgetDecreaseSQL, new { amountToSpend, publisherID = publisherGame.PublisherID }, transaction);
        await transaction.CommitAsync();
    }

    public async Task CalculateRoyaleWinnerForQuarter(int year, int quarter)
    {
        const string calculateSQL =
            """
            SELECT tbl_royale_publisher.PublisherID, tbl_royale_publisher.UserID, tbl_user.DisplayName, SUM(FantasyPoints) AS TotalFantasyPoints FROM tbl_royale_publisher
            JOIN tbl_royale_publishergame ON tbl_royale_publisher.PublisherID = tbl_royale_publishergame.PublisherID
            JOIN tbl_user on tbl_royale_publisher.UserID = tbl_user.UserID
            WHERE Year = @year AND Quarter = @quarter
            GROUP BY tbl_royale_publisher.PublisherID
            ORDER BY TotalFantasyPoints DESC
            LIMIT 1;
            """;

        const string updateSQL =
            """
            UPDATE tbl_royale_supportedquarter SET WinningUser = @winningUserID WHERE Year = @year AND Quarter = @quarter AND WinningUser is NULL;
            """;

        var calculateParam = new
        {
            year,
            quarter
        };


        await using var connection = new MySqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<RoyaleStandingsEntity>(calculateSQL, calculateParam);
        if (result is null)
        {
            return;
        }

        var updateParam = new
        {
            year,
            quarter,
            winningUserID = result.UserID
        };

        await connection.ExecuteAsync(updateSQL, updateParam);
    }

    public async Task StartNewQuarter(YearQuarter nextQuarter)
    {
        const string sql = "insert into tbl_royale_supportedquarter (Year,Quarter,OpenForPlay,Finished) " +
                           "VALUES (@Year,@Quarter,@OpenForPlay,@Finished)";

        var newQuarterObject = new RoyaleYearQuarterEntity()
        {
            Finished = false,
            OpenForPlay = true,
            Quarter = nextQuarter.Quarter,
            Year = nextQuarter.Year
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, newQuarterObject);
    }

    public async Task FinishQuarter(RoyaleYearQuarter supportedQuarter)
    {
        const string sql = "UPDATE tbl_royale_supportedquarter SET Finished = 1 WHERE Year = @year AND Quarter = @quarter;";

        var finishObject = new
        {
            year = supportedQuarter.YearQuarter.Year,
            quarter = supportedQuarter.YearQuarter.Quarter
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, finishObject);
    }
}
