using Duende.IdentityServer;
using Duende.IdentityServer.Models;

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
        // local API
        new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
    };

    public static IEnumerable<ApiScope> APIScopes =>
        new ApiScope[]
        {
            // Added name to be sure that name is added to the JWT
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName, new [] { "name" }),
        };

    public IdentityConfig(string interactiveSecret, string keyName)
    {
        Clients = new Client[]
        {
            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientName = "Interactive Flow",
                ClientSecrets = { new Secret(interactiveSecret.Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },
                RequireConsent = true,
                AllowOfflineAccess = true,
                AllowedScopes = { IdentityServerConstants.LocalApi.ScopeName }
            },
            
        };

        KeyName = keyName;
    }

    public IReadOnlyList<Client> Clients { get; }
    public string KeyName { get; }
}
