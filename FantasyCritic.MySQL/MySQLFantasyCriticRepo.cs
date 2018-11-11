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

namespace FantasyCritic.MySQL
{
    public class MySQLFantasyCriticRepo : IFantasyCriticRepo
    {
        private readonly string _connectionString;
        private readonly IReadOnlyFantasyCriticUserStore _userStore;
        private IReadOnlyList<EligibilityLevel> _eligibilityLevels;

        public MySQLFantasyCriticRepo(string connectionString, IReadOnlyFantasyCriticUserStore userStore)
        {
            _connectionString = connectionString;
            _userStore = userStore;
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
                    "select * from tblleague where LeagueID = @leagueID", queryObject);

                FantasyCriticUser manager = await _userStore.FindByIdAsync(leagueEntity.LeagueManager.ToString(), CancellationToken.None);

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tblleagueyear where LeagueID = @leagueID", queryObject);
                IEnumerable<int> years = yearEntities.Select(x => x.Year);

                League league = leagueEntity.ToDomain(manager, years);
                return league;
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

                var eligibilityLevel = await GetEligibilityLevel(yearEntity.MaximumEligibilityLevel);
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
                foreach (var entity in yearEntities)
                {
                    var league = await GetLeagueByID(entity.LeagueID);
                    if (league.HasNoValue)
                    {
                        throw new Exception($"Cannot find league for league-year (should never happen) LeagueID: {entity.LeagueID}");
                    }

                    var eligibilityLevel = await GetEligibilityLevel(entity.MaximumEligibilityLevel);
                    LeagueYear leagueYear = entity.ToDomain(league.Value, eligibilityLevel);
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
                await connection.ExecuteAsync(
                    "update tblpublishergame SET FantasyPoints = @FantasyPoints where PublisherGameID = @PublisherGameID;",
                    updateEntities);
            }
        }

