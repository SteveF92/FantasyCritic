using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.GG;

public class GGService : IGGService
{
    private readonly HttpClient _client;
    private readonly ILogger<GGService> _logger;

    public GGService(HttpClient client, ILogger<GGService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<GGGame?> GetGGGame(string ggToken)
    {
        try
        {
            const string query = @"query getGameByToken($token: String!) {
                                    getGameByToken(token: $token) {
                                        id
                                        name
                                        coverPath
                                        token
                                        slug
                                    }
                                }";
            var queryObject = new
            {
                query,
                variables = new
                {
                    token = ggToken
                },
                operationName = "getGameByToken"
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(queryObject), Encoding.UTF8, "application/json")
            };

            string responseString;
            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync();
            }

            var parsed = JsonSerializer.Deserialize<GGGraphQLResponse>(responseString, FantasyCriticJsonOptions.Default);
            var typedData = parsed?.Data?.GetGameByToken;
            if (typedData is null)
            {
                return null;
            }

            string? coverPath = null;
            if (!string.IsNullOrWhiteSpace(typedData.CoverPath))
            {
                var split = typedData.CoverPath.Split("/");
                coverPath = split.Last();
            }

            if (typedData.Token is null || typedData.Slug is null)
            {
                return null;
            }

            return new GGGame(typedData.Token, typedData.Slug, coverPath);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message == "Response status code does not indicate success: 404 (Not Found).")
            {
                return null;
            }
            _logger.LogError(httpEx, $"Getting an GG| game failed: {ggToken}");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Getting an GG| game failed: {ggToken}");
            throw;
        }
    }
}
