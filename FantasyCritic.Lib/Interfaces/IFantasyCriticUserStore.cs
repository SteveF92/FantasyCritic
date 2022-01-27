using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Patreon;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticUserStore : IReadOnlyFantasyCriticUserStore, 
        IUserStore<FantasyCriticUser>, IUserEmailStore<FantasyCriticUser>, IUserPasswordStore<FantasyCriticUser>, 
        IUserRoleStore<FantasyCriticUser>, IUserSecurityStampStore<FantasyCriticUser>, 
        IUserAuthenticatorKeyStore<FantasyCriticUser>, IUserTwoFactorStore<FantasyCriticUser>, IUserTwoFactorRecoveryCodeStore<FantasyCriticUser>,
        IUserLoginStore<FantasyCriticUser>, IUserPhoneNumberStore<FantasyCriticUser>
    {
        Task DeleteUserAccount(FantasyCriticUser user);
        Task<IReadOnlyList<FantasyCriticUserWithExternalLogins>> GetUsersWithExternalLogin(string provider);
        Task UpdatePatronInfo(IReadOnlyList<PatronInfo> patronInfo);
        Task AddToRoleProgrammaticAsync(FantasyCriticUser user, string roleName, CancellationToken cancellationToken);
    }
}
