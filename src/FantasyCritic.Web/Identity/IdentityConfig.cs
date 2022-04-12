using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace FantasyCritic.Web.Identity;

public class IdentityConfig
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiResource> APIResources = new List<ApiResource>
    {
        // local API
        new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
    };

    public static IEnumerable<ApiScope> APIScopes =>
        new ApiScope[]
        {
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
        };

    public IdentityConfig(string baseAddress, string interactiveSecret, string keyName)
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

                RedirectUris = { $"{baseAddress}/signin-oidc" },
                FrontChannelLogoutUri = $"{baseAddress}/signout-oidc",
                PostLogoutRedirectUris = { $"{baseAddress}/signout-callback-oidc" },
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
