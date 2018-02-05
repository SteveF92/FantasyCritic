using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace FantasyCritic.PGSQL
{
    public class PGSQLFantasyCriticUserStore : IFantasyCriticUserStore
    {
        private readonly string _connectionString;

        public PGSQLFantasyCriticUserStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IdentityResult> CreateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Guid userID = Guid.NewGuid();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync("insert into tbluser(userid, username, normalizedusername, emailaddress, normalizedemailaddress, passwordhash, securitystamp, emailconfirmed) values(@userID, @userName, @normalizedusername, " +
                                              "@emailAddress, @normalizedEmailAddress, @passwordHash, @securityStamp, @emailConfirmed)",
                    new { userID = userID, userName = user.UserName, normalizedUsername = user.NormalizedUserName, emailAddress = user.EmailAddress, normalizedEmailAddress = user.NormalizedEmailAddress, passwordHash = user.PasswordHash, securityStamp = user.SecurityStamp, emailConfirmed = user.EmailConfirmed});
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync("delete from tbluser where userid = @userID", new {user.UserID});
            }

            return IdentityResult.Success;
        }

        public async Task<FantasyCriticUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Guid parseduserid;
            if (!Guid.TryParse(userId, out parseduserid))
            {
                throw new ArgumentOutOfRangeException("userId", $"'{new {userId}}' is not a valid GUID.");
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var userResult = await connection.QueryAsync<FantasyCriticUser>($@"SELECT * FROM tbluser WHERE UserID = @userID", new { userId });
                return userResult.SingleOrDefault();
            }
        }

        public async Task<FantasyCriticUser> FindByNameAsync(string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var userResult = await connection.QueryAsync<FantasyCriticUser>($@"SELECT * FROM tbluser WHERE normalizedusername = @userName", new { userName });
                return userResult.SingleOrDefault();
            }
        }

        public Task<string> GetNormalizedUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<string> GetUserIdAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserID.ToString());
        }

        public Task<string> GetUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(FantasyCriticUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(FantasyCriticUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($@"UPDATE tbluser SET
                    username = @{nameof(FantasyCriticUser.UserName)},
                    normalizedusername = @{nameof(FantasyCriticUser.NormalizedUserName)},
                    Email = @{nameof(FantasyCriticUser.EmailAddress)},
                    normalizedEmail = @{nameof(FantasyCriticUser.NormalizedEmailAddress)},
                    emailconfirmed = @{nameof(FantasyCriticUser.EmailConfirmed)},
                    passwordhash = @{nameof(FantasyCriticUser.PasswordHash)},
                    WHERE Id = @{nameof(FantasyCriticUser.UserID)}", user);
            }

            return IdentityResult.Success;
        }

        public Task SetEmailAsync(FantasyCriticUser user, string email, CancellationToken cancellationToken)
        {
            user.EmailAddress = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailAddress);
        }

        public Task<bool> GetEmailConfirmedAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(FantasyCriticUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public async Task<FantasyCriticUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<FantasyCriticUser>($@"SELECT * FROM [FantasyCriticUser]
                    WHERE [normalizedEmail] = @{nameof(normalizedEmail)}", new { normalizedEmail });
            }
        }

        public Task<string> GetNormalizedEmailAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmailAddress);
        }

        public Task SetNormalizedEmailAsync(FantasyCriticUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmailAddress = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberAsync(FantasyCriticUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Phone number is not supported.");
        }

        public Task<string> GetPhoneNumberAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Phone number is not supported.");
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Phone number is not supported.");
        }

        public Task SetPhoneNumberConfirmedAsync(FantasyCriticUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Phone number is not supported.");
        }

        public Task SetTwoFactorEnabledAsync(FantasyCriticUser user, bool enabled, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("2FA is not supported.");

        }

        public Task<bool> GetTwoFactorEnabledAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("2FA is not supported.");
        }

        public Task SetPasswordHashAsync(FantasyCriticUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public async Task AddToRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Roles not supported");
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var normalizedName = roleName.ToUpper();
                var roleId = await connection.ExecuteScalarAsync<int?>($"SELECT [Id] FROM [ApplicationRole] WHERE [normalizedName] = @{nameof(normalizedName)}", new { normalizedName });
                if (!roleId.HasValue)
                    roleId = await connection.ExecuteAsync($"INSERT INTO [ApplicationRole]([Name], [normalizedName]) VALUES(@{nameof(roleName)}, @{nameof(normalizedName)})",
                        new { roleName, normalizedName });

                await connection.ExecuteAsync($"IF NOT EXISTS(SELECT 1 FROM [ApplicationUserRole] WHERE [userid] = @userId AND [RoleId] = @{nameof(roleId)}) " +
                    $"INSERT INTO [ApplicationUserRole]([userid], [RoleId]) VALUES(@userId, @{nameof(roleId)})",
                    new { userId = user.UserID, roleId });
            }
        }

        public async Task RemoveFromRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Roles not supported");
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var roleId = await connection.ExecuteScalarAsync<int?>("SELECT [Id] FROM [ApplicationRole] WHERE [normalizedName] = @normalizedName", new { normalizedName = roleName.ToUpper() });
                if (!roleId.HasValue)
                    await connection.ExecuteAsync($"DELETE FROM [ApplicationUserRole] WHERE [userid] = @userId AND [RoleId] = @{nameof(roleId)}", new { userId = user.UserID, roleId });
            }
        }

        public async Task<IList<string>> GetRolesAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Roles not supported");
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var queryResults = await connection.QueryAsync<string>("SELECT r.[Name] FROM [ApplicationRole] r INNER JOIN [ApplicationUserRole] ur ON ur.[RoleId] = r.Id " +
                    "WHERE ur.userid = @userId", new { userId = user.UserID });

                return queryResults.ToList();
            }
        }

        public async Task<bool> IsInRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Roles not supported");
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var roleId = await connection.ExecuteScalarAsync<int?>("SELECT [Id] FROM [ApplicationRole] WHERE [normalizedName] = @normalizedName", new { normalizedName = roleName.ToUpper() });
                if (roleId == default(int)) return false;
                var matchingRoles = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM [ApplicationUserRole] WHERE [userid] = @userId AND [RoleId] = @{nameof(roleId)}",
                    new { userId = user.UserID, roleId });

                return matchingRoles > 0;
            }
        }

        public async Task<IList<FantasyCriticUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Roles not supported");
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var queryResults = await connection.QueryAsync<FantasyCriticUser>("SELECT u.* FROM [FantasyCriticUser] u " +
                    "INNER JOIN [ApplicationUserRole] ur ON ur.[userid] = u.[Id] INNER JOIN [ApplicationRole] r ON r.[Id] = ur.[RoleId] WHERE r.[normalizedName] = @normalizedName",
                    new { normalizedName = roleName.ToUpper() });

                return queryResults.ToList();
            }
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
