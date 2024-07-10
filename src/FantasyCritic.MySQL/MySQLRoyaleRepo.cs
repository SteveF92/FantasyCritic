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
    private readonly IFantasyCriticRepo _fantasyCriticRepo;

    public MySQLRoyaleRepo(RepositoryConfiguration configuration, IReadOnlyFantasyCriticUserStore userStore, IMasterGameRepo masterGameRepo, IFantasyCriticRepo fantasyCriticRepo)
    {
        _connectionString = configuration.ConnectionString;
        _userStore = userStore;
        _masterGameRepo = masterGameRepo;
        _fantasyCriticRepo = fantasyCriticRepo;
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
        const string supportedQuartersSQL = """
                            select tbl_royale_supportedquarter.*, tbl_user.DisplayName as WinningUserDisplayName
                            from tbl_royale_supportedquarter
                            LEFT JOIN tbl_user on tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
                            ORDER BY Year, Quarter;
                           """;
        const string publisherGameSQL = """
                           SELECT * FROM tbl_royale_publishergame
                           JOIN tbl_royale_publisher ON tbl_royale_publishergame.PublisherID = tbl_royale_publisher.PublisherID
                           WHERE tbl_royale_publisher.Year = @P_Year AND tbl_royale_publisher.Quarter = @P_Quarter;
                           """;
        const string publisherSQL = """
                                    select tbl_royale_publisher.*, tbl_user.DisplayName AS PublisherDisplayName from tbl_royale_publisher 
                                    join tbl_user on tbl_user.UserID = tbl_royale_publisher.UserID
                                    where Year = @P_Year and Quarter = @P_Quarter;
                                    """;
        const string masterGameTagSQL = "select * from tbl_mastergame_tag;";

        var param = new
        {
            P_Year = year,
            P_Quarter = quarter
        };

        await using var connection = new MySqlConnection(_connectionString);

        var supportedQuarterEntities = await connection.QueryAsync<RoyaleYearQuarterEntity>(supportedQuartersSQL);
        var publisherEntities = await connection.QueryAsync<RoyalePublisherEntity>(publisherSQL, param);
        var publisherGameEntities = await connection.QueryAsync<RoyalePublisherGameEntity>(publisherGameSQL, param);
        var masterGameTagEntities = await connection.QueryAsync<MasterGameTagEntity>(masterGameTagSQL);

        var supportedQuarters = supportedQuarterEntities.Select(x => x.ToDomain()).ToList();
        var activeRoyaleQuarter = supportedQuarters.SingleOrDefault(x => x.YearQuarter.Year == year && x.YearQuarter.Quarter == quarter);
        if (activeRoyaleQuarter is null)
        {
            return null;
        }

        List<RoyalePublisherGame> domainPublisherGames = new List<RoyalePublisherGame>();
        foreach (var entity in publisherGameEntities)
        {
            var masterGameYear = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID, activeRoyaleQuarter.YearQuarter.Year);
            if (masterGameYear is null)
            {
                var masterGame = await _masterGameRepo.GetMasterGameOrThrow(entity.MasterGameID);
                masterGameYear = new MasterGameYear(masterGame, activeRoyaleQuarter.YearQuarter.Year);
            }
            var domain = entity.ToDomain(activeRoyaleQuarter, masterGameYear);
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

        var tags = masterGameTagEntities.Select(x => x.ToDomain()).ToList();
        return new RoyaleYearQuarterData(supportedQuarters, activeRoyaleQuarter, domainPublishers, tags);
    }

    public async Task<RoyalePublisher?> GetPublisher(Guid publisherID)
    {
        const string publisherSQL = """
                                    select tbl_royale_publisher.*, tbl_user.DisplayName AS PublisherDisplayName from tbl_royale_publisher
                                    join tbl_user on tbl_user.UserID = tbl_royale_publisher.UserID
                                    where PublisherID = @publisherID;
                                    """;

        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<RoyalePublisherEntity>(publisherSQL,
            new
            {
                publisherID
            });
        if (entity is null)
        {
            return null;
        }

        var yearQuarter = await GetYearQuarterOrThrow(entity.Year, entity.Quarter);
        var publisherGames = await GetGamesForPublisher(entity.PublisherID, yearQuarter);
        var domain = entity.ToDomain(yearQuarter, publisherGames);
        return domain;
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
            var domain = entity.ToDomain(yearQuarter, masterGameYear);
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

    private async Task<RoyaleYearQuarter> GetYearQuarterOrThrow(int year, int quarter)
    {
        var yearQuarter = await GetYearQuarter(year, quarter);
        if (yearQuarter is null)
        {
            throw new Exception($"Royale Year Quarter not found: {year} | {quarter}");
        }

        return yearQuarter;
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
            var domain = entity.ToDomain(yearQuarter, masterGameYear);
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

    public async Task<VeryMinimalFantasyCriticUser?> CalculateRoyaleWinnerForQuarter(int year, int quarter)
    {
        const string sql =
            """
            SELECT tbl_royale_publisher.PublisherID, tbl_royale_publisher.UserID, tbl_user.DisplayName, SUM(FantasyPoints) AS TotalFantasyPoints FROM tbl_royale_publisher
            JOIN tbl_royale_publishergame ON tbl_royale_publisher.PublisherID = tbl_royale_publishergame.PublisherID
            JOIN tbl_user on tbl_royale_publisher.UserID = tbl_user.UserID
            WHERE Year = @year AND Quarter = @quarter
            GROUP BY tbl_royale_publisher.PublisherID
            ORDER BY TotalFantasyPoints DESC
            LIMIT 1;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<RoyaleStandingsEntity>(sql);
        if (result is null)
        {
            return null;
        }

        return new VeryMinimalFantasyCriticUser(result.UserID, result.DisplayName);
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
