using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Services;
using FantasyCritic.MySQL.Entities;
using MoreLinq;
using MySql.Data.MySqlClient;
using NLog;
using NLog.Targets.Wrappers;
using NodaTime;

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

                LeagueEntity leagueEntity = await connection.QuerySingleAsync<LeagueEntity>(
                    "select * from vw_league where LeagueID = @leagueID and IsDeleted = 0", queryObject);

                FantasyCriticUser manager = await _userStore.FindByIdAsync(leagueEntity.LeagueManager.ToString(), CancellationToken.None);

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tbl_league_year where LeagueID = @leagueID", queryObject);
                IEnumerable<int> years = yearEntities.Select(x => x.Year);

                League league = leagueEntity.ToDomain(manager, years);
                return league;
            }
        }

        public async Task<IReadOnlyList<League>> GetAllLeagues()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var leagueEntities = await connection.QueryAsync<LeagueEntity>("select * from vw_league where IsDeleted = 0");

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tbl_league_year");
                List<League> leagues = new List<League>();
                var allUsers = await _userStore.GetAllUsers();
                foreach (var leagueEntity in leagueEntities)
                {
                    FantasyCriticUser manager = allUsers.Single(x => x.UserID == leagueEntity.LeagueManager);
                    IEnumerable<int> years = yearEntities.Where(x => x.LeagueID == leagueEntity.LeagueID).Select(x => x.Year);
                    League league = leagueEntity.ToDomain(manager, years);
                    leagues.Add(league);
                }

                return leagues;
            }
        }

        public async Task<Maybe<LeagueYear>> GetLeagueYear(League requestLeague, int requestYear)
        {
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

                var eligibilityLevel = await _masterGameRepo.GetEligibilityLevel(yearEntity.MaximumEligibilityLevel);
                var eligibilityOverrides = await GetEligibilityOverrides(requestLeague, requestYear);
                LeagueYear year = yearEntity.ToDomain(requestLeague, eligibilityLevel, eligibilityOverrides);
                return year;
            }
        }

        public async Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    year
                };

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tbl_league_year where Year = @year", queryObject);
                List<LeagueYear> leagueYears = new List<LeagueYear>();
                IReadOnlyList<League> leagues = await GetAllLeagues();
                Dictionary<Guid, League> leaguesDictionary = leagues.ToDictionary(x => x.LeagueID, y => y);
                var allEligibilityOverrides = await GetAllEligibilityOverrides(year);

                foreach (var entity in yearEntities)
                {
                    var success = leaguesDictionary.TryGetValue(entity.LeagueID, out League league);
                    if (!success)
                    {
                        _logger.Warn($"Cannot find league (probably deleted) LeagueID: {entity.LeagueID}");
                        continue;
                    }

                    var eligibilityLevel = await _masterGameRepo.GetEligibilityLevel(entity.MaximumEligibilityLevel);
                    bool hasOverrides = allEligibilityOverrides.TryGetValue(entity.LeagueID, out var eligibilityOverrides);
                    if (!hasOverrides)
                    {
                        eligibilityOverrides = new List<EligibilityOverride>();
                    }
                    LeagueYear leagueYear = entity.ToDomain(league, eligibilityLevel, eligibilityOverrides);
                    leagueYears.Add(leagueYear);
                }

                return leagueYears;
            }
        }

        public async Task UpdateFantasyPoints(Dictionary<Guid, decimal?> publisherGameScores)
        {
            List<PublisherScoreUpdateEntity> updateEntities = publisherGameScores.Select(x => new PublisherScoreUpdateEntity(x)).ToList();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(
                        "update tbl_league_publishergame SET FantasyPoints = @FantasyPoints where PublisherGameID = @PublisherGameID;",
                        updateEntities, transaction);
                    transaction.Commit();
                }
            }
        }

        public async Task<Result> RemovePublisherGame(Guid publisherGameID)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var removed = await connection.ExecuteAsync("delete from tbl_league_publishergame where PublisherGameID = @publisherGameID;", new { publisherGameID });
                if (removed == 1)
                {
                    return Result.Ok();
                }
                return Result.Fail("Removing game failed.");
            }
        }

        public async Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_league_publishergame SET ManualCriticScore = @manualCriticScore where PublisherGameID = @publisherGameID;", 
                    new { publisherGameID = publisherGame.PublisherGameID, manualCriticScore });
            }
        }

        public async Task CreatePickupBid(PickupBid currentBid)
        {
            var entity = new PickupBidEntity(currentBid);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_pickupbid(BidID,PublisherID,MasterGameID,Timestamp,Priority,BidAmount,Successful) VALUES " +
                    "(@BidID,@PublisherID,@MasterGameID,@Timestamp,@Priority,@BidAmount,@Successful);",
                    entity);
            }
        }

        public async Task RemovePickupBid(PickupBid pickupBid)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    {
                        await connection.ExecuteAsync("delete from tbl_league_pickupbid where BidID = @BidID",
                            new {pickupBid.BidID}, transaction);
                        await connection.ExecuteAsync(
                            "update tbl_league_pickupbid SET Priority = Priority - 1 where PublisherID = @publisherID and Successful is NULL and Priority > @oldPriority",
                            new {publisherID = pickupBid.Publisher.PublisherID, oldPriority = pickupBid.Priority}, transaction);
                        transaction.Commit();
                    }
                }
            }
        }

        public async Task<IReadOnlyList<PickupBid>> GetActivePickupBids(Publisher publisher)
        {
            var leagueYear = publisher.LeagueYear;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var bidEntities = await connection.QueryAsync<PickupBidEntity>("select * from tbl_league_pickupbid where PublisherID = @publisherID and Successful is NULL",
                    new { publisherID = publisher.PublisherID });

                List<PickupBid> domainBids = new List<PickupBid>();
                foreach (var bidEntity in bidEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);

                    PickupBid domain = bidEntity.ToDomain(publisher, masterGame.Value, leagueYear);
                    domainBids.Add(domain);
                }

                return domainBids;
            }
        }

        public async Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActivePickupBids(int year)
        {
            var leagueYears = await GetLeagueYears(year);
            var leagueDictionary = leagueYears.GroupBy(x => x.League.LeagueID).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            var publishers = await GetAllPublishersForYear(year);
            var publisherDictionary = publishers.ToDictionary(x => x.PublisherID, y => y);

            using (var connection = new MySqlConnection(_connectionString))
            {
                var bidEntities = await connection.QueryAsync<PickupBidEntity>("select * from vw_league_pickupbid where Year = @year and Successful is NULL and IsDeleted = 0", new { year });

                Dictionary<LeagueYear, List<PickupBid>> pickupBidsByLeagueYear = leagueYears.ToDictionary(x => x, y => new List<PickupBid>());

                foreach (var bidEntity in bidEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);
                    var publisher = publisherDictionary[bidEntity.PublisherID];
                    var leagueYear = leagueDictionary[publisher.LeagueYear.League.LeagueID].Single(x => x.Year == year);

                    PickupBid domainPickup = bidEntity.ToDomain(publisher, masterGame.Value, leagueYear);
                    pickupBidsByLeagueYear[leagueYear].Add(domainPickup);
                }

                IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> finalDictionary = pickupBidsByLeagueYear.ToDictionary(x => x.Key, y => (IReadOnlyList<PickupBid>) y.Value);

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
            string sql = "insert into tbl_league_publisherqueue(PublisherID,MasterGameID,Rank) VALUES (@PublisherID,@MasterGameID,@Rank);";
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
                    transaction.Commit();
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

                    transaction.Commit();
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
                    transaction.Commit();
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

                PickupBid domain = bidEntity.ToDomain(publisher.Value, masterGame.Value, leagueYear);
                return domain;
            }
        }

        public async Task CreateLeague(League league, int initialYear, LeagueOptions options)
        {
            LeagueEntity entity = new LeagueEntity(league);
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, initialYear, options, PlayStatus.NotStartedDraft);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(
                        "insert into tbl_league(LeagueID,LeagueName,LeagueManager,PublicLeague,TestLeague) VALUES " +
                        "(@LeagueID,@LeagueName,@LeagueManager,@PublicLeague,@TestLeague);",
                        entity, transaction);

                    await connection.ExecuteAsync(
                        "insert into tbl_league_year(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,FreeDroppableGames,WillNotReleaseDroppableGames,WillReleaseDroppableGames,DropOnlyDraftGames," +
                        "MaximumEligibilityLevel,AllowYearlyInstallments,AllowEarlyAccess,AllowFreeToPlay,AllowReleasedInternationally,AllowExpansions,DraftSystem,PickupSystem,ScoringSystem,PlayStatus) VALUES " +
                        "(@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@FreeDroppableGames,@WillNotReleaseDroppableGames,@WillReleaseDroppableGames,@DropOnlyDraftGames,@MaximumEligibilityLevel," +
                        "@AllowYearlyInstallments,@AllowEarlyAccess,@AllowFreeToPlay,@AllowReleasedInternationally,@AllowExpansions,@DraftSystem,@PickupSystem,@ScoringSystem,@PlayStatus);",
                        leagueYearEntity, transaction);

                    transaction.Commit();
                }
            }

            await AddPlayerToLeague(league, league.LeagueManager);
        }

        public async Task EditLeagueYear(LeagueYear leagueYear)
        {
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(leagueYear.League, leagueYear.Year, leagueYear.Options, leagueYear.PlayStatus);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "update tbl_league_year SET StandardGames = @StandardGames, GamesToDraft = @GamesToDraft, CounterPicks = @CounterPicks, " +
                    "FreeDroppableGames = @FreeDroppableGames, WillNotReleaseDroppableGames = @WillNotReleaseDroppableGames, WillReleaseDroppableGames = @WillReleaseDroppableGames, " +
                    "DropOnlyDraftGames = @DropOnlyDraftGames, MaximumEligibilityLevel = @MaximumEligibilityLevel, AllowYearlyInstallments = @AllowYearlyInstallments, AllowEarlyAccess = @AllowEarlyAccess, " +
                    "AllowFreeToPlay = @AllowFreeToPlay, AllowReleasedInternationally = @AllowReleasedInternationally, AllowExpansions = @AllowExpansions, DraftSystem = @DraftSystem, " +
                    "PickupSystem = @PickupSystem, ScoringSystem = @ScoringSystem WHERE LeagueID = @LeagueID and Year = @Year",
                    leagueYearEntity);
            }
        }

        public async Task AddNewLeagueYear(League league, int year, LeagueOptions options)
        {
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, year, options, PlayStatus.NotStartedDraft);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_year(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,FreeDroppableGames,WillNotReleaseDroppableGames,WillReleaseDroppableGames,DropOnlyDraftGames," +
                    "MaximumEligibilityLevel,AllowYearlyInstallments,AllowEarlyAccess,AllowFreeToPlay,AllowReleasedInternationally,AllowExpansions,DraftSystem,PickupSystem,ScoringSystem,PlayStatus) VALUES " +
                    "(@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@FreeDroppableGames,@WillNotReleaseDroppableGames,@WillReleaseDroppableGames,@DropOnlyDraftGames,@MaximumEligibilityLevel," +
                    "@AllowYearlyInstallments,@AllowEarlyAccess,@AllowFreeToPlay,@AllowReleasedInternationally,@AllowExpansions,@DraftSystem,@PickupSystem,@ScoringSystem,@PlayStatus);",
                    leagueYearEntity);
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
                userID = x.UserID,
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
                            userID = userToChange.Key.UserID,
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

                    transaction.Commit();
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
                    userID = user.UserID,
                };

                var sql = "select vw_league.* from vw_league join tbl_league_hasuser on (vw_league.LeagueID = tbl_league_hasuser.LeagueID) where tbl_league_hasuser.UserID = @userID and IsDeleted = 0;";
                leagueEntities = await connection.QueryAsync<LeagueEntity>(sql, queryObject);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
        }

        public async Task<IReadOnlyList<LeagueYear>> GetLeagueYearsForUser(FantasyCriticUser user, int year)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    year,
                    userID = user.UserID
                };

                string sql = "select tbl_league_year.* from tbl_league_year " +
                             "join tbl_league_hasuser on (tbl_league_hasuser.LeagueID = tbl_league_year.LeagueID) " +
                             "where tbl_league_year.Year = @year and tbl_league_hasuser.UserID = @userID";
                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>(sql, queryObject);
                List<LeagueYear> leagueYears = new List<LeagueYear>();
                IReadOnlyList<League> leagues = await GetAllLeagues();
                Dictionary<Guid, League> leaguesDictionary = leagues.ToDictionary(x => x.LeagueID, y => y);
                var allEligibilityOverrides = await GetAllEligibilityOverrides(year);

                foreach (var entity in yearEntities)
                {
                    var success = leaguesDictionary.TryGetValue(entity.LeagueID, out League league);
                    if (!success)
                    {
                        _logger.Warn($"Cannot find league (probably deleted) LeagueID: {entity.LeagueID}");
                        continue;
                    }

                    var eligibilityLevel = await _masterGameRepo.GetEligibilityLevel(entity.MaximumEligibilityLevel);
                    bool hasOverrides = allEligibilityOverrides.TryGetValue(entity.LeagueID, out var eligibilityOverrides);
                    if (!hasOverrides)
                    {
                        eligibilityOverrides = new List<EligibilityOverride>();
                    }
                    LeagueYear leagueYear = entity.ToDomain(league, eligibilityLevel, eligibilityOverrides);
                    leagueYears.Add(leagueYear);
                }

                return leagueYears;
            }
        }

        public async Task<IReadOnlyList<League>> GetFollowedLeagues(FantasyCriticUser currentUser)
        {
            var query = new
            {
                userID = currentUser.UserID
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

        public Task FollowLeague(League league, FantasyCriticUser user)
        {
            var userAddObject = new
            {
                leagueID = league.LeagueID,
                userID = user.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "insert into tbl_user_followingleague(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject);
            }
        }

        public Task UnfollowLeague(League league, FantasyCriticUser user)
        {
            var deleteObject = new
            {
                leagueID = league.LeagueID,
                userID = user.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
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
                email = currentUser.EmailAddress,
                userID = currentUser.UserID
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

        public Task SaveInvite(LeagueInvite leagueInvite)
        {
            var entity = new LeagueInviteEntity(leagueInvite);

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
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

        public async Task RemovePublisher(Publisher deletePublisher, IEnumerable<Publisher> publishersInLeague)
        {
            string deleteSQL = "delete from tbl_league_publisher where PublisherID = @publisherID;";
            string deleteQueueSQL = "delete from tbl_league_publisherqueue where PublisherID = @publisherID;";
            string deleteHistorySQL = "delete from tbl_league_action where PublisherID = @publisherID;";
            string fixDraftOrderSQL = "update tbl_league_publisher SET DraftPosition = @draftPosition where PublisherID = @publisherID;";

            var remainingOrderedPublishers = publishersInLeague.Except(new List<Publisher>{ deletePublisher }).OrderBy(x => x.DraftPosition).ToList();
            IEnumerable<SetDraftOrderEntity> setDraftOrderEntities = remainingOrderedPublishers.Select((publisher, index) => new SetDraftOrderEntity(publisher.PublisherID, index + 1));

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(deleteQueueSQL, new { publisherID = deletePublisher.PublisherID }, transaction);
                    await connection.ExecuteAsync(deleteHistorySQL, new { publisherID = deletePublisher.PublisherID }, transaction);
                    await connection.ExecuteAsync(deleteSQL, new { publisherID = deletePublisher.PublisherID }, transaction);
                    await connection.ExecuteAsync(fixDraftOrderSQL, setDraftOrderEntities);
                    transaction.Commit();
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
                userID = removeUser.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(deleteActiveUserSQL, userDeleteObject, transaction);
                    await connection.ExecuteAsync(deleteUserSQL, userDeleteObject, transaction);
                    transaction.Commit();
                }
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
                var user = usersInLeague.Single(x => x.UserID == entity.UserID);
                var domainPublisher = entity.ToDomain(leagueYear, user, gamesForPublisher);
                domainPublishers.Add(domainPublisher);
            }

            return domainPublishers;
        }

        public async Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year)
        {
            var query = new
            {
                year
            };

            var allUsers = await _userStore.GetAllUsers();
            var usersDictionary = allUsers.ToDictionary(x => x.UserID, y => y);
            IReadOnlyList<LeagueYear> allLeagueYears = await GetLeagueYears(year);
            var leaguesDictionary = allLeagueYears.ToDictionary(x => x.League.LeagueID, y => y);

            string sql = "select tbl_league_publisher.* from tbl_league_publisher " +
                         "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                         "where tbl_league_publisher.Year = @year and tbl_league.IsDeleted = 0;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var publisherEntities = await connection.QueryAsync<PublisherEntity>(sql, query);

                IReadOnlyList<PublisherGame> allDomainGames = await GetAllPublisherGamesForYear(year);
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

        private async Task<IReadOnlyList<PublisherGame>> GetAllPublisherGamesForYear(int year)
        {
            var query = new
            {
                year
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<PublisherGameEntity> gameEntities = await connection.QueryAsync<PublisherGameEntity>(
                    "select tbl_league_publishergame.* from tbl_league_publishergame " +
                    "join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID) " +
                    "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                    "where tbl_league_publisher.Year = @year and IsDeleted = 0;",
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

        public async Task<Maybe<Publisher>> GetPublisher(LeagueYear leagueYear, FantasyCriticUser user)
        {
            var query = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year,
                userID = user.UserID
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

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_league_publishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore,FantasyPoints,MasterGameID,DraftPosition,OverallDraftPosition) VALUES " +
                    "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore,@FantasyPoints,@MasterGameID,@DraftPosition,@OverallDraftPosition);",
                    entity);
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

        public async Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<SupportedYearEntity>("select * from tbl_meta_supportedyear;");
                return results.Select(x => x.ToDomain()).ToList();
            }
        }

        private async Task<IReadOnlyList<League>> ConvertLeagueEntitiesToDomain(IEnumerable<LeagueEntity> leagueEntities)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                List<League> leagues = new List<League>();
                foreach (var leagueEntity in leagueEntities)
                {
                    FantasyCriticUser manager = await _userStore.FindByIdAsync(leagueEntity.LeagueManager.ToString(),
                        CancellationToken.None);

                    IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>(
                        "select * from tbl_league_year where LeagueID = @leagueID", new
                        {
                            leagueID = leagueEntity.LeagueID
                        });

                    IEnumerable<int> years = yearEntities.Select(x => x.Year);
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

                    transaction.Commit();
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

            var dictionary = domainObjects.GroupBy(x => x.Item1).ToDictionary(x => x.Key, y => (IReadOnlyList<EligibilityOverride>)y.Select(z => z.Item2).ToList());

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
                    transaction.Commit();
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
                    transaction.Commit();
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

        public async Task SetBidProcessingMode(bool modeOn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tbl_meta_systemwidesettings set BidProcessingMode = @modeOn;", new { modeOn });
            }
        }

        public Task DeletePublisher(Publisher publisher)
        {
            var deleteObject = new
            {
                publisherID = publisher.PublisherID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "delete from tbl_league_publisher where PublisherID = @publisherID;",
                    deleteObject);
            }
        }

        public Task DeleteLeagueYear(LeagueYear leagueYear)
        {
            var deleteObject = new
            {
                leagueID = leagueYear.League.LeagueID,
                year = leagueYear.Year
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "delete from tbl_league_year where LeagueID = @leagueID and Year = @year;",
                    deleteObject);
            }
        }

        public Task DeleteLeague(League league)
        {
            var deleteObject = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "delete from tbl_league where LeagueID = @leagueID;",
                    deleteObject);
            }
        }

        public Task DeleteLeagueActions(Publisher publisher)
        {
            var deleteObject = new
            {
                publisherID = publisher.PublisherID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "delete from tbl_league_action where PublisherID = @publisherID;",
                    deleteObject);
            }
        }

        public Task<bool> LeagueHasBeenStarted(Guid leagueID)
        {
            var selectObject = new
            {
                leagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteScalarAsync<bool>(
                    "select count(1) from vw_league " +
                    "join tbl_league_year on (vw_league.LeagueID = tbl_league_year.LeagueID) " +
                    "where PlayStatus <> 'NotStartedDraft' and vw_league.LeagueID = @leagueID and IsDeleted = 0;",
                    selectObject);
            }
        }

        public async Task SaveProcessedBidResults(BidProcessingResults bidProcessingResults)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    List<Task> tasks = new List<Task>();
                    tasks.Add(MarkBidStatus(bidProcessingResults.SuccessBids, true, connection, transaction));
                    tasks.Add(MarkBidStatus(bidProcessingResults.FailedBids, false, connection, transaction));
                    tasks.Add(AddLeagueActions(bidProcessingResults.LeagueActions, connection, transaction));
                    tasks.Add(UpdatePublisherBudgetsAndDroppedGames(bidProcessingResults.UpdatedPublishers, connection, transaction));
                    tasks.Add(AddPublisherGames(bidProcessingResults.PublisherGames, connection, transaction));

                    await Task.WhenAll(tasks);
                    transaction.Commit();
                }
            }   
        }

        public async Task SaveProcessedDropResults(DropProcessingResults dropProcessingResults)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    List<Task> tasks = new List<Task>();
                    tasks.Add(MarkDropStatus(dropProcessingResults.SuccessDrops, true, connection, transaction));
                    tasks.Add(MarkDropStatus(dropProcessingResults.FailedDrops, false, connection, transaction));
                    tasks.Add(AddLeagueActions(dropProcessingResults.LeagueActions, connection, transaction));
                    tasks.Add(UpdatePublisherBudgetsAndDroppedGames(dropProcessingResults.PublishersToUpdate, connection, transaction));
                    tasks.Add(DeletePublisherGames(dropProcessingResults.PublisherGames, connection, transaction));

                    await Task.WhenAll(tasks);
                    transaction.Commit();
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
                    await connection.ExecuteAsync(deleteSQL);
                    await connection.ExecuteAsync(insertSQL, entity);
                    transaction.Commit();
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

                    transaction.Commit();
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

        public async Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(int year)
        {
            var leagueYears = await GetLeagueYears(year);
            var leagueDictionary = leagueYears.GroupBy(x => x.League.LeagueID).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            var publishers = await GetAllPublishersForYear(year);
            var publisherDictionary = publishers.ToDictionary(x => x.PublisherID, y => y);

            using (var connection = new MySqlConnection(_connectionString))
            {
                var dropEntities = await connection.QueryAsync<DropRequestEntity>("select * from vw_league_droprequest where Year = @year and Successful is NULL and IsDeleted = 0", new { year });

                Dictionary<LeagueYear, List<DropRequest>> dropRequestsByLeagueYear = leagueYears.ToDictionary(x => x, y => new List<DropRequest>());

                foreach (var dropEntity in dropEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(dropEntity.MasterGameID);
                    var publisher = publisherDictionary[dropEntity.PublisherID];
                    var leagueYear = leagueDictionary[publisher.LeagueYear.League.LeagueID].Single(x => x.Year == year);

                    DropRequest domainDrop = dropEntity.ToDomain(publisher, masterGame.Value, leagueYear);
                    dropRequestsByLeagueYear[leagueYear].Add(domainDrop);
                }

                IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>> finalDictionary = dropRequestsByLeagueYear.ToDictionary(x => x.Key, y => (IReadOnlyList<DropRequest>)y.Value);

                return finalDictionary;
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
            var entities = publisherGames.Select(x => new PublisherGameEntity(x));
            return connection.ExecuteAsync(
                "insert into tbl_league_publishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore,FantasyPoints,MasterGameID,DraftPosition,OverallDraftPosition) VALUES " +
                "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore,@FantasyPoints,@MasterGameID,@DraftPosition,@OverallDraftPosition);",
                entities, transaction);
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
                userID = inviteUser.UserID,
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
                            userID = inviteUser.UserID,
                            activeYear = mostRecentYear.Value.Year
                        };
                        await connection.ExecuteAsync("insert into tbl_league_activeplayer(LeagueID,Year,UserID) VALUES (@leagueID,@activeYear,@userID);", userActiveObject, transaction);
                    }
                    transaction.Commit();
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
    }
}
