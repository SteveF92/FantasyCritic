using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FantasyCritic.Lib.Services
{
    public class FantasyCriticUserManager : UserManager<FantasyCriticUser>
    {
        private IFantasyCriticUserStore _userStore;

        public FantasyCriticUserManager(IFantasyCriticUserStore store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<FantasyCriticUser> passwordHasher,
            IEnumerable<IUserValidator<FantasyCriticUser>> userValidators,
            IEnumerable<IPasswordValidator<FantasyCriticUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<FantasyCriticUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _userStore = store;
        }

        public Task<IReadOnlyList<string>> GetRefreshTokens(FantasyCriticUser user)
        {
            return _userStore.GetRefreshTokens(user);
        }

        public Task AddRefreshToken(FantasyCriticUser user, string refreshToken)
        {
            return _userStore.AddRefreshToken(user, refreshToken);
        }

        public Task RemoveRefreshToken(FantasyCriticUser user, string refreshToken)
        {
            return _userStore.RemoveRefreshToken(user, refreshToken);
        }

        public Task RemoveAllRefreshTokens(FantasyCriticUser user)
        {
            return _userStore.RemoveAllRefreshTokens(user);
        }

        public Task<int> GetOpenDisplayNumber(string displayName)
        {
            return _userStore.GetOpenDisplayNumber(displayName);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetAllUsers()
        {
            return _userStore.GetAllUsers();
        }

        public Task<FantasyCriticUser> FindByDisplayName(string displayName, int displayNumber)
        {
            return _userStore.FindByDisplayName(displayName, displayNumber);
        }
    }
}
