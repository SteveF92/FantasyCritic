using FantasyCritic.Lib.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL.Entities;
using MoreLinq.Extensions;
using MySql.Data.MySqlClient;

namespace FantasyCritic.MySQL
{
    public class MySQLRoyaleRepo : IRoyaleRepo
    {
        private readonly string _connectionString;
        private readonly IReadOnlyFantasyCriticUserStore _userStore;
        private readonly IMasterGameRepo _masterGameRepo;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MySQLRoyaleRepo(string connectionString, IReadOnlyFantasyCriticUserStore userStore, IMasterGameRepo masterGameRepo, IFantasyCriticRepo fantasyCriticRepo)
        {
            _connectionString = connectionString;
            _userStore = userStore;
            _masterGameRepo = masterGameRepo;
            _fantasyCriticRepo = fantasyCriticRepo;
        }

        public async Task CreatePublisher(RoyalePublisher publisher)
        {
            RoyalePublisherEntity entity = new RoyalePublisherEntity(publisher);
            string sql = "insert into tbl_royale_publisher (PublisherID,UserID,Year,Quarter,PublisherName,Budget) " +
                         "VALUES (@PublisherID,@UserID,@Year,@Quarter,@PublisherName,@Budget)";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, entity);
            }
        }

        public async Task ChangePublisherName(RoyalePublisher publisher, string publisherName)
        {
            string sql = "UPDATE tbl_royale_publisher SET PublisherName = @publisherName WHERE PublisherID = @publisherID;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, new
                {
                    publisherID = publisher.PublisherID,
                    publisherName
                });
            }
        }

        public async Task<Maybe<RoyalePublisher>> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user)
        {
            string sql = "select * from tbl_royale_publisher where UserID = @userID and Year = @year and Quarter = @quarter;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entity = await connection.QuerySingleOrDefaultAsync<RoyalePublisherEntity>(sql,
                    new
                    {
                        userID = user.UserID,
                        year = yearQuarter.YearQuarter.Year,
                        quarter = yearQuarter.YearQuarter.Quarter
                    });
                if (entity is null)
                {
                    return Maybe<RoyalePublisher>.None;
                }

                var publisherGames = await GetGamesForPublisher(entity.PublisherID, yearQuarter);
                var domain = entity.ToDomain(yearQuarter, user, publisherGames);
                return domain;
            }
        }

        public async Task<Maybe<RoyalePublisher>> GetPublisher(Guid publisherID)
        {
            string sql = "select * from tbl_royale_publisher where PublisherID = @publisherID;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entity = await connection.QuerySingleOrDefaultAsync<RoyalePublisherEntity>(sql,
                    new
                    {
                        publisherID
                    });
                if (entity is null)
                {
                    return Maybe<RoyalePublisher>.None;
                }

                var user = await _userStore.FindByIdAsync(entity.UserID.ToString(), CancellationToken.None);
                var yearQuarter = await GetYearQuarter(entity.Year, entity.Quarter);
                var publisherGames = await GetGamesForPublisher(entity.PublisherID, yearQuarter.Value);
                var domain = entity.ToDomain(yearQuarter.Value, user, publisherGames);
                return domain;
            }
        }

        public async Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter)
        {
            string sql = "select * from tbl_royale_publisher where Year = @year and Quarter = @quarter";

            var yearQuarter = await GetYearQuarter(year, quarter);
            if (yearQuarter.HasNoValue)
            {
                return new List<RoyalePublisher>();
            }

            var users = await _userStore.GetAllUsers();
            var publisherGames = await GetAllPublisherGames(yearQuarter.Value);

            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<RoyalePublisherEntity>(sql, new {year, quarter} );

                List<RoyalePublisher> domainPublishers = new List<RoyalePublisher>();
                foreach (var entity in entities)
                {
                    var user = users.SingleOrDefault(x => x.UserID == entity.UserID);
                    if (user is null)
                    {
                        continue;
                    }

                    var gamesForPublisher = publisherGames.Where(x => x.PublisherID == entity.PublisherID);

                    var domain = entity.ToDomain(yearQuarter.Value, user, gamesForPublisher);
                    domainPublishers.Add(domain);
                }

                return domainPublishers;
            }
        }

        public async Task UpdateFantasyPoints(Dictionary<(Guid, Guid), decimal?> publisherGameScores)
        {
            List<RoyalePublisherScoreUpdateEntity> updateEntities = publisherGameScores.Select(x => new RoyalePublisherScoreUpdateEntity(x)).ToList();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(
                        "update tbl_royale_publishergame SET FantasyPoints = @FantasyPoints where PublisherID = @PublisherID AND MasterGameID = @MasterGameID;",
                        updateEntities, transaction);
                    transaction.Commit();
                }
            }
        }

        private async Task<IReadOnlyList<RoyalePublisherGame>> GetAllPublisherGames(RoyaleYearQuarter yearQuarter)
        {
            string sql = "SELECT * FROM tbl_royale_publishergame " +
                         "JOIN tbl_royale_publisher ON tbl_royale_publishergame.PublisherID = tbl_royale_publisher.PublisherID " +
                         "WHERE tbl_royale_publisher.Year = @year AND tbl_royale_publisher.Quarter = @quarter; ";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<RoyalePublisherGameEntity>(sql,
                    new
                    {
                        year = yearQuarter.YearQuarter.Year,
                        quarter = yearQuarter.YearQuarter.Quarter
                    });
                List<RoyalePublisherGame> domains = new List<RoyalePublisherGame>();
                foreach (var entity in entities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID, yearQuarter.YearQuarter.Year);
                    var domain = entity.ToDomain(yearQuarter, masterGame.Value);
                    domains.Add(domain);
                }
                return domains;
            }
        }

        public async Task PurchaseGame(RoyalePublisherGame game)
        {
            string gameAddSQL = "INSERT INTO tbl_royale_publishergame(PublisherID,MasterGameID,Timestamp,AmountSpent,AdvertisingMoney,FantasyPoints) VALUES " +
                "(@PublisherID,@MasterGameID,@Timestamp,@AmountSpent,@AdvertisingMoney,@FantasyPoints)";
            string budgetDescreaseSQL = "UPDATE tbl_royale_publisher SET Budget = Budget - @amountSpent WHERE PublisherID = @publisherID";

            RoyalePublisherGameEntity entity = new RoyalePublisherGameEntity(game);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(gameAddSQL, entity, transaction);
                    await connection.ExecuteAsync(budgetDescreaseSQL,
                        new {amountSpent = game.AmountSpent, publisherID = game.PublisherID}, transaction);
                    transaction.Commit();
                }
            }
        }

        public async Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters()
        {
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<RoyaleYearQuarterEntity>("select * from tbl_royale_supportedquarter;");
                return results.Select(x => x.ToDomain(supportedYears.Single(y => y.Year == x.Year))).ToList();
            }
        }

        private async Task<Maybe<RoyaleYearQuarter>> GetYearQuarter(int year, int quarter)
        {
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
            string sql = "select * from tbl_royale_supportedquarter where Year = @year and Quarter = @quarter;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entity = await connection.QuerySingleOrDefaultAsync<RoyaleYearQuarterEntity>(sql, new {year, quarter});
                if (entity is null)
                {
                    return Maybe<RoyaleYearQuarter>.None;
                }

                var domain = entity.ToDomain(supportedYears.Single(x => x.Year == entity.Year));
                return domain;
            }
        }

        private async Task<IReadOnlyList<RoyalePublisherGame>> GetGamesForPublisher(Guid publisherID, RoyaleYearQuarter yearQuarter)
        {
            string sql = "select * from tbl_royale_publishergame where PublisherID = @publisherID;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<RoyalePublisherGameEntity>(sql,
                    new
                    {
                        publisherID
                    });
                List<RoyalePublisherGame> domains = new List<RoyalePublisherGame>();
                foreach (var entity in entities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID, yearQuarter.YearQuarter.Year);
                    var domain = entity.ToDomain(yearQuarter, masterGame.Value);
                    domains.Add(domain);
                }
                return domains;
            }
        }

        public async Task SellGame(RoyalePublisherGame publisherGame)
        {
            string gameRemoveSQL = "DELETE FROM tbl_royale_publishergame WHERE PublisherID = @publisherID AND MasterGameID = @masterGameID";
            string budgetIncreaseSQL = "UPDATE tbl_royale_publisher SET Budget = Budget + @amountGained WHERE PublisherID = @publisherID";
            var amountGained = (publisherGame.AmountSpent / 2) + publisherGame.AdvertisingMoney;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(gameRemoveSQL, new { masterGameID = publisherGame.MasterGame.MasterGame.MasterGameID, publisherID = publisherGame.PublisherID }, transaction);
                    await connection.ExecuteAsync(budgetIncreaseSQL, new { amountGained, publisherID = publisherGame.PublisherID }, transaction);
                    transaction.Commit();
                }
            }
        }

        public async Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney)
        {
            decimal amountToSpend = advertisingMoney - publisherGame.AdvertisingMoney;
            string advertisingMoneySetSQL = "UPDATE tbl_royale_publishergame SET AdvertisingMoney = @advertisingMoney WHERE PublisherID = @publisherID AND MasterGameID = @masterGameID";
            string budgetDescreaseSQL = "UPDATE tbl_royale_publisher SET Budget = Budget - @amountToSpend WHERE PublisherID = @publisherID";
            var masterGameID = publisherGame.MasterGame.MasterGame.MasterGameID;

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(advertisingMoneySetSQL, new { advertisingMoney, publisherID = publisherGame.PublisherID, masterGameID }, transaction);
                    await connection.ExecuteAsync(budgetDescreaseSQL, new { amountToSpend, publisherID = publisherGame.PublisherID }, transaction);
                    transaction.Commit();
                }
            }
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
            string sql =
                "SELECT tbl_royale_publisher.PublisherID, YEAR, QUARTER, SUM(FantasyPoints) AS TotalFantasyPoints FROM tbl_royale_publisher " +
                "JOIN tbl_royale_publishergame ON tbl_royale_publisher.PublisherID = tbl_royale_publishergame.PublisherID " +
                "GROUP BY tbl_royale_publisher.PublisherID";

            Dictionary<FantasyCriticUser, List<RoyaleYearQuarter>> winners = new Dictionary<FantasyCriticUser, List<RoyaleYearQuarter>>();
            using (var connection = new MySqlConnection(_connectionString))
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

                    var topPublisher = group.MaxBy(x => x.TotalFantasyPoints).First();
                    var topDomainPublisher = await GetPublisher(topPublisher.PublisherID);
                    if (!winners.ContainsKey(topDomainPublisher.Value.User))
                    {
                        winners.Add(topDomainPublisher.Value.User, new List<RoyaleYearQuarter>());
                    }

                    winners[topDomainPublisher.Value.User].Add(quarter);
                }
            }

            return winners.SealDictionary();
        }
    }
}
