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

        public async Task<Maybe<FantasyCriticLeague>> GetLeagueByID(Guid id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var queryObject = new
                {
                    leagueID = id
                };

                FantasyCriticLeagueEntity leagueEntity = await connection.QuerySingleAsync<FantasyCriticLeagueEntity>(
                    "select * from tblleague where LeagueID = @leagueID", queryObject);

                FantasyCriticUser manager = await _userStore.FindByIdAsync(leagueEntity.LeagueManager.ToString(), CancellationToken.None);

                IEnumerable<LeagueYearEntity> yearEntities = await connection.QueryAsync<LeagueYearEntity>("select * from tblleagueyear where LeagueID = @leagueID", queryObject);
                IEnumerable<int> years = yearEntities.Select(x => x.Year);

                FantasyCriticLeague league = leagueEntity.ToDomain(manager, years);
                return league;
            }
        }

        public async Task<IReadOnlyList<FantasyCriticUser>> GetPlayersInLeague(FantasyCriticLeague league)
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

        public async Task CreateLeague(FantasyCriticLeague league, int initialYear)
        {
            FantasyCriticLeagueEntity entity = new FantasyCriticLeagueEntity(league);
            LeagueYearEntity leagueYearEntity = new LeagueYearEntity(league, initialYear);

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

        public Task SaveInvite(FantasyCriticLeague league, FantasyCriticUser user)
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

        public async Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(FantasyCriticLeague league)
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

        public async Task AcceptInvite(FantasyCriticLeague league, FantasyCriticUser inviteUser)
        {
            await AddPlayerToLeague(league, inviteUser);

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

        private Task AddPlayerToLeague(FantasyCriticLeague league, FantasyCriticUser inviteUser)
        {
            var userAddObject = new
            {
                leagueID = league.LeagueID,
                userID = league.LeagueManager.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(
                    "insert into tblleagueplayer(LeagueID,UserID) VALUES (@leagueID,@userID);", userAddObject);
            }
        }
    }
}