        public async Task CreateMasterGame(MasterGame masterGame)
        {
            var entity = new MasterGameEntity(masterGame);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblmastergame(MasterGameID,GameName,EstimatedReleaseDate,ReleaseDate,OpenCriticID,CriticScore,MinimumReleaseYear,EligibilityLevel,YearlyInstallment,EarlyAccess,BoxartFileName) VALUES " +
                    "(@MasterGameID,@GameName,@EstimatedReleaseDate,@ReleaseDate,@OpenCriticID,@CriticScore,@MinimumReleaseYear,@EligibilityLevel,@YearlyInstallment,@EarlyAccess,@BoxartFileName);",
                    entity);
            }
        }

        public async Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel)
        {
            var eligbilityLevel = await GetEligibilityLevels();
            return eligbilityLevel.Single(x => x.Level == eligibilityLevel);
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
                    var masterGame = await GetMasterGame(bidEntity.MasterGameID);

                    PickupBid domain = bidEntity.ToDomain(publisher, masterGame.Value);
                    domainBids.Add(domain);
                }

                return domainBids;
            }
        }

        public async Task MarkBidStatus(PickupBid bid, bool success)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblpickupbid SET Successful = @success where BidID = @bidID;",
                    new { bidID = bid.BidID, success });
            }
        }

        public async Task SpendBudget(Publisher publisher, uint bidAmount)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblpublisher SET Budget = Budget - @bidAmount where PublisherID = @publisherID;",
                    new { publisherID = publisher.PublisherID, bidAmount });
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

        public async Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<LeagueActionEntity>(
                    "select * from tblleagueaction " +                          
                    "join tblpublisher on (tblleagueaction.PublisherID = tblpublisher.PublisherID) " +
                    "where tblpublisher.LeagueID = @leagueID and tblpublisher.Year = @leagueYear;",
                    new
                    {
                        leagueID = leagueYear.League.LeagueID,
                        leagueYear = leagueYear.Year
                    });

                List<LeagueAction> leagueActions = new List<LeagueAction>();
                foreach (var entity in entities)
                {
                    Publisher publisher = (await GetPublisher(entity.PublisherID)).Value;
                    LeagueAction leagueAction = entity.ToDomain(publisher);
                    leagueActions.Add(leagueAction);
                }

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

        public async Task ChangeLeagueName(League league, string leagueName)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblleague SET LeagueName = @leagueName where LeagueID = @leagueID;",
                    new { leagueID = league.LeagueID, leagueName });
            }
        }

        public async Task StartDraft(LeagueYear leagueYear)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    $"update tblleagueyear SET PlayStatus = '{PlayStatus.DraftingStandard.Value}' WHERE LeagueID = @leagueID and Year = @year",
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
                var masterGame = await GetMasterGame(bidEntity.MasterGameID);

                PickupBid domain = bidEntity.ToDomain(publisher.Value, masterGame.Value);
                return domain;
            }
        }

        public async Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            if (_eligibilityLevels != null)
            {
                return _eligibilityLevels;
            }
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<EligibilityLevelEntity>("select * from tbleligibilitylevel;");
                _eligibilityLevels =  entities.Select(x => x.ToDomain()).ToList();
                return _eligibilityLevels;
            }
        }

        public async Task CreateLeague(League league, int initialYear, LeagueOptions options)
        {
            LeagueEntity entity = new LeagueEntity(league);
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, initialYear, options, PlayStatus.NotStartedDraft);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblleague(LeagueID,LeagueName,LeagueManager) VALUES " +
                    "(@LeagueID,@LeagueName,@LeagueManager);",
                    entity);

                await connection.ExecuteAsync(
                    "insert into tblleagueyear(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,EstimatedCriticScore,MaximumEligibilityLevel,AllowYearlyInstallments,AllowEarlyAccess,DraftSystem,PickupSystem,ScoringSystem,PlayStarted) VALUES " +
                    "(@LeagueID,@Year,@StandardGames,@GamesToDraft,,@CounterPicks,@EstimatedCriticScore,@MaximumEligibilityLevel,@AllowYearlyInstallments,@AllowEarlyAccess,@DraftSystem,@PickupSystem,@ScoringSystem,@PlayStarted);",
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
                    "update tblleagueyear SET StandardGames = @StandardGames, GamesToDraft = @GamesToDraft, CounterPicks = @CounterPicks, EstimatedCriticScore = @EstimatedCriticScore, " +
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
                    "insert into tblleagueyear(LeagueID,Year,StandardGames,GamesToDraft,CounterPicks,EstimatedCriticScore,MaximumEligibilityLevel,AllowYearlyInstallments,AllowEarlyAccess,DraftSystem,PickupSystem,ScoringSystem) VALUES " +
                    "(@LeagueID,@Year,@StandardGames,@GamesToDraft,@CounterPicks,@EstimatedCriticScore,@MaximumEligibilityLevel,@AllowYearlyInstallments,@AllowEarlyAccess,@DraftSystem,@PickupSystem,@ScoringSystem);",
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
                    "select tblleague.* from tblleague join tblleaguehasuser on (tblleague.LeagueID = tblleaguehasuser.LeagueID) where tblleaguehasuser.UserID = @userID;", queryObject);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
        }

        public async Task<IReadOnlyList<League>> GetLeaguesInvitedTo(FantasyCriticUser currentUser)
        {
            var query = new
            {
                userID = currentUser.UserID
            };

            IEnumerable<LeagueEntity> leagueEntities;
            using (var connection = new MySqlConnection(_connectionString))
            {
                leagueEntities = await connection.QueryAsync<LeagueEntity>(
                    "select tblleague.* from tblleague join tblleagueinvite on (tblleague.LeagueID = tblleagueinvite.LeagueID) where tblleagueinvite.UserID = @userID;",
                    query);
            }

            IReadOnlyList<League> leagues = await ConvertLeagueEntitiesToDomain(leagueEntities);
            return leagues;
        }

        public Task SaveInvite(League league, FantasyCriticUser user)
        {
            var saveObject = new
            {
                leagueID = league.LeagueID,
                userID = user.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "insert into tblleagueinvite(LeagueID,UserID) VALUES (@leagueID, @userID);",
                    saveObject);
            }
        }

        public async Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(League league)
        {
            var query = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<FantasyCriticUserEntity>(
                    "select tbluser.* from tbluser join tblleagueinvite on (tbluser.UserID = tblleagueinvite.UserID) where tblleagueinvite.LeagueID = @leagueID;",
                    query);

                var users = results.Select(x => x.ToDomain()).ToList();
                return users;
            }
        }

        public async Task AcceptInvite(League league, FantasyCriticUser inviteUser)
        {
            await AddPlayerToLeague(league, inviteUser);
            await DeleteInvite(league, inviteUser);
        }

        public Task DeclineInvite(League league, FantasyCriticUser inviteUser)
        {
            return DeleteInvite(league, inviteUser);
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

                Maybe<MasterGame> masterGame = null;
                if (gameEntity.MasterGameID.HasValue)
                {
                    masterGame = await GetMasterGame(gameEntity.MasterGameID.Value);
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

        public async Task AddPublisherGame(Publisher publisher, PublisherGame publisherGame)
        {
            PublisherGameEntity entity = new PublisherGameEntity(publisher, publisherGame);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblpublishergame (PublisherGameID,PublisherID,GameName,Timestamp,CounterPick,FantasyPoints,MasterGameID) VALUES " +
                    "(@PublisherGameID,@PublisherID,@GameName,@Timestamp,@CounterPick,@FantasyPoints,@MasterGameID);",
                    entity);
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

        public async Task<IReadOnlyList<MasterGame>> GetMasterGames()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var masterGameResults = await connection.QueryAsync<MasterGameEntity>("select * from tblmastergame;");
                var masterSubGameResults = await connection.QueryAsync<MasterSubGameEntity>("select * from tblmastersubgame;");

                var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
                List<MasterGame> masterGames = new List<MasterGame>();
                foreach (var entity in masterGameResults)
                {
                    EligibilityLevel eligibilityLevel = await GetEligibilityLevel(entity.EligibilityLevel);
                    MasterGame domain =
                        entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID),
                            eligibilityLevel);
                    masterGames.Add(domain);
                }
                
                return masterGames;
            }
        }

        public async Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                MasterGameEntity masterGame = await connection.QuerySingleOrDefaultAsync<MasterGameEntity>("select * from tblmastergame where MasterGameID = @masterGameID", new { masterGameID });
                if (masterGame == null)
                {
                    return Maybe<MasterGame>.None;
                }

                IEnumerable<MasterSubGameEntity> masterSubGames = await connection.QueryAsync<MasterSubGameEntity>("select * from tblmastersubgame where MasterGameID = @masterGameID", new { masterGameID });

                EligibilityLevel eligibilityLevel = await GetEligibilityLevel(masterGame.EligibilityLevel);
                MasterGame domain = masterGame.ToDomain(masterSubGames.Select(x => x.ToDomain()), eligibilityLevel);
                return Maybe<MasterGame>.From(domain);
            }
        }

        public async Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
        {
            DateTime? releaseDate = null;
            if (openCriticGame.ReleaseDate.HasValue)
            {
                releaseDate = openCriticGame.ReleaseDate.Value.ToDateTimeUnspecified();
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblmastergame set ReleaseDate = @releaseDate, CriticScore = @criticScore where MasterGameID = @masterGameID",
                    new
                    {
                        masterGameID = masterGame.MasterGameID,
                        releaseDate = releaseDate,
                        criticScore = openCriticGame.Score
                    });
            }
        }

        public async Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
        {
            DateTime? releaseDate = null;
            if (openCriticGame.ReleaseDate.HasValue)
            {
                releaseDate = openCriticGame.ReleaseDate.Value.ToDateTimeUnspecified();
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("update tblmastersubgame set ReleaseDate = @releaseDate, CriticScore = @criticScore where MasterSubGameID = @masterSubGameID",
                    new
                    {
                        masterSubGameID = masterSubGame.MasterSubGameID,
                        releaseDate = releaseDate,
                        criticScore = openCriticGame.Score
                    });
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

        private async Task DeleteInvite(League league, FantasyCriticUser inviteUser)
        {
            var deleteObject = new
            {
                leagueID = league.LeagueID,
                userID = inviteUser.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tblleagueinvite where LeagueID = @leagueID and UserID = @userID;",
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
                    Maybe<MasterGame> masterGame = null;
                    if (entity.MasterGameID.HasValue)
                    {
                        masterGame = await GetMasterGame(entity.MasterGameID.Value);
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
    }
}
