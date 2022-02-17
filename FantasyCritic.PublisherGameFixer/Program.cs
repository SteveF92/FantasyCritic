using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using CSharpFunctionalExtensions;
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
using FantasyCritic.Lib.Domain.LeagueActions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Configuration;
using FantasyCritic.Lib.Extensions;
using FuzzyString;

using static MoreLinq.Extensions.BatchExtension;
using static MoreLinq.Extensions.MaxByExtension;

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

            //await FixDraftPositions();
            //await FixBidAmounts();
            await FixFormerPublisherGames();
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

        private static async Task FixFormerPublisherGames()
        {
            _logger.Info("Running former publisher game fixes");
            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
            MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
            MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);

            var draftPositionUpdateEntities = new List<FormerPublisherGameDraftPositionUpdateEntity>();
            var bidAmountUpdateEntities = new List<FormerPublisherGameBidAmountUpdateEntity>();

            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            var realYears = supportedYears.Where(x => x.Year >= 2020).ToList();

            var approvedMappings = new List<(string, string)>()
            {
                ("Persona 5 Scramble: The Phantom Strikers", "Persona 5 Strikers"),
                ("Tom Clancy’s Rainbow Six: Quarantine", "Tom Clancy’s Rainbow Six: Extraction"),
                ("Rumored 2D Metroid Sequel (Unannounced)", "Metroid Dread"),
                ("Ratchet & Clank Rift Apart", "Ratchet & Clank: Rift Apart"),
                ("Super Mario Sunshine Remaster (Rumored)", "Super Mario Sunshine Remaster (Deprecated)"),
                ("Mario Odyssey Sequel (Unannounced)", "Super Mario Odyssey 2 (Unannounced)"),
                ("The Wolf Among Us Season 2", "The Wolf Among Us 2"),
                ("Final Fantasy 7 Remake", "Final Fantasy VII Remake"),
                ("Bravely Default 2", "Bravely Default II"),
                ("GhostWire: Tokyo", "GhostWire Tokyo"),
                ("Gran Turismo Sequel (Unannounced)", "Gran Turismo 7"),
                ("Werewolf: The Apocalypse: Earthblood", "Werewolf: The Apocalypse - Earthblood"),
                ("Bright Memory infinite", "Bright Memory: Infinite"),
                ("Gran Turismo (PS5) (unannounced)", "Gran Turismo 7"),
                ("Twelve Minutes", "12 Minutes"),
                ("God of War: Ragnarok", "God of War: Ragnarök"),
                ("Last of Us Factions 2", "The Last of Us Factions 2"),
                ("Pokemon Let's Go 2 (Unannounced)", "Pokémon Let's Go 2 (Unannounced)"),
                ("Tom Clancy’s Rainbow Six: Parasite", "Tom Clancy’s Rainbow Six: Extraction"),
                ("Unannounced Spyro the Dragon Game", "Unannounced Mainline Spyro the Dragon Game"),
                ("Dying Light 2", "Dying Light 2: Stay Human"),
                ("Forza Motorsport 8 (Unannounced)", "Forza Motorsport (Xbox Series X)"),
                ("Untitled Batman Game from WB Games Montréal (Unannounced)", "Gotham Knights"),
                ("Horizon: Zero Dawn Sequel (Unannounced)", "Horizon Forbidden West"),
                ("STALKER 2", "STALKER 2: Heart of Chernobyl"),
            }.ToHashSet();

            foreach (var supportedYear in realYears)
            {
                _logger.Info($"Running for {supportedYear.Year}");
                var leagueYears = await fantasyCriticRepo.GetLeagueYears(supportedYear.Year, true);
                var allPublishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, leagueYears, true);
                var allBids = await fantasyCriticRepo.GetProcessedPickupBids(supportedYear.Year, leagueYears, allPublishers);
                var allLeagueActions = await GetLeagueActions(supportedYear.Year);
                var leagueActionLookup = allLeagueActions.ToLookup(x => x.LeagueID);
                var bidLookup = allBids.Where(x => x.Successful.HasValue && x.Successful.Value).ToLookup(x => (x.Publisher.PublisherID, x.MasterGame.MasterGameID));
                var publishersByLeagueYear = allPublishers.GroupBy(x => x.LeagueYear).ToList();
                _logger.Info($"Got all data for {supportedYear.Year}");

                foreach (var publisherGroup in publishersByLeagueYear)
                {
                    var leagueActions = leagueActionLookup[publisherGroup.Key.League.LeagueID]
                        .OrderBy(x => x.Timestamp)
                        .ToList();
                    var filteredDraftActions = FilterDraftActions(leagueActions);

                    foreach (var publisher in publisherGroup)
                    {
                        var formerGamesThatNeedStats = publisher.FormerPublisherGames
                            .Where(x => x.PublisherGame.MasterGame.HasValue)
                            .Where(x => !x.PublisherGame.OverallDraftPosition.HasValue && !x.PublisherGame.BidAmount.HasValue)
                            .ToList();
                        foreach (var formerPublisherGame in formerGamesThatNeedStats)
                        {
                            var matchingBid = GetMatchingBid(bidLookup, publisher, formerPublisherGame);
                            if (matchingBid.HasValue)
                            {
                                bidAmountUpdateEntities.Add(new FormerPublisherGameBidAmountUpdateEntity(formerPublisherGame.PublisherGame.PublisherGameID, matchingBid.Value.Timestamp, matchingBid.Value.BidAmount));
                                continue;
                            }

                            var publisherDraftCount = 0;
                            for (var index = 0; index < filteredDraftActions.Count; index++)
                            {
                                var draftAction = filteredDraftActions[index];
                                if (draftAction.PublisherID == publisher.PublisherID)
                                {
                                    publisherDraftCount++;
                                }

                                var actionGameName = draftAction.Description.TrimStart("Drafted game: ").TrimStart("Auto Drafted game: ").Trim('\'');
                                if (!actionGameName.Equals(formerPublisherGame.PublisherGame.GameName, StringComparison.OrdinalIgnoreCase))
                                {
                                    bool approvedMapping = approvedMappings.Contains((actionGameName, formerPublisherGame.PublisherGame.GameName));
                                    if (!approvedMapping)
                                    {
                                        continue;
                                    }
                                }

                                var overallDraftPosition = index + 1;
                                var draftPosition = publisherDraftCount;
                                draftPositionUpdateEntities.Add(new FormerPublisherGameDraftPositionUpdateEntity(formerPublisherGame.PublisherGame.PublisherGameID, draftAction.Timestamp, overallDraftPosition, draftPosition));
                            }
                        }
                    }
                }
            }

            List<string> updateStatements = new List<string>();
            foreach (var updateEntity in draftPositionUpdateEntities)
            {
                string sql = $"UPDATE tbl_league_formerpublishergame SET Timestamp = '{updateEntity.Timestamp}', DraftPosition = {updateEntity.DraftPosition}, OverallDraftPosition = {updateEntity.OverallDraftPosition} WHERE PublisherGameID = '{updateEntity.PublisherGameID}';";
                updateStatements.Add(sql);
            }

            foreach (var updateEntity in bidAmountUpdateEntities)
            {
                string sql = $"UPDATE tbl_league_formerpublishergame SET Timestamp = '{updateEntity.Timestamp}', BidAmount = {updateEntity.BidAmount} WHERE PublisherGameID = '{updateEntity.PublisherGameID}';";
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

            _logger.Info("Done running former publisher game fixes");
        }

        private static IReadOnlyList<TempLeagueActionEntity> FilterDraftActions(List<TempLeagueActionEntity> actions)
        {
            var draftActions = actions.Where(x => x.ActionType.Contains("Drafted")).ToList();
            if (!draftActions.Any())
            {
                return new List<TempLeagueActionEntity>();
            }

            var finalDraftAction = draftActions.Last();
            var actionsDuringDraft = actions
                .Where(x => x.Timestamp <= finalDraftAction.Timestamp)
                .OrderBy(x => x.Timestamp)
                .ToList();

            var removeActions = actionsDuringDraft.Where(x => x.Description.Contains("Removed game:")).ToList();
            if (!removeActions.Any())
            {
                return draftActions;
            }

            var actionsAccountedFor = new HashSet<TempLeagueActionEntity>();
            var filteredActions = new List<TempLeagueActionEntity>();
            foreach (var draftAction in draftActions)
            {
                var gameName = draftAction.Description.TrimStart("Drafted game: ").TrimStart("Auto Drafted game: ").Trim('\'');
                var futureRemoveActions = removeActions.Where(x => x.Timestamp >= draftAction.Timestamp).ToList();
                var matchingRemoveActionForGame = futureRemoveActions.Where(x => x.Description.Contains(gameName, StringComparison.OrdinalIgnoreCase)).ToList();
                var notAlreadyCounted = matchingRemoveActionForGame.Where(x => !actionsAccountedFor.Contains(x)).ToList();
                var firstNotAlreadyCounted = notAlreadyCounted.FirstOrDefault();
                if (firstNotAlreadyCounted is not null)
                {
                    actionsAccountedFor.Add(firstNotAlreadyCounted);
                }
                else
                {
                    filteredActions.Add(draftAction);
                }
            }

            return filteredActions;
        }

        private static async Task<IReadOnlyList<TempLeagueActionEntity>> GetLeagueActions(int year)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<TempLeagueActionEntity>(
                    "select tbl_league_publisher.LeagueID, tbl_league_action.PublisherID, tbl_league_action.Timestamp, tbl_league_action.ActionType, tbl_league_action.Description, tbl_league_action.ManagerAction from tbl_league_action " +
                    "join tbl_league_publisher on (tbl_league_action.PublisherID = tbl_league_publisher.PublisherID) " +
                    "where tbl_league_publisher.Year = @year;",
                    new
                    {
                        year
                    });

                return entities.ToList();
            }
        }

        private static Maybe<PickupBid> GetMatchingBid(ILookup<(Guid PublisherID, Guid MasterGameID), PickupBid> bidLookup, Publisher publisher, FormerPublisherGame formerPublisherGame)
        {
            var possibleBids = bidLookup[(publisher.PublisherID, formerPublisherGame.PublisherGame.MasterGame.Value.MasterGame.MasterGameID)];
            var bidsBeforeDrop = possibleBids.Where(x => x.Timestamp < formerPublisherGame.RemovedTimestamp).ToList();
            if (!bidsBeforeDrop.Any())
            {
                return Maybe<PickupBid>.None;
            }

            var lastBidBeforeDrop = bidsBeforeDrop.MaxBy(x => x.Timestamp).ToList();
            if (lastBidBeforeDrop.Count() != 1)
            {
                throw new Exception("Something strange!");
            }

            var bestBid = lastBidBeforeDrop.Single();
            return bestBid;
        }
    }
}