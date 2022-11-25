using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        additionalClaims = AddScopes(additionalClaims);
        return base.SignInWithClaimsAsync(user, isPersistent, additionalClaims);
    }

    public override Task SignInWithClaimsAsync(FantasyCriticUser user, AuthenticationProperties? authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        additionalClaims = AddScopes(additionalClaims);
        return base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
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
