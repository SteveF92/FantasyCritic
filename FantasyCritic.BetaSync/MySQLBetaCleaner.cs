using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.Entities;
using MoreLinq;
using MySqlConnector;
using NLog;

namespace FantasyCritic.BetaSync
{
    public class MySQLBetaCleaner
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly string _connectionString;

        public MySQLBetaCleaner(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CleanEmailsAndPasswords(IEnumerable<FantasyCriticUser> allUsers, IEnumerable<FantasyCriticUser> betaUsers)
        {
            var nonBetaUsers = allUsers.Except(betaUsers).ToList();
            List<string> updateStatements = new List<string>();
            foreach (var nonBetaUser in nonBetaUsers)
            {
                var fakedEmailAddress = Guid.NewGuid() + "@example.com";
                var fakedNormalizedEmailAddress = fakedEmailAddress.ToUpper();
                string sql = $"UPDATE tbl_user SET EmailAddress = '{fakedEmailAddress}', NormalizedEmailAddress = '{fakedNormalizedEmailAddress}', PasswordHash = '', IsDeleted = 1 WHERE UserID = '{nonBetaUser.UserID}';";
                updateStatements.Add(sql);
            }

            var batches = updateStatements.Batch(500).ToList();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    for (var index = 0; index < batches.Count; index++)
                    {
                        _logger.Info($"Running user clean batch {index + 1}/{batches.Count}");
                        var batch = batches[index];
                        var joinedSQL = string.Join('\n', batch);
                        await connection.ExecuteAsync(joinedSQL, transaction: transaction);
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        internal async Task UpdateMasterGames(IEnumerable<MasterGameTag> productionTags, IEnumerable<MasterGame> productionMasterGames, IEnumerable<MasterGameHasTagEntity> productionGamesHaveTagEntities)
        {
            List<MasterGameTagEntity> tagEntities = productionTags.Select(x => new MasterGameTagEntity(x)).ToList();
            List<MasterGameEntity> masterGameEntities = productionMasterGames.Select(x => new MasterGameEntity(x)).ToList();
            List<MasterSubGameEntity> masterSubGameEntities = productionMasterGames.SelectMany(x => x.SubGames).Select(x => new MasterSubGameEntity(x)).ToList();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync("SET foreign_key_checks = 0;", transaction: transaction);
                    await connection.ExecuteAsync("DELETE FROM tbl_mastergame_hastag;", transaction: transaction);
                    await connection.ExecuteAsync("DELETE FROM tbl_mastergame_subgame;", transaction: transaction);
                    await connection.ExecuteAsync("DELETE FROM tbl_mastergame;", transaction: transaction);
                    await connection.ExecuteAsync("DELETE FROM tbl_mastergame_tag;", transaction: transaction);

                    await connection.BulkInsertAsync(tagEntities, "tbl_mastergame_tag", 500, transaction);
                    await connection.BulkInsertAsync(masterGameEntities, "tbl_mastergame", 500, transaction);
                    await connection.BulkInsertAsync(masterSubGameEntities, "tbl_mastergame_subgame", 500, transaction);
                    await connection.BulkInsertAsync(productionGamesHaveTagEntities, "tbl_mastergame_hastag", 500, transaction);

                    await connection.ExecuteAsync("SET foreign_key_checks = 1;", transaction: transaction);
                    await transaction.CommitAsync();
                }
            }
        }
    }
}
