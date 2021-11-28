using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;

namespace FantasyCritic.Web.Identity
{
    public class IdentityConfig
    {
        public IdentityConfig(string mainSecret, string fcBotSecret)
        {
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
                    ClientUri = "https://fantasy-critic-bot.herokuapp.com/",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = false,
                    AllowAccessTokensViaBrowser = true,
                    ClientSecrets = { new Secret(fcBotSecret.Sha256()) },
                    RedirectUris = {"http://localhost:6501/auth/fc"},
                    AllowOfflineAccess = true,
                    AllowedScopes = { "FantasyCritic.WebAPI" },
                    AccessTokenLifetime = 604800
                }
            };
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
    }
}
