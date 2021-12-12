using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.MySQL.Entities;
using FantasyCritic.MySQL.Entities.Identity;
using MySqlConnector;
using NLog;
using NLog.Targets.Wrappers;
using NodaTime;
using static MoreLinq.Extensions.BatchExtension;

namespace FantasyCritic.MySQL
{
    public class MySQLFantasyCriticRepo : IFantasyCriticRepo
    {
        private readonly string _connectionString;
        private readonly IReadOnlyFantasyCriticUserStore _userStore;
        private readonly IMasterGameRepo _masterGameRepo;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MySQLFantasyCriticRepo(string connectionString, IReadOnlyFantasyCriticUserStore userStore, IMasterGameRepo masterGameRepo)
        {
            _connectionString = connectionString;
            _userStore = userStore;
            _masterGameRepo = masterGameRepo;
        }

        public async Task<Maybe<League>> GetLeagueByID(Guid id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    leagueID = id
                };

                string leagueSQL = "select * from vw_league where LeagueID = @leagueID and IsDeleted = 0;";
                LeagueEntity leagueEntity = await connection.QuerySingleAsync<LeagueEntity>(leagueSQL, queryObject);

                FantasyCriticUser manager = await _userStore.FindByIdAsync(leagueEntity.LeagueManager.ToString(), CancellationToken.None);

