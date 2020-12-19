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
using NodaTime;

namespace FantasyCritic.MySQL
{
    public class MySQLFantasyCriticUserStore : IFantasyCriticUserStore
    {
        private readonly string _connectionString;
        private readonly IClock _clock;

        public MySQLFantasyCriticUserStore(string connectionString, IClock clock)
        {
            _connectionString = connectionString;
            _clock = clock;
        }

        public async Task<IdentityResult> CreateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FantasyCriticUserEntity entity = new FantasyCriticUserEntity(user);
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync(
                    "insert into tbl_user(UserID,DisplayName,DisplayNumber,EmailAddress,NormalizedEmailAddress,PasswordHash,SecurityStamp,LastChangedCredentials,EmailConfirmed,IsDeleted) VALUES " +
                    "(@UserID,@DisplayName,@DisplayNumber,@EmailAddress,@NormalizedEmailAddress,@PasswordHash,@SecurityStamp,@LastChangedCredentials,@EmailConfirmed,@IsDeleted)",
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
                await connection.ExecuteAsync($"delete from tbl_user where UserID = @{nameof(FantasyCriticUserEntity.UserID)}", new { user.UserID });
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UpdateLastUsedCredentials(_clock.GetCurrentInstant());
            await RemoveAllRefreshTokens(user);

            //Not updating password or email confirmed as that breaks password change. Use the SetPasswordHash.
            FantasyCriticUserEntity entity = new FantasyCriticUserEntity(user);
            string sql = $@"UPDATE tbl_user SET DisplayName = @{nameof(FantasyCriticUserEntity.DisplayName)}, " +
                         $"DisplayNumber = @{nameof(FantasyCriticUserEntity.DisplayNumber)}, " +
                         $"EmailAddress = @{nameof(FantasyCriticUserEntity.EmailAddress)}, " +
                         $"NormalizedEmailAddress = @{nameof(FantasyCriticUserEntity.NormalizedEmailAddress)}, " +
                         $"PasswordHash = @{nameof(FantasyCriticUserEntity.PasswordHash)}, " +
                         $"EmailConfirmed = @{nameof(FantasyCriticUserEntity.EmailConfirmed)}, " +
                         $"LastChangedCredentials = @{nameof(FantasyCriticUserEntity.LastChangedCredentials)}, " +
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
                    @"select * from tbl_user WHERE NormalizedEmailAddress = @normalizedEmail",
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
                    @"select * from tbl_user WHERE UserID = @userID",
                    new { userID = parseduserid });
                var entity = userResult.SingleOrDefault();
                return entity?.ToDomain();
            }
        }

        public async Task<FantasyCriticUser> FindByDisplayName(string displayName, int displayNumber)
        {
            string normalizedDisplayName = displayName.ToUpperInvariant();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
                    @"select * from tbl_user WHERE UPPER(DisplayName) = @normalizedDisplayName and DisplayNumber = @displayNumber;",
                    new { normalizedDisplayName, displayNumber });
                var entity = userResult.SingleOrDefault();
                return entity?.ToDomain();
            }
        }

        public async Task<IReadOnlyList<FantasyCriticUser>> GetAllUsers()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
                    @"select * from tbl_user");
                var results = userResult.Select(x => x.ToDomain()).ToList();
                return results;
            }
        }

        public async Task<FantasyCriticUser> FindByNameAsync(string normalizedEmailAddress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var userResult = await connection.QueryAsync<FantasyCriticUserEntity>(
                    @"select * from tbl_user WHERE NormalizedEmailAddress = @normalizedEmailAddress",
                    new { normalizedEmailAddress });
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
            return Task.FromResult(user.NormalizedEmailAddress);
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
            return Task.FromResult(user.NormalizedEmailAddress);
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
            user.NormalizedEmailAddress = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(FantasyCriticUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(FantasyCriticUser user, string userName, CancellationToken cancellationToken)
        {
            user.EmailAddress = userName;
            return Task.CompletedTask;
        }
        
        public async Task<IList<string>> GetRolesAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var roleResults = await connection.QueryAsync<string>(@"select tbl_user_role.Name from tbl_user join tbl_user_hasrole on (tbl_user.UserID = tbl_user_hasrole.UserID) " +
                    "join tbl_user_role on (tbl_user_hasrole.RoleID = tbl_user_role.RoleID) WHERE tbl_user.UserID = @userID", new { userID = user.UserID });
                var roleStrings = roleResults.ToList();
                return roleStrings;
            }
        }

        public async Task<IList<FantasyCriticUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var userResults = await connection.QueryAsync<Guid>(@"select tbl_user.UserID from tbl_user join tbl_user_hasrole on (tbl_user.UserID = tbl_user_hasrole.UserID) " +
                    "join tbl_user_role on (tbl_user_hasrole.RoleID = tbl_user_role.RoleID) WHERE tbl_user_role.Name = @roleName", new { roleName });

                List<FantasyCriticUser> users = new List<FantasyCriticUser>();
                foreach (Guid userID in userResults)
                {
                    var user = await FindByIdAsync(userID.ToString(), cancellationToken);
                    users.Add(user);
                }
                return users;
            }
        }

        public async Task AddToRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new MySqlConnection(_connectionString))
            {
                string retrieveSQL = "select ID from tbl_user_role where Name = @Name";

                await connection.OpenAsync(cancellationToken);
                var roleID = await connection.QueryAsync<int>(retrieveSQL, new { Name = roleName });

                string insertSQL = "insert into tbl_user_hasrole (UserID, RoleID) VALUES (@UserID, @RoleID)";
                await connection.ExecuteAsync(insertSQL, new { UserID = user.UserID, RoleID = roleID });
            }
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

            using (var connection = new MySqlConnection(_connectionString))
            {
                string retrieveSQL = "select ID from tbl_user_role where Name = @Name";

                await connection.OpenAsync(cancellationToken);
                var roleID = await connection.QueryAsync<int>(retrieveSQL, new { Name = roleName });

                string deleteSQL = "delete from tbl_user_hasrole where UserID = @UserID and RoleID = @RoleID)";
                await connection.ExecuteAsync(deleteSQL, new { UserID = user.UserID, RoleID = roleID });
            }
        }

        public async Task<IReadOnlyList<string>> GetRefreshTokens(FantasyCriticUser user)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                IEnumerable<string> refreshTokens = await connection.QueryAsync<string>("select RefreshToken from tbl_user_refreshtoken where UserID = @UserID;", new { user.UserID });

                return refreshTokens.ToList();
            }
        }

        public async Task AddRefreshToken(FantasyCriticUser user, string refreshToken)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync("insert into tbl_user_refreshtoken(UserID,RefreshToken) VALUES (@UserID, @refreshToken);", new { user.UserID, refreshToken });
            }
        }

        public async Task RemoveRefreshToken(FantasyCriticUser user, string refreshToken)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync("delete from tbl_user_refreshtoken where UserID = @UserID and RefreshToken = @refreshToken;", new { user.UserID, refreshToken });
            }
        }

        public async Task RemoveAllRefreshTokens(FantasyCriticUser user)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync("delete from tbl_user_refreshtoken where UserID = @UserID;", new { user.UserID });
            }
        }

        public async Task ClearOldRefreshTokens(FantasyCriticUser user)
        {
            DateTime cutoff = _clock.GetCurrentInstant().ToDateTimeUtc().AddDays(-7);
            int minimumKeep = 5;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                IEnumerable<DateTime> tokenTimestamps = await connection.QueryAsync<DateTime>("select CreatedTimestamp from tbl_user_refreshtoken where UserID = @UserID;", new { user.UserID });
                var tokensToKeep = tokenTimestamps.OrderByDescending(x => x).Take(minimumKeep);
                var oldEnoughTokens = tokenTimestamps.Where(x => x < cutoff);
                var tokensToDelete = oldEnoughTokens.Except(tokensToKeep);
                await connection.ExecuteAsync("delete from tbl_user_refreshtoken where UserID = @UserID and CreatedTimestamp in @tokensToDelete", new { user.UserID, tokensToDelete });
            }
        }

        public async Task DeleteUserAccount(FantasyCriticUser user)
        {
            string deleteRoyaleGames = "delete tbl_royale_publishergame from tbl_royale_publishergame " +
                                       "join tbl_royale_publisher on tbl_royale_publisher.PublisherID = tbl_royale_publishergame.PublisherID " +
                                       "where UserID = @userID;";
            string deleteRoyalePublishers = "delete tbl_royale_publisher from tbl_royale_publisher " +
                                            "where UserID = @userID;";
            string updatePublisherNames = "UPDATE tbl_publisher SET PublisherName = '<Deleted>' WHERE UserID = @userID;";
            string updateUserAccount = "UPDATE tbl_user SET " +
                                       "DisplayName = '<Deleted>', " +
                                       "EmailAddress = '<Deleted>', " +
                                       "NormalizedEmailAddress = '<Deleted>', " +
                                       "PasswordHash = '', " +
                                       "SecurityStamp = '', " +
                                       "IsDeleted = 1 " +
                                       "WHERE UserID = @userID;";

            var deleteObject = new
            {
                userID = user.UserID
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(deleteRoyaleGames, deleteObject, transaction);
                    await connection.ExecuteAsync(deleteRoyalePublishers, deleteObject, transaction);
                    await connection.ExecuteAsync(updatePublisherNames, deleteObject, transaction);
                    await connection.ExecuteAsync(updateUserAccount, deleteObject, transaction);
                    transaction.Commit();
                }
            }
        }

        public async Task<int> GetOpenDisplayNumber(string displayName)
        {
            var allUsers = await GetAllUsers();
            var usersWithMatchingDisplayNames = allUsers.Where(x => string.Equals(x.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
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

        public void Dispose()
        {

        }
    }
}
