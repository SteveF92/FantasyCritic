using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace FantasyCritic.Web.Identity
{
    public class IdentityConfig
    {
        public IdentityConfig(string mainSecret, string fcBotSecret, string keyName, bool isProd)
        {
            string fcBotClientUri = "https://fcbot-dev.herokuapp.com/";
            List<string> fcBotRedirectUris = new List<string>()
            {
                "http://localhost:6501/auth/fc",
                "https://fcbot-dev.herokuapp.com/auth/fc"
            };

            if (isProd)
            {
                fcBotClientUri = "https://fantasy-critic-bot.herokuapp.com/";
                fcBotRedirectUris = new List<string>()
                {
                    "https://fantasy-critc-bot.herokuapp.com/auth/fc"
                };
            }

            Clients = new Client[]
            {
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = { new Secret(mainSecret.Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = { "FantasyCritic.WebAPI" }
                },
                new Client
                {
                    ClientId = "fcbot",
                    ClientName = "Fantasy Critic Discord Bot",
                    ClientUri = fcBotClientUri,
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = false,
                    AllowAccessTokensViaBrowser = true,
                    ClientSecrets = { new Secret(fcBotSecret.Sha256()) },
                    RedirectUris = fcBotRedirectUris,
                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        "FantasyCritic.WebAPI",
                        IdentityServerConstants.StandardScopes.OpenId
                    },
                    AccessTokenLifetime = 604800,
                    RequireConsent = true
                }
            };

            KeyName = keyName;
        }

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("FantasyCritic.WebAPI", "Fantasy Critic API")
            };

        public IReadOnlyList<Client> Clients { get; }
        public string KeyName { get; }
    }
}
