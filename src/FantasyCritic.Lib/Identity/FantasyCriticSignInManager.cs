using System.Security.Claims;
using FantasyCritic.Lib.SharedSerialization.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;

namespace FantasyCritic.Lib.Identity;
public class FantasyCriticSignInManager : SignInManager<FantasyCriticUser>
{
    public FantasyCriticSignInManager(UserManager<FantasyCriticUser> userManager, IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<FantasyCriticUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<FantasyCriticUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<FantasyCriticUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {

    }

    public override Task SignInWithClaimsAsync(FantasyCriticUser user, bool isPersistent, IEnumerable<Claim> additionalClaims)
    {
        CacheUserToSession(user);

        additionalClaims = AddScopes(additionalClaims);
        return base.SignInWithClaimsAsync(user, isPersistent, additionalClaims);
    }

    public override Task SignInWithClaimsAsync(FantasyCriticUser user, AuthenticationProperties? authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        CacheUserToSession(user);
        additionalClaims = AddScopes(additionalClaims);
        return base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
    }

    public override Task SignOutAsync()
    {
        Context.Session.Clear();
        return base.SignOutAsync();
    }

    private void CacheUserToSession(FantasyCriticUser user)
    {
        var serializable = new FantasyCriticUserEntity(user);
        var jsonString = JsonConvert.SerializeObject(serializable, new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
        Context.Session.SetString("current-user", jsonString);
    }

    private static IReadOnlyList<Claim> AddScopes(IEnumerable<Claim> additionalClaims)
    {
        if (!additionalClaims.Any(x => x.Type == "scope" && x.Value == FantasyCriticScopes.ReadScope.Name))
        {
            additionalClaims = additionalClaims.Concat(new List<Claim>() { new Claim("scope", FantasyCriticScopes.ReadScope.Name) });
        }
        if (!additionalClaims.Any(x => x.Type == "scope" && x.Value == FantasyCriticScopes.WriteScope.Name))
        {
            additionalClaims = additionalClaims.Concat(new List<Claim>() { new Claim("scope", FantasyCriticScopes.WriteScope.Name) });
        }

        return additionalClaims.ToList();
    }
}
