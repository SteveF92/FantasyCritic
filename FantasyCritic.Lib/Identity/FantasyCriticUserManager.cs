using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.LayoutRenderers.Wrappers;
using NodaTime;

namespace FantasyCritic.Lib.Identity
{
    public class FantasyCriticUserManager : UserManager<FantasyCriticUser>
    {
        private readonly IFantasyCriticUserStore _userStore;
        private readonly IClock _clock;

        public FantasyCriticUserManager(IFantasyCriticUserStore store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<FantasyCriticUser> passwordHasher,
            IEnumerable<IUserValidator<FantasyCriticUser>> userValidators, IEnumerable<IPasswordValidator<FantasyCriticUser>> passwordValidators, ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<FantasyCriticUser>> logger, IClock clock)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _userStore = store;
            _clock = clock;
        }

        public override async Task<IdentityResult> CreateAsync(FantasyCriticUser user, string password)
        {
            int openUserNumber = await _userStore.GetOpenDisplayNumber(user.UserName);
            var now = _clock.GetCurrentInstant();
            var fullUser = new FantasyCriticUser(user.Id, user.UserName, openUserNumber, user.Email,
                user.NormalizedEmail, user.EmailConfirmed, Guid.NewGuid().ToString(), user.PasswordHash, user.TwoFactorEnabled, user.AuthenticatorKey, now, false);
            var createdUser = await base.CreateAsync(fullUser, password);
            return createdUser;
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

        public Task DeleteUserAccount(FantasyCriticUser user)
        {
            return _userStore.DeleteUserAccount(user);
        }
    }
}
