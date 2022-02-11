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

namespace FantasyCritic.PublisherGameFixer
{
    class Program
    {
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

            var overallDraftPositionUpdateEntities = new List<PublisherGameOverallDraftPositionUpdateEntity>();
            var draftPositionUpdateEntities = new List<PublisherGameDraftPositionUpdateEntity>();
            var bidAmountUpdateEntities = new List<PublisherGameBidAmountUpdateEntity>();

            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                var leagueYears = await fantasyCriticRepo.GetLeagueYears(supportedYear.Year, true);
                var allPublishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, leagueYears, true);
                var publishersByLeagueYear = allPublishers.GroupBy(x => x.LeagueYear);
                foreach (var publisherGroup in publishersByLeagueYear)
                {
                    var allDraftedCounterPicksForLeague = publisherGroup.SelectMany(x => x.PublisherGames).Where(x => x.CounterPick && x.OverallDraftPosition.HasValue).OrderBy(x => x.Timestamp).ToList();
                    for (var index = 0; index < allDraftedCounterPicksForLeague.Count; index++)
                    {
                        var draftedCounterPick = allDraftedCounterPicksForLeague[index];
                        overallDraftPositionUpdateEntities.Add(new PublisherGameOverallDraftPositionUpdateEntity(draftedCounterPick.PublisherGameID, index + 1));
                    }

                    foreach (var publisher in publisherGroup)
                    {
                        var allDraftedCounterPicksForPublisher = publisher.PublisherGames.Where(x => x.CounterPick && x.OverallDraftPosition.HasValue).OrderBy(x => x.Timestamp).ToList();
                        for (var index = 0; index < allDraftedCounterPicksForPublisher.Count; index++)
                        {
                            var draftedCounterPick = allDraftedCounterPicksForPublisher[index];
                            draftPositionUpdateEntities.Add(new PublisherGameDraftPositionUpdateEntity(draftedCounterPick.PublisherGameID, index + 1));
                        }
                    }
                }
            }

            var overallDictionary = overallDraftPositionUpdateEntities.ToDictionary(x => x.PublisherGameID);
            var positionDictionary = draftPositionUpdateEntities.ToDictionary(x => x.PublisherGameID);
            var bidDictionary = bidAmountUpdateEntities.ToDictionary(x => x.PublisherGameID);
            var allKeys = overallDictionary.Keys
                .Concat(positionDictionary.Keys)
                .Concat(bidDictionary.Keys)
                .Distinct()
                .ToList();

            List<PublisherGameCombinedUpdateEntity> combinedEntities = new List<PublisherGameCombinedUpdateEntity>();
            foreach (var key in allKeys)
            {
                overallDictionary.TryGetValue(key, out var overallEntity);
                positionDictionary.TryGetValue(key, out var positionEntity);
                bidDictionary.TryGetValue(key, out var bidEntity);
                combinedEntities.Add(new PublisherGameCombinedUpdateEntity(key, bidEntity?.BidAmount, overallEntity?.OverallDraftPosition, positionEntity?.DraftPosition));
            }

            List<string> updateStatements = new List<string>();
            foreach (var updateEntity in combinedEntities)
            {
                string sql = $"UPDATE tbl_league_publishergame SET BidAmount = {updateEntity.BidAmount}, OverallDraftPosition = {updateEntity.OverallDraftPosition}, DraftPosition = {updateEntity.DraftPosition} WHERE PublisherGameID = '{updateEntity.PublisherGameID}';";
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
                        Console.WriteLine($"Running publisher game update batch {index + 1}/{batches.Count}");
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