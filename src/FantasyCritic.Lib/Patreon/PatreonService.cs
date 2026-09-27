using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using Serilog;

namespace FantasyCritic.Lib.Patreon;

public class PatreonService
{
    private static readonly ILogger _logger = Log.ForContext<PatreonService>();

    private const string PlusTierTitle = "Fantasy Critic Plus";
    private const string DonorTierTitle = "Fantasy Critic Donor";

    private readonly PatreonApiClient _patreonApi;
    private readonly IPatreonTokensRepo _tokensRepo;
    private readonly IFantasyCriticUserStore _userStore;

    public PatreonService(PatreonApiClient patreonApi, IPatreonTokensRepo tokensRepo, IFantasyCriticUserStore userStore)
    {
        _patreonApi = patreonApi;
        _tokensRepo = tokensRepo;
        _userStore = userStore;
    }

    public async Task RefreshPlusUserRole(FantasyCriticUser user)
    {
        var externalLogins = await _userStore.GetLoginsAsync(user, CancellationToken.None);
        var patreonProviderID = externalLogins.SingleOrDefault(x => x.LoginProvider == "Patreon")?.ProviderKey;
        if (patreonProviderID is null)
        {
            return;
        }

        var isPlusUser = await UserIsPlusUser(patreonProviderID);
        if (isPlusUser)
        {
            await _userStore.AddToRoleProgrammaticAsync(user, "PlusUser", CancellationToken.None);
        }
    }

    public async Task<IReadOnlyList<PatronInfo>> GetPatronInfo(IReadOnlyList<FantasyCriticUserWithExternalLogins> patreonUsers)
    {
        _logger.Information("Getting patreon users.");
        Dictionary<string, FantasyCriticUser> patreonUserDictionary = [];
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

        _logger.Information("Making patreon request.");
        var campaignMembers = await GetCampaignMembers();
        _logger.Information("Patreon request successful.");

        List<PatronInfo> patronInfo = [];
        foreach (var member in campaignMembers)
        {
            var fantasyCriticUser = patreonUserDictionary.GetValueOrDefault(member.UserId);
            if (fantasyCriticUser is null)
            {
                continue;
            }

            bool isPlusUser = member.TierTitles.Contains(PlusTierTitle);
            bool isDonorUser = member.TierTitles.Contains(DonorTierTitle);
            string? donorName = null;
            if (isDonorUser)
            {
                isPlusUser = true;
                donorName = member.FullName;
                if (fantasyCriticUser.PatreonDonorNameOverride is not null)
                {
                    donorName = fantasyCriticUser.PatreonDonorNameOverride;
                }
            }

            patronInfo.Add(new PatronInfo(fantasyCriticUser, isPlusUser, donorName));
        }

        return patronInfo;
    }

    public async Task<bool> UserIsPlusUser(string patreonProviderID)
    {
        var campaignMembers = await GetCampaignMembers();
        var member = campaignMembers.FirstOrDefault(x => x.UserId == patreonProviderID);
        return member is not null && member.TierTitles.Contains(PlusTierTitle);
    }

    /// <summary>
    /// Calls Patreon with the newest stored access token. If Patreon rejects it, which it does about monthly as each one
    /// expires, trades the refresh token for a new pair, saves that at once (the old refresh token is now spent), and
    /// tries again.
    /// </summary>
    private async Task<IReadOnlyList<PatreonMember>> GetCampaignMembers()
    {
        var tokens = await _tokensRepo.GetMostRecentTokens();
        var members = await _patreonApi.GetCampaignMembers(tokens.AccessToken);
        if (members.IsSuccess)
        {
            return members.Value;
        }

        _logger.Information("Refreshing the Patreon creator tokens. {Reason}", members.Error);
        var refreshedTokens = await _patreonApi.RefreshTokens(tokens.RefreshToken);
        await _tokensRepo.SaveTokens(refreshedTokens);

        var retried = await _patreonApi.GetCampaignMembers(refreshedTokens.AccessToken);
        if (retried.IsFailure)
        {
            throw new InvalidOperationException($"Patreon rejected a freshly refreshed access token. {retried.Error}");
        }

        return retried.Value;
    }
}
