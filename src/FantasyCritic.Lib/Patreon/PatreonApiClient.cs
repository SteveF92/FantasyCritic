using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FantasyCritic.Lib.Configuration;

namespace FantasyCritic.Lib.Patreon;

/// <summary>
/// The two Patreon API v2 calls we make: list the campaign's members, and refresh the creator tokens. It holds no tokens;
/// <see cref="PatreonService"/> reads them from the database, passes them in, and saves the refreshed pair.
/// </summary>
public class PatreonApiClient
{
    public const string BaseAddress = "https://www.patreon.com/";

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly HttpClient _client;
    private readonly PatreonOptions _patreon;

    public PatreonApiClient(HttpClient client, PatreonOptions patreon)
    {
        _client = client;
        _patreon = patreon;
    }

    /// <summary>
    /// Every member of the campaign, across all pages. Fails only when Patreon rejects the access token, which the caller
    /// answers by refreshing; any other error throws.
    /// </summary>
    public async Task<Result<IReadOnlyList<PatreonMember>>> GetCampaignMembers(string accessToken)
    {
        List<PatreonMember> members = [];
        string? url = $"api/oauth2/v2/campaigns/{_patreon.CampaignId}/members"
                      + "?include=currently_entitled_tiers,user&fields%5Btier%5D=title&fields%5Buser%5D=full_name&page%5Bcount%5D=1000";
        while (url is not null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Result.Failure<IReadOnlyList<PatreonMember>>($"Patreon rejected the access token: {await response.Content.ReadAsStringAsync()}");
            }

            await ThrowIfUnsuccessful(response, "listing campaign members");
            var page = await response.Content.ReadFromJsonAsync<PatreonMembersPage>(_jsonOptions)
                       ?? throw new InvalidOperationException("Patreon returned an empty page of campaign members.");
            members.AddRange(page.ToMembers());
            url = page.Links?.Next;
        }

        return Result.Success<IReadOnlyList<PatreonMember>>(members);
    }

    /// <summary>
    /// Trades the refresh token for a new pair, the way Patreon documents it: a form body carrying the client secret. Both
    /// tokens are single use, so the caller must save the pair this returns. Throws with Patreon's response if it refuses.
    /// </summary>
    public async Task<PatreonTokens> RefreshTokens(string refreshToken)
    {
        using var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _patreon.ClientId,
            ["client_secret"] = _patreon.ClientSecret,
        });
        using var response = await _client.PostAsync("api/oauth2/token", body);
        await ThrowIfUnsuccessful(response, "refreshing the creator tokens");
        var tokens = await response.Content.ReadFromJsonAsync<PatreonTokenResponse>(_jsonOptions)
                     ?? throw new InvalidOperationException("Patreon returned an empty response when refreshing the creator tokens.");
        return new PatreonTokens(tokens.AccessToken, tokens.RefreshToken);
    }

    private static async Task ThrowIfUnsuccessful(HttpResponseMessage response, string action)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Patreon returned {(int)response.StatusCode} {response.StatusCode} when {action}: {body}", null, response.StatusCode);
    }
}
