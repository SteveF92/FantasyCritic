using FantasyCritic.Lib.Identity;
using Patreon.Net;
using NLog;

namespace FantasyCritic.Lib.Patreon;

public class PatreonService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
        _logger.Info("Getting patreon users.");
        Dictionary<string, FantasyCriticUser> patreonUserDictionary = new Dictionary<string, FantasyCriticUser>();
        foreach (var patreonUser in patreonUsers)
        {
            var patreonLogin = patreonUser.UserLogins.SingleOrDefault(x => x.LoginProvider == "Patreon");
            if (patreonLogin == null)
            {
                continue;
            }

            patreonUserDictionary.Add(patreonLogin.ProviderKey, patreonUser.User);
        }
        _logger.Info($"Found {patreonUserDictionary.Count} patreon users.");

        List<PatronInfo> patronInfo = new List<PatronInfo>();
        using (var client = new PatreonClient(_accessToken, _refreshToken, _clientId))
        {
            _logger.Info("Making patreon request.");
            var campaignMembers = await client.GetCampaignMembersAsync(_campaignID, Includes.CurrentlyEntitledTiers | Includes.User);
            if (campaignMembers != null)
            {
                _logger.Info("Patreon request successful.");
                await foreach (var member in campaignMembers)
                {
                    var hasFantasyCriticUser = patreonUserDictionary.TryGetValue(member.Relationships.User.Id, out var fantasyCriticUser);
                    if (!hasFantasyCriticUser)
                    {
                        continue;
                    }

                    bool isPlusUser = member.Relationships.Tiers.Any(x => x.Title == "Fantasy Critic Plus");
                    bool isDonorUser = member.Relationships.Tiers.Any(x => x.Title == "Fantasy Critic Donor");
                    Maybe<string> donorName = Maybe<string>.None;
                    if (isDonorUser)
                    {
                        isPlusUser = true;
                        donorName = member.Relationships.User.FullName;
                        if (fantasyCriticUser.PatreonDonorNameOverride is not null)
                        {
                            donorName = fantasyCriticUser.PatreonDonorNameOverride;
                        }
                    }

                    patronInfo.Add(new PatronInfo(fantasyCriticUser, isPlusUser, donorName));
                }
            }
        }

        return patronInfo;
    }

    public async Task<bool> UserIsPlusUser(string patreonProviderID)
    {
        using (var client = new PatreonClient(_accessToken, _refreshToken, _clientId))
        {
            var campaignMembers = await client.GetCampaignMembersAsync(_campaignID, Includes.CurrentlyEntitledTiers | Includes.User);
            if (campaignMembers != null)
            {
                await foreach (var member in campaignMembers)
                {
                    if (member.Relationships.User.Id != patreonProviderID)
                    {
                        continue;
                    }

                    bool isPlusUser = member.Relationships.Tiers.Any(x => x.Title == "Fantasy Critic Plus");
                    return isPlusUser;
                }
            }
        }

        return false;
    }
}