                string leagueYearSQL = "select Year from tbl_league_year where LeagueID = @leagueID;";
                IEnumerable<int> years = await connection.QueryAsync<int>(leagueYearSQL, queryObject);
                League league = leagueEntity.ToDomain(manager, years);
                return league;
            }
        }

        public async Task<IReadOnlyList<League>> GetAllLeagues(bool includeDeleted = false)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "select * from vw_league where IsDeleted = 0;";
                if (includeDeleted)
                {
                    sql = "select * from vw_league;";
                }

                var leagueEntities = await connection.QueryAsync<LeagueEntity>(sql);

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tbl_league_year");
                var leagueYearLookup = yearEntities.ToLookup(x => x.LeagueID);
                List<League> leagues = new List<League>();
                var allUsers = await _userStore.GetAllUsers();
                var userDictionary = allUsers.ToDictionary(x => x.Id);
                foreach (var leagueEntity in leagueEntities)
                {
                    FantasyCriticUser manager = userDictionary[leagueEntity.LeagueManager];
                    IEnumerable<int> years = leagueYearLookup[leagueEntity.LeagueID].Select(x => x.Year);
                    League league = leagueEntity.ToDomain(manager, years);
                    leagues.Add(league);
                }

                return leagues;
            }
        }

        public async Task<Maybe<LeagueYear>> GetLeagueYear(League requestLeague, int requestYear)
        {
            var leagueTags = await GetLeagueYearTagEntities(requestLeague.LeagueID, requestYear);
            var specialGameSlots = await GetSpecialGameSlotEntities(requestLeague.LeagueID, requestYear);
            var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
            var eligibilityOverrides = await GetEligibilityOverrides(requestLeague, requestYear);
            var tagOverrides = await GetTagOverrides(requestLeague, requestYear);
            var supportedYear = await GetSupportedYear(requestYear);

            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    leagueID = requestLeague.LeagueID,
                    year = requestYear
                };

                LeagueYearEntity yearEntity = await connection.QueryFirstOrDefaultAsync<LeagueYearEntity>("select * from tbl_league_year where LeagueID = @leagueID and Year = @year", queryObject);
                if (yearEntity == null)
                {
                    return Maybe<LeagueYear>.None;
                }

                var domainLeagueTags = ConvertLeagueTagEntities(leagueTags, tagDictionary);
                var domainSpecialGameSlots = ConvertSpecialGameSlotEntities(specialGameSlots, tagDictionary);
                bool hasSpecialRosterSlots = domainSpecialGameSlots.TryGetValue(new LeagueYearKey(requestLeague.LeagueID, requestYear), out var specialGameSlotsForLeagueYear);
                if (!hasSpecialRosterSlots)
                {
                    specialGameSlotsForLeagueYear = new List<SpecialGameSlot>();
                }

                LeagueYear year = yearEntity.ToDomain(requestLeague, supportedYear, eligibilityOverrides, tagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear);
                return year;
                return year;
            }
        }

        public async Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year, bool includeDeleted = false)
        {
            var allLeagueTags = await GetLeagueYearTagEntities(year);
            var allSpecialGameSlots = await GetSpecialGameSlotEntities(year);
            var leagueTagsByLeague = allLeagueTags.ToLookup(x => x.LeagueID);
            var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
            var domainSpecialGameSlots = ConvertSpecialGameSlotEntities(allSpecialGameSlots, tagDictionary);
            var supportedYear = await GetSupportedYear(year);
            var allEligibilityOverrides = await GetAllEligibilityOverrides(year);
            var allTagOverrides = await GetAllTagOverrides(year);
            IReadOnlyList<League> leagues = await GetAllLeagues(includeDeleted);

            Dictionary<Guid, League> leaguesDictionary = leagues.ToDictionary(x => x.LeagueID, y => y);

            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    year
                };

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tbl_league_year where Year = @year", queryObject);
                List<LeagueYear> leagueYears = new List<LeagueYear>();
                
                foreach (var entity in yearEntities)
                {
                    var success = leaguesDictionary.TryGetValue(entity.LeagueID, out League league);
                    if (!success)
                    {
                        _logger.Warn($"Cannot find league (probably deleted) LeagueID: {entity.LeagueID}");
                        continue;
                    }

                    bool hasEligibilityOverrides = allEligibilityOverrides.TryGetValue(entity.LeagueID, out var eligibilityOverrides);
                    if (!hasEligibilityOverrides)
                    {
                        eligibilityOverrides = new List<EligibilityOverride>();
                    }

                    bool hasTagOverrides = allTagOverrides.TryGetValue(entity.LeagueID, out var tagOverrides);
                    if (!hasTagOverrides)
                    {
                        tagOverrides = new List<TagOverride>();
                    }

                    var domainLeagueTags = ConvertLeagueTagEntities(leagueTagsByLeague[entity.LeagueID], tagDictionary);
                    bool hasSpecialRosterSlots = domainSpecialGameSlots.TryGetValue(new LeagueYearKey(entity.LeagueID, entity.Year), out var specialGameSlotsForLeagueYear);
                    if (!hasSpecialRosterSlots)
                    {
                        specialGameSlotsForLeagueYear = new List<SpecialGameSlot>();
                    }

                    LeagueYear leagueYear = entity.ToDomain(league, supportedYear, eligibilityOverrides, tagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear);
                    leagueYears.Add(leagueYear);
                }

                return leagueYears;
            }
        }

        public async Task UpdatePublisherGameCalculatedStats(IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> calculatedStats)
        {
            string sql = "update tbl_league_publishergame SET FantasyPoints = @FantasyPoints where PublisherGameID = @PublisherGameID;";
            List<PublisherGameUpdateEntity> updateEntities = calculatedStats.Select(x => new PublisherGameUpdateEntity(x)).ToList();
            var updateBatches = updateEntities.Batch(1000).ToList();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    for (var index = 0; index < updateBatches.Count; index++)
                    {
                        _logger.Info($"Running publisher game update {index+1}/{updateBatches.Count}");
                        var batch = updateBatches[index];
                        await connection.ExecuteAsync(sql, batch, transaction);
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        public async Task<Result> RemovePublisherGame(PublisherGame publisherGame)
        {
            string sql = "delete from tbl_league_publishergame where PublisherGameID = @publisherGameID;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var removed = await connection.ExecuteAsync(sql, new { publisherGameID = publisherGame.PublisherGameID }, transaction);
                    if (removed != 1)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure("Removing game failed.");
                    }

                    await MakePublisherGameSlotsConsistent(publisherGame.PublisherID, publisherGame.PublisherGameID, connection, transaction);
                    await transaction.CommitAsync();
                    return Result.Success();
                }
            }
        }

        public async Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
        {
            string sql = "update tbl_league_publishergame SET ManualCriticScore = @manualCriticScore where PublisherGameID = @publisherGameID;";
            var updateObject = new { publisherGameID = publisherGame.PublisherGameID, manualCriticScore };
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, updateObject);
            }
        }

        public async Task ManuallySetWillNotRelease(PublisherGame publisherGame, bool willNotRelease)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_league_publishergame SET ManualWillNotRelease = @willNotRelease where PublisherGameID = @publisherGameID;",
                    new { publisherGameID = publisherGame.PublisherGameID, willNotRelease });
            }
        }

        public async Task CreatePickupBid(PickupBid currentBid)
        {
            var entity = new PickupBidEntity(currentBid);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_pickupbid(BidID,PublisherID,MasterGameID,ConditionalDropMasterGameID,CounterPick,Timestamp,Priority,BidAmount,Successful) VALUES " +
                    "(@BidID,@PublisherID,@MasterGameID,@ConditionalDropMasterGameID,@CounterPick,@Timestamp,@Priority,@BidAmount,@Successful);",
                    entity);
            }
        }

        public async Task EditPickupBid(PickupBid bid, Maybe<PublisherGame> conditionalDropPublisherGame, uint bidAmount)
        {
            var entity = new PickupBidEntity(bid, conditionalDropPublisherGame, bidAmount);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("UPDATE tbl_league_pickupbid SET ConditionalDropMasterGameID = @ConditionalDropMasterGameID, " +
                                              "BidAmount = @BidAmount WHERE BidID = @BidID;", entity);
            }
        }

        public async Task RemovePickupBid(PickupBid pickupBid)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync("delete from tbl_league_pickupbid where BidID = @BidID",
                            new {pickupBid.BidID}, transaction);
                    await connection.ExecuteAsync(
                        "update tbl_league_pickupbid SET Priority = Priority - 1 where PublisherID = @publisherID and Successful is NULL and Priority > @oldPriority",
                        new {publisherID = pickupBid.Publisher.PublisherID, oldPriority = pickupBid.Priority}, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task<IReadOnlyList<PickupBid>> GetActivePickupBids(Publisher publisher)
        {
            var leagueYear = publisher.LeagueYear;
            var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear);
            var publisherGameDictionary = publishersInLeague
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame.HasValue)
                .ToDictionary(x => (x.PublisherID, x.MasterGame.Value.MasterGame.MasterGameID));

            string sql = "select * from vw_league_pickupbid where PublisherID = @publisherID and Successful is NULL";
            var queryObject = new
            {
                publisherID = publisher.PublisherID
            };
            using (var connection = new MySqlConnection(_connectionString))
            {
                var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, queryObject);
                List<PickupBid> domainBids = new List<PickupBid>();
                foreach (var bidEntity in bidEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);
                    Maybe<PublisherGame> conditionalDropPublisherGame = Maybe<PublisherGame>.None;
                    if (bidEntity.ConditionalDropMasterGameID.HasValue)
                    {
                        conditionalDropPublisherGame = publisherGameDictionary[(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)];
                    }

                    PickupBid domain = bidEntity.ToDomain(publisher, masterGame.Value, conditionalDropPublisherGame, leagueYear);
                    domainBids.Add(domain);
                }

                return domainBids;
            }
        }

        public async Task<IReadOnlyList<PickupBid>> GetActivePickupBids(LeagueYear leagueYear)
        {
            var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear);
            var publisherDictionary = publishersInLeague.ToDictionary(x => x.PublisherID);
            var publisherGameDictionary = publishersInLeague
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame.HasValue)
                .ToDictionary(x => (x.PublisherID, x.MasterGame.Value.MasterGame.MasterGameID));

            string sql = "select * from vw_league_pickupbid where LeagueID = @leagueID and Year = @year and Successful is NULL";
            var queryObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year,
            };
            using (var connection = new MySqlConnection(_connectionString))
            {
                var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, queryObject);
                List<PickupBid> domainBids = new List<PickupBid>();
                foreach (var bidEntity in bidEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);
                    Maybe<PublisherGame> conditionalDropPublisherGame = Maybe<PublisherGame>.None;
                    if (bidEntity.ConditionalDropMasterGameID.HasValue)
                    {
                        conditionalDropPublisherGame = publisherGameDictionary[(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)];
                    }

                    var publisher = publisherDictionary[bidEntity.PublisherID];
                    PickupBid domain = bidEntity.ToDomain(publisher, masterGame.Value, conditionalDropPublisherGame, leagueYear);
                    domainBids.Add(domain);
                }

                return domainBids;
            }
        }

        public async Task<IReadOnlyList<PickupBid>> GetProcessedPickupBids(int year, IReadOnlyList<LeagueYear> allLeagueYears, IReadOnlyList<Publisher> allPublishersForYear)
        {
            var processedBids = await GetPickupBids(year, allLeagueYears, allPublishersForYear, true);
            var list = processedBids.SelectMany(x => x.Value).ToList();
            return list;
        }


        public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year, IReadOnlyList<LeagueYear> allLeagueYears, IReadOnlyList<Publisher> allPublishersForYear) => GetPickupBids(year, allLeagueYears, allPublishersForYear, false);

        private async Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetPickupBids(int year, IReadOnlyList<LeagueYear> allLeagueYears, IReadOnlyList<Publisher> allPublishersForYear, bool processed)
        {
            var leagueDictionary = allLeagueYears.GroupBy(x => x.League.LeagueID).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            var publisherDictionary = allPublishersForYear.ToDictionary(x => x.PublisherID, y => y);

            var sql = "select * from vw_league_pickupbid where Year = @year and Successful is NULL and IsDeleted = 0";
            if (processed)
            {
                sql = "select * from vw_league_pickupbid where Year = @year and Successful is NOT NULL and IsDeleted = 0";
            }

            var publisherGameDictionary = allPublishersForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame.HasValue)
                .ToDictionary(x => (x.PublisherID, x.MasterGame.Value.MasterGame.MasterGameID));

            using (var connection = new MySqlConnection(_connectionString))
            {
                var bidEntities = await connection.QueryAsync<PickupBidEntity>(sql, new { year });

                Dictionary<LeagueYear, List<PickupBid>> pickupBidsByLeagueYear = allLeagueYears.ToDictionary(x => x, y => new List<PickupBid>());

                foreach (var bidEntity in bidEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);
                    var publisher = publisherDictionary[bidEntity.PublisherID];
                    var leagueYear = leagueDictionary[publisher.LeagueYear.League.LeagueID].Single(x => x.Year == year);
                    Maybe<PublisherGame> conditionalDropPublisherGame = Maybe<PublisherGame>.None;
                    if (bidEntity.ConditionalDropMasterGameID.HasValue)
                    {
                        if (publisherGameDictionary.ContainsKey((bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)))
                        {
                            conditionalDropPublisherGame = publisherGameDictionary[(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value)];
                        }
                    }

                    PickupBid domainPickup = bidEntity.ToDomain(publisher, masterGame.Value, conditionalDropPublisherGame, leagueYear);
                    pickupBidsByLeagueYear[leagueYear].Add(domainPickup);
                }

                IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> finalDictionary = pickupBidsByLeagueYear.ToDictionary(x => x.Key, y => (IReadOnlyList<PickupBid>)y.Value);
                return finalDictionary;
            }
        }

        public async Task<Maybe<DropRequest>> GetDropRequest(Guid dropRequestID)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var dropRequestEntity = await connection.QuerySingleOrDefaultAsync<DropRequestEntity>("select * from tbl_league_droprequest where DropRequestID = @dropRequestID", new { dropRequestID });
                if (dropRequestEntity == null)
                {
                    return Maybe<DropRequest>.None;
                }

                var publisher = await GetPublisher(dropRequestEntity.PublisherID);
                var masterGame = await _masterGameRepo.GetMasterGame(dropRequestEntity.MasterGameID);
                var leagueYear = publisher.Value.LeagueYear;

                DropRequest domain = dropRequestEntity.ToDomain(publisher.Value, masterGame.Value, leagueYear);
                return domain;
            }
        }

        public async Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queuedEntities = await connection.QueryAsync<QueuedGameEntity>("select * from tbl_league_publisherqueue where PublisherID = @publisherID",
                    new { publisherID = publisher.PublisherID });

                List<QueuedGame> domainQueue = new List<QueuedGame>();
                foreach (var queuedEntity in queuedEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(queuedEntity.MasterGameID);

                    QueuedGame domain = queuedEntity.ToDomain(publisher, masterGame.Value);
                    domainQueue.Add(domain);
                }

                return domainQueue;
            }
        }

        public async Task QueueGame(QueuedGame queuedGame)
        {
            var entity = new QueuedGameEntity(queuedGame);
            string sql = "insert into tbl_league_publisherqueue(PublisherID,MasterGameID,`Rank`) VALUES (@PublisherID,@MasterGameID,@Rank);";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, entity);
            }
        }

        public async Task RemoveQueuedGame(QueuedGame queuedGame)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    string deleteSQL = "delete from tbl_league_publisherqueue where PublisherID = @PublisherID AND MasterGameID = @MasterGameID";
                    string alterRankSQL = "update tbl_league_pickupbid SET Priority = Priority - 1 where PublisherID = @publisherID and Successful is NULL and Priority > @oldPriority";
                    await connection.ExecuteAsync(deleteSQL, new { queuedGame.Publisher.PublisherID, queuedGame.MasterGame.MasterGameID }, transaction);
                    await connection.ExecuteAsync(alterRankSQL, new { publisherID = queuedGame.Publisher.PublisherID, oldPriority = queuedGame.Rank }, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks)
        {
            int tempPosition = queueRanks.Select(x => x.Value).Max() + 1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    foreach (var queuedGame in queueRanks)
                    {
                        await connection.ExecuteAsync(
                            "update tbl_league_publisherqueue set Rank = @rank where PublisherID = @PublisherID AND MasterGameID = @MasterGameID",
                            new
                            {
                                queuedGame.Key.MasterGame.MasterGameID,
                                queuedGame.Key.Publisher.PublisherID,
                                rank = tempPosition
                            }, transaction);
                        tempPosition++;
                    }

                    foreach (var queuedGame in queueRanks)
                    {
                        await connection.ExecuteAsync(
                            "update tbl_league_publisherqueue set Rank = @rank where PublisherID = @PublisherID AND MasterGameID = @MasterGameID",
                            new
                            {
                                queuedGame.Key.MasterGame.MasterGameID,
                                queuedGame.Key.Publisher.PublisherID,
                                rank = queuedGame.Value
                            }, transaction);
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        public async Task AddLeagueAction(LeagueAction action)
        {
            LeagueActionEntity entity = new LeagueActionEntity(action);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_action(PublisherID,Timestamp,ActionType,Description,ManagerAction) VALUES " +
                    "(@PublisherID,@Timestamp,@ActionType,@Description,@ManagerAction);", entity);
            }
        }

        public async Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<LeagueActionEntity>(
                    "select tbl_league_action.PublisherID, tbl_league_action.Timestamp, tbl_league_action.ActionType, tbl_league_action.Description, tbl_league_action.ManagerAction from tbl_league_action " +                          
                    "join tbl_league_publisher on (tbl_league_action.PublisherID = tbl_league_publisher.PublisherID) " +
                    "where tbl_league_publisher.LeagueID = @leagueID and tbl_league_publisher.Year = @leagueYear;",
                    new
                    {
                        leagueID = leagueYear.League.LeagueID,
                        leagueYear = leagueYear.Year
                    });

                var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear);

                List<LeagueAction> leagueActions = entities.Select(x => x.ToDomain(publishersInLeague.Single(y => y.PublisherID == x.PublisherID))).ToList();

                return leagueActions;
            }
        }

        public async Task ChangePublisherName(Publisher publisher, string publisherName)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_league_publisher SET PublisherName = @publisherName where PublisherID = @publisherID;",
                    new { publisherID = publisher.PublisherID, publisherName });
            }
        }

        public async Task SetAutoDraft(Publisher publisher, bool autoDraft)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_league_publisher SET AutoDraft = @autoDraft where PublisherID = @publisherID;",
                    new { publisherID = publisher.PublisherID, autoDraft });
            }
        }

        public async Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_league SET LeagueName = @leagueName, PublicLeague = @publicLeague, TestLeague = @testLeague where LeagueID = @leagueID;",
                    new { leagueID = league.LeagueID, leagueName, publicLeague, testLeague });
            }
        }

        public async Task StartDraft(LeagueYear leagueYear)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    $"update tbl_league_year SET PlayStatus = '{PlayStatus.Drafting.Value}', DraftStartedTimestamp = CURRENT_TIMESTAMP WHERE LeagueID = @leagueID and Year = @year",
                    new
                    {
                        leagueID = leagueYear.League.LeagueID,
                        year = leagueYear.Year
                    });
            }
        }

        public async Task CompleteDraft(LeagueYear leagueYear)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    $"update tbl_league_year SET PlayStatus = '{PlayStatus.DraftFinal.Value}' WHERE LeagueID = @leagueID and Year = @year",
                    new
                    {
                        leagueID = leagueYear.League.LeagueID,
                        year = leagueYear.Year
                    });
            }
        }

        public async Task ResetDraft(LeagueYear leagueYear)
        {
            var publishers = await GetPublishersInLeagueForYear(leagueYear);

            string gameDeleteSQL = "delete from tbl_league_publishergame where PublisherID in @publisherIDs";
            string draftResetSQL = $"update tbl_league_year SET PlayStatus = '{PlayStatus.NotStartedDraft.Value}' WHERE LeagueID = @leagueID and Year = @year";

            var paramsObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year,
                publisherIDs = publishers.Select(x => x.PublisherID)
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(gameDeleteSQL, paramsObject, transaction);
                    await connection.ExecuteAsync(draftResetSQL, paramsObject, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task SetDraftPause(LeagueYear leagueYear, bool pause)
        {
            string sql;
            if (pause)
            {
                sql = $"update tbl_league_year SET PlayStatus = '{PlayStatus.DraftPaused.Value}' WHERE LeagueID = @leagueID and Year = @year";
            }
            else
            {
                sql = $"update tbl_league_year SET PlayStatus = '{PlayStatus.Drafting.Value}' WHERE LeagueID = @leagueID and Year = @year";
            }
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql,
                    new
                    {
                        leagueID = leagueYear.League.LeagueID,
                        year = leagueYear.Year
                    });
            }
        }

        public async Task<Maybe<PickupBid>> GetPickupBid(Guid bidID)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var bidEntity = await connection.QuerySingleOrDefaultAsync<PickupBidEntity>("select * from tbl_league_pickupbid where BidID = @bidID", new { bidID });
                if (bidEntity == null)
                {
                    return Maybe<PickupBid>.None;
                }

                var publisher = await GetPublisher(bidEntity.PublisherID);
                var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);
                var leagueYear = publisher.Value.LeagueYear;
                Maybe<PublisherGame> conditionalDropPublisherGame = Maybe<PublisherGame>.None;
                if (bidEntity.ConditionalDropMasterGameID.HasValue)
                {
                    conditionalDropPublisherGame = await GetPublisherGame(bidEntity.PublisherID, bidEntity.ConditionalDropMasterGameID.Value);
                }

                PickupBid domain = bidEntity.ToDomain(publisher.Value, masterGame.Value, conditionalDropPublisherGame, leagueYear);
                return domain;
            }
        }

        public async Task CreateLeague(League league, int initialYear, LeagueOptions options)
        {
            LeagueEntity entity = new LeagueEntity(league);
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, initialYear, options, PlayStatus.NotStartedDraft);
            var tagEntities = options.LeagueTags.Select(x => new LeagueYearTagEntity(league, initialYear, x));
            List<SpecialGameSlotEntity> slotEntities = options.SpecialGameSlots.SelectMany(slot => slot.Tags, (slot, tag) =>
                    new SpecialGameSlotEntity(Guid.NewGuid(), league, initialYear, slot.SpecialSlotPosition, tag)).ToList();

            string createLeagueSQL =
                "insert into tbl_league(LeagueID,LeagueName,LeagueManager,PublicLeague,TestLeague) VALUES " +
                "(@LeagueID,@LeagueName,@LeagueManager,@PublicLeague,@TestLeague);";

            string createLeagueYearSQL =
                "insert into tbl_league_year(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,CounterPicksToDraft,FreeDroppableGames,WillNotReleaseDroppableGames,WillReleaseDroppableGames,DropOnlyDraftGames," +
                "CounterPicksBlockDrops,MinimumBidAmount,DraftSystem,PickupSystem,ScoringSystem,TradingSystem,PlayStatus) VALUES " +
                "(@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@CounterPicksToDraft,@FreeDroppableGames,@WillNotReleaseDroppableGames,@WillReleaseDroppableGames,@DropOnlyDraftGames," +
                "@CounterPicksBlockDrops,@MinimumBidAmount,@DraftSystem,@PickupSystem,@ScoringSystem,@TradingSystem,@PlayStatus);";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(createLeagueSQL, entity, transaction);
                    await connection.ExecuteAsync(createLeagueYearSQL, leagueYearEntity, transaction);
                    await connection.BulkInsertAsync<LeagueYearTagEntity>(tagEntities, "tbl_league_yearusestag", 500, transaction);
                    await connection.BulkInsertAsync<SpecialGameSlotEntity>(slotEntities, "tbl_league_specialgameslot", 500, transaction);

                    await transaction.CommitAsync();
                }
            }

            await AddPlayerToLeague(league, league.LeagueManager);
        }

        public async Task EditLeagueYear(LeagueYear leagueYear)
        {
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(leagueYear.League, leagueYear.Year, leagueYear.Options, leagueYear.PlayStatus);
            var tagEntities = leagueYear.Options.LeagueTags.Select(x => new LeagueYearTagEntity(leagueYear.League, leagueYear.Year, x));

            List<SpecialGameSlotEntity> slotEntities = leagueYear.Options.SpecialGameSlots.SelectMany(slot => slot.Tags, (slot, tag) =>
                new SpecialGameSlotEntity(Guid.NewGuid(), leagueYear.League, leagueYear.Year, slot.SpecialSlotPosition, tag)).ToList();

            string editLeagueYearSQL =
                "update tbl_league_year SET StandardGames = @StandardGames, GamesToDraft = @GamesToDraft, CounterPicks = @CounterPicks, CounterPicksToDraft = @CounterPicksToDraft, " +
                "FreeDroppableGames = @FreeDroppableGames, WillNotReleaseDroppableGames = @WillNotReleaseDroppableGames, WillReleaseDroppableGames = @WillReleaseDroppableGames, " +
                "DropOnlyDraftGames = @DropOnlyDraftGames, CounterPicksBlockDrops = @CounterPicksBlockDrops, MinimumBidAmount = @MinimumBidAmount, DraftSystem = @DraftSystem, " +
                "PickupSystem = @PickupSystem, ScoringSystem = @ScoringSystem, TradingSystem = @TradingSystem WHERE LeagueID = @LeagueID and Year = @Year";

            var deleteTagsSQL = "delete from tbl_league_yearusestag where LeagueID = @leagueID AND Year = @year;";
            var deleteSpecialSlotsSQL = "delete from tbl_league_specialgameslot where LeagueID = @leagueID AND Year = @year;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(editLeagueYearSQL, leagueYearEntity, transaction);
                    await connection.ExecuteAsync(deleteTagsSQL, new { leagueID = leagueYear.League.LeagueID, year = leagueYear.Year }, transaction);
                    await connection.ExecuteAsync(deleteSpecialSlotsSQL, new { leagueID = leagueYear.League.LeagueID, year = leagueYear.Year }, transaction);
                    await connection.BulkInsertAsync<LeagueYearTagEntity>(tagEntities, "tbl_league_yearusestag", 500, transaction);
                    await connection.BulkInsertAsync<SpecialGameSlotEntity>(slotEntities, "tbl_league_specialgameslot", 500, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task AddNewLeagueYear(League league, int year, LeagueOptions options)
        {
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, year, options, PlayStatus.NotStartedDraft);
            var tagEntities = options.LeagueTags.Select(x => new LeagueYearTagEntity(league, year, x));

            string newLeagueYearSQL =
                "insert into tbl_league_year(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,CounterPicksToDraft,FreeDroppableGames,WillNotReleaseDroppableGames,WillReleaseDroppableGames,DropOnlyDraftGames," +
                "DraftSystem,PickupSystem,ScoringSystem,TradingSystem,PlayStatus) VALUES " +
                "(@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@CounterPicksToDraft,@FreeDroppableGames,@WillNotReleaseDroppableGames,@WillReleaseDroppableGames,@DropOnlyDraftGames," +
                "@DraftSystem,@PickupSystem,@ScoringSystem,@TradingSystem,@PlayStatus);";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(newLeagueYearSQL, leagueYearEntity, transaction);
                    await connection.BulkInsertAsync<LeagueYearTagEntity>(tagEntities, "tbl_league_yearusestag", 500, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league)
        {
            var query = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<FantasyCriticUserEntity>(
                    "select tbl_user.* from tbl_user join tbl_league_hasuser on (tbl_user.UserID = tbl_league_hasuser.UserID) where tbl_league_hasuser.LeagueID = @leagueID;",
                    query);

                var users = results.Select(x => x.ToDomain()).ToList();
                return users;
            }
        }

        public async Task<IReadOnlyList<FantasyCriticUser>> GetActivePlayersForLeagueYear(League league, int year)
        {
            var query = new
            {
                leagueID = league.LeagueID,
                year
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<FantasyCriticUserEntity>(
                    "select tbl_user.* from tbl_user join tbl_league_activeplayer on (tbl_user.UserID = tbl_league_activeplayer.UserID) " +
                    "where tbl_league_activeplayer.LeagueID = @leagueID AND year = @year;",
                    query);

                var users = results.Select(x => x.ToDomain()).ToList();
                return users;
            }
        }

        public async Task SetPlayersActive(League league, int year, IReadOnlyList<FantasyCriticUser> mostRecentActivePlayers)
        {
            string insertSQL = "insert into tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@year,@userID);";
            var insertObjects = mostRecentActivePlayers.Select(x => new
            {
                leagueID = league.LeagueID,
                userID = x.Id,
                year
            });

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(insertSQL, insertObjects);
            }
        }

        public async Task SetPlayerActiveStatus(LeagueYear leagueYear, Dictionary<FantasyCriticUser, bool> usersToChange)
        {
            string deleteActiveUserSQL = "delete from tbl_league_activeplayer where LeagueID = @leagueID and Year = @year and UserID = @userID;";
            string insertSQL = "insert into tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@year,@userID);";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    foreach (var userToChange in usersToChange)
                    {
                        var paramsObject = new
                        {
                            leagueID = leagueYear.League.LeagueID,
                            userID = userToChange.Key.Id,
                            leagueYear.Year
                        };

                        if (userToChange.Value)
                        {
                            await connection.ExecuteAsync(insertSQL, paramsObject, transaction);
                        }
                        else
                        {
                            await connection.ExecuteAsync(deleteActiveUserSQL, paramsObject, transaction);
                        }
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        public async Task<IReadOnlyList<FantasyCriticUser>> GetLeagueFollowers(League league)
        {
            var query = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<FantasyCriticUserEntity>(
                    "select tbl_user.* from tbl_user join tbl_user_followingleague on (tbl_user.UserID = tbl_user_followingleague.UserID) where tbl_user_followingleague.LeagueID = @leagueID;",
                    query);

                var users = results.Select(x => x.ToDomain()).ToList();
                return users;
            }
        }

        public async Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser user)
        {
            IEnumerable<LeagueEntity> leagueEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    userID = user.Id,
                };

                var sql = "select vw_league.*, tbl_league_hasuser.Archived from vw_league join tbl_league_hasuser on (vw_league.LeagueID = tbl_league_hasuser.LeagueID) where tbl_league_hasuser.UserID = @userID and IsDeleted = 0;";
                leagueEntities = await connection.QueryAsync<LeagueEntity>(sql, queryObject);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
        }

        public async Task<IReadOnlyList<LeagueYear>> GetLeagueYearsForUser(FantasyCriticUser user, int year)
        {
            var allLeagueTags = await GetLeagueYearTagEntities(year);
            var allSpecialGameSlots = await GetSpecialGameSlotEntities(year);
            var leagueTagsByLeague = allLeagueTags.ToLookup(x => x.LeagueID);
            var tagDictionary = await _masterGameRepo.GetMasterGameTagDictionary();
            var domainSpecialGameSlots = ConvertSpecialGameSlotEntities(allSpecialGameSlots, tagDictionary);
            var allEligibilityOverrides = await GetAllEligibilityOverrides(year);
            var allTagOverrides = await GetAllTagOverrides(year);
            var supportedYear = await GetSupportedYear(year);

            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    year,
                    userID = user.Id
                };

                string sql = "select tbl_league_year.* from tbl_league_year " +
                             "join tbl_league_hasuser on (tbl_league_hasuser.LeagueID = tbl_league_year.LeagueID) " +
                             "where tbl_league_year.Year = @year and tbl_league_hasuser.UserID = @userID";
                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>(sql, queryObject);
                List<LeagueYear> leagueYears = new List<LeagueYear>();
                IReadOnlyList<League> leagues = await GetLeaguesForUser(user);
                Dictionary<Guid, League> leaguesDictionary = leagues.ToDictionary(x => x.LeagueID, y => y);

                foreach (var entity in yearEntities)
                {
                    var success = leaguesDictionary.TryGetValue(entity.LeagueID, out League league);
                    if (!success)
                    {
                        _logger.Warn($"Cannot find league (probably deleted) LeagueID: {entity.LeagueID}");
                        continue;
                    }

                    bool hasEligibilityOverrides = allEligibilityOverrides.TryGetValue(entity.LeagueID, out var eligibilityOverrides);
                    if (!hasEligibilityOverrides)
                    {
                        eligibilityOverrides = new List<EligibilityOverride>();
                    }

                    bool hasTagOverrides = allTagOverrides.TryGetValue(entity.LeagueID, out var tagOverrides);
                    if (!hasTagOverrides)
                    {
                        tagOverrides = new List<TagOverride>();
                    }

                    var domainLeagueTags = ConvertLeagueTagEntities(leagueTagsByLeague[entity.LeagueID], tagDictionary);
                    bool hasSpecialRosterSlots = domainSpecialGameSlots.TryGetValue(new LeagueYearKey(entity.LeagueID, entity.Year), out var specialGameSlotsForLeagueYear);
                    if (!hasSpecialRosterSlots)
                    {
                        specialGameSlotsForLeagueYear = new List<SpecialGameSlot>();
                    }

                    LeagueYear leagueYear = entity.ToDomain(league, supportedYear, eligibilityOverrides, tagOverrides, domainLeagueTags, specialGameSlotsForLeagueYear);
                    leagueYears.Add(leagueYear);
                }

                return leagueYears;
            }
        }

        public async Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser currentUser)
        {
            var query = new
            {
                userID = currentUser.Id
            };

            IEnumerable<LeagueEntity> leagueEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var sql = "select vw_league.* from vw_league join tbl_user_followingleague on (vw_league.LeagueID = tbl_user_followingleague.LeagueID) " +
                          "where tbl_user_followingleague.UserID = @userID and IsDeleted = 0;";
                leagueEntities = await connection.QueryAsync<LeagueEntity>(
                    sql,
                    query);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
        }

        public async Task FollowLeague(League league, FantasyCriticUser user)
        {
            var userAddObject = new
            {
                leagueID = league.LeagueID,
                userID = user.Id
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_user_followingleague(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject);
            }
        }

        public async Task UnfollowLeague(League league, FantasyCriticUser user)
        {
            var deleteObject = new
            {
                leagueID = league.LeagueID,
                userID = user.Id
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tbl_user_followingleague where LeagueID = @leagueID and UserID = @userID;",
                    deleteObject);
            }
        }

        public async Task<Maybe<LeagueInvite>> GetInvite(Guid inviteID)
        {
            var query = new
            {
                inviteID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var entity = await connection.QuerySingleOrDefaultAsync<LeagueInviteEntity>(
                    "select * from tbl_league_invite where tbl_league_invite.InviteID = @inviteID",
                    query);
                return await ConvertLeagueInviteEntity(entity);
            }
        }

        public async Task<IReadOnlyList<LeagueInvite>> GetLeagueInvites(FantasyCriticUser currentUser)
        {
            var query = new
            {
                email = currentUser.Email,
                userID = currentUser.Id
            };

            IEnumerable<LeagueInviteEntity> inviteEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                inviteEntities = await connection.QueryAsync<LeagueInviteEntity>(
                    "select * from tbl_league_invite where tbl_league_invite.EmailAddress = @email OR tbl_league_invite.UserID = @userID;",
                    query);
            }

            var leagueInvites = await ConvertLeagueInviteEntities(inviteEntities);
            return leagueInvites;
        }

        public async Task SaveInvite(LeagueInvite leagueInvite)
        {
            var entity = new LeagueInviteEntity(leagueInvite);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_invite(InviteID,LeagueID,EmailAddress,UserID) VALUES (@inviteID, @leagueID, @emailAddress, @userID);",
                    entity);
            }
        }

        public async Task<IReadOnlyList<LeagueInvite>> GetOutstandingInvitees(League league)
        {
            var query = new
            {
                leagueID = league.LeagueID
            };

            IEnumerable<LeagueInviteEntity> invites;
            using (var connection = new MySqlConnection(_connectionString))
            {
                invites = await connection.QueryAsync<LeagueInviteEntity>(
                    "select * from tbl_league_invite where tbl_league_invite.LeagueID = @leagueID;",
                    query);
            }

            var leagueInvites = await ConvertLeagueInviteEntities(invites);
            return leagueInvites;
        }

        public async Task AcceptInvite(LeagueInvite leagueInvite, FantasyCriticUser user)
        {
            await AddPlayerToLeague(leagueInvite.League, user);
            await DeleteInvite(leagueInvite);
        }

        public async Task DeleteInvite(LeagueInvite invite)
        {
            var deleteObject = new
            {
                inviteID = invite.InviteID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tbl_league_invite where InviteID = @inviteID;",
                    deleteObject);
            }
        }

        public async Task SaveInviteLink(LeagueInviteLink inviteLink)
        {
            LeagueInviteLinkEntity entity = new LeagueInviteLinkEntity(inviteLink);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_invitelink(InviteID,LeagueID,InviteCode,Active) VALUES " +
                    "(@InviteID,@LeagueID,@InviteCode,@Active);",
                    entity);
            }
        }

        public async Task DeactivateInviteLink(LeagueInviteLink inviteLink)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_league_invitelink SET Active = 0 where InviteID = @inviteID;", new {inviteID =  inviteLink.InviteID});
            }
        }

        public async Task<IReadOnlyList<LeagueInviteLink>> GetInviteLinks(League league)
        {
            var query = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<LeagueInviteLinkEntity>("select * from tbl_league_invitelink where LeagueID = @leagueID;", query);

                var inviteLinks = results.Select(x => x.ToDomain(league)).ToList();
                return inviteLinks;
            }
        }

        public async Task<Maybe<LeagueInviteLink>> GetInviteLinkByInviteCode(Guid inviteCode)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var result = await connection.QuerySingleOrDefaultAsync<LeagueInviteLinkEntity>("select * from tbl_league_invitelink where InviteCode = @inviteCode and Active = 1;", new {inviteCode});

                if (result is null)
                {
                    return Maybe<LeagueInviteLink>.None;
                }

                var league = await GetLeagueByID(result.LeagueID);
                return result.ToDomain(league.Value);
            }
        }

        public async Task SetArchiveStatusForUser(League league, bool archive, FantasyCriticUser user)
        {
            string updateSQL = "update tbl_league_hasuser SET Archived = @archive WHERE LeagueID = @leagueID AND UserID = @userID;";
            var parameters = new { leagueID = league.LeagueID, userID = user.Id, archive };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(updateSQL, parameters);
            }
        }

        public async Task FullyRemovePublisher(Publisher deletePublisher, IEnumerable<Publisher> publishersInLeague)
        {
            string deleteSQL = "delete from tbl_league_publisher where PublisherID = @publisherID;";
            string deleteQueueSQL = "delete from tbl_league_publisherqueue where PublisherID = @publisherID;";
            string deleteHistorySQL = "delete from tbl_league_action where PublisherID = @publisherID;";
            string deletePublisherGameSQL = "delete from tbl_league_publishergame WHERE PublisherID = @publisherID;";
            string deletePublisherBidsSQL = "delete from tbl_league_pickupbid WHERE PublisherID = @publisherID;";
            string deletePublisherDropsSQL = "delete from tbl_league_droprequest WHERE PublisherID = @publisherID;";
            string fixDraftOrderSQL = "update tbl_league_publisher SET DraftPosition = @draftPosition where PublisherID = @publisherID;";

            var remainingOrderedPublishers = publishersInLeague.Except(new List<Publisher>{ deletePublisher }).OrderBy(x => x.DraftPosition).ToList();
            IEnumerable<SetDraftOrderEntity> setDraftOrderEntities = remainingOrderedPublishers.Select((publisher, index) => new SetDraftOrderEntity(publisher.PublisherID, index + 1));

            var deleteObject = new
            {
                publisherID = deletePublisher.PublisherID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(deleteQueueSQL, deleteObject, transaction);
                    await connection.ExecuteAsync(deleteHistorySQL, deleteObject, transaction);
                    await connection.ExecuteAsync(deletePublisherGameSQL, deleteObject, transaction);
                    await connection.ExecuteAsync(deletePublisherBidsSQL, deleteObject, transaction);
                    await connection.ExecuteAsync(deletePublisherDropsSQL, deleteObject, transaction);
                    await connection.ExecuteAsync(deleteSQL, deleteObject, transaction);
                    await connection.ExecuteAsync(fixDraftOrderSQL, setDraftOrderEntities);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
        {
            string deleteUserSQL = "delete from tbl_league_hasuser where LeagueID = @leagueID and UserID = @userID;";
            string deleteActiveUserSQL = "delete from tbl_league_activeplayer where LeagueID = @leagueID and UserID = @userID;";

            var userDeleteObject = new
            {
                leagueID = league.LeagueID,
                userID = removeUser.Id
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(deleteActiveUserSQL, userDeleteObject, transaction);
                    await connection.ExecuteAsync(deleteUserSQL, userDeleteObject, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task TransferLeagueManager(League league, FantasyCriticUser newManager)
        {
            string sql = "UPDATE tbl_league SET LeagueManager = @newManagerUserID WHERE LeagueID = @leagueID;";

            var transferObject = new
            {
                leagueID = league.LeagueID,
                newManagerUserID = newManager.Id
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, transferObject);
            }
        }

        public async Task CreatePublisher(Publisher publisher)
        {
            var entity = new PublisherEntity(publisher);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_publisher(PublisherID,PublisherName,LeagueID,Year,UserID,DraftPosition,Budget,FreeGamesDropped,WillNotReleaseGamesDropped,WillReleaseGamesDropped) VALUES " +
                    "(@PublisherID,@PublisherName,@LeagueID,@Year,@UserID,@DraftPosition,@Budget,@FreeGamesDropped,@WillNotReleaseGamesDropped,@WillReleaseGamesDropped);",
                    entity);
            }
        }

        public async Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear)
        {
            var usersInLeague = await GetUsersInLeague(leagueYear.League);
            return await GetPublishersInLeagueForYear(leagueYear, usersInLeague);
        }

        public async Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> usersInLeague)
        {
            var query = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year
            };

            IEnumerable<PublisherEntity> publisherEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                publisherEntities = await connection.QueryAsync<PublisherEntity>(
                    "select * from tbl_league_publisher where tbl_league_publisher.LeagueID = @leagueID and tbl_league_publisher.Year = @year;",
                    query);
            }

            var publisherIDs = publisherEntities.Select(x => x.PublisherID);
            IReadOnlyList<PublisherGame> domainGames = await GetPublisherGamesInLeague(publisherIDs, leagueYear.Year);

            List<Publisher> domainPublishers = new List<Publisher>();
            foreach (var entity in publisherEntities)
            {
                var gamesForPublisher = domainGames.Where(x => x.PublisherID == entity.PublisherID);
                var user = usersInLeague.Single(x => x.Id == entity.UserID);
                var domainPublisher = entity.ToDomain(leagueYear, user, gamesForPublisher);
                domainPublishers.Add(domainPublisher);
            }

            return domainPublishers;
        }

        public async Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year, IReadOnlyList<LeagueYear> allLeagueYears, bool includeDeleted = false)
        {
            var query = new
            {
                year
            };

            var allUsers = await _userStore.GetAllUsers();
            var usersDictionary = allUsers.ToDictionary(x => x.Id, y => y);
            var leaguesDictionary = allLeagueYears.ToDictionary(x => x.League.LeagueID, y => y);

            string sql = "select tbl_league_publisher.* from tbl_league_publisher " +
                         "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                         "where tbl_league_publisher.Year = @year and tbl_league.IsDeleted = 0;";

            if (includeDeleted)
            {
                sql = "select tbl_league_publisher.* from tbl_league_publisher " +
                      "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                      "where tbl_league_publisher.Year = @year;";
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                var publisherEntities = await connection.QueryAsync<PublisherEntity>(sql, query);
                IReadOnlyList<PublisherGame> allDomainGames = await GetAllPublisherGamesForYear(year, includeDeleted);
                Dictionary<Guid, List<PublisherGame>> domainGamesDictionary = publisherEntities.ToDictionary(x => x.PublisherID, y => new List<PublisherGame>());
                foreach (var game in allDomainGames)
                {
                    domainGamesDictionary[game.PublisherID].Add(game);
                }

                List<Publisher> publishers = new List<Publisher>();
                foreach (var entity in publisherEntities)
                {
                    var user = usersDictionary[entity.UserID];
                    var leagueYear = leaguesDictionary[entity.LeagueID];
                    var domainGames = domainGamesDictionary[entity.PublisherID];
                    var domainPublisher = entity.ToDomain(leagueYear, user, domainGames);
                    publishers.Add(domainPublisher);
                }

                return publishers;
            }
        }

        private async Task<IReadOnlyList<Publisher>> GetPublishers(IEnumerable<Guid> publisherIDs)
        {
            var query = new
            {
                publisherIDs
            };

            var allUsers = await _userStore.GetAllUsers();
            var usersDictionary = allUsers.ToDictionary(x => x.Id, y => y);
            
            string sql = "select tbl_league_publisher.* from tbl_league_publisher " +
                         "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                         "where tbl_league_publisher.PublisherID = @publisherIDs and tbl_league.IsDeleted = 0;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var allPublisherEntities = await connection.QueryAsync<PublisherEntity>(sql, query);
                List<Publisher> publishers = new List<Publisher>();
                var publisherEntitiesByYear = allPublisherEntities.GroupBy(x => x.Year);
                foreach (var publisherYearGroup in publisherEntitiesByYear)
                {
                    IReadOnlyList<LeagueYear> allLeagueYears = await GetLeagueYears(publisherYearGroup.Key);
                    var leaguesDictionary = allLeagueYears.ToDictionary(x => x.League.LeagueID, y => y);
                    IReadOnlyList<PublisherGame> allDomainGames = await GetAllPublisherGamesForYear(publisherYearGroup.Key);
                    Dictionary<Guid, List<PublisherGame>> domainGamesDictionary = publisherYearGroup.ToDictionary(x => x.PublisherID, y => new List<PublisherGame>());
                    foreach (var game in allDomainGames)
                    {
                        if (!domainGamesDictionary.ContainsKey(game.PublisherID))
                        {
                            domainGamesDictionary[game.PublisherID] = new List<PublisherGame>();
                        }
                        domainGamesDictionary[game.PublisherID].Add(game);
                    }

                    foreach (var entity in publisherYearGroup)
                    {
                        var user = usersDictionary[entity.UserID];
                        var leagueYear = leaguesDictionary[entity.LeagueID];
                        var domainGames = domainGamesDictionary[entity.PublisherID];
                        var domainPublisher = entity.ToDomain(leagueYear, user, domainGames);
                        publishers.Add(domainPublisher);
                    }
                }

                return publishers;
            }
        }

        private async Task<IReadOnlyList<PublisherGame>> GetAllPublisherGamesForYear(int year, bool includeDeleted = false)
        {
            var query = new
            {
                year
            };

            string sql = "select tbl_league_publishergame.* from tbl_league_publishergame " +
                    "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                    "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                    "where tbl_league_publisher.Year = @year and IsDeleted = 0;";

            if (includeDeleted)
            {
                sql = "select tbl_league_publishergame.* from tbl_league_publishergame " +
                      "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                      "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                      "where tbl_league_publisher.Year = @year;";
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<PublisherGameEntity> gameEntities = await connection.QueryAsync<PublisherGameEntity>(sql, query);

                List<PublisherGame> domainGames = new List<PublisherGame>();
                foreach (var entity in gameEntities)
                {
                    Maybe<MasterGameYear> masterGame = null;
                    if (entity.MasterGameID.HasValue)
                    {
                        masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
                    }

                    domainGames.Add(entity.ToDomain(masterGame));
                }

                return domainGames;
            }
        }

        public async Task<Maybe<Publisher>> GetPublisher(Guid publisherID)
        {
            var query = new
            {
                publisherID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                PublisherEntity publisherEntity = await connection.QuerySingleOrDefaultAsync<PublisherEntity>(
                    "select tbl_league_publisher.* from tbl_league_publisher " +
                    "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                    "where tbl_league_publisher.PublisherID = @publisherID and IsDeleted = 0;",
                    query);

                if (publisherEntity == null)
                {
                    return Maybe<Publisher>.None;
                }

                IReadOnlyList<PublisherGame> domainGames = await GetPublisherGames(publisherEntity.PublisherID, publisherEntity.Year);
                var user = await _userStore.FindByIdAsync(publisherEntity.UserID.ToString(), CancellationToken.None);
                var league = await GetLeagueByID(publisherEntity.LeagueID);
                var leagueYear = await GetLeagueYear(league.Value, publisherEntity.Year);

                var domainPublisher = publisherEntity.ToDomain(leagueYear.Value, user, domainGames);
                return domainPublisher;
            }
        }

        public async Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherGameID)
        {
            var query = new
            {
                publisherGameID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                PublisherGameEntity gameEntity = await connection.QueryFirstOrDefaultAsync<PublisherGameEntity>(
                    "select tbl_league_publishergame.* from tbl_league_publishergame " +
                    "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                    "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                    "where tbl_league_publishergame.PublisherGameID = @publisherGameID and IsDeleted = 0;",
                    query);

                if (gameEntity is null)
                {
                    return Maybe<PublisherGame>.None;
                }

                var publisher = await GetPublisher(gameEntity.PublisherID);
                if (publisher.HasNoValue)
                {
                    throw new Exception($"Publisher cannot be found: {gameEntity.PublisherID}");
                }

                Maybe<MasterGameYear> masterGame = null;
                if (gameEntity.MasterGameID.HasValue)
                {
                    masterGame = await _masterGameRepo.GetMasterGameYear(gameEntity.MasterGameID.Value, publisher.Value.LeagueYear.Year);
                }

                PublisherGame publisherGame = gameEntity.ToDomain(masterGame);
                return publisherGame;
            }
        }

        public async Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherID, Guid masterGameID)
        {
            var query = new
            {
                publisherID,
                masterGameID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                PublisherGameEntity gameEntity = await connection.QueryFirstOrDefaultAsync<PublisherGameEntity>(
                    "select tbl_league_publishergame.* from tbl_league_publishergame " +
                    "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                    "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                    "where tbl_league_publisher.PublisherID = @publisherID and tbl_league_publishergame.MasterGameID = @masterGameID and IsDeleted = 0;",
                    query);

                if (gameEntity is null)
                {
                    return Maybe<PublisherGame>.None;
                }

                var publisher = await GetPublisher(gameEntity.PublisherID);
                if (publisher.HasNoValue)
                {
                    throw new Exception($"Publisher cannot be found: {gameEntity.PublisherID}");
                }

                Maybe<MasterGameYear> masterGame = null;
                if (gameEntity.MasterGameID.HasValue)
                {
                    masterGame = await _masterGameRepo.GetMasterGameYear(gameEntity.MasterGameID.Value, publisher.Value.LeagueYear.Year);
                }

                PublisherGame publisherGame = gameEntity.ToDomain(masterGame);
                return publisherGame;
            }
        }

        public async Task<Maybe<Publisher>> GetPublisher(LeagueYear leagueYear, FantasyCriticUser user)
        {
            var query = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year,
                userID = user.Id
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                PublisherEntity publisherEntity = await connection.QuerySingleOrDefaultAsync<PublisherEntity>(
                    "select * from tbl_league_publisher where tbl_league_publisher.LeagueID = @leagueID and tbl_league_publisher.Year = @year and tbl_league_publisher.UserID = @userID;",
                    query);

                if (publisherEntity == null)
                {
                    return Maybe<Publisher>.None;
                }

                IReadOnlyList<PublisherGame> domainGames = await GetPublisherGames(publisherEntity.PublisherID, publisherEntity.Year);
                var domainPublisher = publisherEntity.ToDomain(leagueYear, user, domainGames);
                return domainPublisher;
            }
        }

        public async Task AddPublisherGame(PublisherGame publisherGame)
        {
            PublisherGameEntity entity = new PublisherGameEntity(publisherGame);

            string sql =
                "insert into tbl_league_publishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore," +
                "ManualWillNotRelease,FantasyPoints,MasterGameID,SlotNumber,DraftPosition,OverallDraftPosition) VALUES " +
                "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore," +
                "@ManualWillNotRelease,@FantasyPoints,@MasterGameID,@SlotNumber,@DraftPosition,@OverallDraftPosition);";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, entity);
            }
        }

        private Task MakePublisherGameSlotsConsistent(Guid publisherID, Guid removedPublisherGameID, MySqlConnection connection, MySqlTransaction transaction)
        {
            return MakePublisherGameSlotsConsistent(new List<Guid>() { publisherID }, new List<Guid>() { removedPublisherGameID }, connection, transaction);
        }

        private async Task MakePublisherGameSlotsConsistent(IEnumerable<Guid> publisherIDs, IEnumerable<Guid> removedPublisherGameIDs, 
            MySqlConnection connection, MySqlTransaction transaction)
        {
            var removedPublisherGameIDsHashSet = removedPublisherGameIDs.ToHashSet();
            var publishers = await GetPublishers(publisherIDs);
            var specialSlotPublisherIDs = publishers.Where(x => x.LeagueYear.Options.HasSpecialSlots())
                .Select(x => x.PublisherID).ToHashSet();

            var updateEntities = new List<PublisherGameSlotNumberUpdateEntity>();
            foreach (var publisher in publishers)
            {
                int slotNumber = 0;
                if (!specialSlotPublisherIDs.Contains(publisher.PublisherID))
                {
                    var standardGames = publisher.PublisherGames
                        .Where(x => !x.CounterPick)
                        .Where(x => !removedPublisherGameIDsHashSet.Contains(x.PublisherGameID))
                        .OrderBy(x => x.SlotNumber);
                    foreach (var standardGame in standardGames)
                    {
                        updateEntities.Add(new PublisherGameSlotNumberUpdateEntity(standardGame.PublisherGameID, slotNumber));
                        slotNumber++;
                    }
                }
                
                //Counter Picks don't have special slots, so they should always be consistent.
                slotNumber = 0;
                var counterPicks = publisher.PublisherGames
                    .Where(x => x.CounterPick)
                    .Where(x => !removedPublisherGameIDsHashSet.Contains(x.PublisherGameID))
                    .OrderBy(x => x.SlotNumber);
                foreach (var counterPick in counterPicks)
                {
                    updateEntities.Add(new PublisherGameSlotNumberUpdateEntity(counterPick.PublisherGameID, slotNumber));
                    slotNumber++;
                }
            }
            
            string sql = "UPDATE tbl_league_publishergame SET SlotNumber = @SlotNumber WHERE PublisherGameID = @PublisherGameID;";
            await connection.ExecuteAsync(sql, updateEntities, transaction);
        }

        public async Task ReorderPublisherGames(Publisher publisher, Dictionary<int, Guid?> slotStates)
        {
            var updateEntities = new List<PublisherGameSlotNumberUpdateEntity>();
            foreach (var slotState in slotStates)
            {
                updateEntities.Add(new PublisherGameSlotNumberUpdateEntity(slotState.Value, slotState.Key));
            }

            string sql = "UPDATE tbl_league_publishergame SET SlotNumber = @SlotNumber WHERE PublisherGameID = @PublisherGameID;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, updateEntities);
            }
        }

        public async Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_league_publishergame set MasterGameID = @masterGameID where PublisherGameID = @publisherGameID",
                    new
                    {
                        masterGameID = masterGame.MasterGameID,
                        publisherGameID = publisherGame.PublisherGameID
                    });
            }
        }

        public async Task MergeMasterGame(MasterGame removeMasterGame, MasterGame mergeIntoMasterGame)
        {
            string mergeSQL =
                "UPDATE tbl_league_droprequest SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_league_eligibilityoverride SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_league_pickupbid SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_league_publishergame SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_league_publisherqueue SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_mastergame_changerequest SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_mastergame_request SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_mastergame_subgame SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;" +
                "UPDATE tbl_royale_publishergame SET MasterGameID = @mergeIntoMasterGameID WHERE MasterGameID = @removeMasterGameID;";

            string removeGameSQL = "DELETE FROM tbl_mastergame WHERE MasterGameID = @removeMasterGameID;";
            string removeTagsSQL = "DELETE FROM tbl_mastergame_hastag WHERE MasterGameID = @removeMasterGameID;";

            var requestObject = new
            {
                removeMasterGameID = removeMasterGame.MasterGameID,
                mergeIntoMasterGameID = mergeIntoMasterGame.MasterGameID,
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(mergeSQL, requestObject, transaction);
                    await connection.ExecuteAsync(removeTagsSQL, requestObject, transaction);
                    await connection.ExecuteAsync(removeGameSQL, requestObject, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<SupportedYearEntity>("select * from tbl_meta_supportedyear;");
                return results.Select(x => x.ToDomain()).ToList();
            }
        }

        public async Task<SupportedYear> GetSupportedYear(int year)
        {
            var supportedYears = await GetSupportedYears();
            var supportedYear = supportedYears.Single(x => x.Year == year);
            return supportedYear;
        }

        private async Task<IReadOnlyList<League>> ConvertLeagueEntitiesToDomain(IEnumerable<LeagueEntity> leagueEntities)
        {
            var relevantUserIDs = leagueEntities.Select(x => x.LeagueManager).Distinct();
            var relevantUsers = await _userStore.GetUsers(relevantUserIDs);
            var userDictionary = relevantUsers.ToDictionary(x => x.Id);

            string sql = "select * from tbl_league_year where LeagueID in @leagueIDs";
            var queryObject = new
            {
                leagueIDs = leagueEntities.Select(x => x.LeagueID)
            };
            using (var connection = new MySqlConnection(_connectionString))
            {
                List<League> leagues = new List<League>();
                IEnumerable<LeagueYearEntity> allLeagueYears = await connection.QueryAsync<LeagueYearEntity>(sql, queryObject);
                var leagueYearLookup = allLeagueYears.ToLookup(x => x.LeagueID);

                foreach (var leagueEntity in leagueEntities)
                {
                    FantasyCriticUser manager = userDictionary[leagueEntity.LeagueManager];
                    IEnumerable<int> years = leagueYearLookup[leagueEntity.LeagueID].Select(x => x.Year);
                    League league = leagueEntity.ToDomain(manager, years);
                    leagues.Add(league);
                }

                return leagues;
            }
        }

        public async Task<IReadOnlyList<PublisherGame>> GetPublisherGames(Guid publisherID, int leagueYear)
        {
            var query = new
            {
                publisherID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<PublisherGameEntity> gameEntities = await connection.QueryAsync<PublisherGameEntity>(
                    "select tbl_league_publishergame.* from tbl_league_publishergame " +
                    "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                    "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                    "where tbl_league_publishergame.PublisherID = @publisherID and IsDeleted = 0;",
                    query);

                List<PublisherGame> domainGames = new List<PublisherGame>();
                foreach (var entity in gameEntities)
                {
                    Maybe<MasterGameYear> masterGame = null;
                    if (entity.MasterGameID.HasValue)
                    {
                        masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, leagueYear);
                    }

                    domainGames.Add(entity.ToDomain(masterGame));
                }

                return domainGames;
            }
        }

        private async Task<IReadOnlyList<PublisherGame>> GetPublisherGamesInLeague(IEnumerable<Guid> publisherIDs, int year)
        {
            var query = new
            {
                publisherIDs
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<PublisherGameEntity> gameEntities = await connection.QueryAsync<PublisherGameEntity>(
                    "select tbl_league_publishergame.* from tbl_league_publishergame " +
                    "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                    "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                    "where tbl_league_publishergame.PublisherID in @publisherIDs and IsDeleted = 0;",
                    query);

                List<PublisherGame> domainGames = new List<PublisherGame>();
                foreach (var entity in gameEntities)
                {
                    Maybe<MasterGameYear> masterGame = null;
                    if (entity.MasterGameID.HasValue)
                    {
                        masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
                    }

                    domainGames.Add(entity.ToDomain(masterGame));
                }

                return domainGames;
            }
        }

        public async Task SetDraftOrder(IReadOnlyList<KeyValuePair<Publisher, int>> draftPositions)
        {
            int tempPosition = draftPositions.Select(x => x.Value).Max() + 1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    foreach (var draftPosition in draftPositions)
                    {
                        await connection.ExecuteAsync(
                            "update tbl_league_publisher set DraftPosition = @draftPosition where PublisherID = @publisherID",
                            new
                            {
                                publisherID = draftPosition.Key.PublisherID,
                                draftPosition = tempPosition
                            }, transaction);
                        tempPosition++;
                    }

                    foreach (var draftPosition in draftPositions)
                    {
                        await connection.ExecuteAsync(
                            "update tbl_league_publisher set DraftPosition = @draftPosition where PublisherID = @publisherID",
                            new
                            {
                                publisherID = draftPosition.Key.PublisherID,
                                draftPosition = draftPosition.Value
                            }, transaction);
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        public async Task<IReadOnlyList<EligibilityOverride>> GetEligibilityOverrides(League league, int year)
        {
            string sql = "select * from tbl_league_eligibilityoverride where LeagueID = @leagueID and Year = @year;";
            var queryObject = new
            {
                leagueID = league.LeagueID,
                year
            };

            IEnumerable<EligibilityOverrideEntity> results;
            using (var connection = new MySqlConnection(_connectionString))
            {
                results = await connection.QueryAsync<EligibilityOverrideEntity>(sql, queryObject);
            }

            List<EligibilityOverride> domainObjects = new List<EligibilityOverride>();
            foreach (var result in results)
            {
                var masterGame = await _masterGameRepo.GetMasterGame(result.MasterGameID);
                if (masterGame.HasNoValue)
                {
                    throw new Exception($"Cannot find game {masterGame.Value.MasterGameID} for eligibility override. This should not happen.");
                }

                EligibilityOverride domain = result.ToDomain(masterGame.Value);
                domainObjects.Add(domain);
            }

            return domainObjects;
        }

        private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<EligibilityOverride>>> GetAllEligibilityOverrides(int year)
        {
            string sql = "select tbl_league_eligibilityoverride.* from tbl_league_eligibilityoverride " +
                         "join tbl_league on (tbl_league.LeagueID = tbl_league_eligibilityoverride.LeagueID) " +
                         "where Year = @year and IsDeleted = 0;";
            var queryObject = new
            {
                year
            };

            IEnumerable<EligibilityOverrideEntity> results;
            using (var connection = new MySqlConnection(_connectionString))
            {
                results = await connection.QueryAsync<EligibilityOverrideEntity>(sql, queryObject);
            }

            List<Tuple<Guid, EligibilityOverride>> domainObjects = new List<Tuple<Guid, EligibilityOverride>>();
            foreach (var result in results)
            {
                var masterGame = await _masterGameRepo.GetMasterGame(result.MasterGameID);
                if (masterGame.HasNoValue)
                {
                    throw new Exception($"Cannot find game {masterGame.Value.MasterGameID} for eligibility override. This should not happen.");
                }

                EligibilityOverride domain = result.ToDomain(masterGame.Value);
                domainObjects.Add(new Tuple<Guid, EligibilityOverride>(result.LeagueID, domain));
            }

            var dictionary = domainObjects
                .GroupBy(x => x.Item1)
                .ToDictionary(x => x.Key, y => (IReadOnlyList<EligibilityOverride>)y.Select(z => z.Item2).ToList());
            return dictionary;
        }

        private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<TagOverride>>> GetAllTagOverrides(int year)
        {
            string sql = "select tbl_league_tagoverride.* from tbl_league_tagoverride " +
                         "join tbl_league on (tbl_league.LeagueID = tbl_league_tagoverride.LeagueID) " +
                         "where Year = @year and IsDeleted = 0;";
            var queryObject = new
            {
                year
            };

            IEnumerable<TagOverrideEntity> results;
            using (var connection = new MySqlConnection(_connectionString))
            {
                results = await connection.QueryAsync<TagOverrideEntity>(sql, queryObject);
            }

            var allTags = await _masterGameRepo.GetMasterGameTags();
            var tagDictionary = allTags.ToDictionary(x => x.Name);

            List<Tuple<Guid, TagOverride>> domainObjects = new List<Tuple<Guid, TagOverride>>();

            var groupedResults = results.GroupBy(x => (x.LeagueID, x.Year, x.MasterGameID));
            foreach (var resultGroup in groupedResults)
            {
                var masterGame = await _masterGameRepo.GetMasterGame(resultGroup.Key.MasterGameID);
                if (masterGame.HasNoValue)
                {
                    throw new Exception($"Cannot find game {masterGame.Value.MasterGameID} for tag override. This should not happen.");
                }

                List<MasterGameTag> tagsForGroup = new List<MasterGameTag>();
                foreach (var result in resultGroup)
                {
                    var fullTag = tagDictionary[result.TagName];
                    tagsForGroup.Add(fullTag);
                }

                TagOverride domain = new TagOverride(masterGame.Value, tagsForGroup);
                domainObjects.Add(new Tuple<Guid, TagOverride>(resultGroup.Key.LeagueID, domain));
            }
            
            var dictionary = domainObjects.GroupBy(x => x.Item1).ToDictionary(x => x.Key, y => (IReadOnlyList<TagOverride>)y.Select(z => z.Item2).ToList());
            return dictionary;
        }

        public async Task DeleteEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await DeleteEligibilityOverride(leagueYear, masterGame, connection, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task SetEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, bool eligible)
        {
            string sql = "insert into tbl_league_eligibilityoverride(LeagueID,Year,MasterGameID,Eligible) VALUES " +
                         "(@leagueID,@year,@masterGameID,@eligible)";

            var insertObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year,
                masterGameID = masterGame.MasterGameID,
                eligible
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await DeleteEligibilityOverride(leagueYear, masterGame, connection, transaction);
                    await connection.ExecuteAsync(sql, insertObject, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task<IReadOnlyList<TagOverride>> GetTagOverrides(League league, int year)
        {
            string sql = "select tbl_league_tagoverride.* from tbl_league_tagoverride " +
                         "JOIN tbl_mastergame_tag ON tbl_league_tagoverride.TagName = tbl_mastergame_tag.Name " +
                         "WHERE LeagueID = @leagueID AND Year = @year;";
            var queryObject = new
            {
                leagueID = league.LeagueID,
                year
            };

            var allTags = await _masterGameRepo.GetMasterGameTags();
            var allMasterGames = await _masterGameRepo.GetMasterGames();
            var tagDictionary = allTags.ToDictionary(x => x.Name);
            var masterGameDictionary = allMasterGames.ToDictionary(x => x.MasterGameID);

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<TagOverrideEntity> entities = await connection.QueryAsync<TagOverrideEntity>(sql, queryObject);
                List<TagOverride> domains = new List<TagOverride>();
                var entitiesByMasterGameID = entities.GroupBy(x => x.MasterGameID);
                foreach (var entitySet in entitiesByMasterGameID)
                {
                    var masterGame = masterGameDictionary[entitySet.Key];
                    List<MasterGameTag> tagsForMasterGame = new List<MasterGameTag>();
                    foreach (var entity in entitySet)
                    {
                        var fullTag = tagDictionary[entity.TagName];
                        tagsForMasterGame.Add(fullTag);
                    }

                    domains.Add(new TagOverride(masterGame, tagsForMasterGame));
                }

                return domains;
            }
        }

        public async Task<IReadOnlyList<MasterGameTag>> GetTagOverridesForGame(League league, int year, MasterGame masterGame)
        {
            string sql = "select tbl_mastergame_tag.* from tbl_league_tagoverride " +
                         "JOIN tbl_mastergame_tag ON tbl_league_tagoverride.TagName = tbl_mastergame_tag.Name " +
                         "WHERE LeagueID = @leagueID AND Year = @year AND MasterGameID = @masterGameID;";
            var queryObject = new
            {
                leagueID = league.LeagueID,
                year,
                masterGameID = masterGame.MasterGameID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<MasterGameTagEntity> entities = await connection.QueryAsync<MasterGameTagEntity>(sql, queryObject);
                return entities.Select(x => x.ToDomain()).ToList();
            }
        }

        public async Task SetTagOverride(LeagueYear leagueYear, MasterGame masterGame, List<MasterGameTag> requestedTags)
        {
            string deleteSQL = "DELETE from tbl_league_tagoverride where LeagueID = @leagueID AND Year = @year AND MasterGameID = @masterGameID;";

            var deleteObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year,
                masterGameID = masterGame.MasterGameID
            };

            var insertEntities = requestedTags.Select(x => new TagOverrideEntity()
            {
                LeagueID = leagueYear.League.LeagueID,
                Year = leagueYear.Year,
                MasterGameID = masterGame.MasterGameID,
                TagName = x.Name
            });

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(deleteSQL, deleteObject, transaction);
                    if (insertEntities.Any())
                    {
                        await connection.BulkInsertAsync(insertEntities, "tbl_league_tagoverride", 500, transaction);
                    }
                    await transaction.CommitAsync();
                }
            }
        }

        private async Task DeleteEligibilityOverride(LeagueYear leagueYear, MasterGame masterGame, MySqlConnection connection, MySqlTransaction transaction)
        {
            string sql = "delete from tbl_league_eligibilityoverride where LeagueID = @leagueID and Year = @year and MasterGameID = @masterGameID;";
            var queryObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year,
                masterGameID = masterGame.MasterGameID
            };

            await connection.ExecuteAsync(sql, queryObject, transaction);
        }

        public async Task<SystemWideValues> GetSystemWideValues()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var result = await connection.QuerySingleAsync<SystemWideValuesEntity>("select * from tbl_caching_systemwidevalues;");
                return result.ToDomain();
            }
        }

        public async Task<SystemWideSettings> GetSystemWideSettings()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var result = await connection.QuerySingleAsync<SystemWideSettingsEntity>("select * from tbl_meta_systemwidesettings;");
                return result.ToDomain();
            }
        }

        public async Task<SiteCounts> GetSiteCounts()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var result = await connection.QuerySingleAsync<SiteCountsEntity>("select * from vw_meta_sitecounts;");
                return result.ToDomain();
            }
        }

        public async Task SetActionProcessingMode(bool modeOn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_meta_systemwidesettings set ActionProcessingMode = @modeOn;", new { modeOn });
            }
        }

        public async Task EditPublisher(EditPublisherRequest editValues, LeagueAction leagueAction)
        {
            string sql = "update tbl_league_publisher SET ";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("publisherID", editValues.Publisher.PublisherID);

            if (editValues.NewPublisherName.HasValue)
            {
                sql += "PublisherName = @publisherName ";
                parameters.Add("publisherName", editValues.NewPublisherName.Value);
            }
            if (editValues.Budget.HasValue)
            {
                sql += "Budget = @budget ";
                parameters.Add("budget", editValues.Budget.Value);
            }
            if (editValues.WillNotReleaseGamesDropped.HasValue)
            {
                sql += "WillNotReleaseGamesDropped = @willNotReleaseGamesDropped ";
                parameters.Add("willNotReleaseGamesDropped", editValues.WillNotReleaseGamesDropped.Value);
            }
            if (editValues.WillReleaseGamesDropped.HasValue)
            {
                sql += "WillReleaseGamesDropped = @willReleaseGamesDropped ";
                parameters.Add("willReleaseGamesDropped", editValues.WillReleaseGamesDropped.Value);
            }
            if (editValues.FreeGamesDropped.HasValue)
            {
                sql += "FreeGamesDropped = @freeGamesDropped ";
                parameters.Add("freeGamesDropped", editValues.FreeGamesDropped.Value);
            }
            sql += "WHERE PublisherID = @publisherID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(sql, parameters, transaction);
                    await AddLeagueActions(new[] {leagueAction}, connection, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task DeletePublisher(Publisher publisher)
        {
            var deleteObject = new
            {
                publisherID = publisher.PublisherID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tbl_league_publisher where PublisherID = @publisherID;",
                    deleteObject);
            }
        }

        public async Task DeleteLeagueYear(LeagueYear leagueYear)
        {
            var deleteObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tbl_league_year where LeagueID = @leagueID and Year = @year;",
                    deleteObject);
            }
        }

        public async Task DeleteLeague(League league)
        {
            var deleteObject = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("delete from tbl_league where LeagueID = @leagueID;", deleteObject);
            }
        }

        public async Task DeleteLeagueActions(Publisher publisher)
        {
            var deleteObject = new
            {
                publisherID = publisher.PublisherID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("delete from tbl_league_action where PublisherID = @publisherID;", deleteObject);
            }
        }

        public async Task<bool> LeagueHasBeenStarted(Guid leagueID)
        {
            var selectObject = new
            {
                leagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<bool>(
                    "select count(1) from vw_league " +
                    "join tbl_league_year on (vw_league.LeagueID = tbl_league_year.LeagueID) " +
                    "where PlayStatus <> 'NotStartedDraft' and vw_league.LeagueID = @leagueID and IsDeleted = 0;",
                    selectObject);
            }
        }

        public async Task SaveProcessedActionResults(ActionProcessingResults actionProcessingResults)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await MarkBidStatus(actionProcessingResults.SuccessBids, true, connection, transaction);
                    await MarkBidStatus(actionProcessingResults.FailedBids, false, connection, transaction);

                    await MarkDropStatus(actionProcessingResults.SuccessDrops, true, connection, transaction);
                    await MarkDropStatus(actionProcessingResults.FailedDrops, false, connection, transaction);

                    await AddLeagueActions(actionProcessingResults.LeagueActions, connection, transaction);
                    await UpdatePublisherBudgetsAndDroppedGames(actionProcessingResults.UpdatedPublishers, connection, transaction);

                    await DeletePublisherGames(actionProcessingResults.RemovedPublisherGames, connection, transaction);
                    await AddPublisherGames(actionProcessingResults.AddedPublisherGames, connection, transaction);

                    var allPublisherIDsToAdjust = actionProcessingResults.SuccessDrops.Select(x => x.Publisher.PublisherID);
                    var removedPublisherGameIDs = actionProcessingResults.RemovedPublisherGames.Select(x => x.PublisherGameID);
                    await MakePublisherGameSlotsConsistent(allPublisherIDsToAdjust, removedPublisherGameIDs, connection, transaction);

                    await transaction.CommitAsync();
                }
            }
        }

        public async Task UpdateSystemWideValues(SystemWideValues systemWideValues)
        {
            string deleteSQL = "delete from tbl_caching_systemwidevalues;";
            string insertSQL = "INSERT into tbl_caching_systemwidevalues VALUES (@AverageStandardGamePoints,@AverageCounterPickPoints);";

            SystemWideValuesEntity entity = new SystemWideValuesEntity(systemWideValues);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(deleteSQL, transaction: transaction);
                    await connection.ExecuteAsync(insertSQL, entity, transaction);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
        {
            int tempPosition = bidPriorities.Select(x => x.Value).Max() + 1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    foreach (var bidPriority in bidPriorities)
                    {
                        await connection.ExecuteAsync(
                            "update tbl_league_pickupbid set Priority = @bidPriority where BidID = @bidID",
                            new
                            {
                                bidID = bidPriority.Key.BidID,
                                bidPriority = tempPosition
                            }, transaction);
                        tempPosition++;
                    }

                    foreach (var bidPriority in bidPriorities)
                    {
                        await connection.ExecuteAsync(
                            "update tbl_league_pickupbid set Priority = @bidPriority where BidID = @bidID",
                            new
                            {
                                bidID = bidPriority.Key.BidID,
                                bidPriority = bidPriority.Value
                            }, transaction);
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        public async Task CreateDropRequest(DropRequest currentDropRequest)
        {
            var entity = new DropRequestEntity(currentDropRequest);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_droprequest(DropRequestID,PublisherID,MasterGameID,Timestamp,Successful) VALUES " +
                    "(@DropRequestID,@PublisherID,@MasterGameID,@Timestamp,@Successful);",
                    entity);
            }
        }

        public async Task RemoveDropRequest(DropRequest dropRequest)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("delete from tbl_league_droprequest where DropRequestID = @DropRequestID", new { dropRequest.DropRequestID });
            }
        }

        public async Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(Publisher publisher)
        {
            var leagueYear = publisher.LeagueYear;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var dropEntities = await connection.QueryAsync<DropRequestEntity>("select * from tbl_league_droprequest where PublisherID = @publisherID and Successful is NULL",
                    new { publisherID = publisher.PublisherID });

                List<DropRequest> domainDrops = new List<DropRequest>();
                foreach (var dropEntity in dropEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(dropEntity.MasterGameID);

                    DropRequest domain = dropEntity.ToDomain(publisher, masterGame.Value, leagueYear);
                    domainDrops.Add(domain);
                }

                return domainDrops;
            }
        }

        public async Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(int year, IReadOnlyList<LeagueYear> allLeagueYears, IReadOnlyList<Publisher> allPublishersForYear)
        {
            var leagueDictionary = allLeagueYears.GroupBy(x => x.League.LeagueID).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            var publisherDictionary = allPublishersForYear.ToDictionary(x => x.PublisherID, y => y);

            using (var connection = new MySqlConnection(_connectionString))
            {
                var dropEntities = await connection.QueryAsync<DropRequestEntity>("select * from vw_league_droprequest where Year = @year and Successful is NULL and IsDeleted = 0", new { year });

                Dictionary<LeagueYear, List<DropRequest>> dropRequestsByLeagueYear = allLeagueYears.ToDictionary(x => x, y => new List<DropRequest>());

                foreach (var dropEntity in dropEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(dropEntity.MasterGameID);
                    var publisher = publisherDictionary[dropEntity.PublisherID];
                    var leagueYear = leagueDictionary[publisher.LeagueYear.League.LeagueID].Single(x => x.Year == year);

                    DropRequest domainDrop = dropEntity.ToDomain(publisher, masterGame.Value, leagueYear);
                    dropRequestsByLeagueYear[leagueYear].Add(domainDrop);
                }

                return dropRequestsByLeagueYear.SealDictionary();
            }
        }

        private Task MarkBidStatus(IEnumerable<PickupBid> bids, bool success, MySqlConnection connection, MySqlTransaction transaction)
        {
            var entities = bids.Select(x => new PickupBidEntity(x, success));
            return connection.ExecuteAsync("update tbl_league_pickupbid SET Successful = @Successful where BidID = @BidID;", entities, transaction);
        }

        private Task MarkDropStatus(IEnumerable<DropRequest> drops, bool success, MySqlConnection connection, MySqlTransaction transaction)
        {
            var entities = drops.Select(x => new DropRequestEntity(x, success));
            return connection.ExecuteAsync("update tbl_league_droprequest SET Successful = @Successful where DropRequestID = @DropRequestID;", entities, transaction);
        }

        private Task UpdatePublisherBudgetsAndDroppedGames(IEnumerable<Publisher> updatedPublishers, MySqlConnection connection, MySqlTransaction transaction)
        {
            string sql = "update tbl_league_publisher SET Budget = @Budget, FreeGamesDropped = @FreeGamesDropped, " +
                         "WillNotReleaseGamesDropped = @WillNotReleaseGamesDropped, WillReleaseGamesDropped = @WillReleaseGamesDropped where PublisherID = @PublisherID;";
            var entities = updatedPublishers.Select(x => new PublisherEntity(x));
            return connection.ExecuteAsync(sql, entities, transaction);
        }

        private Task AddLeagueActions(IEnumerable<LeagueAction> actions, MySqlConnection connection, MySqlTransaction transaction)
        {
            var entities = actions.Select(x => new LeagueActionEntity(x));
            return connection.ExecuteAsync(
                "insert into tbl_league_action(PublisherID,Timestamp,ActionType,Description,ManagerAction) VALUES " +
                "(@PublisherID,@Timestamp,@ActionType,@Description,@ManagerAction);", entities, transaction);
        }

        private Task AddPublisherGames(IEnumerable<PublisherGame> publisherGames, MySqlConnection connection, MySqlTransaction transaction)
        {
            string sql =
                "insert into tbl_league_publishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore," +
                "ManualWillNotRelease,FantasyPoints,MasterGameID,SlotNumber,DraftPosition,OverallDraftPosition) VALUES " +
                "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore," +
                "@ManualWillNotRelease,@FantasyPoints,@MasterGameID,@SlotNumber,@DraftPosition,@OverallDraftPosition);";
            var entities = publisherGames.Select(x => new PublisherGameEntity(x));
            return connection.ExecuteAsync(sql, entities, transaction);
        }

        private Task DeletePublisherGames(IEnumerable<PublisherGame> publisherGames, MySqlConnection connection, MySqlTransaction transaction)
        {
            var publisherGameIDs = publisherGames.Select(x => x.PublisherGameID);
            return connection.ExecuteAsync(
                "delete from tbl_league_publishergame where PublisherGameID in @publisherGameIDs;", new { publisherGameIDs }, transaction);
        }

        public async Task AddPlayerToLeague(League league, FantasyCriticUser inviteUser)
        {
            var mostRecentYear = await GetLeagueYear(league, league.Years.Max());
            bool mostRecentYearNotStarted = mostRecentYear.HasValue && !mostRecentYear.Value.PlayStatus.PlayStarted;

            var userAddObject = new
            {
                leagueID = league.LeagueID,
                userID = inviteUser.Id,
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync("insert into tbl_league_hasuser(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject, transaction);
                    if (mostRecentYearNotStarted)
                    {
                        var userActiveObject = new
                        {
                            leagueID = league.LeagueID,
                            userID = inviteUser.Id,
                            activeYear = mostRecentYear.Value.Year
                        };
                        await connection.ExecuteAsync("insert into tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@activeYear,@userID);", userActiveObject, transaction);
                    }
                    await transaction.CommitAsync();
                }
            }
        }

        private async Task<IReadOnlyList<LeagueInvite>> ConvertLeagueInviteEntities(IEnumerable<LeagueInviteEntity> entities)
        {
            List<LeagueInvite> leagueInvites = new List<LeagueInvite>();
            foreach (var entity in entities)
            {
                var league = await GetLeagueByID(entity.LeagueID);
                if (league.HasNoValue)
                {
                    //League has probably been deleted.
                    _logger.Warn($"Cannot find league (probably deleted) LeagueID: {entity.LeagueID}");
                    continue;
                }

                if (entity.UserID.HasValue)
                {
                    FantasyCriticUser user = await _userStore.FindByIdAsync(entity.UserID.Value.ToString(), CancellationToken.None);
                    leagueInvites.Add(entity.ToDomain(league.Value, user));
                }
                else
                {
                    leagueInvites.Add(entity.ToDomain(league.Value));
                }
            }

            return leagueInvites;
        }

        private async Task<LeagueInvite> ConvertLeagueInviteEntity(LeagueInviteEntity entity)
        {
            var league = await GetLeagueByID(entity.LeagueID);
            if (league.HasNoValue)
            {
                throw new Exception($"Cannot find league for league (should never happen) LeagueID: {entity.LeagueID}");
            }

            if (entity.UserID.HasValue)
            {
                FantasyCriticUser user = await _userStore.FindByIdAsync(entity.UserID.Value.ToString(), CancellationToken.None);
                return entity.ToDomain(league.Value, user);
            }

            return entity.ToDomain(league.Value);
        }

        private async Task<IReadOnlyList<LeagueYearTagEntity>> GetLeagueYearTagEntities(int year)
        {
            var sql = "select * from tbl_league_yearusestag where Year = @year;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<LeagueYearTagEntity> entities = await connection.QueryAsync<LeagueYearTagEntity>(sql, new { year });
                return entities.ToList();
            }
        }

        private async Task<IReadOnlyList<LeagueYearTagEntity>> GetLeagueYearTagEntities(Guid leagueID, int year)
        {
            var sql = "select * from tbl_league_yearusestag where LeagueID = @leagueID AND Year = @year;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<LeagueYearTagEntity> entities = await connection.QueryAsync<LeagueYearTagEntity>(sql, new { leagueID, year });
                return entities.ToList();
            }
        }

        private async Task<IReadOnlyList<SpecialGameSlotEntity>> GetSpecialGameSlotEntities(int year)
        {
            var sql = "select * from tbl_league_specialgameslot where Year = @year;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<SpecialGameSlotEntity> entities = await connection.QueryAsync<SpecialGameSlotEntity>(sql, new { year });
                return entities.ToList();
            }
        }

        private async Task<IReadOnlyList<SpecialGameSlotEntity>> GetSpecialGameSlotEntities(Guid leagueID, int year)
        {
            var sql = "select * from tbl_league_specialgameslot where LeagueID = @leagueID AND Year = @year;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<SpecialGameSlotEntity> entities = await connection.QueryAsync<SpecialGameSlotEntity>(sql, new { leagueID, year });
                return entities.ToList();
            }
        }

        private static IReadOnlyList<LeagueTagStatus> ConvertLeagueTagEntities(IEnumerable<LeagueYearTagEntity> leagueTags, IReadOnlyDictionary<string, MasterGameTag> tagOptions)
        {
            var domains = leagueTags.Select(x => x.ToDomain(tagOptions[x.Tag])).ToList();
            return domains;
        }

        private static IReadOnlyDictionary<LeagueYearKey, IReadOnlyList<SpecialGameSlot>> ConvertSpecialGameSlotEntities(IEnumerable<SpecialGameSlotEntity> specialGameSlotEntities, IReadOnlyDictionary<string, MasterGameTag> tagOptions)
        {
            Dictionary<LeagueYearKey, List<SpecialGameSlot>> fullDomains = new Dictionary<LeagueYearKey, List<SpecialGameSlot>>();
            var groupByLeagueYearKey = specialGameSlotEntities.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
            foreach (var leagueYearGroup in groupByLeagueYearKey)
            {
                List<SpecialGameSlot> domainsForLeagueYear = new List<SpecialGameSlot>();
                var groupByPosition = leagueYearGroup.GroupBy(x => x.SpecialSlotPosition);
                foreach (var positionGroup in groupByPosition)
                {
                    var tags = positionGroup.Select(x => tagOptions[x.Tag]);
                    domainsForLeagueYear.Add(new SpecialGameSlot(positionGroup.Key, tags));
                }

                fullDomains[leagueYearGroup.Key] = domainsForLeagueYear;
            }
            
            return fullDomains.SealDictionary();
        }

        public async Task PostNewManagerMessage(LeagueYear leagueYear, ManagerMessage message)
        {
            var entity = new ManagerMessageEntity(leagueYear, message);
            string sql = "INSERT INTO tbl_league_managermessage(MessageID,LeagueID,Year,MessageText,IsPublic,Timestamp) VALUES " +
                         "(@MessageID,@LeagueID,@Year,@MessageText,@IsPublic,@Timestamp);";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, entity);
            }
        }

        public async Task<IReadOnlyList<ManagerMessage>> GetManagerMessages(LeagueYear leagueYear)
        {
            var messageSQL = "select * from tbl_league_managermessage where LeagueID = @leagueID AND Year = @year;";
            var queryObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year
            };

            var dismissSQL = "select * from tbl_league_managermessagedismissal where MessageID in @messageIDs;";

            IEnumerable<ManagerMessageEntity> messageEntities;
            IEnumerable<ManagerMessageDismissalEntity> dismissalEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                messageEntities = await connection.QueryAsync<ManagerMessageEntity>(messageSQL, queryObject);

                var messageIDs = messageEntities.Select(x => x.MessageID);
                var dismissQueryObject = new
                {
                    messageIDs
                };
                dismissalEntities = await connection.QueryAsync<ManagerMessageDismissalEntity>(dismissSQL, dismissQueryObject);
            }

            List<ManagerMessage> domainMessages = new List<ManagerMessage>();
            var dismissalLookup = dismissalEntities.ToLookup(x => x.MessageID);
            foreach (var messageEntity in messageEntities)
            {
                var dismissedUserIDs = dismissalLookup[messageEntity.MessageID].Select(x => x.UserID);
                domainMessages.Add(messageEntity.ToDomain(dismissedUserIDs));
            }

            return domainMessages;
        }

        public async Task DeleteManagerMessage(Guid messageId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("delete from tbl_league_managermessage where MessageID = @messageId;", new { messageId });
            }
        }

        public async Task<Result> DismissManagerMessage(Guid messageId, Guid userId)
        {
            string sql = "INSERT IGNORE INTO `tbl_league_managermessagedismissal` " +
                         "(`MessageID`, `UserID`) VALUES(@messageId, @userID);";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var rowsAdded = await connection.ExecuteAsync(sql, new { messageId, userId });
                if (rowsAdded != 1)
                {
                    return Result.Failure("Invalid request");
                }
            }

            return Result.Success();
        }

        public async Task FinishYear(SupportedYear supportedYear)
        {
            string sql = "UPDATE tbl_meta_supportedyear SET Finished = 1 WHERE Year = @year;";

            var finishObject = new
            {
                year = supportedYear.Year
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, finishObject);
            }
        }
    }
}
