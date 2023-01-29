using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;
using FantasyCritic.Lib.Identity;
using FantasyCritic.MySQL.Entities.Identity;
using FantasyCritic.Lib.Patreon;

namespace FantasyCritic.MySQL;

public sealed class MySQLFantasyCriticUserStore : IFantasyCriticUserStore
{
    private readonly string _connectionString;
    private readonly IClock _clock;
    private List<FantasyCriticUser>? _userCache;

    public MySQLFantasyCriticUserStore(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
        _clock = configuration.Clock;
    }

    public async Task<IdentityResult> CreateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        FantasyCriticUserEntity entity = new FantasyCriticUserEntity(user);
        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await connection.ExecuteAsync(
                "insert into tbl_user(UserID,DisplayName,PatreonDonorNameOverride,DisplayNumber,EmailAddress,NormalizedEmailAddress,PasswordHash,SecurityStamp," +
                "TwoFactorEnabled,AuthenticatorKey,LastChangedCredentials,EmailConfirmed,ShowDecimalPoints,IsDeleted) VALUES " +
                "(@UserID,@DisplayName,@PatreonDonorNameOverride,@DisplayNumber,@EmailAddress,@NormalizedEmailAddress,@PasswordHash,@SecurityStamp," +
                "@TwoFactorEnabled,@AuthenticatorKey,@LastChangedCredentials,@EmailConfirmed,@ShowDecimalPoints,@IsDeleted)",
                entity);
        }

        _userCache = null;
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await connection.ExecuteAsync($"delete from tbl_user where UserID = @{nameof(FantasyCriticUserEntity.UserID)}", new
            {
                UserID = user.Id
            });
        }

        _userCache = null;
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdateLastUsedCredentials(_clock.GetCurrentInstant());

        FantasyCriticUserEntity entity = new FantasyCriticUserEntity(user);
        const string sql = $@"UPDATE tbl_user SET DisplayName = @{nameof(FantasyCriticUserEntity.DisplayName)}, " +
                           $"PatreonDonorNameOverride = @{nameof(FantasyCriticUserEntity.PatreonDonorNameOverride)}, " +
                           $"DisplayNumber = @{nameof(FantasyCriticUserEntity.DisplayNumber)}, " +
                           $"EmailAddress = @{nameof(FantasyCriticUserEntity.EmailAddress)}, " +
                           $"NormalizedEmailAddress = @{nameof(FantasyCriticUserEntity.NormalizedEmailAddress)}, " +
                           $"PasswordHash = @{nameof(FantasyCriticUserEntity.PasswordHash)}, " +
                           $"EmailConfirmed = @{nameof(FantasyCriticUserEntity.EmailConfirmed)}, " +
                           $"LastChangedCredentials = @{nameof(FantasyCriticUserEntity.LastChangedCredentials)}, " +
                           $"TwoFactorEnabled = @{nameof(FantasyCriticUserEntity.TwoFactorEnabled)}, " +
                           $"AuthenticatorKey = @{nameof(FantasyCriticUserEntity.AuthenticatorKey)}, " +
                           $"SecurityStamp = @{nameof(FantasyCriticUserEntity.SecurityStamp)} " +
                           $"ShowDecimalPoints = @{nameof(FantasyCriticUserEntity.ShowDecimalPoints)} " +
                           $"WHERE UserID = @{nameof(FantasyCriticUserEntity.UserID)}";

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await connection.ExecuteAsync(sql, entity);
        }

        _userCache = null;
        return IdentityResult.Success;
    }

    public async Task<FantasyCriticUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
            @"select * from tbl_user WHERE NormalizedEmailAddress = @normalizedEmail",
            new { normalizedEmail });
        var entity = userResult.SingleOrDefault();
        return entity?.ToDomain();
    }

    public async Task<FantasyCriticUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(userId, out var parsedUserID))
        {
            throw new ArgumentOutOfRangeException(nameof(userId), $"'{userId}' is not a valid GUID.");
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
            @"select * from tbl_user WHERE UserID = @userID",
            new { userID = parsedUserID });
        var entity = userResult.SingleOrDefault();
        return entity?.ToDomain();
    }

    public async Task<FantasyCriticUser?> FindByDisplayName(string displayName, int displayNumber)
    {
        string normalizedDisplayName = displayName.ToUpperInvariant();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
            @"select * from tbl_user WHERE UPPER(DisplayName) = @normalizedDisplayName and DisplayNumber = @displayNumber;",
            new { normalizedDisplayName, displayNumber });
        var entity = userResult.SingleOrDefault();
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<FantasyCriticUser>> GetAllUsers()
    {
        if (_userCache is not null)
        {
            return _userCache.ToList();
        }

        const string sql = "select * from tbl_user";
        await using var connection = new MySqlConnection(_connectionString);
        var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(sql);
        var results = userResult.Select(x => x.ToDomain()).ToList();
        _userCache = results;
        return results;
    }

    public async Task<IReadOnlyList<FantasyCriticUser>> GetUsers(IEnumerable<Guid> userIDs)
    {
        var hashSet = userIDs.ToHashSet();
        if (_userCache is not null)
        {
            return _userCache.Where(x => hashSet.Contains(x.Id)).ToList();
        }

        const string sql = "select * from tbl_user WHERE UserID IN @userIDs";
        var queryObject = new
        {
            userIDs
        };
        await using var connection = new MySqlConnection(_connectionString);
        var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(sql, queryObject);
        var results = userResult.Select(x => x.ToDomain()).ToList();
        return results;
    }

    public async Task<FantasyCriticUser?> FindByNameAsync(string normalizedEmailAddress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
            @"select * from tbl_user WHERE NormalizedEmailAddress = @normalizedEmailAddress",
            new { normalizedEmailAddress });
        var entity = userResult.SingleOrDefault();
        return entity?.ToDomain();
    }

    public Task<string?> GetEmailAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string?> GetNormalizedEmailAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.NormalizedEmail);
    }

    public Task<string?> GetNormalizedUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string?> GetPasswordHashAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<string> GetUserIdAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.UserName);
    }

    public Task<bool> HasPasswordAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        bool has = !string.IsNullOrWhiteSpace(user.PasswordHash);
        return Task.FromResult(has);
    }

    public Task<string?> GetSecurityStampAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    public Task SetSecurityStampAsync(FantasyCriticUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task SetEmailAsync(FantasyCriticUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email!;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(FantasyCriticUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(FantasyCriticUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail!;
        return Task.CompletedTask;
    }

    public Task SetNormalizedUserNameAsync(FantasyCriticUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetPasswordHashAsync(FantasyCriticUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(FantasyCriticUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName!;
        return Task.CompletedTask;
    }

    public async Task<IList<string>> GetRolesAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var roleResults = await connection.QueryAsync<string>(@"select tbl_user_role.Name from tbl_user join tbl_user_hasrole on (tbl_user.UserID = tbl_user_hasrole.UserID) " +
                                                              "join tbl_user_role on (tbl_user_hasrole.RoleID = tbl_user_role.RoleID) WHERE tbl_user.UserID = @userID", new { userID = user.Id });
        var roleStrings = roleResults.ToList();
        return roleStrings;
    }

    public async Task<IList<FantasyCriticUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var userResults = await connection.QueryAsync<Guid>(@"select tbl_user.UserID from tbl_user join tbl_user_hasrole on (tbl_user.UserID = tbl_user_hasrole.UserID) " +
                                                            "join tbl_user_role on (tbl_user_hasrole.RoleID = tbl_user_role.RoleID) WHERE tbl_user_role.Name = @roleName", new { roleName });

        List<FantasyCriticUser> users = new List<FantasyCriticUser>();
        foreach (Guid userID in userResults)
        {
            var user = await this.FindByIdOrThrowAsync(userID, cancellationToken);
            users.Add(user);
        }
        return users;
    }

    public async Task AddToRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using (var connection = new MySqlConnection(_connectionString))
        {
            const string retrieveSQL = "select RoleID from tbl_user_role where Name = @Name";

            await connection.OpenAsync(cancellationToken);
            var roleID = await connection.QuerySingleAsync<int>(retrieveSQL, new { Name = roleName });

            const string insertSQL = "insert ignore into tbl_user_hasrole (UserID, RoleID, ProgrammaticallyAssigned) VALUES (@UserID, @RoleID, 0)";
            await connection.ExecuteAsync(insertSQL, new { UserID = user.Id, RoleID = roleID });
        }

        _userCache = null;
    }

    public async Task AddToRoleProgrammaticAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using (var connection = new MySqlConnection(_connectionString))
        {
            const string retrieveSQL = "select RoleID from tbl_user_role where Name = @Name";

            await connection.OpenAsync(cancellationToken);
            var roleID = await connection.QuerySingleAsync<int>(retrieveSQL, new { Name = roleName });

            const string insertSQL = "insert ignore into tbl_user_hasrole (UserID, RoleID, ProgrammaticallyAssigned) VALUES (@UserID, @RoleID, 1)";
            await connection.ExecuteAsync(insertSQL, new { UserID = user.Id, RoleID = roleID });
        }

        _userCache = null;
    }

    public async Task<bool> IsInRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
    {
        var roles = await GetRolesAsync(user, cancellationToken);
        bool inRole = roles.Select(x => x.ToLower()).Any(x => x.Contains(roleName.ToLower()));
        return inRole;
    }

    public async Task RemoveFromRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using (var connection = new MySqlConnection(_connectionString))
        {
            const string retrieveSQL = "select ID from tbl_user_role where Name = @Name";

            await connection.OpenAsync(cancellationToken);
            var roleID = await connection.QueryAsync<int>(retrieveSQL, new { Name = roleName });

            const string deleteSQL = "delete from tbl_user_hasrole where UserID = @UserID and RoleID = @RoleID)";
            await connection.ExecuteAsync(deleteSQL, new { UserID = user.Id, RoleID = roleID });
        }

        _userCache = null;
    }

    public async Task DeleteUserAccount(FantasyCriticUser user)
    {
        const string deleteRoyaleGames = "delete tbl_royale_publishergame from tbl_royale_publishergame " +
                                         "join tbl_royale_publisher on tbl_royale_publisher.PublisherID = tbl_royale_publishergame.PublisherID " +
                                         "where UserID = @userID;";
        const string deleteRoyalePublishers = "delete tbl_royale_publisher from tbl_royale_publisher " +
                                              "where UserID = @userID;";
        const string updatePublisherNames = "UPDATE tbl_league_publisher SET PublisherName = '<Deleted>' WHERE UserID = @userID;";
        const string deleteUnprocessedDrops = "DELETE tbl_league_pickupbid FROM tbl_league_pickupbid " +
                                              "join tbl_league_publisher on tbl_league_publisher.PublisherID = tbl_league_pickupbid.PublisherID " +
                                              "WHERE UserID = @userID AND Successful IS null;";
        const string deleteUnprocessedBids = "DELETE tbl_league_droprequest FROM tbl_league_droprequest " +
                                             "join tbl_league_publisher on tbl_league_publisher.PublisherID = tbl_league_droprequest.PublisherID " +
                                             "WHERE UserID = @userID AND Successful IS null;";
        const string deleteExternalLogins = "delete from tbl_user_externallogin where UserID = @userID;";

        const string updateUserAccount = "UPDATE tbl_user SET " +
                                         "DisplayName = '<Deleted>', " +
                                         "EmailAddress = @fakeEmailAddress, " +
                                         "NormalizedEmailAddress = @fakeEmailAddress, " +
                                         "PasswordHash = '', " +
                                         "SecurityStamp = '', " +
                                         "IsDeleted = 1 " +
                                         "WHERE UserID = @userID;";

        var deleteObject = new
        {
            userID = user.Id,
            fakeEmailAddress = $"{user.Id}@fake.fake"
        };

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(deleteRoyaleGames, deleteObject, transaction);
            await connection.ExecuteAsync(deleteRoyalePublishers, deleteObject, transaction);
            await connection.ExecuteAsync(updatePublisherNames, deleteObject, transaction);
            await connection.ExecuteAsync(deleteUnprocessedDrops, deleteObject, transaction);
            await connection.ExecuteAsync(deleteUnprocessedBids, deleteObject, transaction);
            await connection.ExecuteAsync(deleteExternalLogins, deleteObject, transaction);
            await connection.ExecuteAsync(updateUserAccount, deleteObject, transaction);
            await transaction.CommitAsync();
        }

        _userCache = null;
    }

    public async Task<int> GetOpenDisplayNumber(string displayName)
    {
        var allUsers = await GetAllUsers();
        var usersWithMatchingDisplayNames = allUsers.Where(x => string.Equals(x.UserName, displayName, StringComparison.OrdinalIgnoreCase));
        var existingDisplayNumbers = usersWithMatchingDisplayNames.Select(x => x.DisplayNumber).ToList();

        Random randomGenerator = new Random();
        while (true)
        {
            var randomNumber = randomGenerator.Next(1000, 10000);
            if (!existingDisplayNumbers.Contains(randomNumber))
            {
                return randomNumber;
            }
        }
    }

    public Task SetAuthenticatorKeyAsync(FantasyCriticUser user, string key, CancellationToken cancellationToken)
    {
        user.AuthenticatorKey = key;
        return Task.CompletedTask;
    }

    public Task<string?> GetAuthenticatorKeyAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.AuthenticatorKey);
    }

    public Task SetTwoFactorEnabledAsync(FantasyCriticUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.TwoFactorEnabled);
    }

    public async Task ReplaceCodesAsync(FantasyCriticUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        List<RecoveryCodeEntity> codeEntities = recoveryCodes.Select(x => new RecoveryCodeEntity(user.Id, x)).ToList();

        var paramsObject = new
        {
            userID = user.Id
        };

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await connection.ExecuteAsync("DELETE FROM tbl_user_recoverycode WHERE UserID = @userID;", paramsObject, transaction);
            await connection.BulkInsertAsync(codeEntities, "tbl_user_recoverycode", 500, transaction);

            await transaction.CommitAsync(cancellationToken);
        }

        _userCache = null;
    }

    public async Task<bool> RedeemCodeAsync(FantasyCriticUser user, string code, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "DELETE from tbl_user_recoverycode where UserID = @UserID and Code = @RecoveryCode";

        _userCache = null;

        RecoveryCodeEntity entity = new RecoveryCodeEntity(user.Id, code);
        await using var connection = new MySqlConnection(_connectionString);
        int count = await connection.ExecuteAsync(sql, entity);
        return count == 1;
    }

    public async Task<int> CountCodesAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "select count(*) from tbl_user_recoverycode where UserID = @userID";

        var queryObject = new
        {
            userID = user.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        int count = await connection.QuerySingleAsync<int>(sql, queryObject);
        return count;
    }

    public async Task AddLoginAsync(FantasyCriticUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ExternalLoginEntity entity = new ExternalLoginEntity()
        {
            UserID = user.Id,
            LoginProvider = login.LoginProvider,
            ProviderKey = login.ProviderKey,
            ProviderDisplayName = login.ProviderDisplayName,
        };

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            const string insertSQL = "insert into tbl_user_externallogin (LoginProvider,ProviderKey,UserID,ProviderDisplayName) " +
                                     "VALUES (@LoginProvider,@ProviderKey,@UserID,@ProviderDisplayName);";
            await connection.ExecuteAsync(insertSQL, entity);
        }

        _userCache = null;
    }

    public async Task RemoveLoginAsync(FantasyCriticUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "DELETE from tbl_user_externallogin where LoginProvider = @LoginProvider and ProviderKey = @ProviderKey and UserID = @UserID;";

        var deleteObject = new
        {
            LoginProvider = loginProvider,
            ProviderKey = providerKey,
            UserID = user.Id
        };

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, deleteObject);
        }

        _userCache = null;
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        const string sql = "select * from tbl_user_externallogin WHERE UserID = @userID;";
        var queryObject = new
        {
            userID = user.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        var externalLogins = await connection.QueryAsync<ExternalLoginEntity>(sql, queryObject);
        List<UserLoginInfo> logins = externalLogins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName)).ToList();
        return logins;
    }

    public async Task<FantasyCriticUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "select tbl_user.* from tbl_user JOIN tbl_user_externallogin " +
                           "ON tbl_user.UserID = tbl_user_externallogin.UserID " +
                           "WHERE LoginProvider = @loginProvider AND ProviderKey = @providerKey";

        var queryObject = new
        {
            loginProvider,
            providerKey
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(sql, queryObject);
        var entity = userResult.SingleOrDefault();
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<FantasyCriticUserWithExternalLogins>> GetUsersWithExternalLogin(string provider)
    {
        const string sql = "select tbl_user.*, LoginProvider, ProviderKey, tbl_user_externallogin.UserID, ProviderDisplayName, TimeAdded " +
                           "FROM tbl_user JOIN tbl_user_externallogin ON tbl_user.UserID = tbl_user_externallogin.UserID " +
                           "WHERE LoginProvider = @provider;";

        var queryObject = new
        {
            provider
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var userResults = await connection.QueryAsync<FantasyCriticUserEntity, ExternalLoginEntity, Tuple<FantasyCriticUserEntity, ExternalLoginEntity>>(
            sql, (user, externalLogin) => new Tuple<FantasyCriticUserEntity, ExternalLoginEntity>(user, externalLogin), queryObject, splitOn: "LoginProvider");
        List<FantasyCriticUserWithExternalLogins> domainResults = new List<FantasyCriticUserWithExternalLogins>();
        foreach (var userEntity in userResults)
        {
            List<UserLoginInfo> userLogins = new List<UserLoginInfo>()
            {
                new UserLoginInfo(userEntity.Item2.LoginProvider, userEntity.Item2.ProviderKey, userEntity.Item2.ProviderDisplayName)
            };
            var domain = new FantasyCriticUserWithExternalLogins(userEntity.Item1.ToDomain(), userLogins);
            domainResults.Add(domain);
        }

        return domainResults;
    }

    public async Task<IReadOnlyList<FantasyCriticUserWithEmailSettings>> GetAllEmailSettings()
    {
        const string emailSQL = "select * from tbl_user_emailsettings;";
        ILookup<Guid, FantasyCriticUserEmailSettingEntity> userEmailSettings;
        await using (var connection = new MySqlConnection(_connectionString))
        {
            var emailResult = await connection.QueryAsync<FantasyCriticUserEmailSettingEntity>(emailSQL);
            userEmailSettings = emailResult.ToLookup(x => x.UserID);
        }

        var allUsers = await GetAllUsers();
        List<FantasyCriticUserWithEmailSettings> usersWithEmailSettings = new List<FantasyCriticUserWithEmailSettings>();
        foreach (var user in allUsers)
        {
            var emailSettings = userEmailSettings[user.Id];
            if (emailSettings.Any())
            {
                var domainTypes = emailSettings.Select(x => EmailType.FromValue(x.EmailType)).ToList();
                usersWithEmailSettings.Add(new FantasyCriticUserWithEmailSettings(user, domainTypes));
            }
        }

        return usersWithEmailSettings;
    }

    public async Task SetEmailSettings(FantasyCriticUser user, bool sendPublicBidEmails)
    {
        var settingsEntities = new List<FantasyCriticUserEmailSettingEntity>();

        if (sendPublicBidEmails)
        {
            settingsEntities.Add(new FantasyCriticUserEmailSettingEntity(user, EmailType.PublicBids));
        }

        var parameters = new
        {
            userID = user.Id
        };
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync("DELETE FROM tbl_user_emailsettings WHERE UserID = @userID;", parameters, transaction: transaction);
        await connection.BulkInsertAsync(settingsEntities, "tbl_user_emailsettings", 500, transaction);

        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<EmailType>> GetEmailSettings(FantasyCriticUser user)
    {
        const string emailSQL = "select * from tbl_user_emailsettings where UserID = @userID;";
        var parameters = new
        {
            userID = user.Id
        };

        await using var connection = new MySqlConnection(_connectionString);
        var emailResult = await connection.QueryAsync<FantasyCriticUserEmailSettingEntity>(emailSQL, parameters);
        return emailResult.Select(x => EmailType.FromValue(x.EmailType)).ToList();
    }

    public Task SetGeneralSettings(FantasyCriticUser user, GeneralUserSettings generalSettings)
    {
        var modifiedUser = user.UpdateGeneralSettings(generalSettings);
        return UpdateAsync(modifiedUser, CancellationToken.None);
    }

    public async Task UpdatePatronInfo(IReadOnlyList<PatronInfo> patronInfo)
    {
        List<FantasyCriticUserHasRoleEntity> roleEntities = patronInfo
            .Where(x => x.IsPlusUser)
            .Select(x => new FantasyCriticUserHasRoleEntity(x.User.Id, 4, true))
            .ToList();

        List<FantasyCriticUserDonorEntity> donorEntities = patronInfo
            .Where(x => x.DonorName is not null)
            .Select(x => new FantasyCriticUserDonorEntity(x.User, x.DonorName!))
            .ToList();

        await using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync("DELETE FROM tbl_user_hasrole WHERE ProgrammaticallyAssigned = 1;", transaction: transaction);
            await connection.ExecuteAsync("DELETE FROM tbl_user_donorname;", transaction: transaction);
            await connection.BulkInsertAsync(roleEntities, "tbl_user_hasrole", 500, transaction, insertIgnore: true);
            await connection.BulkInsertAsync(donorEntities, "tbl_user_donorname", 500, transaction);

            await transaction.CommitAsync();
        }

        _userCache = null;
    }

    public async Task<IReadOnlyList<string>> GetDonors()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var donors = await connection.QueryAsync<string>("select DonorName FROM tbl_user_donorname;");
        return donors.ToList();
    }

    public Task SetPhoneNumberAsync(FantasyCriticUser user, string? phoneNumber, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<string?> GetPhoneNumberAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(FantasyCriticUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    public Task SetPhoneNumberConfirmedAsync(FantasyCriticUser user, bool confirmed, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {

    }
}
