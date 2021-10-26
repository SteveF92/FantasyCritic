using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FantasyCritic.Lib.Domain;
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

        public async Task UpdateMasterGames(IEnumerable<MasterGame> productionMasterGames)
        {
            return;
        }
    }
}
