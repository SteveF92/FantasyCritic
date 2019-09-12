using FantasyCritic.Lib.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;
using FantasyCritic.MySQL.Entities;
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
                        publisherID = publisherID
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
            var amountGained = publisherGame.AmountSpent / 2;
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
    }
}
