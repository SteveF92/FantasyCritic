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
    }
}
