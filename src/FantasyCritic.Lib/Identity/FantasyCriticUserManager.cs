using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Patreon;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FantasyCritic.Lib.Identity;

public class FantasyCriticUserManager : UserManager<FantasyCriticUser>
{
    private readonly IFantasyCriticUserStore _userStore;
    private readonly IClock _clock;
    private readonly PatreonService _patreonService;

    public FantasyCriticUserManager(IFantasyCriticUserStore store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<FantasyCriticUser> passwordHasher,
        IEnumerable<IUserValidator<FantasyCriticUser>> userValidators, IEnumerable<IPasswordValidator<FantasyCriticUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<FantasyCriticUser>> logger, IClock clock, PatreonService patreonService)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _userStore = store;
        _clock = clock;
        _patreonService = patreonService;
    }

    public override async Task<IdentityResult> CreateAsync(FantasyCriticUser user, string password)
    {
        var existingAccountWithEmail = await _userStore.FindByEmailAsync(user.Email, CancellationToken.None);
        if (existingAccountWithEmail is not null)
        {
            return IdentityResult.Failed(new IdentityError() { Code = "Email Taken", Description = "An account with that email address already exists." });
        }

        int openUserNumber = await _userStore.GetOpenDisplayNumber(user.UserName);
        var now = _clock.GetCurrentInstant();
        var fullUser = new FantasyCriticUser(user.Id, user.UserName, null, openUserNumber, user.Email,
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
        var fullUser = new FantasyCriticUser(user.Id, user.UserName, null, openUserNumber, user.Email,
            user.NormalizedEmail, user.EmailConfirmed, Guid.NewGuid().ToString(), user.PasswordHash, user.TwoFactorEnabled, user.AuthenticatorKey, now, false);
        var createdUser = await base.CreateAsync(fullUser);
        return createdUser;
    }

    public Task<IReadOnlyList<FantasyCriticUserWithExternalLogins>> GetAllPatreonUsers()
    {
        return _userStore.GetUsersWithExternalLogin("Patreon");
    }

    public Task<FantasyCriticUser?> FindByDisplayName(string displayName, int displayNumber)
    {
        return _userStore.FindByDisplayName(displayName, displayNumber);
    }

    public Task DeleteUserAccount(FantasyCriticUser user)
    {
        return _userStore.DeleteUserAccount(user);
    }

    public Task UpdatePatronInfo(IReadOnlyList<PatronInfo> patronInfo)
    {
        return _userStore.UpdatePatronInfo(patronInfo);
    }

    public Task<IReadOnlyList<FantasyCriticUserWithEmailSettings>> GetAllEmailSettings()
    {
        return _userStore.GetAllEmailSettings();
    }

    public Task SetEmailSettings(FantasyCriticUser user, bool sendPublicBidEmails)
    {
        return _userStore.SetEmailSettings(user, sendPublicBidEmails);
    }

    public Task<IReadOnlyList<EmailType>> GetEmailSettings(FantasyCriticUser user)
    {
        return _userStore.GetEmailSettings(user);
    }

    public async Task RefreshExternalLoginFeatures(FantasyCriticUser user)
    {
        var externalLogins = await _userStore.GetLoginsAsync(user, CancellationToken.None);
        var patreonProviderID = externalLogins.SingleOrDefault(x => x.LoginProvider == "Patreon")?.ProviderKey;
        if (patreonProviderID is null)
        {
            return;
        }

        var isPlusUser = await _patreonService.UserIsPlusUser(patreonProviderID);
        if (isPlusUser)
        {
            await _userStore.AddToRoleProgrammaticAsync(user, "PlusUser", CancellationToken.None);
        }
    }

    public Task<IReadOnlyList<string>> GetDonors()
    {
        return _userStore.GetDonors();
    }
}
