using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FantasyCritic.Lib.Identity;
using MySqlConnector;
using Serilog;

namespace FantasyCritic.MySQL.SyncingRepos;

public class MySQLBetaCleaner
{
    private static readonly ILogger _logger = Log.ForContext<MySQLBetaCleaner>();

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
            string sql = $"UPDATE tbl_user SET EmailAddress = '{fakedEmailAddress}', NormalizedEmailAddress = '{fakedNormalizedEmailAddress}', PasswordHash = null, AuthenticatorKey = null, IsDeleted = 1 WHERE UserID = '{nonBetaUser.Id}';";
            updateStatements.Add(sql);
        }

        var batches = updateStatements.Chunk(500).ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        for (var index = 0; index < batches.Count; index++)
        {
            _logger.Information($"Running user clean batch {index + 1}/{batches.Count}");
            var batch = batches[index];
            var joinedSQL = string.Join('\n', batch);
            await connection.ExecuteAsync(joinedSQL, transaction: transaction);
        }

        await transaction.CommitAsync();
    }
}
