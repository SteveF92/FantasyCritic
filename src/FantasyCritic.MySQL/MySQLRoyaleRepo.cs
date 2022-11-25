using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL.Entities;

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

    public async Task<RoyalePublisher?> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user)
    {
        const string sql = "select * from tbl_royale_publisher where UserID = @userID and Year = @year and Quarter = @quarter;";
        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<RoyalePublisherEntity>(sql,
            new
            {
                userID = user.Id,
                year = yearQuarter.YearQuarter.Year,
                quarter = yearQuarter.YearQuarter.Quarter
            });
        if (entity is null)
        {
            return null;
        }

        var publisherGames = await GetGamesForPublisher(entity.PublisherID, yearQuarter);
        var domain = entity.ToDomain(yearQuarter, user, publisherGames);
        return domain;
    }

    public async Task<RoyalePublisher?> GetPublisher(Guid publisherID)
    {
        const string sql = "select * from tbl_royale_publisher where PublisherID = @publisherID;";
        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<RoyalePublisherEntity>(sql,
            new
            {
                publisherID
            });
        if (entity is null)
        {
            return null;
        }

        var user = await _userStore.FindByIdAsync(entity.UserID.ToString(), CancellationToken.None);
        var yearQuarter = await GetYearQuarterOrThrow(entity.Year, entity.Quarter);
        var publisherGames = await GetGamesForPublisher(entity.PublisherID, yearQuarter);
        var domain = entity.ToDomain(yearQuarter, user!, publisherGames);
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

        const string sql = "select * from tbl_royale_publisher where Year = @year and Quarter = @quarter";
        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<RoyalePublisherEntity>(sql, new { year, quarter });

        List<RoyalePublisher> domainPublishers = new List<RoyalePublisher>();
        foreach (var entity in entities)
        {
            var user = users.SingleOrDefault(x => x.Id == entity.UserID);
            if (user is null)
            {
                continue;
            }

            var gamesForPublisher = publisherGameLookup[entity.PublisherID];

            var domain = entity.ToDomain(yearQuarter, user, gamesForPublisher);
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
        const string sql = "SELECT * FROM tbl_royale_publishergame " +
                           "JOIN tbl_royale_publisher ON tbl_royale_publishergame.PublisherID = tbl_royale_publisher.PublisherID " +
                           "WHERE tbl_royale_publisher.Year = @year AND tbl_royale_publisher.Quarter = @quarter; ";

        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<RoyalePublisherGameEntity>(sql,
            new
            {
                year = yearQuarter.YearQuarter.Year,
                quarter = yearQuarter.YearQuarter.Quarter
            });
        List<RoyalePublisherGame> domains = new List<RoyalePublisherGame>();
        foreach (var entity in entities)
        {
            var masterGame = await _masterGameRepo.GetMasterGameYearOrThrow(entity.MasterGameID, yearQuarter.YearQuarter.Year);
            var domain = entity.ToDomain(yearQuarter, masterGame);
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
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        await using var connection = new MySqlConnection(_connectionString);
        var results = await connection.QueryAsync<RoyaleYearQuarterEntity>("select * from tbl_royale_supportedquarter ORDER BY Year,Quarter;");
        return results.Select(x => x.ToDomain(supportedYears.Single(y => y.Year == x.Year))).ToList();
    }

    private async Task<RoyaleYearQuarter?> GetYearQuarter(int year, int quarter)
    {
        var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
        const string sql = "select * from tbl_royale_supportedquarter where Year = @year and Quarter = @quarter;";
        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<RoyaleYearQuarterEntity>(sql, new { year, quarter });

        var domain = entity?.ToDomain(supportedYears.Single(x => x.Year == entity.Year));
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
            var masterGame = await _masterGameRepo.GetMasterGameYearOrThrow(entity.MasterGameID, yearQuarter.YearQuarter.Year);
            var domain = entity.ToDomain(yearQuarter, masterGame);
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

    public async Task<IReadOnlyList<RoyaleYearQuarter>> GetQuartersWonByUser(FantasyCriticUser user)
    {
        var royaleWinners = await GetRoyaleWinners();
        if (royaleWinners.ContainsKey(user))
        {
            return royaleWinners[user];
        }

        return new List<RoyaleYearQuarter>();
    }

    public async Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<RoyaleYearQuarter>>> GetRoyaleWinners()
    {
        var quarters = await GetYearQuarters();
        const string sql =
            "SELECT tbl_royale_publisher.PublisherID, YEAR, QUARTER, SUM(FantasyPoints) AS TotalFantasyPoints FROM tbl_royale_publisher " +
            "JOIN tbl_royale_publishergame ON tbl_royale_publisher.PublisherID = tbl_royale_publishergame.PublisherID " +
            "GROUP BY tbl_royale_publisher.PublisherID";

        Dictionary<FantasyCriticUser, List<RoyaleYearQuarter>> winners = new Dictionary<FantasyCriticUser, List<RoyaleYearQuarter>>();
        await using (var connection = new MySqlConnection(_connectionString))
        {
            var results = await connection.QueryAsync<RoyaleStandingsEntity>(sql);
            var groupedByQuarter = results.GroupBy(x => (x.Year, x.Quarter));
            foreach (var group in groupedByQuarter)
            {
                var quarter = quarters.Single(x => x.YearQuarter.Year == group.Key.Year && x.YearQuarter.Quarter == group.Key.Quarter);
                if (!quarter.Finished)
                {
                    continue;
                }

                var topPublisher = group.MaxBy(x => x.TotalFantasyPoints);
                if (topPublisher is null)
                {
                    throw new Exception("Error finding royale winners (Can't find which publisher has highest total fantasy points).");
                }

                var topDomainPublisher = await GetPublisher(topPublisher.PublisherID);
                if (topDomainPublisher is null)
                {
                    throw new Exception("Error finding royale winners (Can't find the publisher that has highest total fantasy points).");
                }

                if (!winners.ContainsKey(topDomainPublisher.User))
                {
                    winners.Add(topDomainPublisher.User, new List<RoyaleYearQuarter>());
                }

                winners[topDomainPublisher.User].Add(quarter);
            }
        }

        return winners.SealDictionary();
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
