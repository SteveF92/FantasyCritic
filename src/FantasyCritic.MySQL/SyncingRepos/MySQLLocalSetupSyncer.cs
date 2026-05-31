using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.MySQL.Entities;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace FantasyCritic.MySQL.SyncingRepos;

public class MySQLLocalSetupSyncer
{
    private static readonly ILogger _logger = Log.ForContext<MySQLLocalSetupSyncer>();

    private readonly string _connectionString;

    public MySQLLocalSetupSyncer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task EnsureLocalAdminUser(IClock clock)
    {
        var localAdminUserId = Guid.Empty;
        const string email = "localadmin@example.com";

        await using var connection = new MySqlConnection(_connectionString);

        var exists = await connection.QuerySingleOrDefaultAsync<int?>(
            "SELECT 1 FROM tbl_user WHERE UserID = @userID", new { userID = localAdminUserId });

        if (exists.HasValue)
        {
            _logger.Information("Local admin user already exists, skipping creation.");
            return;
        }

        var hasher = new PasswordHasher<FantasyCriticUser>();
        var passwordHash = hasher.HashPassword(new FantasyCriticUser(), "localadminpassword");

        var entity = new FantasyCriticUserEntity
        {
            UserID                  = localAdminUserId,
            DisplayName             = "LocalAdmin",
            PatreonDonorNameOverride = null,
            DisplayNumber           = 1000,
            EmailAddress            = email,
            NormalizedEmailAddress  = email.ToUpperInvariant(),
            EmailConfirmed          = true,
            SecurityStamp           = Guid.NewGuid().ToString(),
            PasswordHash            = passwordHash,
            TwoFactorEnabled        = false,
            AuthenticatorKey        = null,
            LastChangedCredentials  = clock.GetCurrentInstant(),
            ShowDecimalPoints       = false,
            IsDeleted               = false
        };

        await connection.ExecuteAsync(
            "INSERT INTO tbl_user (UserID, DisplayName, PatreonDonorNameOverride, DisplayNumber, EmailAddress, " +
            "NormalizedEmailAddress, PasswordHash, SecurityStamp, TwoFactorEnabled, AuthenticatorKey, " +
            "LastChangedCredentials, EmailConfirmed, ShowDecimalPoints, IsDeleted) VALUES " +
            "(@UserID, @DisplayName, @PatreonDonorNameOverride, @DisplayNumber, @EmailAddress, " +
            "@NormalizedEmailAddress, @PasswordHash, @SecurityStamp, @TwoFactorEnabled, @AuthenticatorKey, " +
            "@LastChangedCredentials, @EmailConfirmed, @ShowDecimalPoints, @IsDeleted)",
            entity);

        var roles = (await connection.QueryAsync<(int RoleID, string Name)>(
            "SELECT RoleID, Name FROM tbl_user_role")).ToList();

        foreach (var role in roles)
        {
            await connection.ExecuteAsync(
                "INSERT IGNORE INTO tbl_user_hasrole (UserID, RoleID, ProgrammaticallyAssigned) VALUES (@UserID, @RoleID, 0)",
                new { UserID = localAdminUserId, role.RoleID });
        }

        _logger.Information("Created local admin user '{Email}' with {Count} roles.", email, roles.Count);
    }

    public async Task UpsertSupportedYears(IEnumerable<SupportedYearEntity> years)
    {
        const string sql = """
            INSERT INTO tbl_meta_supportedyear
                (Year, OpenForCreation, OpenForPlay, OpenForBetaUsers, StartDate, Finished)
            VALUES
                (@Year, @OpenForCreation, @OpenForPlay, @OpenForBetaUsers, @StartDate, @Finished)
            ON DUPLICATE KEY UPDATE
                OpenForCreation  = VALUES(OpenForCreation),
                OpenForPlay      = VALUES(OpenForPlay),
                OpenForBetaUsers = VALUES(OpenForBetaUsers),
                StartDate        = VALUES(StartDate),
                Finished         = VALUES(Finished);
            """;

        var yearsList = years.ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, yearsList);
        _logger.Information("Upserted {Count} supported years.", yearsList.Count);
    }

    public async Task UpsertRoyaleYearQuarters(IEnumerable<RoyaleYearQuarter> quarters)
    {
        const string sql = """
            INSERT INTO tbl_royale_supportedquarter
                (Year, Quarter, OpenForPlay, Finished, WinningUser)
            VALUES
                (@Year, @Quarter, @OpenForPlay, @Finished, NULL)
            ON DUPLICATE KEY UPDATE
                OpenForPlay = VALUES(OpenForPlay),
                Finished    = VALUES(Finished);
            """;

        var entities = quarters.Select(x => new RoyaleYearQuarterEntity(x)).ToList();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, entities);
        _logger.Information("Upserted {Count} royale year quarters.", entities.Count);
    }
}
