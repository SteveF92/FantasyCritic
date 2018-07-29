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
using MySql.Data.MySqlClient;

namespace FantasyCritic.MySQL
{
    public class MySQLFantasyCriticRepo : IFantasyCriticRepo
    {
        private readonly string _connectionString;
        private readonly IReadOnlyFantasyCriticUserStore _userStore;

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
                LeagueYear year = yearEntity.ToDomain(requestLeague);
                return year;
            }
        }


        public async Task<IReadOnlyList<FantasyCriticUser>> GetPlayersInLeague(League league)
        {
            var query = new
            {
                leagueID = league.LeagueID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<FantasyCriticUserEntity>(
                    "select tbluser.* from tbluser join tblleagueplayer on (tbluser.UserID = tblleagueplayer.UserID) where tblleagueplayer.LeagueID = @leagueID;",
                    query);

                var users = results.Select(x => x.ToDomain()).ToList();
                return users;
            }
        }

        public async Task CreateLeague(League league, int initialYear, LeagueOptions options)
        {
            LeagueEntity entity = new LeagueEntity(league);
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, initialYear, options);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblleague(LeagueID,LeagueName,LeagueManager,DraftGames,WaiverGames,AntiPicks,EstimatedGameScore,EligibilitySystem,DraftSystem,WaiverSystem,ScoringSystem) VALUES " +
                    "(@LeagueID,@LeagueName,@LeagueManager,@DraftGames,@WaiverGames,@AntiPicks,@EstimatedGameScore,@EligibilitySystem,@DraftSystem,@WaiverSystem,@ScoringSystem);",
                    entity);

                await connection.ExecuteAsync(
                    "insert into tblleagueyear(LeagueID,Year) VALUES (@LeagueID, @Year);",
                    leagueYearEntity);
            }

            await AddPlayerToLeague(league, league.LeagueManager);
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

        public async Task<IReadOnlyList<int>> GetOpenYears()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<int>("select tblsupportedyear.Year from tblsupportedyear where OpenForPlay = true");
                return results.ToList();
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
                    "select * from tblleague join tblleagueplayer on (tblleague.LeagueID = tblleagueplayer.LeagueID) where tblleagueplayer.UserID = @userID;", queryObject);
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

        public async Task<IReadOnlyList<MasterGame>> GetMasterGames()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var results = await connection.QueryAsync<MasterGameEntity>(
                    "select * from tblmastergame;");

                var masterGames = results.Select(x => x.ToDomain()).ToList();
                return masterGames;
            }
        }

        public async Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                MasterGameEntity result = await connection.QuerySingleOrDefaultAsync<MasterGameEntity>(
                    "select * from tblmastergame where MasterGameID = @masterGameID", new { masterGameID });

                if (result == null)
                {
                    return Maybe<MasterGame>.None;
                }

                MasterGame domain = result.ToDomain();
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

        public async Task AddPlayerGame(League requestLeague, PlayerGame playerGame)
        {
            PlayerHasGameEntity entity = new PlayerHasGameEntity(requestLeague, playerGame);

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "insert into tblplayerhasgame (LeagueID,Year,UserID,GameName,Timestamp,Waiver,AntiPick,FantasyScore,MasterGameID) VALUES " +
                    "(@LeagueID,@Year,@UserID,@GameName,@Timestamp,@Waiver,@AntiPick,@FantasyScore,@MasterGameID);",
                    entity);
            }
        }

        public async Task<IReadOnlyList<PlayerGame>> GetPlayerGames(League league, FantasyCriticUser user)
        {
            var query = new
            {
                leagueID = league.LeagueID,
                userID = user.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                IEnumerable<PlayerHasGameEntity> entities = await connection.QueryAsync<PlayerHasGameEntity>(
                    "select * from tblplayerhasgame where LeagueID = @leagueID and UserID = @UserID",
                    query);

                List<PlayerGame> playerGames = new List<PlayerGame>();
                foreach (var entity in entities)
                {
                    Maybe<MasterGame> masterGame = null;
                    if (entity.MasterGameID.HasValue)
                    {
                        masterGame = await GetMasterGame(entity.MasterGameID.Value);
                    }

                    playerGames.Add(entity.ToDomain(user, masterGame));
                }

                return playerGames;
            }
        }

        public async Task<bool> GameIsEligible(MasterGame masterGame, EligibilitySystem eligibilitySystem)
        {
            var query = new
            {
                masterGameID = masterGame.MasterGameID,
                eligibilitySystem = eligibilitySystem.Value
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                dynamic entity = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "select * from tblmastergameeligibility where EligibilitySystem = @eligibilitySystem and MasterGameID = @masterGameID;",
                    query);

                return entity != null;
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
                    "insert into tblleagueplayer(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject);
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
    }
}
