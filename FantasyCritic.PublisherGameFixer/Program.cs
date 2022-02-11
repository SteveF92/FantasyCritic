using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
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
using Microsoft.Extensions.Configuration;
using MoreLinq;

namespace FantasyCritic.PublisherGameFixer
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static string _connectionString;
        private static IClock _clock;

        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            _connectionString = config["ConnectionString"];

            _clock = SystemClock.Instance;
            DapperNodaTimeSetup.Register();

            await FixDraftPositions();
            await FixBidAmounts();
        }

        private static async Task FixDraftPositions()
        {
            _logger.Info("Running draft position fixes");
            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
            MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
            MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);

            var draftPositionUpdateEntities = new List<PublisherGameDraftPositionUpdateEntity>();

            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            var realYears = supportedYears.Where(x => x.Year > 2018).ToList();
            foreach (var supportedYear in realYears)
            {
                _logger.Info($"Running for {supportedYear.Year}");
                var leagueYears = await fantasyCriticRepo.GetLeagueYears(supportedYear.Year, true);
                var allPublishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, leagueYears, true);
                var publishersByLeagueYear = allPublishers.GroupBy(x => x.LeagueYear).ToList();
                _logger.Info($"Got all data for {supportedYear.Year}");

                for (var i = 0; i < publishersByLeagueYear.Count; i++)
                {
                    if (i % 100 == 0)
                    {
                        _logger.Info($"Running league year {i+1}/{publishersByLeagueYear.Count}");
                    }
                    var publisherGroup = publishersByLeagueYear[i];
                    var allPublisherGames = publisherGroup.SelectMany(x => x.PublisherGames).ToList();
                    var allCounterPicksForLeague = allPublisherGames
                        .Where(x => x.CounterPick).OrderBy(x => x.Timestamp)
                        .ToList();
                    var overallDraftOrder = allCounterPicksForLeague
                        .Select((counterPick, index) => (counterPick, index))
                        .ToDictionary(x => x.counterPick.PublisherGameID, x => x.index);

                    foreach (var publisher in publisherGroup)
                    {
                        var allCounterPicksForPublisher = publisher.PublisherGames
                            .Where(x => x.CounterPick).OrderBy(x => x.Timestamp)
                            .ToList();
                        for (var index = 0; index < allCounterPicksForPublisher.Count; index++)
                        {
                            bool counterPickWasDrafted = index < publisherGroup.Key.Options.CounterPicksToDraft;
                            if (counterPickWasDrafted)
                            {
                                var overallDraftPosition = overallDraftOrder[allCounterPicksForPublisher[index].PublisherGameID] + 1;
                                var draftPosition = index + 1;
                                draftPositionUpdateEntities.Add(new PublisherGameDraftPositionUpdateEntity(allCounterPicksForPublisher[index].PublisherGameID, overallDraftPosition, draftPosition));
                            }
                        }
                    }
                }
            }

            List<string> updateStatements = new List<string>();
            foreach (var updateEntity in draftPositionUpdateEntities)
            {
                string sql = $"UPDATE tbl_league_publishergame SET OverallDraftPosition = {updateEntity.OverallDraftPosition}, DraftPosition = {updateEntity.DraftPosition} WHERE PublisherGameID = '{updateEntity.PublisherGameID}';";
                updateStatements.Add(sql);
            }

            _logger.Info("Starting database updates");
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

            _logger.Info("Done running draft position fixes");
        }

        private static async Task FixBidAmounts()
        {
            _logger.Info("Running bid amount fixes");
            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
            MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
            MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);

            var bidAmountUpdateEntities = new List<PublisherGameBidAmountUpdateEntity>();

            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            var realYears = supportedYears.Where(x => x.Year > 2018).ToList();
            foreach (var supportedYear in realYears)
            {
                _logger.Info($"Running for {supportedYear.Year}");
                var leagueYears = await fantasyCriticRepo.GetLeagueYears(supportedYear.Year, true);
                var allPublishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, leagueYears, true);
                var allBids = await fantasyCriticRepo.GetProcessedPickupBids(supportedYear.Year, leagueYears, allPublishers);
                var bidLookup = allBids.Where(x => x.Successful.HasValue && x.Successful.Value).ToLookup(x => (x.Publisher.PublisherID, x.MasterGame.MasterGameID));
                var publishersByLeagueYear = allPublishers.GroupBy(x => x.LeagueYear).ToList();
                _logger.Info($"Got all data for {supportedYear.Year}");

                for (var i = 0; i < publishersByLeagueYear.Count; i++)
                {
                    if (i % 100 == 0)
                    {
                        _logger.Info($"Running league year {i + 1}/{publishersByLeagueYear.Count}");
                    }
                    var publisherGroup = publishersByLeagueYear[i];
                    var allPublisherGames = publisherGroup.SelectMany(x => x.PublisherGames).ToList();

                    var undraftedPublisherGamesWithMasterGames = allPublisherGames.Where(x => x.MasterGame.HasValue && !x.OverallDraftPosition.HasValue).ToList();
                    foreach (var publisherGame in undraftedPublisherGamesWithMasterGames)
                    {
                        var possibleBids = bidLookup[(publisherGame.PublisherID, publisherGame.MasterGame.Value.MasterGame.MasterGameID)];
                        var bidsBeforeAcquisition =
                            possibleBids.Where(x => x.Timestamp < publisherGame.Timestamp).ToList();
                        if (!bidsBeforeAcquisition.Any())
                        {
                            continue;
                        }

                        var lastBidBeforeAcquisition = bidsBeforeAcquisition.MaxBy(x => x.Timestamp).ToList();
                        if (lastBidBeforeAcquisition.Count() != 1)
                        {
                            throw new Exception("Something strange!");
                        }

                        var bestBid = lastBidBeforeAcquisition.Single();
                        bidAmountUpdateEntities.Add(new PublisherGameBidAmountUpdateEntity(publisherGame.PublisherGameID, bestBid.BidAmount));
                    }
                }
            }
            List<string> updateStatements = new List<string>();
            foreach (var updateEntity in bidAmountUpdateEntities)
            {
                string sql = $"UPDATE tbl_league_publishergame SET BidAmount = {updateEntity.BidAmount} WHERE PublisherGameID = '{updateEntity.PublisherGameID}';";
                updateStatements.Add(sql);
            }

            _logger.Info("Starting database updates");
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

            _logger.Info("Done running bid amount fixes");
        }
    }
}