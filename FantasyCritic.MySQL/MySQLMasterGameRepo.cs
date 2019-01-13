using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.MySQL.Entities;
using MySql.Data.MySqlClient;

namespace FantasyCritic.MySQL
{
    public class MySQLMasterGameRepo : IMasterGameRepo
    {
        private readonly string _connectionString;
        private IReadOnlyList<EligibilityLevel> _eligibilityLevels;

        public MySQLMasterGameRepo(string connectionString)
        {
            _connectionString = connectionString;
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
                    MasterGame domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID),
                            eligibilityLevel);
                    masterGames.Add(domain);
                }

                return masterGames;
            }
        }

        public async Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var masterGameResults = await connection.QueryAsync<MasterGameYearEntity>("select * from vwmastergame where Year = @year;", new { year });
                var masterSubGameResults = await connection.QueryAsync<MasterSubGameEntity>("select * from tblmastersubgame;");

                var masterSubGames = masterSubGameResults.Select(x => x.ToDomain()).ToList();
                List<MasterGameYear> masterGames = new List<MasterGameYear>();
                foreach (var entity in masterGameResults)
                {
                    EligibilityLevel eligibilityLevel = await GetEligibilityLevel(entity.EligibilityLevel);
                    MasterGameYear domain = entity.ToDomain(masterSubGames.Where(sub => sub.MasterGameID == entity.MasterGameID),
                            eligibilityLevel, year);
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

        public async Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                MasterGameYearEntity masterGame = await connection.QuerySingleOrDefaultAsync<MasterGameYearEntity>("select * from vwmastergame where MasterGameID = @masterGameID and Year = @year", new { masterGameID, year });
                if (masterGame == null)
                {
                    return Maybe<MasterGameYear>.None;
                }

                IEnumerable<MasterSubGameEntity> masterSubGames = await connection.QueryAsync<MasterSubGameEntity>("select * from tblmastersubgame where MasterGameID = @masterGameID", new { masterGameID });

                EligibilityLevel eligibilityLevel = await GetEligibilityLevel(masterGame.EligibilityLevel);
                MasterGameYear domain = masterGame.ToDomain(masterSubGames.Select(x => x.ToDomain()), eligibilityLevel, year);
                return Maybe<MasterGameYear>.From(domain);
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

        public async Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            if (_eligibilityLevels != null)
            {
                return _eligibilityLevels;
            }
            using (var connection = new MySqlConnection(_connectionString))
            {
                var entities = await connection.QueryAsync<EligibilityLevelEntity>("select * from tbleligibilitylevel;");
                _eligibilityLevels = entities.Select(x => x.ToDomain()).ToList();
                return _eligibilityLevels;
            }
        }
    }
}
