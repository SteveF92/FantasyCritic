using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.MySQL.Entities;
using MySql.Data.MySqlClient;
using NodaTime;

namespace FantasyCritic.MySQL
{
    public class MySQLMasterGameRepo : IMasterGameRepo
    {
        private readonly string _connectionString;
        private IReadOnlyList<EligibilityLevel> _eligibilityLevels;
        private readonly Dictionary<int, Dictionary<Guid, MasterGameYear>> _masterGameYearsCache;
        private readonly IReadOnlyFantasyCriticUserStore _userStore;

        private Dictionary<Guid, MasterGame> _masterGamesCache;

        public MySQLMasterGameRepo(string connectionString, IReadOnlyFantasyCriticUserStore userStore)
        {
            _connectionString = connectionString;
            _userStore = userStore;
            _masterGamesCache = new Dictionary<Guid, MasterGame>();
            _masterGameYearsCache = new Dictionary<int, Dictionary<Guid, MasterGameYear>>();
        }

        public async Task<IReadOnlyList<MasterGame>> GetMasterGames()
        {
            if (_masterGamesCache.Any())
            {
                return _masterGamesCache.Values.ToList();
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                var masterGameResults = await connection.QueryAsync<MasterGameEntity>("select * from tbl_mastergame;");
                var masterSubGameResults = await connection.QueryAsync<MasterSubGameEntity>("select * from tbl_mastergame_subgame;");

                var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
                List<MasterGame> masterGames = new List<MasterGame>();
                foreach (var entity in masterGameResults)
                {
                    EligibilityLevel eligibilityLevel = await GetEligibilityLevel(entity.EligibilityLevel);
                    MasterGame domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID),
                            eligibilityLevel);
                    masterGames.Add(domain);
                }

