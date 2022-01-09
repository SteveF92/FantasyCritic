using System;
using System.Collections.Generic;
using System.Threading;
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
            var existingAccountWithEmail = await _userStore.FindByEmailAsync(user.Email, CancellationToken.None);
            if (existingAccountWithEmail is not null)
            {
                return IdentityResult.Failed(new IdentityError(){Code = "Email Taken", Description = "An account with that email address already exists." });
            }
            
            int openUserNumber = await _userStore.GetOpenDisplayNumber(user.UserName);
            var now = _clock.GetCurrentInstant();
            var fullUser = new FantasyCriticUser(user.Id, user.UserName, openUserNumber, user.Email,
                user.NormalizedEmail, user.EmailConfirmed, Guid.NewGuid().ToString(), user.PasswordHash, user.TwoFactorEnabled, user.AuthenticatorKey, now, false);
            var createdUser = await base.CreateAsync(fullUser, password);
            return createdUser;
        }

        public override async Task<IdentityResult> CreateAsync(FantasyCriticUser user)
        {
            var existingAccountWithEmail = await _userStore.FindByEmailAsync(user.Email, CancellationToken.None);
            if (existingAccountWithEmail is not null)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "Email Taken", Description = "An account with that email address already exists." });
            }

            int openUserNumber = await _userStore.GetOpenDisplayNumber(user.UserName);
            var now = _clock.GetCurrentInstant();
            var fullUser = new FantasyCriticUser(user.Id, user.UserName, openUserNumber, user.Email,
                user.NormalizedEmail, user.EmailConfirmed, Guid.NewGuid().ToString(), user.PasswordHash, user.TwoFactorEnabled, user.AuthenticatorKey, now, false);
            var createdUser = await base.CreateAsync(fullUser);
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
