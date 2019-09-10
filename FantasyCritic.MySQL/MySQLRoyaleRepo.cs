using FantasyCritic.Lib.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Task CreatePublisher(RoyalePublisher publisher)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<RoyalePublisher>> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user)
        {
            throw new NotImplementedException();
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
    }
}
