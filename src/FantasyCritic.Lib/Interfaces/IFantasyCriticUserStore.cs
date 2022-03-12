using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Patreon;
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
        Task<IReadOnlyList<string>> GetDonors();
        Task<IReadOnlyList<FantasyCriticUserWithEmailSettings>> GetAllEmailSettings();
        Task SetEmailSettings(FantasyCriticUser user, bool sendPublicBidEmails);
        Task<IReadOnlyList<EmailType>> GetEmailSettings(FantasyCriticUser user);
    }
}
