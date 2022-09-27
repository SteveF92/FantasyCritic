using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using Patreon.Net;
using Patreon.Net.Models;
using Serilog;

namespace FantasyCritic.Lib.Patreon;

public class PatreonService
{
    private static readonly ILogger _logger = Log.ForContext<PatreonService>();

    private readonly string _clientId;
    private readonly string _campaignID;
    private readonly IPatreonTokensRepo _tokensRepo;

    public PatreonService(PatreonConfig config, IPatreonTokensRepo tokensRepo)
    {
        _clientId = config.ClientId;
        _campaignID = config.CampaignID;
        _tokensRepo = tokensRepo;
    }

    public async Task<IReadOnlyList<PatronInfo>> GetPatronInfo(IReadOnlyList<FantasyCriticUserWithExternalLogins> patreonUsers)
    {
        _logger.Information("Getting patreon users.");
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

        _logger.Information($"Found {patreonUserDictionary.Count} patreon users.");

        var tokens = await _tokensRepo.GetMostRecentTokens();
        using var client = new PatreonClient(tokens.AccessToken, tokens.RefreshToken, _clientId);
        client.TokensRefreshedAsync += SaveNewTokens;
        
        List<PatronInfo> patronInfo = new List<PatronInfo>();
        _logger.Information("Making patreon request.");
        var campaignMembers = await client.GetCampaignMembersAsync(_campaignID, Includes.CurrentlyEntitledTiers | Includes.User);
        if (campaignMembers != null)
        {
            _logger.Information("Patreon request successful.");
            await foreach (var member in campaignMembers)
            {
                var fantasyCriticUser = patreonUserDictionary.GetValueOrDefault(member.Relationships.User.Id);
                if (fantasyCriticUser is null)
                {
                    continue;
                }

                bool isPlusUser = member.Relationships.Tiers.Any(x => x.Title == "Fantasy Critic Plus");
                bool isDonorUser = member.Relationships.Tiers.Any(x => x.Title == "Fantasy Critic Donor");
                string? donorName = null;
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

        return patronInfo;
    }

    public async Task<bool> UserIsPlusUser(string patreonProviderID)
    {
        var tokens = await _tokensRepo.GetMostRecentTokens();
        using var client = new PatreonClient(tokens.AccessToken, tokens.RefreshToken, _clientId);
        client.TokensRefreshedAsync += SaveNewTokens;
        
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
        
        return false;
    }

    private Task SaveNewTokens(OAuthToken token)
    {
        return _tokensRepo.SaveTokens(new PatreonTokens(token.AccessToken, token.RefreshToken));
    }
}
