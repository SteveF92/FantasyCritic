using FantasyCritic.Lib.Identity;
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
        var betaUserList = betaUsers.ToList();
        var betaUserIds = betaUserList.Select(u => u.Id).ToHashSet();
        var nonBetaUsers = allUsers.Except(betaUserList).ToList();
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

        await CleanExternalLogins(connection, transaction, nonBetaUsers);
        await CleanDiscordData(connection, transaction, betaUserIds);

        await transaction.CommitAsync();
    }

    private static async Task CleanExternalLogins(MySqlConnection connection, MySqlTransaction transaction, IReadOnlyList<FantasyCriticUser> nonBetaUsers)
    {
        if (nonBetaUsers.Count == 0)
        {
            return;
        }

        _logger.Information("Cleaning external logins for {Count} non-beta users.", nonBetaUsers.Count);
        var nonBetaUserIds = nonBetaUsers.Select(u => u.Id).ToList();
        foreach (var batch in nonBetaUserIds.Chunk(500))
        {
            await connection.ExecuteAsync(
                "DELETE FROM tbl_user_externallogin WHERE UserID IN @userIds",
                new { userIds = batch.ToList() },
                transaction);
        }
    }

    private static async Task CleanDiscordData(MySqlConnection connection, MySqlTransaction transaction, HashSet<Guid> betaUserIds)
    {
        _logger.Information("Cleaning Discord data.");

        IReadOnlyList<Guid> leaguesToKeepDiscord;
        if (betaUserIds.Count == 0)
        {
            leaguesToKeepDiscord = [];
        }
        else
        {
            leaguesToKeepDiscord = (await connection.QueryAsync<Guid>(
                """
                SELECT DISTINCT l.LeagueID
                FROM tbl_league l
                JOIN tbl_league_hasuser lhu ON l.LeagueID = lhu.LeagueID
                WHERE l.TestLeague = 1
                AND lhu.UserID IN @betaUserIds
                """,
                new { betaUserIds = betaUserIds.ToList() },
                transaction)).ToList();
        }

        _logger.Information("Keeping Discord data for {Count} test leagues with beta users.", leaguesToKeepDiscord.Count);

        await connection.ExecuteAsync("DELETE FROM tbl_discord_gamenewschannelskiptag", transaction: transaction);
        await connection.ExecuteAsync("DELETE FROM tbl_discord_gamenewschannel", transaction: transaction);

        if (leaguesToKeepDiscord.Count == 0)
        {
            await connection.ExecuteAsync("DELETE FROM tbl_discord_leaguechannel", transaction: transaction);
            await connection.ExecuteAsync("DELETE FROM tbl_discord_conferencechannel", transaction: transaction);
        }
        else
        {
            await connection.ExecuteAsync(
                "DELETE FROM tbl_discord_leaguechannel WHERE LeagueID NOT IN @leagueIds",
                new { leagueIds = leaguesToKeepDiscord.ToList() },
                transaction);

            await connection.ExecuteAsync(
                """
                DELETE FROM tbl_discord_conferencechannel
                WHERE ConferenceID NOT IN (
                    SELECT ConferenceID FROM tbl_league
                    WHERE LeagueID IN @leagueIds AND ConferenceID IS NOT NULL
                )
                """,
                new { leagueIds = leaguesToKeepDiscord.ToList() },
                transaction);
        }
    }
}
