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

            await AddMissingFormerPublisherGames();
        }


        private static async Task AddMissingFormerPublisherGames()
        {
            _logger.Info("Running missing former publisher game additions");
            MySQLFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_connectionString, _clock);
            MySQLMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_connectionString, userStore);
            MySQLFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_connectionString, userStore, masterGameRepo);

            var formerPublisherGameEntities = new List<FormerPublisherGameEntity>();

            var supportedYears = await fantasyCriticRepo.GetSupportedYears();
            var realYears = supportedYears.Where(x => x.Year > 2018).ToList();
            var masterGames = await masterGameRepo.GetMasterGames();

            foreach (var supportedYear in realYears)
            {
                _logger.Info($"Running for {supportedYear.Year}");
                var leagueYears = await fantasyCriticRepo.GetLeagueYears(supportedYear.Year, true);
                var allPublishers = await fantasyCriticRepo.GetAllPublishersForYear(supportedYear.Year, leagueYears, true);
                var allLeagueActions = await GetLeagueActions(supportedYear.Year);
                var leagueActionLookup = allLeagueActions.ToLookup(x => x.LeagueID);
                var publishersByLeagueYear = allPublishers.GroupBy(x => x.LeagueYear).ToList();
                var allBids = await fantasyCriticRepo.GetProcessedPickupBids(supportedYear.Year, leagueYears, allPublishers);
                var bidLookup = allBids.Where(x => x.Successful.HasValue && x.Successful.Value).ToLookup(x => (x.Publisher.PublisherID));
                _logger.Info($"Got all data for {supportedYear.Year}");

                foreach (var publisherGroup in publishersByLeagueYear)
                {
                    if (!publisherGroup.Key.PlayStatus.DraftFinished)
                    {
                        continue;
                    }
                    var leagueActions = leagueActionLookup[publisherGroup.Key.League.LeagueID].ToList();
                    var draftActions = leagueActions.Where(x => x.ActionType.Contains("Drafted")).ToList();
                    if (!draftActions.Any())
                    {
                        throw new Exception($"No draft actions for league: {publisherGroup.Key.League.LeagueID}");
                    }

                    var finalDraftAction = draftActions.Last();
                    var removesAfterDraft = leagueActions
                        .Where(x => x.Timestamp > finalDraftAction.Timestamp)
                        .Where(x => x.ActionType == "Publisher Game Removed")
                        .OrderBy(x => x.Timestamp)
                        .ToList();

                    var filteredDraftActions = FilterDraftActions(leagueActions);
                    var claimActions = leagueActions.Where(x => x.ActionType == "Publisher Game Claimed").ToList();
                    var bidActions = leagueActions.Where(x => x.ActionType == "Pickup Successful").ToList();

                    foreach (var publisher in publisherGroup)
                    {
                        var removeActionsForPublisher = removesAfterDraft.Where(x => x.PublisherID == publisher.PublisherID).ToList();
                        if (!removeActionsForPublisher.Any())
                        {
                            continue;
                        }

                        var bidsForPublisher = bidLookup[publisher.PublisherID].ToList();
                        var bidActionsForPublisher = bidActions.Where(x => x.PublisherID == publisher.PublisherID).ToList();
                        var draftActionsForPublisher = filteredDraftActions.Where(x => x.PublisherID == publisher.PublisherID).ToList();
                        var claimActionsForPublisher = claimActions.Where(x => x.PublisherID == publisher.PublisherID).ToList();

                        foreach (var removeAction in removeActionsForPublisher)
                        {
                            var actionGameName = removeAction.Description.TrimStart("Removed game: ").Trim('\'');
                            var existingFormerGame = publisher.FormerPublisherGames.Where(x => GameNameMatches(actionGameName, x.PublisherGame.GameName) 
                                    || new Interval(x.RemovedTimestamp.Minus(Duration.FromSeconds(5)), x.RemovedTimestamp.Plus(Duration.FromSeconds(5)))
                                    .Contains(removeAction.Timestamp))
                                .ToList();
                            if (existingFormerGame.Any())
                            {
                                continue;
                            }

                            var matchingBid = GetMatchingBid(bidsForPublisher, publisher, actionGameName, removeAction);
                            if (matchingBid.HasValue)
                            {
                                var matchingBidAction = GetMatchingBidAction(matchingBid.Value, bidActionsForPublisher);
                                if (matchingBidAction.HasNoValue)
                                {
                                    continue;
                                }

                                formerPublisherGameEntities.Add(new FormerPublisherGameEntity(Guid.NewGuid(), publisher.PublisherID, matchingBid.Value.MasterGame.GameName, 
                                    matchingBidAction.Value.Timestamp, matchingBid.Value.CounterPick, null, false, null, matchingBid.Value.MasterGame.MasterGameID, null, null, matchingBid.Value.BidAmount, null, removeAction.Timestamp, "Removed by league manager"));
                                continue;
                            }

                            Maybe<TempLeagueActionEntity> draftAction = GetMatchingDraftAction(draftActionsForPublisher, publisher, actionGameName, removeAction);
                            if (draftAction.HasValue)
                            {
                                Maybe<MasterGame> matchingMasterGame = GetMatchingMasterGame(actionGameName, masterGames);
                                if (matchingMasterGame.HasNoValue)
                                {
                                    continue;
                                }

                                var draftUpdate = GetDraftUpdate(filteredDraftActions, publisher, matchingMasterGame.Value);

                                bool draftActionIsCounterPick = draftAction.Value.ActionType.ToLower().Contains("counterpick");

                                formerPublisherGameEntities.Add(new FormerPublisherGameEntity(Guid.NewGuid(), publisher.PublisherID, matchingMasterGame.Value.GameName,
                                    draftAction.Value.Timestamp, draftActionIsCounterPick, null, false, null, matchingMasterGame.Value.MasterGameID, 
                                    draftUpdate.GetValueOrDefault(x => x.DraftPosition), draftUpdate.GetValueOrDefault(x => x.OverallDraftPosition), null, null, removeAction.Timestamp, "Removed by league manager"));
                                continue;
                            }

                            Maybe<TempLeagueActionEntity> claimAction = GetMatchingClaimAction(claimActionsForPublisher, publisher, actionGameName, removeAction);
                            if (claimAction.HasValue)
                            {
                                Maybe<MasterGame> matchingMasterGame = GetMatchingMasterGame(actionGameName, masterGames);
                                if (matchingMasterGame.HasNoValue)
                                {
                                    continue;
                                }

                                bool claimActionIsCounterPick = false;

                                formerPublisherGameEntities.Add(new FormerPublisherGameEntity(Guid.NewGuid(), publisher.PublisherID, matchingMasterGame.Value.GameName,
                                    draftAction.Value.Timestamp, claimActionIsCounterPick, null, false, null, matchingMasterGame.Value.MasterGameID, null, null, null, null, removeAction.Timestamp, "Removed by league manager"));
                                continue;
                            }
                        }
                    }
                }
            }

            //_logger.Info("Starting database inserts");
            //using (var connection = new MySqlConnection(_connectionString))
            //{
            //    await connection.OpenAsync();
            //    await connection.BulkInsertAsync(formerPublisherGameEntities, "tbl_league_formerpublishergame", 500);
            //}

            //_logger.Info("Done running missing former publisher game additions");
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

        private static Maybe<PickupBid> GetMatchingBid(IReadOnlyList<PickupBid> pickupBidsForPublisher, Publisher publisher, 
            string gameName, TempLeagueActionEntity removeAction)
        {
            var bidsBeforeRemove = pickupBidsForPublisher.Where(x => x.Timestamp < removeAction.Timestamp).OrderByDescending(x => x.Timestamp).ToList();
            if (!bidsBeforeRemove.Any())
            {
                return Maybe<PickupBid>.None;
            }

            List<PickupBid> possibleBids = new List<PickupBid>();
            foreach (var bid in bidsBeforeRemove)
            {
                if (!GameNameMatches(gameName, bid.MasterGame.GameName))
                {
                    continue;
                }

                possibleBids.Add(bid);
            }


            if (!possibleBids.Any())
            {
                return Maybe<PickupBid>.None;
            }

            var lastBidBeforeRemove = possibleBids.MaxBy(x => x.Timestamp).ToList();
            if (lastBidBeforeRemove.Count() != 1)
            {
                throw new Exception("Something strange!");
            }

            return lastBidBeforeRemove.Single();
        }

        private static Maybe<TempLeagueActionEntity> GetMatchingBidAction(PickupBid bid, IReadOnlyList<TempLeagueActionEntity> bidActionsForPublisher)
        {
            List<TempLeagueActionEntity> possibleBidActions = new List<TempLeagueActionEntity>();
            foreach (var bidAction in bidActionsForPublisher)
            {
                var actionGameName = bidAction.Description.TrimStart("Acquired game ").TrimStartingFromFirstInstance(" with a bid of ").Trim('\'');
                if (!GameNameMatches(actionGameName, bid.MasterGame.GameName))
                {
                    continue;
                }

                bool bidInRange = new Interval(bid.Timestamp, bid.Timestamp.Plus(Duration.FromDays(7))).Contains(bidAction.Timestamp);
                if (!bidInRange)
                {
                    continue;
                }

                possibleBidActions.Add(bidAction);
            }

            if (!possibleBidActions.Any())
            {
                return Maybe<TempLeagueActionEntity>.None;
            }

            if (possibleBidActions.Count > 1)
            {
                throw new Exception($"More than one match for bid: {bid.BidID}");
            }

            return possibleBidActions.Single();
        }


        private static Maybe<TempLeagueActionEntity> GetMatchingDraftAction(IReadOnlyList<TempLeagueActionEntity> filteredDraftActions, Publisher publisher, 
            string gameName, TempLeagueActionEntity removeAction)
        {
            return Maybe<TempLeagueActionEntity>.None;
            //var publisherDraftCount = 0;
            //for (var index = 0; index < filteredDraftActions.Count; index++)
            //{
            //    var draftAction = filteredDraftActions[index];
            //    if (draftAction.PublisherID == publisher.PublisherID)
            //    {
            //        publisherDraftCount++;
            //    }

            //    var actionGameName = draftAction.Description.TrimStart("Drafted game: ").TrimStart("Auto Drafted game: ").Trim('\'');
            //    if (!actionGameName.Equals(formerPublisherGame.PublisherGame.GameName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        bool approvedMapping = approvedMappings.Contains((actionGameName, formerPublisherGame.PublisherGame.GameName));
            //        if (!approvedMapping)
            //        {
            //            continue;
            //        }
            //    }

            //    var overallDraftPosition = index + 1;
            //    var draftPosition = publisherDraftCount;
            //    var update = new FormerPublisherGameDraftPositionUpdateEntity(
            //        formerPublisherGame.PublisherGame.PublisherGameID, draftAction.Timestamp, overallDraftPosition,
            //        draftPosition);
            //    return update;
            //}

            //return Maybe<FormerPublisherGameDraftPositionUpdateEntity>.None;
        }

        private static Maybe<TempLeagueActionEntity> GetMatchingClaimAction(IReadOnlyList<TempLeagueActionEntity> claimActionsForPublisher, Publisher publisher,
            string gameName, TempLeagueActionEntity removeAction)
        {
            return Maybe<TempLeagueActionEntity>.None;
            //var claimActionsBeforeDrop = claimActionsForPublisher
            //    .Where(x => x.Timestamp < formerPublisherGame.RemovedTimestamp).OrderByDescending(x => x.Timestamp)
            //    .ToList();
            //foreach (var claimAction in claimActionsBeforeDrop)
            //{
            //    var actionGameName = claimAction.Description.TrimStart("Claimed game: ").Trim('\'');
            //    if (!actionGameName.Equals(formerPublisherGame.PublisherGame.GameName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        bool approvedMapping = approvedMappings.Contains((actionGameName, formerPublisherGame.PublisherGame.GameName));
            //        if (!approvedMapping)
            //        {
            //            continue;
            //        }
            //    }

            //    var update = new FormerPublisherGameClaimUpdateEntity(formerPublisherGame.PublisherGame.PublisherGameID, claimAction.Timestamp);
            //    return update;
            //}

            //return Maybe<FormerPublisherGameClaimUpdateEntity>.None;
        }

        private static bool GameNameMatches(string rawGameName, string possibleMatch)
        {
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
                ("Call of Duty 2021", "Call of Duty Vanguard"),
                ("Mario Kart 9 (Unannounced)", "Mario Kart (Unannounced Next Mainline Console Game)"),
                ("Call of Duty 2020 (Unannounced)", "Call of Duty: Black Ops Cold War"),
                ("Bravely Default 2", "Bravely Default II"),
                ("Grand Theft Auto 6 (Unannounced)", "Grand Theft Auto 6"),
                ("Gods & Monsters", "Immortals: Fenyx Rising"),
                ("Unannounced 3D Mario", "Unannounced Mainline 3D Mario Platformer"),
                ("Deltarune", "Deltarune Chapter 2"),
                ("Unannounced Call of Duty 2022", "Call of Duty: Modern Warfare Sequel (2022)"),
                ("Unannounced Main Series Sonic Game", "Sonic Frontiers"),
                ("HuniePop 2", "HuniePop 2: Double Date"),
                ("Unannounced Next Assassin's Creed", "Assassin's Creed Infinity"),
                ("Battlefield 2042", "Next Gen Battlefield Game"),
                ("Demon Souls Remake", "Demon's Souls (2020)"),
                ("Baldo", "Baldo: The Guardian Owls"),
                ("Destruction Allstars ", "Destruction Allstars"),
                ("Dark Alliance", "Dungeons & Dragons: Dark Alliance"),
                ("Fable (Unannounced)", "Fable (Xbox Series X)"),
                ("Untitled Final Fantasy XIV Expansion", "Final Fantasy XIV: Endwalker"),
            }.ToHashSet();

            if (!rawGameName.Equals(possibleMatch, StringComparison.OrdinalIgnoreCase))
            {
                bool approvedMapping = approvedMappings.Contains((rawGameName, possibleMatch));
                return approvedMapping;
            }

            return true;
        }

        private static Maybe<MasterGame> GetMatchingMasterGame(string actionGameName, IReadOnlyList<MasterGame> masterGames)
        {
            var matches = masterGames.Where(x => GameNameMatches(actionGameName, x.GameName)).ToList();
            if (!matches.Any())
            {
                return Maybe<MasterGame>.None;
            }

            if (matches.Count > 1)
            {
                throw new Exception($"More than one match for: {actionGameName}");
            }

            return matches.Single();
        }

        private static Maybe<FormerPublisherGameDraftPositionUpdateEntity> GetDraftUpdate(IReadOnlyList<TempLeagueActionEntity> filteredDraftActions, Publisher publisher, MasterGame masterGame)
        {
            var publisherDraftCount = 0;
            for (var index = 0; index < filteredDraftActions.Count; index++)
            {
                var draftAction = filteredDraftActions[index];
                if (draftAction.PublisherID == publisher.PublisherID)
                {
                    publisherDraftCount++;
                }

                var actionGameName = draftAction.Description.TrimStart("Drafted game: ").TrimStart("Auto Drafted game: ").Trim('\'');
                if (!GameNameMatches(actionGameName, masterGame.GameName))
                {
                    continue;
                }

                var overallDraftPosition = index + 1;
                var draftPosition = publisherDraftCount;
                var update = new FormerPublisherGameDraftPositionUpdateEntity(Guid.NewGuid(), draftAction.Timestamp, overallDraftPosition, draftPosition);
                return update;
            }

            return Maybe<FormerPublisherGameDraftPositionUpdateEntity>.None;
        }
    }
}