using Duende.IdentityServer.Models;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Identity;

public class IdentityConfig
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiResource> APIResources = new List<ApiResource>
    {
        new ApiResource(FantasyCriticScopes.ReadScope.Name, FantasyCriticScopes.ReadScope.DisplayName),
        new ApiResource(FantasyCriticScopes.WriteScope.Name, FantasyCriticScopes.WriteScope.DisplayName),
    };

    public static IEnumerable<ApiScope> APIScopes =>
        new ApiScope[]
        {
            new ApiScope(FantasyCriticScopes.ReadScope.Name, FantasyCriticScopes.ReadScope.DisplayName),
            new ApiScope(FantasyCriticScopes.WriteScope.Name, FantasyCriticScopes.WriteScope.DisplayName),
        };

    public IdentityConfig(string fcBotSecret, string keyName)
    {
        Clients = new Client[]
        {
            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "fcbot",
                ClientName = "Fantasy Critic Bot",
                ClientSecrets = { new Secret(fcBotSecret.Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },
                RequireConsent = true,
                AllowOfflineAccess = true,
                AllowedScopes = { FantasyCriticScopes.ReadScope.Name }
            },

        };

        KeyName = keyName;
    }

    public IReadOnlyList<Client> Clients { get; }
    public string KeyName { get; }
}