                _masterGamesCache = masterGames.ToDictionary(x => x.MasterGameID, y => y);
                return masterGames;
            }
        }

        public async Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year, bool useCache)
        {
            if (useCache && _masterGameYearsCache.ContainsKey(year))
            {
                return _masterGameYearsCache[year].Values.ToList();
            }

            string sqlSource = useCache ? "tbl_caching_mastergameyear" : "vw_cacher_mastergameyear";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var masterGameResults = await connection.QueryAsync<MasterGameYearEntity>($"select * from {sqlSource} where Year = @year;", new { year });
                var masterSubGameResults = await connection.QueryAsync<MasterSubGameEntity>("select * from tbl_mastergame_subgame;");

                var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
                List<MasterGameYear> masterGames = new List<MasterGameYear>();
                foreach (var entity in masterGameResults)
                {
                    EligibilityLevel eligibilityLevel = await GetEligibilityLevel(entity.EligibilityLevel);
                    MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID),
                            eligibilityLevel, year);
                    masterGames.Add(domain);
                }

                if (useCache)
                {
                    _masterGameYearsCache[year] = masterGames.ToDictionary(x => x.MasterGame.MasterGameID, y => y);
                }

                return masterGames;
            }
        }

        public async Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            if (!_masterGamesCache.Any())
            {
                await GetMasterGames();
            }

            _masterGamesCache.TryGetValue(masterGameID, out MasterGame foundMasterGame);
            if (foundMasterGame is null)
            {
                return Maybe<MasterGame>.None;
            }

            return foundMasterGame;
        }

        public async Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year)
        {
            if (!_masterGameYearsCache.ContainsKey(year))
            {
                await GetMasterGameYears(year, true);
            }

            var yearCache = _masterGameYearsCache[year];
            yearCache.TryGetValue(masterGameID, out MasterGameYear foundMasterGame);
            if (foundMasterGame is null)
            {
                return Maybe<MasterGameYear>.None;
            }

            return foundMasterGame;
        }

        public async Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
        {
            DateTime? releaseDate = null;
            if (openCriticGame.ReleaseDate.HasValue)
            {
                releaseDate = openCriticGame.ReleaseDate.Value.ToDateTimeUnspecified();
            }

            string setFirstTimestamp = "";
            if (!masterGame.CriticScore.HasValue && openCriticGame.Score.HasValue)
            {
                setFirstTimestamp = ", FirstCriticScoreTimestamp = CURRENT_TIMESTAMP ";
            }

            string setReleaseDate = "";
            if (!masterGame.DoNotRefreshDate)
            {
                setFirstTimestamp = ", ReleaseDate = @releaseDate ";
            }

            string sql = $"update tbl_mastergame set CriticScore = @criticScore {setReleaseDate} {setFirstTimestamp} where MasterGameID = @masterGameID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql,
                    new
                    {
                        masterGameID = masterGame.MasterGameID,
                        criticScore = openCriticGame.Score,
                        releaseDate,
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
                await connection.ExecuteAsync("update tbl_mastergame_subgame set ReleaseDate = @releaseDate, CriticScore = @criticScore where MasterSubGameID = @masterSubGameID",
                    new
                    {
                        masterSubGameID = masterSubGame.MasterSubGameID,
                        releaseDate = releaseDate,
                        criticScore = openCriticGame.Score
                    });
            }
        }

        public async Task CreateMasterGame(MasterGame masterGame)
        {
            var entity = new MasterGameEntity(masterGame);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_mastergame(MasterGameID,GameName,EstimatedReleaseDate,SortableEstimatedReleaseDate,ReleaseDate,OpenCriticID,CriticScore,MinimumReleaseDate," +
                    "EligibilityLevel,YearlyInstallment,EarlyAccess,FreeToPlay,ReleasedInternationally,ExpansionPack,UnannouncedGame,Notes,BoxartFileName) VALUES " +
                    "(@MasterGameID,@GameName,@EstimatedReleaseDate,@SortableEstimatedReleaseDate,@ReleaseDate,@OpenCriticID,@CriticScore,@MinimumReleaseDate," +
                    "@EligibilityLevel,@YearlyInstallment,@EarlyAccess,@FreeToPlay,@ReleasedInternationally,@ExpansionPack,@UnannouncedGame,@Notes,@BoxartFileName);",
                    entity);
            }
        }

        public async Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel)
        {
            var eligbilityLevel = await GetEligibilityLevels();
            return eligbilityLevel.Single(x => x.Level == eligibilityLevel);
        }

        public async Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            if (_eligibilityLevels != null)
            {
                return _eligibilityLevels;
            }
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<EligibilityLevelEntity>("select * from tbl_settings_eligibilitylevel;");
                _eligibilityLevels = entities.Select(x => x.ToDomain()).ToList();
                return _eligibilityLevels;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year)
        {
            var sql = "select distinct MasterGameID from tbl_league_publishergame " +
                      "join tbl_league_publisher on(tbl_league_publisher.PublisherID = tbl_league_publishergame.PublisherID) " +
                      "join tbl_league on (tbl_league.LeagueID = tbl_league_publisher.LeagueID) " +
                      "where Year = @year and tbl_league.TestLeague = 0 and tbl_league.IsDeleted = 0 and MasterGameID IS NOT NULL;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<Guid> guids = await connection.QueryAsync<Guid>(sql, new { year });
                return guids.ToList();
            }
        }

        public async Task CreateMasterGameRequest(MasterGameRequest domainRequest)
        {
            var entity = new MasterGameRequestEntity(domainRequest);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_mastergame_request(RequestID,UserID,RequestTimestamp,RequestNote,GameName,SteamID,OpenCriticID,ReleaseDate,EstimatedReleaseDate,EligibilityLevel," +
                    "YearlyInstallment,EarlyAccess,FreeToPlay,ReleasedInternationally,ExpansionPack,UnannouncedGame,Answered,ResponseTimestamp,ResponseNote,MasterGameID,Hidden) VALUES " +
                    "(@RequestID,@UserID,@RequestTimestamp,@RequestNote,@GameName,@SteamID,@OpenCriticID,@ReleaseDate,@EstimatedReleaseDate," +
                    "@EligibilityLevel,@YearlyInstallment,@EarlyAccess,@FreeToPlay,@ReleasedInternationally,@ExpansionPack,@UnannouncedGame,@Answered,@ResponseTimestamp,@ResponseNote,@MasterGameID,@Hidden);",
                    entity);
            }
        }

        public async Task CreateMasterGameChangeRequest(MasterGameChangeRequest domainRequest)
        {
            var entity = new MasterGameChangeRequestEntity(domainRequest);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tbl_mastergame_changerequest(RequestID,UserID,RequestTimestamp,RequestNote,MasterGameID,OpenCriticID,Answered,ResponseTimestamp,ResponseNote,Hidden) VALUES " +
                    "(@RequestID,@UserID,@RequestTimestamp,@RequestNote,@MasterGameID,@OpenCriticID,@Answered,@ResponseTimestamp,@ResponseNote,@Hidden);",
                    entity);
            }
        }

        public async Task<IReadOnlyList<MasterGameRequest>> GetAllMasterGameRequests()
        {
            var sql = "select * from tbl_mastergame_request where Answered = 0";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<MasterGameRequestEntity> entities = await connection.QueryAsync<MasterGameRequestEntity>(sql);
                return await ConvertMasterGameRequestEntities(entities);
            }
        }

        public async Task<IReadOnlyList<MasterGameChangeRequest>> GetAllMasterGameChangeRequests()
        {
            var sql = "select * from tbl_mastergame_changerequest where Answered = 0";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<MasterGameChangeRequestEntity> entities = await connection.QueryAsync<MasterGameChangeRequestEntity>(sql);
                return await ConvertMasterGameChangeRequestEntities(entities);
            }
        }

        public async Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote, Maybe<MasterGame> masterGame)
        {
            Guid? masterGameID = null;
            if (masterGame.HasValue)
            {
                masterGameID = masterGame.Value.MasterGameID;
            }
            string sql = "update tbl_mastergame_request set Answered = 1, ResponseTimestamp = @responseTime, " +
                         "ResponseNote = @responseNote, MasterGameID = @masterGameID where RequestID = @requestID;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql,
                    new
                    {
                        requestID = masterGameRequest.RequestID,
                        masterGameID,
                        responseTime = responseTime.ToDateTimeUtc(),
                        responseNote
                    });
            }
        }

        public async Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime, string responseNote)
        {
            string sql = "update tbl_mastergame_changerequest set Answered = 1, ResponseTimestamp = @responseTime, " +
                         "ResponseNote = @responseNote where RequestID = @requestID;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql,
                    new
                    {
                        requestID = masterGameRequest.RequestID,
                        responseTime = responseTime.ToDateTimeUtc(),
                        responseNote
                    });
            }
        }

        public async Task<IReadOnlyList<MasterGameRequest>> GetMasterGameRequestsForUser(FantasyCriticUser user)
        {
            var sql = "select * from tbl_mastergame_request where UserID = @userID and Hidden = 0";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<MasterGameRequestEntity> entities = await connection.QueryAsync<MasterGameRequestEntity>(sql, new { userID = user.UserID });
                return await ConvertMasterGameRequestEntities(entities);
            }
        }

        public async Task<IReadOnlyList<MasterGameChangeRequest>> GetMasterGameChangeRequestsForUser(FantasyCriticUser user)
        {
            var sql = "select * from tbl_mastergame_changerequest where UserID = @userID and Hidden = 0";

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<MasterGameChangeRequestEntity> entities = await connection.QueryAsync<MasterGameChangeRequestEntity>(sql, new { userID = user.UserID });
                return await ConvertMasterGameChangeRequestEntities(entities);
            }
        }

        private async Task<IReadOnlyList<MasterGameRequest>> ConvertMasterGameRequestEntities(IEnumerable<MasterGameRequestEntity> entities)
        {
            var eligibilityLevels = await GetEligibilityLevels();
            var masterGames = await GetMasterGames();
            var users = await _userStore.GetAllUsers();
            List<MasterGameRequest> domainRequests = new List<MasterGameRequest>();
            foreach (var entity in entities)
            {
                EligibilityLevel eligibilityLevel = eligibilityLevels.Single(x => x.Level == entity.EligibilityLevel);
                Maybe<MasterGame> masterGame = Maybe<MasterGame>.None;
                if (entity.MasterGameID.HasValue)
                {
                    masterGame = masterGames.Single(x => x.MasterGameID == entity.MasterGameID.Value);
                }

                MasterGameRequest domain = entity.ToDomain(users.Single(x => x.UserID == entity.UserID), eligibilityLevel, masterGame);
                domainRequests.Add(domain);
            }

            return domainRequests;
        }

        private async Task<IReadOnlyList<MasterGameChangeRequest>> ConvertMasterGameChangeRequestEntities(IEnumerable<MasterGameChangeRequestEntity> entities)
        {
            var masterGames = await GetMasterGames();
            var users = await _userStore.GetAllUsers();
            List<MasterGameChangeRequest> domainRequests = new List<MasterGameChangeRequest>();
            foreach (var entity in entities)
            {
                var masterGame = masterGames.Single(x => x.MasterGameID == entity.MasterGameID);
                MasterGameChangeRequest domain = entity.ToDomain(users.Single(x => x.UserID == entity.UserID), masterGame);
                domainRequests.Add(domain);
            }

            return domainRequests;
        }

        public async Task<Maybe<MasterGameRequest>> GetMasterGameRequest(Guid requestID)
        {
            var sql = "select * from tbl_mastergame_request where RequestID = @requestID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                MasterGameRequestEntity entity = await connection.QuerySingleOrDefaultAsync<MasterGameRequestEntity>(sql, new { requestID });
                if (entity == null)
                {
                    return Maybe<MasterGameRequest>.None;
                }

                var eligibilityLevel = await GetEligibilityLevel(entity.EligibilityLevel);
                Maybe<MasterGame> masterGame = Maybe<MasterGame>.None;
                if (entity.MasterGameID.HasValue)
                {
                    masterGame = await GetMasterGame(entity.MasterGameID.Value);
                }

                var user = await _userStore.FindByIdAsync(entity.UserID.ToString(), CancellationToken.None);

                return entity.ToDomain(user, eligibilityLevel, masterGame);
            }
        }

        public async Task<Maybe<MasterGameChangeRequest>> GetMasterGameChangeRequest(Guid requestID)
        {
            var sql = "select * from tbl_mastergame_changerequest where RequestID = @requestID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                MasterGameChangeRequestEntity entity = await connection.QuerySingleOrDefaultAsync<MasterGameChangeRequestEntity>(sql, new { requestID });
                if (entity == null)
                {
                    return Maybe<MasterGameChangeRequest>.None;
                }

                var masterGame = await GetMasterGame(entity.MasterGameID);

                if (masterGame.HasNoValue)
                {
                    throw new Exception($"Something has gone horribly wrong with master game change requests. ID: {requestID}");
                }

                var user = await _userStore.FindByIdAsync(entity.UserID.ToString(), CancellationToken.None);

                return entity.ToDomain(user, masterGame.Value);
            }
        }

        public async Task DeleteMasterGameRequest(MasterGameRequest request)
        {
            var deleteObject = new
            {
                requestID = request.RequestID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tbl_mastergame_request where RequestID = @requestID;",
                    deleteObject);
            }
        }

        public async Task DeleteMasterGameChangeRequest(MasterGameChangeRequest request)
        {
            var deleteObject = new
            {
                requestID = request.RequestID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "delete from tbl_mastergame_changerequest where RequestID = @requestID;",
                    deleteObject);
            }
        }

        public async Task DismissMasterGameRequest(MasterGameRequest masterGameRequest)
        {
            var dismissObject = new
            {
                requestID = masterGameRequest.RequestID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "update tbl_mastergame_request SET Hidden = 1 where RequestID = @requestID;",
                    dismissObject);
            }
        }

        public async Task DismissMasterGameChangeRequest(MasterGameChangeRequest request)
        {
            var dismissObject = new
            {
                requestID = request.RequestID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "update tbl_mastergame_changerequest SET Hidden = 1 where RequestID = @requestID;",
                    dismissObject);
            }
        }

        public async Task LinkToOpenCritic(MasterGame masterGame, int openCriticID)
        {
            var linkObject = new
            {
                masterGameID = masterGame.MasterGameID,
                openCriticID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "update tbl_mastergame SET OpenCriticID = @openCriticID where MasterGameID = @masterGameID;",
                    linkObject);
            }
        }

        public async Task UpdateReleaseDateEstimates(LocalDate tomorrow)
        {
            var sql = "UPDATE tbl_mastergame SET MinimumReleaseDate = ReleaseDate, SortableEstimatedReleaseDate = ReleaseDate, EstimatedReleaseDate = ReleaseDate where ReleaseDate is not NULL;";
            var sql2 = "UPDATE tbl_mastergame SET MinimumReleaseDate = @tomorrow WHERE MinimumReleaseDate < @tomorrow AND ReleaseDate IS NULL;";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(sql);
                    await connection.ExecuteAsync(sql2, new { tomorrow = tomorrow.ToDateTimeUnspecified() });
                    transaction.Commit();
                }
            }
        }

        public async Task UpdateHypeFactors(IEnumerable<MasterGameCalculatedStats> hypeScores, int year)
        {
            List<MasterGameYearEntity> masterGameYearEntities = hypeScores.Select(x => new MasterGameYearEntity(x)).ToList();

            var excludeFields = new List<string>() {"DoNotRefreshDate", "DoNotRefreshAnything"};
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync("delete from tbl_caching_mastergameyear where Year = @year", new {year});
                    await connection.BulkInsertAsync(masterGameYearEntities, "tbl_caching_mastergameyear", 500, transaction, excludeFields);
                    transaction.Commit();
                }
            }
        }
    }
}
