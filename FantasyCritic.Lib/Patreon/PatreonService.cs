using FantasyCritic.Lib.Identity;
using Microsoft.Extensions.Logging;
using Patreon.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Patreon
{
    public class PatreonService
    {
        private readonly string _accessToken;
        private readonly string _refreshToken;
        private readonly string _clientId;
        private readonly string _campaignID;

        public PatreonService(string accessToken, string refreshToken, string clientId, string campaignID)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _clientId = clientId;
            _campaignID = campaignID;
        }

        public async Task<IReadOnlyList<PatronInfo>> GetPatronInfo(IReadOnlyList<FantasyCriticUserWithExternalLogins> patreonUsers)
        {
            List<PatronInfo> patronInfo = new List<PatronInfo>();

            try
            {
                using (var client = new PatreonClient(_accessToken, _refreshToken, _clientId))
                {
                    var campaignMembers = await client.GetCampaignMembersAsync(_campaignID, Includes.CurrentlyEntitledTiers | Includes.User);
                    if (campaignMembers != null)
                    {
                        await foreach (var member in campaignMembers)
                        {

                        }
                    }
                }
            }
            catch (NullReferenceException)
            {

            }
            
            return patronInfo;
        }
    }
}
