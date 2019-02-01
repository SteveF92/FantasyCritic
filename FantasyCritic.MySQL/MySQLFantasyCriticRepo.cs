using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
using NLog.Targets.Wrappers;

namespace FantasyCritic.MySQL
{
    public class MySQLFantasyCriticRepo : IFantasyCriticRepo
    {
        private readonly string _connectionString;
        private readonly IReadOnlyFantasyCriticUserStore _userStore;
        private readonly IMasterGameRepo _masterGameRepo;

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
                    "select * from vwleague where LeagueID = @leagueID", queryObject);

                FantasyCriticUser manager = await _userStore.FindByIdAsync(leagueEntity.LeagueManager.ToString(), CancellationToken.None);

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tblleagueyear where LeagueID = @leagueID", queryObject);
                IEnumerable<int> years = yearEntities.Select(x => x.Year);

                League league = leagueEntity.ToDomain(manager, years);
                return league;
            }
        }

        public async Task<IReadOnlyList<League>> GetAllLeagues()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var leagueEntities = await connection.QueryAsync<LeagueEntity>("select * from vwleague");

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tblleagueyear");
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

                LeagueYearEntity yearEntity = await connection.QueryFirstOrDefaultAsync<LeagueYearEntity>("select * from tblleagueyear where LeagueID = @leagueID and Year = @year", queryObject);
                if (yearEntity == null)
                {
                    return Maybe<LeagueYear>.None;
                }

                var eligibilityLevel = await _masterGameRepo.GetEligibilityLevel(yearEntity.MaximumEligibilityLevel);
                LeagueYear year = yearEntity.ToDomain(requestLeague, eligibilityLevel);
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

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tblleagueyear where Year = @year", queryObject);
                List<LeagueYear> leagueYears = new List<LeagueYear>();
                IReadOnlyList<League> leagues = await GetAllLeagues();
                Dictionary<Guid, League> leaguesDictionary = leagues.ToDictionary(x => x.LeagueID, y => y);
                foreach (var entity in yearEntities)
                {
                    var success = leaguesDictionary.TryGetValue(entity.LeagueID, out League league);
                    if (!success)
                    {
                        throw new Exception($"Cannot find league for league-year (should never happen) LeagueID: {entity.LeagueID}");
                    }

                    var eligibilityLevel = await _masterGameRepo.GetEligibilityLevel(entity.MaximumEligibilityLevel);
                    LeagueYear leagueYear = entity.ToDomain(league, eligibilityLevel);
                    leagueYears.Add(leagueYear);
                }

                return leagueYears;
            }
        }

        public async Task UpdateFantasyPoints(Dictionary<Guid, decimal?> publisherGameScores)
        {
            List<PublisherScoreUpdateEntity> updateEntities =
                publisherGameScores.Select(x => new PublisherScoreUpdateEntity(x)).ToList();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "update tblpublishergame SET FantasyPoints = @FantasyPoints where PublisherGameID = @PublisherGameID;",
                    updateEntities);
            }
        }

        public async Task<Result> RemovePublisherGame(Guid publisherGameID)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var removed = await connection.ExecuteAsync("delete from tblpublishergame where PublisherGameID = @publisherGameID;", new { publisherGameID });
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
                await connection.ExecuteAsync("update tblpublishergame SET ManualCriticScore = @manualCriticScore where PublisherGameID = @publisherGameID;", 
                    new { publisherGameID = publisherGame.PublisherGameID, manualCriticScore });
            }
        }

        public async Task CreatePickupBid(PickupBid currentBid)
        {
            var entity = new PickupBidEntity(currentBid);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblpickupbid(BidID,PublisherID,MasterGameID,Timestamp,Priority,BidAmount,Successful) VALUES " +
                    "(@BidID,@PublisherID,@MasterGameID,@Timestamp,@Priority,@BidAmount,@Successful);",
                    entity);
            }
        }

        public async Task RemovePickupBid(PickupBid pickupBid)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("delete from tblpickupbid where BidID = @bidID", new {pickupBid.BidID});
                await connection.ExecuteAsync("update tblpickupbid SET Priority = Priority - 1 where PublisherID = @publisherID and Successful is NULL and Priority > @oldPriority", 
                    new { publisherID = pickupBid.Publisher.PublisherID, oldPriority = pickupBid.Priority });
            }
        }

        public async Task<IReadOnlyList<PickupBid>> GetActivePickupBids(Publisher publisher)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var bidEntities = await connection.QueryAsync<PickupBidEntity>("select * from tblpickupbid where PublisherID = @publisherID and Successful is NULL",
                    new { publisherID = publisher.PublisherID });

                List<PickupBid> domainBids = new List<PickupBid>();
                foreach (var bidEntity in bidEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);

                    PickupBid domain = bidEntity.ToDomain(publisher, masterGame.Value);
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
                var bidEntities = await connection.QueryAsync<PickupBidEntity>("select * from vwpickupbid where Year = @year and Successful is NULL", new { year });

                Dictionary<LeagueYear, List<PickupBid>> pickupBidsByLeagueYear = leagueYears.ToDictionary(x => x, y => new List<PickupBid>());

                foreach (var bidEntity in bidEntities)
                {
                    var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);
                    var publisher = publisherDictionary[bidEntity.PublisherID];
                    var leagueYear = leagueDictionary[publisher.League.LeagueID].Single(x => x.Year == year);

                    PickupBid domainPickup = bidEntity.ToDomain(publisher, masterGame.Value);
                    pickupBidsByLeagueYear[leagueYear].Add(domainPickup);
                }

                IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>> finalDictionary = pickupBidsByLeagueYear.ToDictionary(x => x.Key, y => (IReadOnlyList<PickupBid>) y.Value);

                return finalDictionary;
            }
        }

        public async Task MarkBidStatus(IEnumerable<PickupBid> bids, bool success)
        {
            var entities = bids.Select(x => new PickupBidEntity(x, success));
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync("update tblpickupbid SET Successful = @Successful where BidID = @BidID;", entities, transaction);
                    transaction.Commit();
                }
            }
        }

        public async Task SpendBudgets(IEnumerable<BudgetExpenditure> expenditures)
        {
            var entities = expenditures.Select(x => new BudgetExpenditureEntity(x));
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync("update tblpublisher SET Budget = Budget - @BidAmount where PublisherID = @PublisherID;", entities, transaction);
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
                    "insert into tblleagueaction(PublisherID,Timestamp,ActionType,Description,ManagerAction) VALUES " +
                    "(@PublisherID,@Timestamp,@ActionType,@Description,@ManagerAction);", entity);
            }
        }

        public async Task AddLeagueActions(IEnumerable<LeagueAction> actions)
        {
            var entities = actions.Select(x => new LeagueActionEntity(x));

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(
                        "insert into tblleagueaction(PublisherID,Timestamp,ActionType,Description,ManagerAction) VALUES " +
                        "(@PublisherID,@Timestamp,@ActionType,@Description,@ManagerAction);", entities, transaction);
                    transaction.Commit();
                }
            }
        }

        public async Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<LeagueActionEntity>(
                    "select tblleagueaction.PublisherID, tblleagueaction.Timestamp, tblleagueaction.ActionType, tblleagueaction.Description, tblleagueaction.ManagerAction from tblleagueaction " +                          
                    "join tblpublisher on (tblleagueaction.PublisherID = tblpublisher.PublisherID) " +
                    "where tblpublisher.LeagueID = @leagueID and tblpublisher.Year = @leagueYear;",
                    new
                    {
                        leagueID = leagueYear.League.LeagueID,
                        leagueYear = leagueYear.Year
                    });

                var publishersInLeague = await GetPublishersInLeagueForYear(leagueYear.League, leagueYear.Year);

                List<LeagueAction> leagueActions = entities.Select(x => x.ToDomain(publishersInLeague.Single(y => y.PublisherID == x.PublisherID))).ToList();

                return leagueActions;
            }
        }

        public async Task ChangePublisherName(Publisher publisher, string publisherName)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblpublisher SET PublisherName = @publisherName where PublisherID = @publisherID;",
                    new { publisherID = publisher.PublisherID, publisherName });
            }
        }

        public async Task ChangeLeagueOptions(League league, string leagueName, bool publicLeague, bool testLeague)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblleague SET LeagueName = @leagueName, PublicLeague = @publicLeague, TestLeague = @testLeague where LeagueID = @leagueID;",
                    new { leagueID = league.LeagueID, leagueName, publicLeague, testLeague });
            }
        }

        public Task StartDraft(LeagueYear leagueYear)
        {
            return SetDraftStatus(leagueYear, PlayStatus.Drafting);
        }

        public Task CompleteDraft(LeagueYear leagueYear)
        {
            return SetDraftStatus(leagueYear, PlayStatus.DraftFinal);
        }

        private async Task SetDraftStatus(LeagueYear leagueYear, PlayStatus playStatus)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    $"update tblleagueyear SET PlayStatus = '{playStatus.Value}' WHERE LeagueID = @leagueID and Year = @year",
                    new
                    {
                        leagueID = leagueYear.League.LeagueID,
                        year = leagueYear.Year
                    });
            }
        }

        public async Task SetDraftPause(LeagueYear leagueYear, bool pause)
        {
            string sql;
            if (pause)
            {
                sql = $"update tblleagueyear SET PlayStatus = '{PlayStatus.DraftPaused.Value}' WHERE LeagueID = @leagueID and Year = @year";
            }
            else
            {
                sql = $"update tblleagueyear SET PlayStatus = '{PlayStatus.Drafting.Value}' WHERE LeagueID = @leagueID and Year = @year";
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
                var bidEntity = await connection.QuerySingleOrDefaultAsync<PickupBidEntity>("select * from tblpickupbid where BidID = @bidID", new { bidID });
                if (bidEntity == null)
                {
                    return Maybe<PickupBid>.None;
                }

                var publisher = await GetPublisher(bidEntity.PublisherID);
                var masterGame = await _masterGameRepo.GetMasterGame(bidEntity.MasterGameID);

                PickupBid domain = bidEntity.ToDomain(publisher.Value, masterGame.Value);
                return domain;
            }
        }

        public async Task CreateLeague(League league, int initialYear, LeagueOptions options)
        {
            LeagueEntity entity = new LeagueEntity(league);
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, initialYear, options, PlayStatus.NotStartedDraft);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblleague(LeagueID,LeagueName,LeagueManager,PublicLeague,TestLeague) VALUES " +
                    "(@LeagueID,@LeagueName,@LeagueManager,@PublicLeague,@TestLeague);",
                    entity);

                await connection.ExecuteAsync(
                    "insert into tblleagueyear(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,MaximumEligibilityLevel,AllowYearlyInstallments,AllowEarlyAccess,DraftSystem,PickupSystem,ScoringSystem,PlayStatus) VALUES " +
                    "(@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@MaximumEligibilityLevel,@AllowYearlyInstallments,@AllowEarlyAccess,@DraftSystem,@PickupSystem,@ScoringSystem,@PlayStatus);",
                    leagueYearEntity);
            }

            await AddPlayerToLeague(league, league.LeagueManager);
        }

        public async Task EditLeagueYear(LeagueYear leagueYear)
        {
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(leagueYear.League, leagueYear.Year, leagueYear.Options, leagueYear.PlayStatus);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "update tblleagueyear SET StandardGames = @StandardGames, GamesToDraft = @GamesToDraft, CounterPicks = @CounterPicks, " +
                    "MaximumEligibilityLevel = @MaximumEligibilityLevel, AllowYearlyInstallments = @AllowYearlyInstallments, AllowEarlyAccess = @AllowEarlyAccess, DraftSystem = @DraftSystem, " +
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
                    "insert into tblleagueyear(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,MaximumEligibilityLevel,AllowYearlyInstallments,AllowEarlyAccess,DraftSystem,PickupSystem,ScoringSystem,PlayStatus) VALUES " +
                    "(@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@MaximumEligibilityLevel,@AllowYearlyInstallments,@AllowEarlyAccess,@DraftSystem,@PickupSystem,@ScoringSystem,@PlayStatus);",
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
                    "select tbluser.* from tbluser join tblleaguehasuser on (tbluser.UserID = tblleaguehasuser.UserID) where tblleaguehasuser.LeagueID = @leagueID;",
                    query);

                var users = results.Select(x => x.ToDomain()).ToList();
                return users;
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
                    "select tbluser.* from tbluser join tbluserfollowingleague on (tbluser.UserID = tbluserfollowingleague.UserID) where tbluserfollowingleague.LeagueID = @leagueID;",
                    query);

                var users = results.Select(x => x.ToDomain()).ToList();
                return users;
            }
        }

        public async Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser currentUser)
        {
            IEnumerable<LeagueEntity> leagueEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    userID = currentUser.UserID,
                };

                leagueEntities = await connection.QueryAsync<LeagueEntity>(
                    "select vwleague.* from vwleague join tblleaguehasuser on (vwleague.LeagueID = tblleaguehasuser.LeagueID) where tblleaguehasuser.UserID = @userID;", queryObject);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
        }

        public async Task<IReadOnlyList<League>> GetLeaguesInvitedTo(FantasyCriticUser currentUser)
        {
            var query = new
            {
                email = currentUser.EmailAddress
            };

            IEnumerable<LeagueEntity> leagueEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                leagueEntities = await connection.QueryAsync<LeagueEntity>(
                    "select vwleague.* from vwleague join tblleagueinvite on (vwleague.LeagueID = tblleagueinvite.LeagueID) where tblleagueinvite.EmailAddress = @email;",
                    query);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
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
                leagueEntities = await connection.QueryAsync<LeagueEntity>(
                    "select vwleague.* from vwleague join tbluserfollowingleague on (vwleague.LeagueID = tbluserfollowingleague.LeagueID) where tbluserfollowingleague.UserID = @userID;",
                    query);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
        }

        public Task SaveInvite(League league, string emailAddress)
        {
            var saveObject = new
            {
                inviteID = Guid.NewGuid(),
                leagueID = league.LeagueID,
                emailAddress
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "insert into tblleagueinvite(InviteID,LeagueID,EmailAddress) VALUES (@inviteID, @leagueID, @emailAddress);",
                    saveObject);
            }
        }

        public Task RescindInvite(League league, string emailAddress)
        {
            var deleteObject = new
            {
                leagueID = league.LeagueID,
                emailAddress
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "delete from tblleagueinvite where LeagueID = @leagueID and EmailAddress = @emailAddress;",
                    deleteObject);
            }
        }

        public async Task<IReadOnlyList<string>> GetOutstandingInvitees(League league)
        {
            var query = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<string>(
                    "select EmailAddress from tblleagueinvite where tblleagueinvite.LeagueID = @leagueID;",
                    query);

                return results.ToList();
            }
        }

        public async Task AcceptInvite(League league, FantasyCriticUser inviteUser)
        {
            await AddPlayerToLeague(league, inviteUser);
            await DeleteInvite(league, inviteUser.EmailAddress);
        }

        public Task DeclineInvite(League league, FantasyCriticUser inviteUser)
        {
            return DeleteInvite(league, inviteUser.EmailAddress);
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
                    "insert into tbluserfollowingleague(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject);
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
                    "delete from tbluserfollowingleague where LeagueID = @leagueID and UserID = @userID;",
                    deleteObject);
            }
        }

        public async Task RemovePublisher(Publisher publisher)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("delete from tblpublisher where PublisherID = @publisherID;", new {publisherID = publisher.PublisherID});
            }
        }

        public async Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "delete from tblleaguehasuser where LeagueID = @leagueID and UserID = @userID;";
                await connection.ExecuteAsync(sql, new { leagueID = league.LeagueID, userID = removeUser.UserID });
            }
        }

        public async Task CreatePublisher(Publisher publisher)
        {
            var entity = new PublisherEnity(publisher);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblpublisher(PublisherID,PublisherName,LeagueID,Year,UserID,DraftPosition,Budget) VALUES " +
                    "(@PublisherID,@PublisherName,@LeagueID,@Year,@UserID,@DraftPosition,@Budget);",
                    entity);
            }
        }

        public async Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year)
        {
            var usersInLeague = await GetUsersInLeague(league);

            List<Publisher> publishers = new List<Publisher>();
            foreach (var user in usersInLeague)
            {
                var publisher = await GetPublisher(league, year, user);
                if (publisher.HasNoValue)
                {
                    continue;
                }
                publishers.Add(publisher.Value);
            }

            return publishers;
        }

        public async Task<IReadOnlyList<Publisher>> GetAllPublishersForYear(int year)
        {
            var query = new
            {
                year
            };

            var allUsers = await _userStore.GetAllUsers();
            var usersDictionary = allUsers.ToDictionary(x => x.UserID, y => y);
            var allLeagues = await GetAllLeagues();
            var leaguesDictionary = allLeagues.ToDictionary(x => x.LeagueID, y => y);
            using (var connection = new MySqlConnection(_connectionString))
            {
                var publisherEntities = await connection.QueryAsync<PublisherEnity>("select * from tblpublisher where tblpublisher.Year = @year;", query);

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
                    var league = leaguesDictionary[entity.LeagueID];
                    var domainGames = domainGamesDictionary[entity.PublisherID];
                    var domainPublisher = entity.ToDomain(league, user, domainGames);
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
                    "select * from tblpublishergame join tblpublisher on (tblpublishergame.PublisherID = tblpublisher.PublisherID) where tblpublisher.Year = @year;",
                    query);

                List<PublisherGame> domainGames = new List<PublisherGame>();
                foreach (var entity in gameEntities)
                {
                    Maybe<MasterGameYear> masterGame = null;
                    if (entity.MasterGameID.HasValue)
                    {
                        masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, year);
                    }

                    domainGames.Add(entity.ToDomain(masterGame, year));
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
                PublisherEnity publisherEntity = await connection.QuerySingleOrDefaultAsync<PublisherEnity>(
                    "select * from tblpublisher where tblpublisher.PublisherID = @publisherID;",
                    query);

                if (publisherEntity == null)
                {
                    return Maybe<Publisher>.None;
                }

                IReadOnlyList<PublisherGame> domainGames = await GetPublisherGames(publisherEntity.PublisherID, publisherEntity.Year);
                var user = await _userStore.FindByIdAsync(publisherEntity.UserID.ToString(), CancellationToken.None);
                var league = await GetLeagueByID(publisherEntity.LeagueID);

                var domainPublisher = publisherEntity.ToDomain(league.Value, user, domainGames);
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
                    "select * from tblpublishergame where tblpublishergame.PublisherGameID = @publisherGameID;",
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
                    masterGame = await _masterGameRepo.GetMasterGameYear(gameEntity.MasterGameID.Value, publisher.Value.Year);
                }

                PublisherGame publisherGame = gameEntity.ToDomain(masterGame, publisher.Value.Year);
                return publisherGame;
            }
        }

        public async Task<Maybe<Publisher>> GetPublisher(League league, int year, FantasyCriticUser user)
        {
            var query = new
            {
                leagueID = league.LeagueID,
                year,
                userID = user.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                PublisherEnity publisherEntity = await connection.QuerySingleOrDefaultAsync<PublisherEnity>(
                    "select * from tblpublisher where tblpublisher.LeagueID = @leagueID and tblpublisher.Year = @year and tblpublisher.UserID = @userID;",
                    query);

                if (publisherEntity == null)
                {
                    return Maybe<Publisher>.None;
                }

                IReadOnlyList<PublisherGame> domainGames = await GetPublisherGames(publisherEntity.PublisherID, publisherEntity.Year);
                var domainPublisher = publisherEntity.ToDomain(league, user, domainGames);
                return domainPublisher;
            }
        }

        public async Task AddPublisherGame(PublisherGame publisherGame)
        {
            PublisherGameEntity entity = new PublisherGameEntity(publisherGame);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblpublishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore,FantasyPoints,MasterGameID,DraftPosition,OverallDraftPosition) VALUES " +
                    "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore,@FantasyPoints,@MasterGameID,@DraftPosition,@OverallDraftPosition);",
                    entity);
            }
        }

        public async Task AddPublisherGames(IEnumerable<PublisherGame> publisherGames)
        {
            var entities = publisherGames.Select(x => new PublisherGameEntity(x));

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(
                        "insert into tblpublishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,ManualCriticScore,FantasyPoints,MasterGameID,DraftPosition,OverallDraftPosition) VALUES " +
                        "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@ManualCriticScore,@FantasyPoints,@MasterGameID,@DraftPosition,@OverallDraftPosition);",
                        entities, transaction);
                }
                
            }
        }

        public async Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblpublishergame set MasterGameID = @masterGameID where PublisherGameID = @publisherGameID",
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
                var results = await connection.QueryAsync<SupportedYearEntity>("select * from tblsupportedyear;");
                return results.Select(x => x.ToDomain()).ToList();
            }
        }

        private Task AddPlayerToLeague(League league, FantasyCriticUser inviteUser)
        {
            var userAddObject = new
            {
                leagueID = league.LeagueID,
                userID = inviteUser.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "insert into tblleaguehasuser(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject);
            }
        }

        private async Task DeleteInvite(League league, string emailAddress)
        {
            var deleteObject = new
            {
                leagueID = league.LeagueID,
                emailAddress
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tblleagueinvite where LeagueID = @leagueID and EmailAddress = @emailAddress;",
                    deleteObject);
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
                        "select * from tblleagueyear where LeagueID = @leagueID", new
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
                    "select * from tblpublishergame where tblpublishergame.PublisherID = @publisherID;",
                    query);

                List<PublisherGame> domainGames = new List<PublisherGame>();
                foreach (var entity in gameEntities)
                {
                    Maybe<MasterGameYear> masterGame = null;
                    if (entity.MasterGameID.HasValue)
                    {
                        masterGame = await _masterGameRepo.GetMasterGameYear(entity.MasterGameID.Value, leagueYear);
                    }

                    domainGames.Add(entity.ToDomain(masterGame, leagueYear));
                }

                return domainGames;
            }
        }

        public async Task SetDraftOrder(IEnumerable<KeyValuePair<Publisher, int>> draftPositions)
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
                            "update tblpublisher set DraftPosition = @draftPosition where PublisherID = @publisherID",
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
                            "update tblpublisher set DraftPosition = @draftPosition where PublisherID = @publisherID",
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

        public async Task<SystemWideValues> GetSystemWideValues()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var result = await connection.QuerySingleAsync<SystemWideValuesEntity>("select * from vwsystemwidevalues;");
                return result.ToDomain();
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
                    "delete from tblpublisher where PublisherID = @publisherID;",
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
                    "delete from tblleagueyear where LeagueID = @leagueID and Year = @year;",
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
                    "delete from tblleague where LeagueID = @leagueID;",
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
                    "delete from tblleagueaction where PublisherID = @publisherID;",
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
                    "select count(1) from vwleague " +
                    "join tblleagueyear on (vwleague.LeagueID = tblleagueyear.LeagueID) " +
                    "where PlayStatus <> 'NotStartedDraft' and vwleague.LeagueID = @leagueID;",
                    selectObject);
            }
        }
    }
}
