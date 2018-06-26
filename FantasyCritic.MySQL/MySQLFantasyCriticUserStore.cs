using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;
using Dapper;
using FantasyCritic.MySQL.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Linq;

namespace FantasyCritic.MySQL
{
    public class MySQLFantasyCriticUserStore : IFantasyCriticUserStore
    {
        private readonly string _connectionString;

        public MySQLFantasyCriticUserStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IdentityResult> CreateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FantasyCriticUserEntity entity = new FantasyCriticUserEntity(user);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync(
                    "insert into tbluser(UserID,UserName,NormalizedUserName,EmailAddress,NormalizedEmailAddress,PasswordHash,SecurityStamp,EmailConfirmed) VALUES " +
                    "(@UserID,@UserName,@NormalizedUserName,@EmailAddress,@NormalizedEmailAddress,@PasswordHash,@SecurityStamp,@EmailConfirmed)",
                    entity);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($"delete from tbluser where UserID = @{nameof(FantasyCriticUserEntity.UserID)}", new { user.UserID });
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Not updating password or email confirmed as that breaks password change. Use the SetPasswordHash.
            FantasyCriticUserEntity entity = new FantasyCriticUserEntity(user);
            string sql = $@"UPDATE tbluser SET UserName = @{nameof(FantasyCriticUserEntity.UserName)}, " +
                         $"NormalizedUserName = @{nameof(FantasyCriticUserEntity.NormalizedUserName)}, " +
                         $"EmailAddress = @{nameof(FantasyCriticUserEntity.EmailAddress)}, " +
                         $"NormalizedEmailAddress = @{nameof(FantasyCriticUserEntity.NormalizedEmailAddress)}, " +
                         $"PasswordHash = @{nameof(FantasyCriticUserEntity.PasswordHash)}, " +
                         $"EmailConfirmed = @{nameof(FantasyCriticUserEntity.EmailConfirmed)}, " +
                         $"SecurityStamp = @{nameof(FantasyCriticUserEntity.SecurityStamp)} " +
                         $"WHERE UserID = @{nameof(FantasyCriticUserEntity.UserID)}";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync(sql, entity);
            }

            return IdentityResult.Success;
        }

        public async Task<FantasyCriticUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
                    @"select * from tbluser WHERE NormalizedEmailAddress = @normalizedEmail",
                    new { normalizedEmail });
                var entity = userResult.SingleOrDefault();
                return entity?.ToDomain();
            }
        }

        public async Task<FantasyCriticUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Guid parseduserid;
            if (!Guid.TryParse(userId, out parseduserid))
            {
                throw new ArgumentOutOfRangeException("userId", $"'{new { userId }}' is not a valid GUID.");
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
                    @"select * from tbluser WHERE UserID = @userID",
                    new { userID = parseduserid });
                var entity = userResult.SingleOrDefault();
                return entity?.ToDomain();
            }
        }

        public async Task<FantasyCriticUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
                    @"select * from tbluser WHERE NormalizedUserName = @normalizedUserName",
                    new { normalizedUserName });
                var entity = userResult.SingleOrDefault();
                return entity?.ToDomain();
            }
        }

        public Task<string> GetEmailAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailAddress);
        }

        public Task<bool> GetEmailConfirmedAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<string> GetNormalizedEmailAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmailAddress);
        }

        public Task<string> GetNormalizedUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserID.ToString());
        }

        public Task<string> GetUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> HasPasswordAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            bool has = !string.IsNullOrWhiteSpace(user.PasswordHash);
            return Task.FromResult(has);
        }

        public Task<string> GetSecurityStampAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(FantasyCriticUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task SetEmailAsync(FantasyCriticUser user, string email, CancellationToken cancellationToken)
        {
            user.EmailAddress = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(FantasyCriticUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(FantasyCriticUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmailAddress = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(FantasyCriticUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(FantasyCriticUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(FantasyCriticUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(FantasyCriticUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task SetPhoneNumberConfirmedAsync(FantasyCriticUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task SetTwoFactorEnabledAsync(FantasyCriticUser user, bool enabled, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetPhoneNumberAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> GetTwoFactorEnabledAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<FantasyCriticUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task AddToRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> IsInRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveFromRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}
