using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.FakeRepo.Factories;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace FantasyCritic.FakeRepo
{
    public class FakeFantasyCriticUserStore : IFantasyCriticUserStore
    {
        private readonly IClock _clock;
        private readonly List<FantasyCriticUser> _fantasyCriticUsers;

        public FakeFantasyCriticUserStore(IClock clock)
        {
            _clock = clock;
            _fantasyCriticUsers = UserFactory.GetUsers();
        }

        public Task<string> GetUserIdAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserID.ToString());
        }

        public Task<string> GetUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailAddress);
        }

        public Task SetUserNameAsync(FantasyCriticUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmailAddress);
        }

        public Task SetNormalizedUserNameAsync(FantasyCriticUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<FantasyCriticUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_fantasyCriticUsers.SingleOrDefault(x => x.UserID.ToString() == userId));
        }

        public Task<FantasyCriticUser> FindByNameAsync(string normalizedEmailAddress, CancellationToken cancellationToken)
        {
            return Task.FromResult(_fantasyCriticUsers.SingleOrDefault(x => x.NormalizedEmailAddress == normalizedEmailAddress));
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetAllUsers()
        {
            return Task.FromResult<IReadOnlyList<FantasyCriticUser>>(_fantasyCriticUsers);
        }

        public Task<int> GetOpenDisplayNumber(string displayName)
        {
            throw new NotImplementedException();
        }

        public Task<FantasyCriticUser> FindByDisplayName(string displayName, int displayNumber)
        {
            return Task.FromResult(_fantasyCriticUsers.SingleOrDefault(x => string.Equals(x.DisplayName, displayName, StringComparison.OrdinalIgnoreCase) && x.DisplayNumber == displayNumber));
        }

        public Task SetEmailAsync(FantasyCriticUser user, string email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Task<FantasyCriticUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.FromResult(_fantasyCriticUsers.SingleOrDefault(x => x.NormalizedEmailAddress == normalizedEmail));
        }

        public Task<string> GetNormalizedEmailAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmailAddress);
        }

        public Task SetNormalizedEmailAsync(FantasyCriticUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(FantasyCriticUser user, string passwordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task AddToRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<bool> IsInRoleAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<IList<FantasyCriticUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetSecurityStampAsync(FantasyCriticUser user, string stamp, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSecurityStampAsync(FantasyCriticUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<IReadOnlyList<string>> GetRefreshTokens(FantasyCriticUser user)
        {
            throw new NotImplementedException();
        }

        public Task AddRefreshToken(FantasyCriticUser user, string refreshToken)
        {
            return Task.CompletedTask;
        }

        public Task RemoveRefreshToken(FantasyCriticUser user, string refreshToken)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAllRefreshTokens(FantasyCriticUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}
