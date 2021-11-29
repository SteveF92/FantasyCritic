using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Dapper;
using Dapper.NodaTime;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL;
using Microsoft.Extensions.Logging;
using NLog;
using NodaTime;
using FantasyCritic.MySQL.Entities;
using MySqlConnector;
using FantasyCritic.Lib.Domain;
using MoreLinq;

namespace FantasyCritic.DatabaseUtility
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static string _connectionString;
        private static IClock _clock;

        static async Task Main(string[] args)
        {
            _connectionString = ConfigurationManager.AppSettings["ConnectionString"];

            _clock = SystemClock.Instance;
            DapperNodaTimeSetup.Register();

            await InitializeSpecialRosterSlots();
        }

        private static async Task InitializeSpecialRosterSlots()
        {
            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
            MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
            MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);

            var updateEntities = new List<PublisherGameUpdateEntity>();
            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                var allPublishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year);
                foreach (var publisher in allPublishers)
                {
                    var orderedStandardGames = publisher.PublisherGames.Where(x => !x.CounterPick).OrderBy(x => x.Timestamp).ToList();
                    for (var index = 0; index < orderedStandardGames.Count; index++)
                    {
                        var standardGame = orderedStandardGames[index];
                        updateEntities.Add(new PublisherGameUpdateEntity(standardGame.PublisherGameID, index));
                    }
                    var orderedCounterPickGames = publisher.PublisherGames.Where(x => x.CounterPick).OrderBy(x => x.Timestamp).ToList();
                    for (var index = 0; index < orderedCounterPickGames.Count; index++)
                    {
                        var counterPick = orderedCounterPickGames[index];
                        updateEntities.Add(new PublisherGameUpdateEntity(counterPick.PublisherGameID, index));
                    }
                }
            }

            List<string> updateStatements = new List<string>();
            foreach (var updateEntity in updateEntities)
            {
                string sql = $"UPDATE tbl_league_publishergame SET SlotNumber = {updateEntity.SlotNumber} WHERE PublisherGameID = '{updateEntity.PublisherGameID}';";
                updateStatements.Add(sql);
            }

            var batches = updateStatements.Batch(500).ToList();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    for (var index = 0; index < batches.Count; index++)
                    {
                        _logger.Info($"Running publisher game update batch {index + 1}/{batches.Count}");
                        var batch = batches[index];
                        var joinedSQL = string.Join('\n', batch);
                        await connection.ExecuteAsync(joinedSQL, transaction: transaction);
                    }

                    await transaction.CommitAsync();
                }
            }
        }
    }
}
